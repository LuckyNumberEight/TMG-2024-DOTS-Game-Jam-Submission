using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    class PlayerAuthoring : MonoBehaviour
    {
        [Header("Destroyer Parameters")]

        [SerializeField] GameObject deckGunTurret;
        [SerializeField] GameObject deckGunBarrel;
        [SerializeField] GameObject bowCIWS;
        [SerializeField] GameObject sternCIWS;
        [SerializeField] GameObject missleLauncher;
        [SerializeField] ArmourType armourLevel = ArmourType.Light;
        [SerializeField] float hitPoints = 5000f;
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
        [Range(0f, 10f)] float missileReloadCooldownInSeconds = 20.0f;
        [SerializeField]
        [Range(0f, 10f)] int missileMagazineCapacity = 5;

        class PlayerAuthoringBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                // Tag Components
                AddComponent<PlayerTag>(entity);
                AddComponent<Destructible>(entity);

                // Ship Components

                AddComponent(entity, new Destroyer
                {
                    DeckGunTurret = GetEntity(authoring.deckGunTurret, TransformUsageFlags.Dynamic),
                    DeckGunBarrel = GetEntity(authoring.deckGunBarrel, TransformUsageFlags.Dynamic),
                    BowCIWS = GetEntity(authoring.bowCIWS, TransformUsageFlags.Dynamic),
                    SternCIWS = GetEntity(authoring.sternCIWS, TransformUsageFlags.Dynamic),
                    MissleLauncher = GetEntity(authoring.missleLauncher, TransformUsageFlags.Dynamic),
                });
                AddComponent(entity, new MoveComponent
                {
                    MoveSpeed = authoring.moveSpeedMetersPerSecond,
                    TurnSpeed = math.radians(authoring.turnSpeedDegreesPerSecond)
                });
                AddComponent(entity, new Unit
                {
                    ID = 0,
                    UnitType = UnitType.Destroyer,
                    UnitArmourLevel = authoring.armourLevel,
                    UnitMaxHitPoints = authoring.hitPoints,
                    UnitCurrentHitPoints = authoring.hitPoints,
                });
                //AddComponent(entity, new BoidSurface { FlockID = 0 });
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
                AddComponent<CIWSPlayerFireCommand>(entity);

                // Cannon Components

                AddComponent(entity, new Cannon
                {
                    Turret = GetEntity(authoring.deckGunTurret, TransformUsageFlags.Dynamic),
                    Barrel = GetEntity(authoring.deckGunBarrel, TransformUsageFlags.Dynamic),
                    TurretOffset = authoring.deckGunTurret.transform.position,
                    BarrelOffset = authoring.deckGunBarrel.transform.position + authoring.deckGunTurret.transform.position,
                    TurretRotateSpeedRadiansPerSecond = math.radians(authoring.deckGunRotateSpeedDegreesPerSecond),
                    ReloadCooldownInSeconds = authoring.deckGunReloadCooldownInSeconds,
                    MagazineCapacity = authoring.deckGunMagazineCapacity,
                });
               AddComponent(entity, new CannonPlayerShootCommand
                {
                    IsShootCommand = false,
                    MagazineCapacityCounter = authoring.deckGunMagazineCapacity,
                    CooldownTimerInSeconds = 0f,
                });


                // Missile Components

                AddComponent(entity, new MissileLauncher
                {
                    ReloadCooldownInSeconds = authoring.missileReloadCooldownInSeconds,
                    MagazineCapacity = authoring.missileMagazineCapacity,
                    LauncherOffset = authoring.missleLauncher.transform.position,
                });

                AddComponent(entity, new MissilePlayerLaunchCommand
                {
                    IsLaunchCommand = false,
                    MagazineCapacityCounter = authoring.missileMagazineCapacity,
                    CooldownTimerInSeconds = 0f,
                });
            }
        }
    }

    struct PlayerTag : IComponentData
    {
    }

    struct MoveComponent : IComponentData
    {
        public float3 Direction;
        public float3 Velocity;
        public float MoveSpeed;
        public float TurnSpeed;
    }

    struct TargetPosition : IComponentData
    {
        public float3 Value;
    }
}
