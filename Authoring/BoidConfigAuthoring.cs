using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    class BoidConfigAuthoring : MonoBehaviour
    {
        [Header("Air Tracking Parameters")]
        [SerializeField] int airCellSize = 100;

        [Header("Missile Tracking Parameters")]
        [SerializeField] float missileStep = 1.25f;
        [SerializeField] float missilePerceptionRadiusInMeters = 90;
        [SerializeField]
        [Range(0.0f, 1.0f)] float missileCohesionBias = 0.2f;
        [SerializeField]
        [Range(0.0f, 1.0f)] float missileSeparationBias = 0.15f;
        [SerializeField]
        [Range(0.0f, 1.0f)] float missileAlignmentBias = 0.25f;
        [SerializeField]
        [Range(0.0f, 1.0f)] float missileTargetBias = 0.8f;

        [Header("Aircraft Tracking Parameters")]
        [SerializeField] float aircraftStep = 1.25f;
        [SerializeField] float aircraftPerceptionRadiusInMeters = 90;
        [SerializeField]
        [Range(0.0f, 1.0f)] float aircraftCohesionBias = 0.5f;
        [SerializeField]
        [Range(0.0f, 1.0f)] float aircraftSeparationBias = 0.15f;
        [SerializeField]
        [Range(0.0f, 1.0f)] float aircraftAlignmentBias = 0.5f;
        [SerializeField]
        [Range(0.0f, 1.0f)] float aircraftTargetBias = 0.15f;

        [Header("Ship Tracking Parameters")]
        [SerializeField] int shipCellSize = 100;
        [SerializeField] float shipStep = 1.25f;
        [SerializeField] float shipPerceptionRadiusInMeters = 90;
        [SerializeField]
        [Range(0.0f, 1.0f)] float shipCohesionBias = 0.5f;
        [SerializeField]
        [Range(0.0f, 1.0f)] float shipSeparationBias = 0.3f;
        [SerializeField]
        [Range(0.0f, 1.0f)] float shipAlignmentBias = 0.7f;
        [SerializeField]
        [Range(0.0f, 1.0f)] float shipTargetBias = 0.15f;
        [SerializeField]
        [Range(0.0f, 1.0f)] float shipAvoidanceBias = 0.5f;
        [SerializeField] int shipRayCount = 15;
        class Baker : Baker<BoidConfigAuthoring>
        {
            public override void Bake(BoidConfigAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new BoidConfigData
                {
                    // Air Tracking Paramters
                    AirCellSize = authoring.airCellSize,

                    // Missile Tracking Parameters
                    MissileStep = authoring.missileStep,
                    MissilePerceptionRadiusInMeters = authoring.missilePerceptionRadiusInMeters,
                    MissileCohesionBias = authoring.missileCohesionBias,
                    MissileSeparationBias = authoring.missileSeparationBias * 10f,
                    MissileAlignmentBias = authoring.missileAlignmentBias,
                    MissileTargetBias = authoring.missileTargetBias,

                    // Aircraft Tracking Parameters
                    AircraftStep = authoring.aircraftStep,
                    AircraftPerceptionRadiusInMeters = authoring.aircraftPerceptionRadiusInMeters,
                    AircraftCohesionBias = authoring.aircraftCohesionBias,
                    AircraftSeparationBias = authoring.aircraftSeparationBias * 10f,
                    AircraftAlignmentBias = authoring.aircraftAlignmentBias,
                    AircraftTargetBias = authoring.aircraftTargetBias,

                    // Ship Tracking Parameters
                    ShipCellSize = authoring.shipCellSize,
                    ShipStep = authoring.shipStep,
                    ShipPerceptionRadiusInMeters = authoring.shipPerceptionRadiusInMeters,
                    ShipCohesionBias = authoring.shipCohesionBias / 10f,
                    ShipSeparationBias = authoring.shipSeparationBias,
                    ShipAlignmentBias = authoring.shipAlignmentBias / 10f,
                    ShipTargetBias = authoring.shipTargetBias / 10f,
                    ShipAvoidanceBias = authoring.shipAvoidanceBias * 10f,
                    ShipRayCount = authoring.shipRayCount,
                });
            }
        }
    }

    struct BoidConfigData :IComponentData
    {
        // Air Tracking Parameters
        public int AirCellSize;
        // Missile Tracking Parameters
        public float MissileStep;
        public float MissilePerceptionRadiusInMeters;
        public float MissileCohesionBias;
        public float MissileSeparationBias;
        public float MissileAlignmentBias;
        public float MissileTargetBias;

        // Aircraft Tracking Parameters
        public float AircraftStep;
        public float AircraftPerceptionRadiusInMeters;
        public float AircraftCohesionBias;
        public float AircraftSeparationBias;
        public float AircraftAlignmentBias;
        public float AircraftTargetBias;

        //  Ship Tracking Parameters
        public int ShipCellSize;
        public float ShipStep;
        public float ShipPerceptionRadiusInMeters;
        public float ShipCohesionBias;
        public float ShipSeparationBias;
        public float ShipAlignmentBias;
        public float ShipTargetBias;
        public float ShipAvoidanceBias;
        public int ShipRayCount;
    }

    struct BoidAir : IComponentData
    {
        public float3 TargetPosition;
        public float3 Position;
        public float3 Velocity;
        public float3 Acceleration;
        public int FlockID;
        public bool IsMissile;
        public float MoveSpeed;
        public float TurnSpeed;
    }
    struct BoidSurface : IComponentData
    {
        public float3 TargetPosition;
        public float3 Position;
        public float3 Velocity;
        public float3 Acceleration;
        public int FlockID;
        public float MoveSpeed;
        public float TurnSpeed;
    }
}
