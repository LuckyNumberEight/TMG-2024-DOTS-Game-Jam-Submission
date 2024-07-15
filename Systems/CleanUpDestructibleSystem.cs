using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    partial struct CleanUpDestructibleSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameConfigData>();
            state.RequireForUpdate<WeaponConfigData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var gameConfigData = SystemAPI.GetSingleton<GameConfigData>();
            var weaponConfigData = SystemAPI.GetSingleton<WeaponConfigData>();

            foreach (var (destructible, unit, entity) in SystemAPI.Query<Destructible, Unit>().WithEntityAccess())
            {
                if (!destructible.IsDestroyed)
                    continue;

                gameConfigData.Score += (int)unit.UnitMaxHitPoints;

                if (!gameConfigData.IsScoreUpdated)
                    gameConfigData.IsScoreUpdated = true;

                ecb.DestroyEntity(entity);

                if (!destructible.IsSpawnExplosion)
                    continue;

                Entity explosionEntity = ecb.Instantiate(weaponConfigData.MissileExplosionPrefab);
                ecb.SetComponent(explosionEntity, new LocalTransform
                {
                    Position = destructible.ExplosionPosition,
                    Rotation = quaternion.identity,
                    Scale = 5f,
                });
                ecb.SetComponent(explosionEntity, new URPMaterialPropertyBaseColor { Value = new float4(75, 45, 0f, 0.1f) });
            }

            SystemAPI.SetSingleton(gameConfigData);

            foreach (var (destructible, entity) in SystemAPI.Query<Destructible>().WithEntityAccess())
            {
                if (!destructible.IsDestroyed)
                    continue;

                ecb.DestroyEntity(entity);

                if (!destructible.IsSpawnExplosion)
                    continue;

                Entity explosionEntity = ecb.Instantiate(weaponConfigData.MissileExplosionPrefab);
                ecb.SetComponent(explosionEntity, new LocalTransform
                {
                    Position = destructible.ExplosionPosition,
                    Rotation = quaternion.identity,
                    Scale = 2f,
                });
                ecb.SetComponent(explosionEntity, new URPMaterialPropertyBaseColor { Value = new float4(75, 45, 0f, 0.1f) });
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        
        }
    }
}
