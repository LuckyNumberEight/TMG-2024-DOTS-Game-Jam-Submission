using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    enum TargetType
    {
        None = 0,
        Invalid = 1,
        Ground = 2,
        Air = 3,
    };

    class PlayerInputAuthoring : MonoBehaviour
    {
        [Header("Player Lock On Parameters")]
        [SerializeField]
        [Range(0.0f, 5.0f)] float ciwsLockOnRangeInMeters = 1.0f;
        [SerializeField]
        [Range(0.0f, 10.0f)] float missileLockOnRangeInMeters = 4f;
        [SerializeField]
        [Range(0.0f, 2.0f)] float missileLockOnTimeInSeconds = 1.0f;

        class Baker : Baker<PlayerInputAuthoring>
        {

            public override void Bake(PlayerInputAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent<CameraPosition>(entity);
                AddComponent<InputMoveDirection>(entity);
                AddComponent<InputMousePosition>(entity);
                AddComponent<InputMouseClick>(entity);
                AddComponent<InputKeyPress>(entity);
                AddComponent<TargetClosestToCenter>(entity);
                AddBuffer<CurrentTarget>(entity);
                AddComponent(entity, new LockOnConfigData
                {
                    CIWSLockOnRangeInMeters = authoring.ciwsLockOnRangeInMeters,
                    MissileLockOnRangeInMeters = authoring.missileLockOnRangeInMeters,
                    MissileLockOnTimeInSeconds = authoring.missileLockOnTimeInSeconds,
                });
            }
        }
    }

    // Singleton Components on the one Input Entity
    struct CameraPosition : IComponentData
    {
        public float3 Value;
    }

    struct InputMoveDirection : IComponentData
    {
        public float3 Value;
    }

    struct InputMousePosition : IComponentData
    {
        public float3 OriginPosition;
        public float3 EndPosition;
    }

    struct InputMouseClick : IComponentData
    {
        public bool IsLeftClick;
        public bool IsRightClick;
    }

    struct InputKeyPress : IComponentData
    {
        public bool IsSelectCIWS;
        public bool IsSelectCannon;
        public bool IsSelectMissile;
        public bool IsSelectDrone;
        public bool IsDeployChaff;
        public bool IsToggleMenu;
    }

    struct LockOnConfigData : IComponentData
    {
        public float CIWSLockOnRangeInMeters;
        public float MissileLockOnRangeInMeters;
        public float MissileLockOnTimeInSeconds;
    }
    struct CurrentTarget : IBufferElementData
    {
        public Entity TargetEntity;
        public TargetType TargetType;
        public int TargetID;
        public float3 TargetPosition;
        public float LockOnTimer;
        public bool IsLockedOn;
        public float4 PreviousColour;
    }

    struct TargetClosestToCenter : IComponentData
    {
        public Entity Entity;
        public float3 Position;
        public float3 Velocity;
        public float DistanceFromCenter;
        public bool IsTargetUpdated;
    }

    // Components for Individual Units and Weapon Systems
    struct TargetableTag : IComponentData
    {
    }

    struct CIWSPlayerFireCommand : IComponentData
    {
        public bool IsFireCommand;
        public float ROFTimerInSeconds;
        public float3 BowCIWSDirection;
        public float3 SternCIWSDirection;
        public float CooldownTimerInSeconds;
    }

    struct CannonPlayerShootCommand : IComponentData
    {
        public bool IsShootCommand;
        public float CooldownTimerInSeconds;
        public int MagazineCapacityCounter;
        public float3 BarrelPosition;
        public quaternion BarrelRotation;
    }

    struct MissilePlayerLaunchCommand : IComponentData
    {
        public bool IsLaunchCommand;
        public float CooldownTimerInSeconds;
        public int MagazineCapacityCounter;
    }
}
