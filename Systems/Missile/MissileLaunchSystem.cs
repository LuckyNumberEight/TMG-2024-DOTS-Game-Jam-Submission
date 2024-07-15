using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    partial struct MissileLaunchSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AudioConfigData>();
            state.RequireForUpdate<WeaponConfigData>();
            state.RequireForUpdate<AIConfigData>();
            state.RequireForUpdate<CurrentTarget>();
            state.RequireForUpdate<PlayerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            var currentTargets = SystemAPI.GetSingletonBuffer<CurrentTarget>();
            var audioConfigEntity = SystemAPI.GetSingletonEntity<AudioConfigData>();
            var audioConfigData = SystemAPI.GetSingleton<AudioConfigData>();
            var weaponConfigData = SystemAPI.GetSingleton<WeaponConfigData>();
            var aiConfigData = SystemAPI.GetSingleton<AIConfigData>();
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var deltaTime = SystemAPI.Time.DeltaTime;

            new LaunchPlayerMissileJob
            {
                ECB = ecb,
                CurrentTargets = currentTargets,
                AudioConfigEntity = audioConfigEntity,
                AudioConfig = audioConfigData,
                WeaponConfig = weaponConfigData,
                DeltaTime = deltaTime,
            }.Schedule();

            new LaunchAIMissilesJob
            {
                ECB = ecb,
                PlayerEntity = playerEntity,
                WeaponConfig = weaponConfigData,
                AIConfig = aiConfigData,
                DeltaTime = deltaTime,
            }.Schedule();

            new UpdateBoosterColourJob { ECB = ecb }.Schedule();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        [BurstCompile]
        partial struct UpdateBoosterColourJob : IJobEntity
        {
            internal EntityCommandBuffer ECB;

            void Execute(in Entity entity, UpdateBoosterColourTag boosterColourTag, DynamicBuffer<Child> children)
            {
                foreach (var child in children)
                    ECB.SetComponent(child.Value, new URPMaterialPropertyBaseColor { Value = new float4(75f, 45f, 0f, 1f) });

                ECB.SetComponentEnabled<UpdateBoosterColourTag>(entity, false);
                ECB.SetComponentEnabled<Destructible>(entity, true);
            }
        }

        [BurstCompile]
        partial struct LaunchPlayerMissileJob : IJobEntity
        {
            [ReadOnly] public DynamicBuffer<CurrentTarget> CurrentTargets;
            public EntityCommandBuffer ECB;
            public Entity AudioConfigEntity;
            public AudioConfigData AudioConfig;
            public WeaponConfigData WeaponConfig;
            public float DeltaTime;

            void Execute(ref MissilePlayerLaunchCommand launchCommand, in MissileLauncher launcher, in Unit unit, in LocalTransform transform, in Entity entity)
            {
                if (launchCommand.CooldownTimerInSeconds > 0)
                {
                    launchCommand.CooldownTimerInSeconds -= DeltaTime;
                    return; ;
                }

                if (!launchCommand.IsLaunchCommand)
                    return;

                launchCommand.IsLaunchCommand = false;
                launchCommand.MagazineCapacityCounter -= CurrentTargets.Length;

                if (launchCommand.MagazineCapacityCounter <= 0)
                    launchCommand.CooldownTimerInSeconds = launcher.ReloadCooldownInSeconds;

                for (int i = 0; i < CurrentTargets.Length; i++)
                {
                    Entity missileEntity = ECB.Instantiate(WeaponConfig.MissilePlayerPrefab);

                    if (unit.UnitType == UnitType.Fighter)
                    {
                        ECB.SetComponent(missileEntity, new LocalTransform
                        {
                            Position = transform.Position + new float3(0f, -1f, 0f),
                            Rotation = transform.Rotation,
                            Scale = 1,
                        });
                    }
                    else
                    {
                        ECB.SetComponent(missileEntity, new LocalTransform
                        {
                            Position = transform.Position + launcher.LauncherOffset.z * transform.Forward() + new float3(0f, 0.5f, 0f),
                            Rotation = math.mul(transform.Rotation, quaternion.Euler(-90f, 0f, 0f)),
                            Scale = 1,
                        });
                    }

                    ECB.SetComponent(missileEntity, new URPMaterialPropertyBaseColor { Value = 10f * unit.Colour });

                    ECB.SetComponent(missileEntity, new Missile
                    {
                        Origin = entity,
                        Target = CurrentTargets[i].TargetEntity,
                        Colour = 10f * unit.Colour,
                        LifetimeCounter = 0,
                    });

                    ECB.SetComponent(missileEntity, new BoidAir
                    {
                        Position = transform.Position,
                        Velocity = WeaponConfig.MissileMoveSpeedMetersPerSecond,
                        Acceleration = float3.zero,
                        FlockID = 0,
                        MoveSpeed = WeaponConfig.MissilePlayerMoveSpeedMetersPerSecond,
                        TurnSpeed = WeaponConfig.MissileTurnSpeedRadiansPerSecond,
                        IsMissile = true,
                    });

                    ECB.SetComponent(AudioConfigEntity, new AudioConfigData
                    {
                        IsPlayingCIWSFiringSoundFX = AudioConfig.IsPlayingCIWSFiringSoundFX,
                        IsPlayingCannonShootSoundFX = AudioConfig.IsPlayingCannonShootSoundFX,
                        IsPlayingMissileLaunchSoundFX = true,
                        ExplosionHealthCounter = AudioConfig.ExplosionHealthCounter,
                    });
                }
            }
        }
        
        [BurstCompile]
        partial struct LaunchAIMissilesJob : IJobEntity
        {
            public EntityCommandBuffer ECB;
            public WeaponConfigData WeaponConfig;
            public AIConfigData AIConfig;
            public Entity PlayerEntity;
            public float DeltaTime;

            void Execute(ref MissileAILaunchCommand launchCommand, in MissileLauncher launcher, in Unit unit, in LocalTransform transform, in Entity entity)
            {
                if (!launchCommand.IsLaunchCommand)
                    return;

                launchCommand.IsLaunchCommand = false;
                launchCommand.VolleyTimerInSeconds = AIConfig.MissileTimeBetweenLaunchesInSeconds;
                launchCommand.MissileVolleyCounter++;

                Entity missileEntity = ECB.Instantiate(WeaponConfig.MissilePrefab);

                if (unit.UnitType == UnitType.Fighter)
                {
                    ECB.SetComponent(missileEntity, new LocalTransform
                    {
                        Position = transform.Position + launcher.LauncherOffset.z * transform.Forward() + new float3(0f, -1f, 0f),
                        Rotation = transform.Rotation,
                        Scale = 1
                    });
                }
                else
                {
                    ECB.SetComponent(missileEntity, new LocalTransform
                    {
                        Position = transform.Position + launcher.LauncherOffset.z * transform.Forward() + new float3(0f, 0.5f, 0f),
                        Rotation = math.mul(transform.Rotation, quaternion.Euler(-90f, 0f, 0f)),
                        Scale = 1
                    });
                }

                ECB.SetComponent(missileEntity, new URPMaterialPropertyBaseColor { Value = 10f * unit.Colour });

                ECB.SetComponent(missileEntity, new Missile
                {
                    Origin = entity,
                    Target = PlayerEntity,
                    Colour = 10f * unit.Colour,
                    LifetimeCounter = 0,
                });

                ECB.SetComponent(missileEntity, new BoidAir
                {
                    Position = transform.Position,
                    Velocity = WeaponConfig.MissileMoveSpeedMetersPerSecond,
                    Acceleration = float3.zero,
                    FlockID = 1,
                    MoveSpeed = WeaponConfig.MissileMoveSpeedMetersPerSecond,
                    TurnSpeed = WeaponConfig.MissileTurnSpeedRadiansPerSecond,
                    IsMissile = true,
                });
            }
        }
    }
}
