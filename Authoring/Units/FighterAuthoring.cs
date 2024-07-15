using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    class FighterAuthoring : MonoBehaviour
    {
        [Header("Aircraft Parameters")]
        [SerializeField] GameObject missleLauncher;
        [SerializeField] ArmourType armourLevel = ArmourType.None;
        [SerializeField] float hitPoints = 100f;
        [SerializeField]
        [Range(0f, 8f)] float moveSpeedMetersPerSecond = 8.0f;
        [SerializeField]
        [Range(0f, 360f)] float turnSpeedDegreesPerSecond = 90.0f;


        [Header("Missile Parameters")]
        [SerializeField]
        [Range(0f, 10f)] float missileReloadCooldownInSeconds = 5.0f;
        [SerializeField]
        [Range(0f, 10f)] int missileMagazineCapacity = 6;

        class Baker : Baker<FighterAuthoring>
        {
            public override void Bake(FighterAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                // Tag Components
                AddComponent<Destructible>(entity);
                AddComponent<TargetableTag>(entity);

                // AircraftComponents
                AddComponent(entity, new Unit
                {
                    UnitType = UnitType.Fighter,
                    UnitArmourLevel = authoring.armourLevel,
                    UnitMaxHitPoints = authoring.hitPoints,
                    UnitCurrentHitPoints = authoring.hitPoints,
                });
                AddComponent(entity, new BoidAir
                {
                    MoveSpeed = authoring.moveSpeedMetersPerSecond,
                    TurnSpeed = math.radians(authoring.turnSpeedDegreesPerSecond),
                    FlockID = 3
                });

                // Missile Components
                AddComponent(entity, new MissileLauncher
                {
                    ReloadCooldownInSeconds = authoring.missileReloadCooldownInSeconds,
                    MagazineCapacity = authoring.missileMagazineCapacity,
                    LauncherOffset = authoring.missleLauncher.transform.position,
                });
                AddComponent<MissileAILaunchCommand>(entity);
            }
        }
    }

    struct Fighter : IComponentData
    {
        public Entity MissileLauncher;
    }
}
