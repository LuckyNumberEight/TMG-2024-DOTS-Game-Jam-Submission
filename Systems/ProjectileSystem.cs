using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    partial struct ProjectileSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WeaponConfigData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new ProjectileJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                GravityConstant = SystemAPI.GetSingleton<WeaponConfigData>().GravityConstant,
            }.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        
        }

        [BurstCompile]
        partial struct ProjectileJob : IJobEntity
        {
            public float DeltaTime;
            public float GravityConstant;

            void Execute(ref Projectile projectile, ref Destructible destructible, ref LocalTransform transform)
            {
                transform.Position += DeltaTime * projectile.VelocityMetersPerSecond;
                projectile.LifetimeCounterInSeconds += DeltaTime;
                projectile.VelocityMetersPerSecond += DeltaTime * new float3(0f, -GravityConstant, 0f);

                if (projectile.VelocityMetersPerSecond.y < 0)
                    transform = transform.RotateX(- 0.1f * projectile.VelocityMetersPerSecond.y * DeltaTime);


                if (transform.Position.y < 0 || projectile.LifetimeCounterInSeconds > 3f)
                    destructible.IsDestroyed = true;
            }
        }
    }
}
