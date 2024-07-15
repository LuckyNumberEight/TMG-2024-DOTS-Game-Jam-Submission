using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    partial struct CIWSTrackingSystem : ISystem
    {
        EntityQuery playerCIWSQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WeaponConfigData>();
            state.RequireForUpdate<TargetClosestToCenter>();
            playerCIWSQuery = SystemAPI.QueryBuilder().WithAllRW<CIWSPlayerFireCommand>().WithAll<CIWS, LocalTransform, TargetPosition, PlayerTag>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var targetClosestToCenter = SystemAPI.GetSingleton<TargetClosestToCenter>();

            new CIWSPlayerTrackingJob { ClosestTarget = targetClosestToCenter }.Schedule(playerCIWSQuery);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        
        }

        [BurstCompile]
        partial struct CIWSPlayerTrackingJob : IJobEntity
        {
            public TargetClosestToCenter ClosestTarget;
            void Execute(in CIWS ciws, ref CIWSPlayerFireCommand fireCommand, in LocalTransform transform, in TargetPosition targetPosition)
            {
                float3 trackPosition = targetPosition.Value + new float3(0f, 2f, -2f);

                if (!ClosestTarget.Entity.Equals(Entity.Null))
                {
                    float3 trackingOffset = float3.zero;

                    if (math.length(ClosestTarget.Velocity) > 0)
                        trackingOffset = math.distance(targetPosition.Value.xz, transform.Position.xz) / 10f * math.normalize(ClosestTarget.Velocity);

                    trackPosition = ClosestTarget.Position + trackingOffset;

                    if (trackPosition.y < 0.1f) // Compensate for ships sitting directly on the sea level
                        trackPosition.y = 0.5f;
                }

                var bowCIWSPositon = transform.Position + ciws.BowCIWSOffset.z * transform.Forward()
                    + ciws.BowCIWSOffset.y * transform.Up();
                var bowCIWSDistanceToTarget = math.distance(trackPosition, bowCIWSPositon);
                var bowDistanceModifier = bowCIWSDistanceToTarget / 25f;
                var bowCIWSDirectionToTarget = trackPosition + math.square(bowDistanceModifier) * math.up() - bowCIWSPositon;

                fireCommand.BowCIWSDirection = bowCIWSDirectionToTarget;
                Debug.DrawLine(bowCIWSPositon, bowCIWSDistanceToTarget * bowCIWSDirectionToTarget);

                var sternCIWSPositon = transform.Position + ciws.SternCIWSOffset.z * transform.Forward()
                    + ciws.SternCIWSOffset.y * transform.Up();
                var sternCIWSDistanceToTarget = math.distance(trackPosition, sternCIWSPositon);
                var sternDistanceModifier = sternCIWSDistanceToTarget / 25f;
                var sternCIWSDirectionToTarget = trackPosition + math.square(sternDistanceModifier) * math.up() - sternCIWSPositon;

                fireCommand.SternCIWSDirection = sternCIWSDirectionToTarget;
                Debug.DrawLine(sternCIWSPositon, sternCIWSDistanceToTarget * sternCIWSDirectionToTarget);
            }
        }
    }
}
