using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    enum UnitType
    {
        None = 0,
        Carrier = 10,
        Battleship = 30,
        Destroyer = 50,
        Fighter = 100,
        ShoreBattery = 200,
        PracticeTarget = 201,
        USV = 300,
        UAV = 400,
        Missile = 500,
    }

    enum ArmourType
    {
        None = 1,
        Light = 10,
        Heavy = 100,
    }

    class UnitConfigAuthoring : MonoBehaviour
    {
        [Header("Unit Colours")]
        [SerializeField] Color playerColour = Color.blue;
        [SerializeField] Color enemyColour = Color.red;

        [Header("Ship Prefabs")]
        [SerializeField] GameObject playerPrefab;
        [SerializeField] GameObject practiceTargetPrefab;
        [SerializeField] GameObject fighterPrefab;
        [SerializeField] GameObject destroyerPrefab;

        class Baker : Baker<UnitConfigAuthoring>
        {
            public override void Bake(UnitConfigAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new UnitConfigData
                {
                    // Unit Colours
                    PlayerColour = new float4(authoring.playerColour.r, authoring.playerColour.g, authoring.playerColour.b, authoring.playerColour.a),
                    EnemyColour = new float4(authoring.enemyColour.r, authoring.enemyColour.g, authoring.enemyColour.b, authoring.enemyColour.a),

                    // Ship Prefabs
                    PlayerPefab = GetEntity(authoring.playerPrefab, TransformUsageFlags.Dynamic),
                    PracticeTargetPrefab = GetEntity(authoring.practiceTargetPrefab, TransformUsageFlags.Dynamic),
                    FighterPrefab = GetEntity(authoring.fighterPrefab, TransformUsageFlags.Dynamic),
                    DestroyerPrefab = GetEntity(authoring.destroyerPrefab, TransformUsageFlags.Dynamic),

                    // Ship ID's
                    BossIDCounter = 10,
                    EnemyIDCounter = 100,
                });
            }
        }
    }

    // Singleton Component to be used on the one Ship Config Data Entity
    struct UnitConfigData : IComponentData
    {
        // Unit Colours
        public float4 PlayerColour;
        public float4 EnemyColour;

        // Target Prefabs
        public Entity PlayerPefab; 
        public Entity PracticeTargetPrefab;
        public Entity FighterPrefab;
        public Entity DestroyerPrefab;

        // Ship ID's
        public int BossIDCounter;
        public int EnemyIDCounter;
    }

    // Components for Individual Units and Weapon Systems
    struct Unit : IComponentData, IEnableableComponent
    {
        public int ID;
        public UnitType UnitType;
        public float4 Colour;
        public ArmourType UnitArmourLevel;
        public float UnitMaxHitPoints;
        public float UnitCurrentHitPoints;
        public bool IsHealthUpdated;
    }

    struct Destructible : IComponentData, IEnableableComponent
    {
        public bool IsDestroyed;
        public bool IsSpawnExplosion;
        public float3 ExplosionPosition;
    }
}
