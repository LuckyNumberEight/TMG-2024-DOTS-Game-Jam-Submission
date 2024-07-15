using Unity.Entities;
using UnityEngine;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    class PlayerMissileAuthoring : MonoBehaviour
    {
        class Baker : Baker<PlayerMissileAuthoring>
        {
            public override void Bake(PlayerMissileAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                // Add Tags
                AddComponent<Destructible>(entity);
                SetComponentEnabled<Destructible>(entity, false);

                // Add Missile Components
                AddComponent<Missile>(entity);
                AddComponent<BoidAir>(entity);
                AddComponent<UpdateBoosterColourTag>(entity);
            }
        }
    }
}
