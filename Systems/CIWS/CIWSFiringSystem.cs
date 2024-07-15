using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    partial struct CIWSFiringSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WeaponConfigData>();
            state.RequireForUpdate<TargetClosestToCenter>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            var weaponConfigData = SystemAPI.GetSingleton<WeaponConfigData>();

            new FirePlayerCIWSJob
            {
                ECB = ecb,
                ConfigData = weaponConfigData,
                BulletTransform = state.EntityManager.GetComponentData<LocalTransform>(weaponConfigData.CIWSBulletPlayerPrefab),
                ClosestToCenter = SystemAPI.GetSingleton<TargetClosestToCenter>(),
                DeltaTime = SystemAPI.Time.DeltaTime,
            }.Schedule();

            new FireAICIWSJob
            {
                ECB = ecb,
                ConfigData = weaponConfigData,
                BulletTransform = state.EntityManager.GetComponentData<LocalTransform>(weaponConfigData.CIWSBulletPrefab),
                DeltaTime = SystemAPI.Time.DeltaTime,
            }.Schedule();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        
        }

        [BurstCompile]
        partial struct FirePlayerCIWSJob : IJobEntity
        {
            public EntityCommandBuffer ECB;
            public WeaponConfigData ConfigData;
            public LocalTransform BulletTransform;
            public TargetClosestToCenter ClosestToCenter;
            public float DeltaTime;

            void Execute(ref CIWS ciws, ref CIWSPlayerFireCommand fireCommand, in LocalTransform transform, in MoveComponent move, in Entity entity)
            {
                if (!fireCommand.IsFireCommand)
                {
                    fireCommand.ROFTimerInSeconds = 0f;
                    return;
                }

                if (fireCommand.ROFTimerInSeconds > 0)
                {
                    fireCommand.ROFTimerInSeconds -= DeltaTime;
                    return;
                }

                fireCommand.ROFTimerInSeconds = 1 / ciws.CIWSRoundsPerSecond;

                BulletTransform.Position = transform.Position + transform.Forward() * ciws.BowCIWSOffset.z + transform.Up() * ciws.BowCIWSOffset.y;
                BulletTransform.Rotation = quaternion.LookRotation(fireCommand.BowCIWSDirection, math.up());

                var moveOffset = float3.zero;

                if (ClosestToCenter.Entity.Equals(Entity.Null))
                    moveOffset = move.Velocity;

                Entity bowBulletEntity = ECB.Instantiate(ConfigData.CIWSBulletPlayerPrefab);
                ECB.SetComponent(bowBulletEntity, BulletTransform);
                ECB.SetComponent(bowBulletEntity, new Projectile
                {
                    Origin = entity,
                    LifetimeCounterInSeconds = 0f,
                    VelocityMetersPerSecond = ConfigData.CIWSBulletSpeedMetersPerSecond * math.normalize(fireCommand.BowCIWSDirection) + moveOffset,
                    ArmourPenetrationLevel = ConfigData.CIWSBulletArmourPenetrationLevel,
                    DamageAmount = ConfigData.CIWSBulletDamageAmount,
                });
                ECB.SetComponent(bowBulletEntity, new URPMaterialPropertyBaseColor { Value = new float4(25f, 15f, 0f, 1f) });

                BulletTransform.Position = transform.Position + transform.Forward() * ciws.SternCIWSOffset.z + transform.Up() * ciws.SternCIWSOffset.y;
                BulletTransform.Rotation = quaternion.LookRotation(fireCommand.SternCIWSDirection, math.up());

                Entity sternBulletEntity = ECB.Instantiate(ConfigData.CIWSBulletPlayerPrefab);
                ECB.SetComponent(sternBulletEntity, BulletTransform);
                ECB.SetComponent(sternBulletEntity, new Projectile
                {
                    Origin = entity,
                    LifetimeCounterInSeconds = 0f,
                    VelocityMetersPerSecond = ConfigData.CIWSBulletSpeedMetersPerSecond * math.normalize(fireCommand.SternCIWSDirection) + moveOffset,
                    ArmourPenetrationLevel = ConfigData.CIWSBulletArmourPenetrationLevel,
                    DamageAmount = ConfigData.CIWSBulletDamageAmount,
                });
                ECB.SetComponent(sternBulletEntity, new URPMaterialPropertyBaseColor { Value = new float4(25f, 15f, 0f, 1f) });
            }
        }

        [BurstCompile]
        partial struct FireAICIWSJob : IJobEntity
        {
            public EntityCommandBuffer ECB;
            public WeaponConfigData ConfigData;
            public LocalTransform BulletTransform;
            public float DeltaTime;

            void Execute(ref CIWS ciws, ref CIWSAIFireCommand fireCommand, in LocalTransform transform, in BoidSurface boid, in Entity entity)
            {
                if (!fireCommand.IsFireCommand)
                {
                    fireCommand.ROFTimerInSeconds = 0f;
                    return;
                }

                if (fireCommand.ROFTimerInSeconds > 0)
                {
                    fireCommand.ROFTimerInSeconds -= DeltaTime;
                    return;
                }

                fireCommand.ROFTimerInSeconds = 1 / ciws.CIWSRoundsPerSecond;

                var bowCIWSPosition = transform.Position + transform.Forward() * ciws.BowCIWSOffset.z + transform.Up() * ciws.BowCIWSOffset.y;
                var bowCIWSDirection = fireCommand.TargetPosition + math.square(fireCommand.DistanceToTarget / 25f) * math.up() - bowCIWSPosition;
                BulletTransform.Position = bowCIWSPosition;
                BulletTransform.Rotation = quaternion.LookRotation(bowCIWSDirection, math.up());

                Entity bowBulletEntity = ECB.Instantiate(ConfigData.CIWSBulletPrefab);
                ECB.SetComponent(bowBulletEntity, BulletTransform);
                ECB.SetComponent(bowBulletEntity, new Projectile
                {
                    Origin = entity,
                    LifetimeCounterInSeconds = 0f,
                    VelocityMetersPerSecond = ConfigData.CIWSBulletSpeedMetersPerSecond * math.normalize(bowCIWSDirection) + boid.Velocity,
                    ArmourPenetrationLevel = ConfigData.CIWSBulletArmourPenetrationLevel,
                    DamageAmount = ConfigData.CIWSBulletDamageAmount,
                });
                ECB.SetComponent(bowBulletEntity, new URPMaterialPropertyBaseColor { Value = new float4(25f, 15f, 0f, 1f) });

                var sternCIWSPosition = transform.Position + transform.Forward() * ciws.SternCIWSOffset.z + transform.Up() * ciws.SternCIWSOffset.y;
                var sternCIWSDirection = fireCommand.TargetPosition + math.square(fireCommand.DistanceToTarget / 25f) * math.up() - sternCIWSPosition;
                BulletTransform.Position = sternCIWSPosition;
                BulletTransform.Rotation = quaternion.LookRotation(sternCIWSDirection, math.up());

                Entity sternBulletEntity = ECB.Instantiate(ConfigData.CIWSBulletPrefab);
                ECB.SetComponent(sternBulletEntity, BulletTransform);
                ECB.SetComponent(sternBulletEntity, new Projectile
                {
                    Origin = entity,
                    LifetimeCounterInSeconds = 0f,
                    VelocityMetersPerSecond = ConfigData.CIWSBulletSpeedMetersPerSecond * math.normalize(sternCIWSDirection) + boid.Velocity,
                    ArmourPenetrationLevel = ConfigData.CIWSBulletArmourPenetrationLevel,
                    DamageAmount = ConfigData.CIWSBulletDamageAmount,
                });
                ECB.SetComponent(sternBulletEntity, new URPMaterialPropertyBaseColor { Value = new float4(25f, 15f, 0f, 1f) });
            }
        }
    }
}
