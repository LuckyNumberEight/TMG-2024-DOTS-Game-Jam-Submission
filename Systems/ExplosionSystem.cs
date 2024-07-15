using Unity.Burst;
using Unity.Entities;
using Unity.VisualScripting;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    partial struct ExplosionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new ExplosionJob { DeltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        
        }

        [BurstCompile]
        partial struct ExplosionJob : IJobEntity
        {
            public float DeltaTime;

            void Execute(ref Explosion explosion, ref Destructible destructible)
            {
                if (explosion.LifeTimerInSeconds > 0)
                {
                    explosion.LifeTimerInSeconds -= DeltaTime;
                    return;
                }

                destructible.IsDestroyed = true;
            }
        }
    }
}
