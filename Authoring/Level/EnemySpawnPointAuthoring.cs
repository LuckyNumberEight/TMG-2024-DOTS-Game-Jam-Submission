using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    class EnemySpawnPointAuthoring : MonoBehaviour
    {
        [SerializeField] UnitType unitType;
        [SerializeField] bool isContinuosSpawn = false;
        [SerializeField][Range(-1.0f, 360f)] float timeBeforeInitialSpawnInSeconds = 10.0f;
        [SerializeField] [Range(-1.0f, 120f)]float timeBetweenSpawnInSeconds = 10.0f;

        class EnemySpawnPointAuthoringBaker : Baker<EnemySpawnPointAuthoring>
        {
            public override void Bake(EnemySpawnPointAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.WorldSpace);
                AddComponent(entity, new EnemySpawnPoint { 
                    UnitType = authoring.unitType,
                    Position = authoring.transform.position,
                    Rotation = authoring.transform.rotation,
                    IsContinuosSpawn = authoring.isContinuosSpawn,
                    TimeBeforeInitialSpawnInSeconds = authoring.timeBeforeInitialSpawnInSeconds,
                    DelayTimer = authoring.timeBeforeInitialSpawnInSeconds,
                    TimeBetweenSpawnInSeconds = authoring.timeBetweenSpawnInSeconds,
                });
            }
        }
    }

    struct EnemySpawnPoint : IComponentData
    {
        public UnitType UnitType;
        public float3 Position;
        public quaternion Rotation;
        public bool IsContinuosSpawn;
        public float TimeBeforeInitialSpawnInSeconds;
        public float TimeBetweenSpawnInSeconds;
        public float DelayTimer;
        public float CooldownTimer;
    }
}
