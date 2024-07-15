using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    class WeaponConfigAuthoring : MonoBehaviour
    {
        [Header("Projectile Parameters")]
        [SerializeField]
        [Range(0.0f, 20.0f)] float cannonGravityConstant = 9.81f;

        [Header("CIWS Parameters")]
        [SerializeField] GameObject ciwsBulletPrefab;
        [SerializeField] GameObject ciwsBulletPlayerPrefab;
        [SerializeField] ArmourType ciwsArmourPenetrationLevel = ArmourType.Light;
        [SerializeField]
        [Range(0f, 100f)] float ciwsDamageAmount = 1;
        [SerializeField] 
        [Range(0f, 100f)] float ciwsBulletMoveSpeedMetersPerSecond = 35.0f;

        [Header("Cannon Parameters")]
        [SerializeField] GameObject cannonShellPrefab;
        [SerializeField] GameObject cannonShellPlayerPrefab;
        [SerializeField] ArmourType cannonArmourPenetrationLevel = ArmourType.Heavy;
        [SerializeField]
        [Range(0f, 500f)] float cannonDamageAmount = 1500f;
        [SerializeField]
        [Range(0f, 50f)] float cannonShellMoveSpeedMetersPerSecond = 20.0f;

        [Header("Missile Parameters")]
        [SerializeField] GameObject missilePrefab;
        [SerializeField] GameObject missilePlayerPrefab;
        [SerializeField] GameObject missileExplosionPrefab;
        [SerializeField] ArmourType missileArmourPenetrationLevel = ArmourType.Heavy;
        [SerializeField]
        [Range(0f, 500f)] float missileDamageAmount = 1000f;
        [SerializeField]
        [Range(0f, 50f)] float missileMoveSpeedMetersPerSecond = 5.0f;
        [SerializeField]
        [Range(0f, 50f)] float missilePlayerMoveSpeedMetersPerSecond = 10.0f;
        [SerializeField]
        [Range(0f, 1080f)] float missileTurnSpeedDegreesPerSecond = 360.0f;

        class Baker : Baker<WeaponConfigAuthoring>
        {
            public override void Bake(WeaponConfigAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new WeaponConfigData
                {
                    GravityConstant = authoring.cannonGravityConstant,
                    // CIWS Parameters
                    CIWSBulletPrefab = GetEntity(authoring.ciwsBulletPrefab, TransformUsageFlags.Dynamic),
                    CIWSBulletPlayerPrefab = GetEntity(authoring.ciwsBulletPlayerPrefab, TransformUsageFlags.Dynamic),
                    CIWSBulletSpeedMetersPerSecond = authoring.ciwsBulletMoveSpeedMetersPerSecond,
                    CIWSBulletArmourPenetrationLevel = authoring.ciwsArmourPenetrationLevel,
                    CIWSBulletDamageAmount = authoring.ciwsDamageAmount,
                    // Cannon Parameters
                    CannonShellPrefab = GetEntity(authoring.cannonShellPrefab, TransformUsageFlags.Dynamic),
                    CannonShellPlayerPrefab = GetEntity(authoring.cannonShellPlayerPrefab, TransformUsageFlags.Dynamic),
                    CannonShellMoveSpeedMetersPerSecond = authoring.cannonShellMoveSpeedMetersPerSecond,
                    CannonShellArmourPenetrationLevel = authoring.cannonArmourPenetrationLevel,
                    CannonShellDamageAmount = authoring.cannonDamageAmount,
                    // Missile Parameters
                    MissilePrefab = GetEntity(authoring.missilePrefab, TransformUsageFlags.Dynamic),
                    MissilePlayerPrefab = GetEntity(authoring.missilePlayerPrefab, TransformUsageFlags.Dynamic),
                    MissileExplosionPrefab = GetEntity(authoring.missileExplosionPrefab, TransformUsageFlags.Dynamic),
                    MissileMoveSpeedMetersPerSecond = authoring.missileMoveSpeedMetersPerSecond,
                    MissilePlayerMoveSpeedMetersPerSecond = authoring.missilePlayerMoveSpeedMetersPerSecond,
                    MissileTurnSpeedRadiansPerSecond = math.radians(authoring.missileTurnSpeedDegreesPerSecond),
                    MissileArmourPenetrationLevel = authoring.missileArmourPenetrationLevel,
                    MissileDamageAmount = authoring.missileDamageAmount,
                });
            }
        }
    }

    // Singleton Components on the one Weapon Config Entity

    struct WeaponConfigData : IComponentData
    {
        public float GravityConstant;
        // CIWS Parameters
        public Entity CIWSBulletPrefab;
        public Entity CIWSBulletPlayerPrefab;
        public float CIWSBulletSpeedMetersPerSecond;
        public ArmourType CIWSBulletArmourPenetrationLevel;
        public float CIWSBulletDamageAmount;
        // Cannon Parameters
        public Entity CannonShellPrefab;
        public Entity CannonShellPlayerPrefab;
        public float CannonShellMoveSpeedMetersPerSecond;
        public ArmourType CannonShellArmourPenetrationLevel;
        public float CannonShellDamageAmount;
        // Missile Parameters
        public Entity MissilePrefab;
        public Entity MissilePlayerPrefab;
        public Entity MissileExplosionPrefab;
        public float MissileMoveSpeedMetersPerSecond;
        public float MissilePlayerMoveSpeedMetersPerSecond;
        public float MissileTurnSpeedRadiansPerSecond;
        public ArmourType MissileArmourPenetrationLevel;
        public float MissileDamageAmount;
    }

    // Components for Individual Units and Weapon Systems

    struct CIWS : IComponentData
    {
        public Entity BowCIWS;
        public Entity SternCIWS;
        public float3 BowCIWSOffset;
        public float3 SternCIWSOffset;
        public int CIWSRoundsPerSecond;
    }

    struct Cannon : IComponentData
    {
        public Entity Turret;
        public Entity Barrel;
        public float3 TurretOffset;
        public float3 BarrelOffset;
        public float TurretRotateSpeedRadiansPerSecond;
        public float ReloadCooldownInSeconds;
        public int MagazineCapacity;
    }

    struct MissileLauncher : IComponentData
    {
        public float3 LauncherOffset;
        public float ReloadCooldownInSeconds;
        public int MagazineCapacity;
    }
}
