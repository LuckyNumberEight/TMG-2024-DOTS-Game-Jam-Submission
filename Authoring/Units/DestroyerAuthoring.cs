using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    class DestroyerAuthoring : MonoBehaviour
    {
        [Header("Destroyer Parameters")]

        [SerializeField] GameObject deckGunTurret;
        [SerializeField] GameObject deckGunBarrel;
        [SerializeField] GameObject bowCIWS;
        [SerializeField] GameObject sternCIWS;
        [SerializeField] GameObject missleLauncher;
        [SerializeField] ArmourType armourLevel = ArmourType.Light;
        [SerializeField] float hitPoints = 1000f;
        [SerializeField]
        [Range(0f, 10f)] float moveSpeedMetersPerSecond = 3.0f;
        [SerializeField]
        [Range(0f, 360f)] float turnSpeedDegreesPerSecond = 60.0f;

        [Header("CIWS Parameters")]
        [SerializeField]
        [Range(0f, 5)] int ciwsRoundsPerSeond = 3;

        [Header("Cannon Parameters")]
        [SerializeField]
        [Range(0f, 360f)] float deckGunRotateSpeedDegreesPerSecond = 180.0f;
        [SerializeField]
        [Range(0f, 10f)] float deckGunReloadCooldownInSeconds = 20.0f;
        [SerializeField]
        [Range(0f, 10f)] int deckGunMagazineCapacity = 5;

        [Header("Missile Parameters")]
        [SerializeField]
        [Range(0f, 10f)] float missileReloadCooldownInSeconds = 10.0f;
        [SerializeField]
        [Range(0f, 10f)] int missileMagazineCapacity = 40;

        class DestroyerAuthoringBaker : Baker<DestroyerAuthoring>
        {
            public override void Bake(DestroyerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                // Tag Components
                AddComponent<Destructible>(entity);
                AddComponent<TargetableTag>(entity);

                // Ship Components
                AddComponent(entity, new Destroyer
                {
                    DeckGunTurret = GetEntity(authoring.deckGunTurret, TransformUsageFlags.Dynamic),
                    DeckGunBarrel = GetEntity(authoring.deckGunBarrel, TransformUsageFlags.Dynamic),
                    BowCIWS = GetEntity(authoring.bowCIWS, TransformUsageFlags.Dynamic),
                    SternCIWS = GetEntity(authoring.sternCIWS, TransformUsageFlags.Dynamic),
                    MissleLauncher = GetEntity(authoring.missleLauncher, TransformUsageFlags.Dynamic),
                });
                AddComponent(entity, new Unit { 
                    UnitType = UnitType.Destroyer,
                    UnitArmourLevel = authoring.armourLevel,
                    UnitMaxHitPoints = authoring.hitPoints,
                    UnitCurrentHitPoints = authoring.hitPoints,
                });
                AddComponent(entity, new BoidSurface
                {
                    MoveSpeed = authoring.moveSpeedMetersPerSecond,
                    TurnSpeed = math.radians(authoring.turnSpeedDegreesPerSecond),
                    FlockID = 2
                });
                AddComponent<TargetPosition>(entity);

                // CIWS Components
                AddComponent(entity, new CIWS
                {
                    BowCIWS = GetEntity(authoring.bowCIWS, TransformUsageFlags.Dynamic),
                    SternCIWS = GetEntity(authoring.sternCIWS, TransformUsageFlags.Dynamic),
                    CIWSRoundsPerSecond = authoring.ciwsRoundsPerSeond,
                    BowCIWSOffset = authoring.bowCIWS.transform.position,
                    SternCIWSOffset = authoring.sternCIWS.transform.position,
                });
                AddComponent<CIWSAIFireCommand>(entity);

                // Cannon Components
                AddComponent(entity, new Cannon
                {
                    Turret = GetEntity(authoring.deckGunTurret, TransformUsageFlags.Dynamic),
                    Barrel = GetEntity(authoring.deckGunBarrel, TransformUsageFlags.Dynamic),
                    TurretRotateSpeedRadiansPerSecond = math.radians(authoring.deckGunRotateSpeedDegreesPerSecond),
                    ReloadCooldownInSeconds = authoring.deckGunReloadCooldownInSeconds,
                    MagazineCapacity = authoring.deckGunMagazineCapacity,
                });
                AddComponent<CannonAIShootCommand>(entity);

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

    struct Destroyer : IComponentData
    {
        public Entity DeckGunTurret;
        public Entity DeckGunBarrel;
        public Entity BowCIWS;
        public Entity SternCIWS;
        public Entity MissleLauncher;
    }
}
