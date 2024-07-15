using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    [UpdateBefore(typeof(TransformSystemGroup))]
    partial struct BoidsAirSystem : ISystem
    {
        NativeParallelMultiHashMap<int, BoidAir> cellVsBoidAirPositions;
        EntityQuery boidAirQuery;
        EntityQuery enemyBoidAirQuery;

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
            state.RequireForUpdate<BoidAir>();
            boidAirQuery = SystemAPI.QueryBuilder().WithAll<BoidAir>().Build();
            enemyBoidAirQuery = SystemAPI.QueryBuilder().WithAll<BoidAir, TargetableTag>().Build();
            cellVsBoidAirPositions = new NativeParallelMultiHashMap<int, BoidAir>(0, Allocator.Persistent);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var boidConfigData = SystemAPI.GetSingleton<BoidConfigData>();
            var boidEntityCount = boidAirQuery.CalculateEntityCount();

            // Update Player Missile Target Positions
            foreach (var (boid, missile, transform, entity) in SystemAPI.Query<RefRW<BoidAir>, RefRW<Missile>, RefRO<LocalTransform>>()
                .WithNone<TargetableTag>().WithEntityAccess())
            {
                if (state.EntityManager.HasComponent<LocalTransform>(missile.ValueRO.Target))
                    boid.ValueRW.TargetPosition = state.EntityManager.
                        GetComponentData<LocalTransform>(missile.ValueRO.Target).Position;
                else
                    boid.ValueRW.TargetPosition = transform.ValueRO.Position + 10f * transform.ValueRO.Forward();
            }

            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();

            new UpdateBoidAirTargetPosition
            {
                TargetPosition = state.EntityManager.GetComponentData<LocalTransform>(playerEntity).Position
            }.Schedule(enemyBoidAirQuery);

            cellVsBoidAirPositions.Clear();

            if (boidEntityCount > cellVsBoidAirPositions.Capacity)
                cellVsBoidAirPositions.Capacity = boidEntityCount;

            var cellVsBoidAirPositionsParrallelWriter = cellVsBoidAirPositions.AsParallelWriter();

            new UpdateCellVsBoidAirPositionJob
            {
                CellVsBoidAirPositionsParallelWriter = cellVsBoidAirPositionsParrallelWriter,
                CellSize = boidConfigData.AirCellSize,
            }.ScheduleParallel();
            
            new UpdateBoidAirTransformJob
            {
                CellVsBoidAirPositions = cellVsBoidAirPositions,
                BoidData = boidConfigData,
                DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            cellVsBoidAirPositions.Dispose();
        }

        [BurstCompile]
        partial struct UpdateBoidAirTargetPosition : IJobEntity
        {
            public float3 TargetPosition;
            void Execute(ref BoidAir boid)
            {
                boid.TargetPosition = boid.IsMissile ? TargetPosition : TargetPosition + new float3(0f, 3f, 0f);
            }
        }

        [BurstCompile]
        partial struct UpdateCellVsBoidAirPositionJob : IJobEntity
        {
            public NativeParallelMultiHashMap<int, BoidAir>.ParallelWriter CellVsBoidAirPositionsParallelWriter;
            public int CellSize;
            void Execute(ref BoidAir boid, ref LocalTransform transform)
            {
                CellVsBoidAirPositionsParallelWriter.Add(GetUniqueKeyForPosition(transform.Position, CellSize), new BoidAir
                {
                    Position = transform.Position,
                    Velocity = boid.Velocity,
                    Acceleration = boid.Acceleration,
                    FlockID = boid.FlockID,
                    MoveSpeed = boid.MoveSpeed,
                    TurnSpeed = boid.TurnSpeed,
                    IsMissile = boid.IsMissile,
                });
            }
        }

        [BurstCompile]
        partial struct UpdateBoidAirTransformJob : IJobEntity
        {
            [ReadOnly] public NativeParallelMultiHashMap<int, BoidAir> CellVsBoidAirPositions;
            public BoidConfigData BoidData;
            public float DeltaTime;

            void Execute(ref BoidAir boid, ref LocalTransform transform)
            {
                int key = GetUniqueKeyForPosition(transform.Position, BoidData.AirCellSize);
                NativeParallelMultiHashMapIterator<int> keyIterator;
                BoidAir neighbor;
                int totalFlockCounter = 0;
                int sameFlockCounter = 0;
                float3 separation = float3.zero;
                float3 alignment = float3.zero;
                float3 cohesion = float3.zero;
                float perceptionRadius = BoidData.MissilePerceptionRadiusInMeters;
                float separationBias = BoidData.MissileSeparationBias;
                float alignmentBias = BoidData.MissileAlignmentBias;
                float cohesionBias = BoidData.MissileCohesionBias;
                float targetBias = BoidData.MissileTargetBias;
                float step = BoidData.MissileStep;

                if (!boid.IsMissile)
                {
                    perceptionRadius = BoidData.AircraftPerceptionRadiusInMeters;
                    separationBias = BoidData.AircraftSeparationBias;
                    alignmentBias = BoidData.AircraftAlignmentBias;
                    cohesionBias = BoidData.AircraftCohesionBias;
                    targetBias = BoidData.AircraftTargetBias;
                    step = BoidData.AircraftStep;
                }

                if (!CellVsBoidAirPositions.TryGetFirstValue(key, out neighbor, out keyIterator))
                    return;
                do
                {
                    if (transform.Position.Equals(neighbor.Position) || math.distance(transform.Position, neighbor.Position) > perceptionRadius)
                        continue;

                    if (!boid.FlockID.Equals(neighbor.FlockID))
                        continue;

                    totalFlockCounter++;
                    separation += (transform.Position - neighbor.Position) / math.distance(transform.Position, neighbor.Position);

                    sameFlockCounter++;
                    cohesion += neighbor.Position;
                    alignment += neighbor.Velocity;

                } while (CellVsBoidAirPositions.TryGetNextValue(out neighbor, ref keyIterator));

                if (totalFlockCounter > 0)
                    separation = math.normalize((separation / totalFlockCounter) - boid.Velocity) * separationBias;

                if (sameFlockCounter > 0)
                {
                    cohesion = math.normalize((cohesion / sameFlockCounter) - (transform.Position + boid.Velocity)) * cohesionBias;
                    alignment = math.normalize((alignment / sameFlockCounter) - boid.Velocity) * alignmentBias;
                }

                transform.Rotation = math.slerp(transform.Rotation, math.normalize(
                    quaternion.LookRotation(math.normalize(boid.Velocity), math.up())), DeltaTime * boid.TurnSpeed);

                boid.Acceleration += (cohesion + alignment + separation);
                boid.Velocity = math.normalize(boid.Velocity + boid.Acceleration) * boid.MoveSpeed;
                transform.Position = math.lerp(transform.Position, transform.Position + boid.MoveSpeed * transform.Forward(), DeltaTime * step);
                boid.Acceleration = math.normalize(boid.TargetPosition - transform.Position) * targetBias;
                boid.Position = transform.Position;
            }
        }
    }
}
