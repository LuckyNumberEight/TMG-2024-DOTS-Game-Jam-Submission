using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    class PracticeTargetAuthoring : MonoBehaviour
    {
        [SerializeField] UnitType unitType;
        class Baker : Baker<PracticeTargetAuthoring>
        {
            public override void Bake(PracticeTargetAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Unit
                {
                    UnitType = authoring.unitType,
                    Colour = new float4(3f, 0.2f, 0.2f, 1f),
                    UnitArmourLevel = ArmourType.None,
                    UnitMaxHitPoints = 1.0f,
                    UnitCurrentHitPoints = 1.0f
                });
                AddComponent<Destructible>(entity);
                AddComponent<TargetableTag>(entity);
                AddComponent<PracticeTargetTag>(entity);
            }
        }
    }

    struct PracticeTargetTag : IComponentData, IEnableableComponent
    {
    }
}
