using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    class PlayerSpawnPointAuthoring : MonoBehaviour
    {
        class Baker : Baker<PlayerSpawnPointAuthoring>
        {
            public override void Bake(PlayerSpawnPointAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.WorldSpace);
                AddComponent(entity, new PlayerSpawnPoint { Position = authoring.transform.position });
            }
        }
    }

    struct PlayerSpawnPoint : IComponentData
    {
        public float3 Position;
    }
}
