using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    partial struct BoidsSurfaceSystem : ISystem
    {
        NativeParallelMultiHashMap<int, BoidSurface> cellVsBoidSurfacePositions;
        EntityQuery boidSurfaceQuery;
        EntityQuery updateEnemyBoidSurfaceTargetPositionsQuery;
        EntityQuery updateEnemyBoidSurfaceTransformsQuery;
        CollisionWorld collisionWorld;

        [BurstCompile]
        public static int GetUniqueKeyForPosition(in float3 position, int cellSize)
        {
            return (int)((15 * math.floor(position.x / cellSize))
                + (17 * math.floor(position.y / cellSize))
                + (19 * math.floor(position.z / cellSize)));
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<BoidConfigData>();
            state.RequireForUpdate<BoidSurface>();
            boidSurfaceQuery = SystemAPI.QueryBuilder().WithAll<BoidSurface>().Build();
            updateEnemyBoidSurfaceTargetPositionsQuery = SystemAPI.QueryBuilder().WithAll<BoidSurface, TargetableTag>().Build();
            updateEnemyBoidSurfaceTransformsQuery = SystemAPI.QueryBuilder().WithAllRW<BoidSurface, LocalTransform>().WithAll<TargetableTag>().Build();
            cellVsBoidSurfacePositions = new NativeParallelMultiHashMap<int, BoidSurface>(0, Allocator.Persistent);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var boidConfigData = SystemAPI.GetSingleton<BoidConfigData>();
            var boidEntityCount = boidSurfaceQuery.CalculateEntityCount();
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

            new UpdateBoidSurfaceTargetPosition
            {
                TargetPosition = state.EntityManager.GetComponentData<LocalTransform>(playerEntity).Position
            }.Schedule(updateEnemyBoidSurfaceTargetPositionsQuery);

            cellVsBoidSurfacePositions.Clear();

            if (boidEntityCount > cellVsBoidSurfacePositions.Capacity)
                cellVsBoidSurfacePositions.Capacity = boidEntityCount;

            var cellVsBoidSurfacePositionsParrallelWriter = cellVsBoidSurfacePositions.AsParallelWriter();

            new UpdateCellVsBoidSurfacePositionJob
            {
                CellVsBoidSurfacePositionsParallelWriter = cellVsBoidSurfacePositionsParrallelWriter,
                CellSize = boidConfigData.AirCellSize,
            }.ScheduleParallel();

            new UpdateBoidSurfaceTransformJob
            {
                CellVsBoidSurfacePositions = cellVsBoidSurfacePositions,
                BoidData = boidConfigData,
                RayCollisionWorld = collisionWorld,
                DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(updateEnemyBoidSurfaceTransformsQuery);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            cellVsBoidSurfacePositions.Dispose();
        }

        [BurstCompile]
        partial struct UpdateBoidSurfaceTargetPosition : IJobEntity
        {
            public float3 TargetPosition;
            void Execute(ref BoidSurface boid)
            {
                boid.TargetPosition = TargetPosition;
            }
        }

        [BurstCompile]
        partial struct UpdateCellVsBoidSurfacePositionJob : IJobEntity
        {
            public NativeParallelMultiHashMap<int, BoidSurface>.ParallelWriter CellVsBoidSurfacePositionsParallelWriter;
            public int CellSize;
            void Execute(ref BoidSurface boid, ref LocalTransform transform)
            {
                CellVsBoidSurfacePositionsParallelWriter.Add(GetUniqueKeyForPosition(transform.Position, CellSize), new BoidSurface
                {
                    Position = transform.Position,
                    Velocity = boid.Velocity,
                    Acceleration = boid.Acceleration,
                    FlockID = boid.FlockID,
                    MoveSpeed = boid.MoveSpeed,
                    TurnSpeed = boid.TurnSpeed,
                });
            }
        }

        [BurstCompile]
        partial struct UpdateBoidSurfaceTransformJob : IJobEntity
        {
            [ReadOnly] public NativeParallelMultiHashMap<int, BoidSurface> CellVsBoidSurfacePositions;
            [ReadOnly] public CollisionWorld RayCollisionWorld;
            public BoidConfigData BoidData;
            public float DeltaTime;

            void Execute(ref BoidSurface boid, ref LocalTransform transform)
            {
                int key = GetUniqueKeyForPosition(transform.Position, BoidData.AirCellSize);
                NativeParallelMultiHashMapIterator<int> keyIterator;
                BoidSurface neighbor;
                int totalFlockCounter = 0;
                int sameFlockCounter = 0;
                int obstacleCounter = 0;
                float3 avoidance = float3.zero;
                float3 separation = float3.zero;
                float3 alignment = float3.zero;
                float3 cohesion = float3.zero;

                for (int i = -BoidData.ShipRayCount / 2; i < BoidData.ShipRayCount / 2 + 1; i++)
                {
                    float3 rayDir = math.mul(quaternion.RotateY(i * 2f * math.PI / BoidData.ShipRayCount), transform.Forward());
                    Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
                    RaycastInput input = new RaycastInput()
                    {
                        Start = transform.Position,
                        End = transform.Position + rayDir * BoidData.ShipPerceptionRadiusInMeters,
                        Filter = new CollisionFilter()
                        {
                            BelongsTo = 1 << 7,
                            CollidesWith = 1 << 8 | 1 << 11,
                        }
                    };

                    if (RayCollisionWorld.CastRay(input, out hit))
                    {
                        obstacleCounter++;
                        avoidance += (transform.Position - hit.Position) / math.distance(transform.Position, hit.Position);
                    }

                    if (obstacleCounter > 0)
                        avoidance = math.normalize((avoidance / obstacleCounter) - boid.Velocity) * BoidData.ShipAvoidanceBias;

                    //Debug.DrawLine(transform.Position, transform.Position + rayDir * BoidData.ShipPerceptionRadiusInMeters);
                }

                if (!CellVsBoidSurfacePositions.TryGetFirstValue(key, out neighbor, out keyIterator))
                    return;
                do
                {
                    if (transform.Position.Equals(neighbor.Position) || math.distance(transform.Position, neighbor.Position) > BoidData.ShipPerceptionRadiusInMeters)
                        continue;

                    totalFlockCounter++;
                    separation += (transform.Position - neighbor.Position) / math.distance(transform.Position, neighbor.Position);

                    if (!boid.FlockID.Equals(neighbor.FlockID))
                        continue;

                    sameFlockCounter++;
                    cohesion += neighbor.Position;
                    alignment += neighbor.Velocity;

                } while (CellVsBoidSurfacePositions.TryGetNextValue(out neighbor, ref keyIterator));

                if (totalFlockCounter > 0)
                    separation = math.normalize((separation / totalFlockCounter) - boid.Velocity) * BoidData.ShipSeparationBias;

                if (sameFlockCounter > 0)
                {
                    cohesion = math.normalize((cohesion / sameFlockCounter) - (transform.Position + boid.Velocity)) * BoidData.ShipCohesionBias;
                    alignment = math.normalize((alignment / sameFlockCounter) - boid.Velocity) * BoidData.ShipAlignmentBias;
                }

                transform.Rotation = math.slerp(transform.Rotation, math.normalize(
                    quaternion.LookRotation(math.normalize(boid.Velocity), math.up())), DeltaTime * boid.TurnSpeed);

                var acceleration = cohesion + alignment + separation + avoidance;

                if (math.isnan(acceleration.x) || math.isnan(acceleration.y) || math.isnan(acceleration.z))
                    acceleration = float3.zero;

                boid.Acceleration += acceleration;
                boid.Velocity = math.normalize(boid.Velocity + boid.Acceleration) * boid.MoveSpeed;
                transform.Position = math.lerp(transform.Position, transform.Position + boid.MoveSpeed * transform.Forward(), DeltaTime * BoidData.ShipStep);
                boid.Acceleration = math.normalize(boid.TargetPosition - transform.Position) * BoidData.ShipTargetBias;
                boid.Position = transform.Position;
            }
        }
    }
}