using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    [UpdateBefore(typeof(TransformSystemGroup))]
    partial struct MissileSystem : ISystem
    {

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new UpdateMissile { DeltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            
        }

        [BurstCompile]
        partial struct UpdateMissile : IJobEntity
        {
            public float DeltaTime;

            void Execute(ref Missile missile, ref Destructible destructible)
            {

                if (missile.LifetimeCounter > 10f)
                    destructible.IsDestroyed = true;
                else
                    missile.LifetimeCounter += DeltaTime;
            }
        }
    }
}
