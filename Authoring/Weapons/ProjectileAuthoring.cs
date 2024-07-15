using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    class ProjectileAuthoring : MonoBehaviour
    {
        class Baker : Baker<ProjectileAuthoring>
        {
            public override void Bake(ProjectileAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Projectile>(entity);
                AddComponent<Destructible>(entity);
            }
        }
    }

    struct Projectile : IComponentData
    {
        public Entity Origin;
        public float3 VelocityMetersPerSecond;
        public float LifetimeCounterInSeconds;
        public ArmourType ArmourPenetrationLevel;
        public float DamageAmount;
    }
}
