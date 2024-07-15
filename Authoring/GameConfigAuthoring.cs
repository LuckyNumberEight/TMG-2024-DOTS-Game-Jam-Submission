using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    class GameConfigAuthoring : MonoBehaviour
    {
        [Header("Level Setup Parameters")]
        [SerializeField] GameObject gridPrefab;
        [SerializeField] float2 gridStartPosition = new float2(-150, -100);
        [SerializeField] float2 gridSize = new float2(30f, 50f);

        [Header("Audio Setup Parameters")]
        [SerializeField] float ciwsFiringSoundFXCooldownInSeconds = 0.1f;
        [SerializeField] float cannonShootSoundFXCooldownInSeconds = 0.2f;
        [SerializeField] float missileLaunchSoundFXCooldownInSeconds = 0.5f;
        [SerializeField] float explosionSoundFXCooldownInSeconds = 0.5f;
        class Baker : Baker<GameConfigAuthoring>
        {
            public override void Bake(GameConfigAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new GameConfigData
                {
                    // Level Setup Parameters
                    GridPrefab = GetEntity(authoring.gridPrefab, TransformUsageFlags.WorldSpace),
                    GridStartPosition = authoring.gridStartPosition,
                    GridSize = authoring.gridSize,

                    // Audio Setup Parameters
                    CIWSFiringSoundFXCooldownInSeconds = authoring.ciwsFiringSoundFXCooldownInSeconds,
                    CannonShootSoundFXCooldownInSeconds = authoring.cannonShootSoundFXCooldownInSeconds,
                    MissileLaunchSoundFXCooldownInSeconds = authoring.missileLaunchSoundFXCooldownInSeconds,
                    ExplosionSoundFXCooldownInSeconds = authoring.missileLaunchSoundFXCooldownInSeconds,
                });
                AddComponent<AudioConfigData>(entity);
            }
        }
    }

    struct GameConfigData : IComponentData
    {
        // Game State Parameters
        public int Score;
        public bool IsScoreUpdated;
        public bool IsGameOver;

        // Level Setup Parameters
        public Entity GridPrefab;
        public float2 GridStartPosition;
        public float2 GridSize;

        // Audio Setup Parameters
        public float CIWSFiringSoundFXCooldownInSeconds;
        public float CannonShootSoundFXCooldownInSeconds;
        public float MissileLaunchSoundFXCooldownInSeconds;
        public float ExplosionSoundFXCooldownInSeconds;

        // Audio Cooldown Timers
        public float CIWSFiringSoundFXTimerInSeconds;
        public float CannonShootSoundFXTimerInSeconds;
        public float MissileLaunchSoundFXTimerInSeconds;
        public float ExplosionSoundFXTimerInSeconds;
    }

    struct AudioConfigData : IComponentData
    {
        public bool IsPlayingCIWSFiringSoundFX;
        public bool IsPlayingCannonShootSoundFX;
        public bool IsPlayingMissileLaunchSoundFX;
        public float ExplosionHealthCounter;
    }
}
