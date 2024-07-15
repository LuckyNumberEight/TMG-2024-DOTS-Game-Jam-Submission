using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    partial struct EnemySpawnSystem : ISystem
    {
        Random random;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<UnitConfigData>();
            random = new Random(123);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var unitConfigData = SystemAPI.GetSingleton<UnitConfigData>();

            // Setup Practice Targets
            foreach (var (practiceTarget, entity) in SystemAPI.Query<PracticeTargetTag>().WithEntityAccess())
            {
                state.EntityManager.SetComponentData(entity, new URPMaterialPropertyBaseColor { Value = 5f * unitConfigData.EnemyColour });
                state.EntityManager.SetComponentEnabled<PracticeTargetTag>(entity, false);
            }
            
            // Spawn Enemies
            foreach (var (spawnPoint, transform) in SystemAPI.Query<RefRW<EnemySpawnPoint>, RefRO<LocalToWorld>>())
            {
                if (spawnPoint.ValueRO.CooldownTimer.Equals(-1.0f))   // Spawn Disabled
                    continue;

                if (spawnPoint.ValueRO.DelayTimer > 0)
                {
                    spawnPoint.ValueRW.DelayTimer -= SystemAPI.Time.DeltaTime;
                    continue;
                }

                if (spawnPoint.ValueRO.CooldownTimer > 0)
                {
                    spawnPoint.ValueRW.CooldownTimer -= SystemAPI.Time.DeltaTime;
                    continue;
                }

                if (spawnPoint.ValueRO.IsContinuosSpawn)
                    spawnPoint.ValueRW.CooldownTimer = spawnPoint.ValueRO.TimeBetweenSpawnInSeconds;

                else if (!spawnPoint.ValueRO.CooldownTimer.Equals(-1.0f))
                    spawnPoint.ValueRW.CooldownTimer = -1.0f;

                Entity spawnEntityPrefab;

                switch(spawnPoint.ValueRO.UnitType)
                {
                    case UnitType.Destroyer:
                        spawnEntityPrefab = unitConfigData.DestroyerPrefab; 
                        break;

                    case UnitType.Fighter:
                        spawnEntityPrefab= unitConfigData.FighterPrefab;
                        break;

                    default:
                        spawnEntityPrefab = unitConfigData.PracticeTargetPrefab; 
                        break;
                }

                Entity entity = state.EntityManager.Instantiate(spawnEntityPrefab);

                var unit = state.EntityManager.GetComponentData<Unit>(entity);
                var colour = 2f * unitConfigData.EnemyColour;

                if (unit.UnitType == UnitType.PracticeTarget)
                    colour = 5f * unitConfigData.EnemyColour;

                state.EntityManager.SetComponentData(entity, new Unit
                {
                    ID = unitConfigData.EnemyIDCounter++,
                    UnitType = spawnPoint.ValueRO.UnitType,
                    Colour = colour,
                    UnitArmourLevel = unit.UnitArmourLevel,
                    UnitMaxHitPoints = unit.UnitMaxHitPoints,
                    UnitCurrentHitPoints = unit.UnitCurrentHitPoints,
                });

                var linkedEntities = state.EntityManager.GetBuffer<LinkedEntityGroup>(entity);
                foreach (var linkedEntity in linkedEntities)
                    if (state.EntityManager.HasComponent<URPMaterialPropertyBaseColor>(linkedEntity.Value))
                        state.EntityManager.SetComponentData(linkedEntity.Value,
                            new URPMaterialPropertyBaseColor { Value = 2 * unitConfigData.EnemyColour });

                state.EntityManager.SetComponentData(entity, new LocalTransform
                {
                    Position = transform.ValueRO.Position,
                    Rotation = transform.ValueRO.Rotation,
                    Scale = 1,
                });

                if (state.EntityManager.HasComponent<BoidSurface>(entity))
                {
                    var boidSurface = state.EntityManager.GetComponentData<BoidSurface>(entity);
                    state.EntityManager.SetComponentData(entity, new BoidSurface
                    {
                        Position = spawnPoint.ValueRO.Position,
                        Velocity = boidSurface.MoveSpeed * transform.ValueRO.Forward,
                        Acceleration = float3.zero,
                        FlockID = 2,
                        MoveSpeed = boidSurface.MoveSpeed,
                        TurnSpeed = boidSurface.TurnSpeed,
                    });
                }

                if (state.EntityManager.HasComponent<BoidAir>(entity))
                {
                    var boidAir = state.EntityManager.GetComponentData<BoidAir>(entity);
                    state.EntityManager.SetComponentData(entity, new BoidAir
                    {
                        Position = spawnPoint.ValueRO.Position,
                        Velocity = boidAir.MoveSpeed * transform.ValueRO.Forward,
                        Acceleration = float3.zero,
                        FlockID = 1,
                        MoveSpeed = boidAir.MoveSpeed,
                        TurnSpeed = boidAir.TurnSpeed,
                    });
                }

                if (state.EntityManager.HasComponent<MissileAILaunchCommand>(entity))
                {
                    var aiConfigData = SystemAPI.GetSingleton<AIConfigData>();

                    state.EntityManager.SetComponentData(entity, new MissileAILaunchCommand
                    {
                        IsLaunchCommand = false,
                        MissileVolleyCounter = aiConfigData.MissilesNumberPerVolley,
                        WaveTimerInSeconds = random.NextFloat(0.5f, 1.5f) * aiConfigData.MissileTimeBetweenLaunchesInSeconds,
                    });
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        
        }
    }
}
