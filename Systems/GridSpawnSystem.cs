using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    partial struct GridSpawnSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameConfigData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;

            var gridConfigData = SystemAPI.GetSingleton<GameConfigData>();

            for (int i = 0; i < gridConfigData.GridSize.x; i++)
            {
                for (int j = 0; j < gridConfigData.GridSize.y; j++)
                { 
                    Entity gridEntity = state.EntityManager.Instantiate(gridConfigData.GridPrefab);
                    state.EntityManager.SetComponentData(gridEntity, new LocalTransform
                    {
                        Position = new float3(i * 10f + 5f + gridConfigData.GridStartPosition.x, 0.5f, j * 10f + 5f + gridConfigData.GridStartPosition.y),
                        Rotation = quaternion.Euler(math.radians(90f), 0f, 0f),
                        Scale = 10f,
                    });
                    state.EntityManager.SetComponentData(gridEntity, new URPMaterialPropertyBaseColor { Value = new float4(0.5f, 0.5f, 0.5f, 0.5f) });
                }
            }

        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        
        }
    }
}
