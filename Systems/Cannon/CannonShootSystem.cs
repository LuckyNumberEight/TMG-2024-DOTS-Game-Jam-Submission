using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    partial struct CannonShootSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WeaponConfigData>();
            state.RequireForUpdate<GameConfigData>();
            state.RequireForUpdate<TargetClosestToCenter>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            var weaponConfigData = SystemAPI.GetSingleton<WeaponConfigData>();
            var audioConfigEntity = SystemAPI.GetSingletonEntity<AudioConfigData>();
            var audioConfigData = SystemAPI.GetSingleton<AudioConfigData>();

            new ShootPlayerCannonJob
            {
                ECB = ecb,
                AudioConfigEntity = audioConfigEntity,
                AudioConfig = audioConfigData,
                WeaponConfig = weaponConfigData,
                ShellTransform = state.EntityManager.GetComponentData<LocalTransform>(weaponConfigData.CannonShellPlayerPrefab),
                ClosestToCenter = SystemAPI.GetSingleton<TargetClosestToCenter>(),
                DeltaTime = SystemAPI.Time.DeltaTime,
            }.Schedule();
        }

        [BurstCompile]
        partial struct ShootPlayerCannonJob : IJobEntity
        {
            public EntityCommandBuffer ECB;
            public Entity AudioConfigEntity;
            public AudioConfigData AudioConfig;
            public WeaponConfigData WeaponConfig;
            public LocalTransform ShellTransform;
            public TargetClosestToCenter ClosestToCenter;
            public float DeltaTime;

            void Execute(in Cannon cannon, ref CannonPlayerShootCommand shootCommand, MoveComponent move, Entity entity)
            {
                if (shootCommand.CooldownTimerInSeconds > 0)
                {
                    shootCommand.CooldownTimerInSeconds -= DeltaTime;
                    return;
                }

                if (!shootCommand.IsShootCommand)
                    return;

                shootCommand.IsShootCommand = false;
                shootCommand.MagazineCapacityCounter = shootCommand.MagazineCapacityCounter - 1;

                if (shootCommand.MagazineCapacityCounter < 0)
                    shootCommand.CooldownTimerInSeconds = cannon.ReloadCooldownInSeconds;

                ShellTransform.Position = shootCommand.BarrelPosition;
                ShellTransform.Rotation = shootCommand.BarrelRotation;

                var moveOffset = float3.zero;

                if (ClosestToCenter.Entity.Equals(Entity.Null))
                    moveOffset = move.Velocity;

                Entity shellEntity = ECB.Instantiate(WeaponConfig.CannonShellPlayerPrefab);
                ECB.SetComponent(shellEntity, ShellTransform);
                ECB.SetComponent(shellEntity, new Projectile
                {
                    Origin = entity,
                    LifetimeCounterInSeconds = 0f,
                    VelocityMetersPerSecond = WeaponConfig.CannonShellMoveSpeedMetersPerSecond * math.normalize(ShellTransform.Up()) + moveOffset,
                    ArmourPenetrationLevel = WeaponConfig.CannonShellArmourPenetrationLevel,
                    DamageAmount = WeaponConfig.CannonShellDamageAmount,
                });
                ECB.SetComponent(shellEntity, new URPMaterialPropertyBaseColor { Value = new float4(75f, 45f, 0f, 1f) });
                ECB.SetComponent(AudioConfigEntity, new AudioConfigData
                {
                    IsPlayingCannonShootSoundFX = true,
                    IsPlayingCIWSFiringSoundFX = AudioConfig.IsPlayingCIWSFiringSoundFX,
                    IsPlayingMissileLaunchSoundFX = AudioConfig.IsPlayingMissileLaunchSoundFX,
                    ExplosionHealthCounter = AudioConfig.ExplosionHealthCounter,
                });
            }
        }
    }
}
