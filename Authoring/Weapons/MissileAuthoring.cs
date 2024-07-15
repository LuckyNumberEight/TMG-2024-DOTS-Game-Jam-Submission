using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    class MissileAuthoring : MonoBehaviour
    {
        class MissileAuthoringBaker : Baker<MissileAuthoring>
        {
            public override void Bake(MissileAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                // Add Tags
                AddComponent<TargetableTag>(entity);
                AddComponent<Destructible>(entity);
                SetComponentEnabled<Destructible>(entity, false);

                // Add Missile Components
                AddComponent<Missile>(entity);
                AddComponent<BoidAir>(entity);
                AddComponent<UpdateBoosterColourTag>(entity);
            }
        }
    }

    struct Missile : IComponentData
    {
        public Entity Origin;
        public Entity Target;
        public float4 Colour;
        public float LifetimeCounter;
    }

    struct UpdateBoosterColourTag : IComponentData, IEnableableComponent 
    { 
    }
}
