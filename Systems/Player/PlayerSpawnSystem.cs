using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    partial struct PlayerSpawnSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<UnitConfigData>();
            state.RequireForUpdate<PlayerSpawnPoint>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            UnitConfigData unitConfigData = SystemAPI.GetSingleton<UnitConfigData>();
            Entity entity = state.EntityManager.Instantiate(unitConfigData.PlayerPefab);

            var unit = state.EntityManager.GetComponentData<Unit>(entity);
            state.EntityManager.SetComponentData(entity, new Unit
            {
                ID = 0,
                UnitType = UnitType.Destroyer,
                UnitArmourLevel = unit.UnitArmourLevel,
                UnitMaxHitPoints = unit.UnitMaxHitPoints,
                UnitCurrentHitPoints = unit.UnitCurrentHitPoints,
                Colour = 2f * unitConfigData.PlayerColour,
            });

            SystemAPI.SetSingleton(new AudioConfigData { ExplosionHealthCounter = unit.UnitMaxHitPoints });

            var linkedEntities = state.EntityManager.GetBuffer<LinkedEntityGroup>(entity);
            foreach (var linkedEntity in linkedEntities)
                if (state.EntityManager.HasComponent<URPMaterialPropertyBaseColor>(linkedEntity.Value))
                    state.EntityManager.SetComponentData(linkedEntity.Value,
                        new URPMaterialPropertyBaseColor { Value = 2f * unitConfigData.PlayerColour });

            var spawnPoint = SystemAPI.GetSingleton<PlayerSpawnPoint>();

            state.EntityManager.SetComponentData(entity, new LocalTransform
            {
                Position = spawnPoint.Position,
                Rotation = quaternion.identity,
                Scale = 1
            });
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        
        }
    }
}
