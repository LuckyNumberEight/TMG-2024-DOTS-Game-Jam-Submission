using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    class AIConfigAuthoring : MonoBehaviour
    {
        [Header("AI CIWS Fire Parameters")]
        [SerializeField]
        [Range(0.0f, 30.0f)] float ciwsMaxEngagementDistance = 15.0f;
        [SerializeField]
        [Range(0.0f, 5.0f)] float ciwsMaxFiringPeriodInSeconds = 2.0f;
        [SerializeField]
        [Range(0.0f, 30.0f)] float ciwsCooldownPeriodInSeconds = 10.0f;

        [Header("AI Missile Launch Parameters")]
        [SerializeField]
        [Range(0.0f, 50.0f)] float missileMaxLaunchDistanceInMeters = 30.0f;
        [SerializeField]
        [Range(0.0f, 30.0f)] float missileTimeBetweenVolleysInSeconds = 10.0f;
        [SerializeField]
        [Range(0.0f, 1.0f)] float missileTimeBetweenLaunchesInSeconds = 1.0f;
        [SerializeField]
        [Range(0, 50)] int missilesNumberPerVolley = 2;
        class Baker : Baker<AIConfigAuthoring>
        {
            public override void Bake(AIConfigAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new AIConfigData
                {
                    // AI CIWS Fire Parameters
                    CIWSMaxEngagementDistanceInMeters = authoring.ciwsMaxEngagementDistance,
                    CIWSMaxFiringPeriodInSeconds = authoring.ciwsMaxFiringPeriodInSeconds,
                    CIWSCooldownPeriodInSeconds = authoring.ciwsCooldownPeriodInSeconds,

                    // AI Missile Launch Parameters
                    MissileTimeBetweenVolleysInSeconds = authoring.missileTimeBetweenVolleysInSeconds,
                    MissileTimeBetweenLaunchesInSeconds = authoring.missileTimeBetweenLaunchesInSeconds,
                    MissilesNumberPerVolley = authoring.missilesNumberPerVolley,
                    MissileMaxLaunchDistanceInMeters = authoring.missileMaxLaunchDistanceInMeters,
                });
            }
        }
    }

    // Singleton Components on the one AIConfig Entity
    struct AIConfigData : IComponentData
    {
        // AI CIWS Fire Parameters
        public float CIWSMaxEngagementDistanceInMeters;
        public float CIWSMaxFiringPeriodInSeconds;
        public float CIWSCooldownPeriodInSeconds;

        // AI Missile Launch Parameters
        public float MissileMaxLaunchDistanceInMeters;
        public float MissileTimeBetweenVolleysInSeconds;
        public float MissileTimeBetweenLaunchesInSeconds;
        public int MissilesNumberPerVolley;
    }

    // Components for Individual Units and Weapon Systems
    struct CIWSAIFireCommand : IComponentData, IEnableableComponent
    {
        public bool IsFireCommand;
        public float3 TargetPosition;
        public float DistanceToTarget;
        public float ROFTimerInSeconds;
        public float FireTimerInSeconds;
        public float CooldownTimerInSeconds;
    }

    struct CannonAIShootCommand : IComponentData, IEnableableComponent
    {
    }

    struct MissileAILaunchCommand : IComponentData, IEnableableComponent
    {
        public bool IsLaunchCommand;
        public int MissileVolleyCounter;
        public float VolleyTimerInSeconds;
        public float WaveTimerInSeconds;
    }
}
