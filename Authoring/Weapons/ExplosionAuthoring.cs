using Unity.Entities;
using UnityEngine;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    class ExplosionAuthoring : MonoBehaviour
    {
        [SerializeField] float lifetimeInSeconds = 0.5f;

        class ExplosionAuthoringBaker : Baker<ExplosionAuthoring>
        {
            public override void Bake(ExplosionAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Explosion { LifeTimerInSeconds = authoring.lifetimeInSeconds });
                AddComponent<Destructible>(entity);
            }
        }
    }

    struct Explosion : IComponentData
    {
        public float LifeTimerInSeconds;
    }
}
