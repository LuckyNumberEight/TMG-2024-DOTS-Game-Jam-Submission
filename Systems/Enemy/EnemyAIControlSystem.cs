using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    partial struct EnemyAIControlSystem : ISystem
    {
        EntityQuery playerMissilesQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AIConfigData>();
            state.RequireForUpdate<PlayerTag>();
            playerMissilesQuery = SystemAPI.QueryBuilder().WithAll<Missile, LocalTransform>().WithNone<TargetableTag>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var aiConfigData = SystemAPI.GetSingleton<AIConfigData>();
            var playerMissileCount = playerMissilesQuery.CalculateEntityCount();
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerTransform = state.EntityManager.GetComponentData<LocalTransform>(playerEntity);

            new AILaunchMissileJob
            {
                PlayerTransform = playerTransform,
                AIConfig = aiConfigData,
                DeltaTime = SystemAPI.Time.DeltaTime
            }.Schedule();

            new AIFireCIWSJob
            {
                PlayerMissileTransforms = playerMissilesQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob),
                AIConfig = aiConfigData,
                DeltaTime = SystemAPI.Time.DeltaTime
            }.Schedule();

        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            
        }

        [BurstCompile]
        partial struct AIFireCIWSJob : IJobEntity
        {
            public AIConfigData AIConfig;
            [DeallocateOnJobCompletion] public NativeArray<LocalTransform> PlayerMissileTransforms;
            public float DeltaTime;

            void Execute(ref CIWSAIFireCommand fireCommand, in LocalTransform transform)
            {
                fireCommand.IsFireCommand = false;
                fireCommand.TargetPosition = transform.Position;
                fireCommand.DistanceToTarget = float.MaxValue;

                if (fireCommand.CooldownTimerInSeconds > 0)
                {
                    fireCommand.CooldownTimerInSeconds -= DeltaTime;

                    if (fireCommand.CooldownTimerInSeconds <= 0)
                        fireCommand.FireTimerInSeconds = 0f;
                    return;
                }

                if (fireCommand.FireTimerInSeconds > AIConfig.CIWSMaxFiringPeriodInSeconds)
                {
                    fireCommand.CooldownTimerInSeconds = AIConfig.CIWSCooldownPeriodInSeconds;
                    return;
                }
                
                for (int i = 0; i < PlayerMissileTransforms.Length; i++)
                {
                    var currentDistance = math.distance(transform.Position, PlayerMissileTransforms[i].Position);
                    if (currentDistance < AIConfig.CIWSMaxEngagementDistanceInMeters && currentDistance < fireCommand.DistanceToTarget)
                    {
                        fireCommand.TargetPosition = PlayerMissileTransforms[i].Position;
                        fireCommand.DistanceToTarget = currentDistance;
                    }
                }

                if (math.distance(fireCommand.TargetPosition, transform.Position) < 1.0f)
                    return;
                
                fireCommand.IsFireCommand = true;
                fireCommand.FireTimerInSeconds += DeltaTime;
            }
        }

        [BurstCompile]
        partial struct AILaunchMissileJob : IJobEntity
        {
            public LocalTransform PlayerTransform;
            public AIConfigData AIConfig;
            public float DeltaTime;

            void Execute(ref MissileAILaunchCommand launchCommand, in LocalTransform transform)
            {
                if (launchCommand.WaveTimerInSeconds > 0)
                    launchCommand.WaveTimerInSeconds -= DeltaTime;

                else
                {
                    launchCommand.MissileVolleyCounter = 0;
                    launchCommand.WaveTimerInSeconds = AIConfig.MissileTimeBetweenVolleysInSeconds;
                }

                if (launchCommand.MissileVolleyCounter >= AIConfig.MissilesNumberPerVolley)
                    return;

                if (launchCommand.VolleyTimerInSeconds > 0)
                {
                    launchCommand.VolleyTimerInSeconds -= DeltaTime;
                    return;
                }

                if (math.distance(PlayerTransform.Position, transform.Position) > AIConfig.MissileMaxLaunchDistanceInMeters)
                    return;

                launchCommand.IsLaunchCommand = true;
            }
        }
    }
}
