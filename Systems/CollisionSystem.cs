using Unity.Burst;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Physics;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    partial struct CollisionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<WeaponConfigData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var CannonShellTriggerJobHandle = new CollisionTriggerJob
            {
                PlayerEntity = SystemAPI.GetSingletonEntity<PlayerTag>(),
                WeaponConfig = SystemAPI.GetSingleton<WeaponConfigData>(),
                AllUnits = SystemAPI.GetComponentLookup<Unit>(),
                AllProjectiles = SystemAPI.GetComponentLookup<Projectile>(),
                AllMissiles = SystemAPI.GetComponentLookup<Missile>(),
                AllBoidsAir = SystemAPI.GetComponentLookup<BoidAir>(),
                AllBoidsSurface = SystemAPI.GetComponentLookup<BoidSurface>(),
                AllDestructibles = SystemAPI.GetComponentLookup<Destructible>(),
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

            CannonShellTriggerJobHandle.Complete();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        [BurstCompile]
        partial struct CollisionTriggerJob : ITriggerEventsJob
        {
            public Entity PlayerEntity;
            public WeaponConfigData WeaponConfig;
            public ComponentLookup<Unit> AllUnits;
            public ComponentLookup<Projectile> AllProjectiles;
            public ComponentLookup<Missile> AllMissiles;
            public ComponentLookup<BoidAir> AllBoidsAir;
            public ComponentLookup<BoidSurface> AllBoidsSurface;
            public ComponentLookup<Destructible> AllDestructibles;

            public void Execute(TriggerEvent triggerEvent)
            {
                // Collision Check Between Ships

                if (AllUnits.HasComponent(triggerEvent.EntityA) && AllUnits.HasComponent(triggerEvent.EntityB)) // Ignore Collisions between ships
                    return;

                // Collision Check Between Projectiles and Ships

                else if (AllProjectiles.HasComponent(triggerEvent.EntityA) && AllUnits.HasComponent(triggerEvent.EntityB))
                {
                    if (AllProjectiles[triggerEvent.EntityA].Origin.Equals(triggerEvent.EntityB)) // Ignore Collision with self
                        return;

                    var projectile = AllProjectiles[triggerEvent.EntityA];
                    var unit = AllUnits[triggerEvent.EntityB];
                    unit.UnitCurrentHitPoints -= math.clamp((float)projectile.ArmourPenetrationLevel / (float)unit.UnitArmourLevel, 0f, 1f) * projectile.DamageAmount;
                    AllUnits[triggerEvent.EntityB] = unit;

                    var destructibleA = AllDestructibles[triggerEvent.EntityA];
                    destructibleA.IsDestroyed = true;

                    if (projectile.ArmourPenetrationLevel == ArmourType.Heavy)
                    {
                        if (AllBoidsSurface.HasComponent(triggerEvent.EntityB))
                        {
                            destructibleA.IsSpawnExplosion = true;
                            destructibleA.ExplosionPosition = AllBoidsSurface[triggerEvent.EntityB].Position;
                        }
                        else if (AllBoidsAir.HasComponent(triggerEvent.EntityB))
                        {
                            destructibleA.IsSpawnExplosion = true;
                            destructibleA.ExplosionPosition = AllBoidsSurface[triggerEvent.EntityB].Position;
                        }
                    }

                    AllDestructibles[triggerEvent.EntityA] = destructibleA;

                    if (triggerEvent.EntityB.Equals(PlayerEntity))
                    {
                        unit.IsHealthUpdated = true;
                        AllUnits[triggerEvent.EntityB] = unit;
                        return;
                    }

                    if (unit.UnitCurrentHitPoints <= 0f && AllDestructibles.HasComponent(triggerEvent.EntityB))
                    {
                        var destructibleB = AllDestructibles[triggerEvent.EntityB];
                        destructibleB.IsDestroyed = true;

                        if (AllBoidsSurface.HasComponent(triggerEvent.EntityB))
                        {
                            destructibleB.IsSpawnExplosion = true;
                            destructibleB.ExplosionPosition = AllBoidsSurface[triggerEvent.EntityB].Position;
                        }
                        else if (AllBoidsAir.HasComponent(triggerEvent.EntityB))
                        {
                            destructibleB.IsSpawnExplosion = true;
                            destructibleB.ExplosionPosition = AllBoidsSurface[triggerEvent.EntityB].Position;
                        }

                        AllDestructibles[triggerEvent.EntityB] = destructibleB;
                    }
                }

                else if (AllProjectiles.HasComponent(triggerEvent.EntityB) && AllUnits.HasComponent(triggerEvent.EntityA))
                {
                    if (AllProjectiles[triggerEvent.EntityB].Origin.Equals(triggerEvent.EntityA)) // Ignore Collision with self
                        return;

                    var projectile = AllProjectiles[triggerEvent.EntityB];
                    var unit = AllUnits[triggerEvent.EntityA];
                    unit.UnitCurrentHitPoints -= math.clamp((float)projectile.ArmourPenetrationLevel / (float)unit.UnitArmourLevel, 0f, 1f) * projectile.DamageAmount;
                    AllUnits[triggerEvent.EntityA] = unit;

                    var destructibleB = AllDestructibles[triggerEvent.EntityB];
                    destructibleB.IsDestroyed = true;

                    if (projectile.ArmourPenetrationLevel == ArmourType.Heavy)
                    {
                        if (AllBoidsSurface.HasComponent(triggerEvent.EntityA))
                        {
                            destructibleB.IsSpawnExplosion = true;
                            destructibleB.ExplosionPosition = AllBoidsSurface[triggerEvent.EntityA].Position;
                        }

                        else if (AllBoidsAir.HasComponent(triggerEvent.EntityA))
                        {
                            destructibleB.IsSpawnExplosion = true;
                            destructibleB.ExplosionPosition = AllBoidsAir[triggerEvent.EntityA].Position;
                        }
                    }

                    AllDestructibles[triggerEvent.EntityB] = destructibleB;


                    if (triggerEvent.EntityA.Equals(PlayerEntity))
                    {
                        unit.IsHealthUpdated = true;
                        AllUnits[triggerEvent.EntityA] = unit;
                        return;
                    }

                    if (unit.UnitCurrentHitPoints <= 0f && AllDestructibles.HasComponent(triggerEvent.EntityA))
                    {
                        var destructibleA = AllDestructibles[triggerEvent.EntityA];
                        destructibleA.IsDestroyed = true;

                        if (AllBoidsSurface.HasComponent(triggerEvent.EntityA))
                        {
                            destructibleA.IsSpawnExplosion = true;
                            destructibleA.ExplosionPosition = AllBoidsSurface[triggerEvent.EntityA].Position;
                        }

                        else if (AllBoidsAir.HasComponent(triggerEvent.EntityA))
                        {
                            destructibleA.IsSpawnExplosion = true;
                            destructibleA.ExplosionPosition = AllBoidsAir[triggerEvent.EntityA].Position;
                        }

                        AllDestructibles[triggerEvent.EntityA] = destructibleA;
                    }
                }

                // Collision Check Between Missiles and Ships

                else if (AllMissiles.HasComponent(triggerEvent.EntityA) && AllMissiles.HasComponent(triggerEvent.EntityB))
                {
                    var destructibleA = AllDestructibles[triggerEvent.EntityA];
                    destructibleA.IsDestroyed = true;
                    destructibleA.IsSpawnExplosion = true;
                    destructibleA.ExplosionPosition = AllBoidsAir[triggerEvent.EntityA].Position;
                    AllDestructibles[triggerEvent.EntityA] = destructibleA;

                    var destructibleB = AllDestructibles[triggerEvent.EntityB];
                    destructibleB.IsDestroyed = true;
                    AllDestructibles[triggerEvent.EntityB] = destructibleB;
                }

                else if (AllMissiles.HasComponent(triggerEvent.EntityA) && AllUnits.HasComponent(triggerEvent.EntityB))
                {
                    if (AllMissiles[triggerEvent.EntityA].Origin.Equals(triggerEvent.EntityB)) // Ignore Collision with self
                        return;

                    var missile = AllMissiles[triggerEvent.EntityA];
                    var unit = AllUnits[triggerEvent.EntityB];
                    unit.UnitCurrentHitPoints -= math.clamp((float) WeaponConfig.MissileArmourPenetrationLevel / (float) unit.UnitArmourLevel, 0f, 1f) * WeaponConfig.MissileDamageAmount;
                    AllUnits[triggerEvent.EntityB] = unit;

                    var destructibleA = AllDestructibles[triggerEvent.EntityA];
                    destructibleA.IsDestroyed = true;
                    destructibleA.IsSpawnExplosion = true;
                    destructibleA.ExplosionPosition = AllBoidsAir[triggerEvent.EntityA].Position;
                    AllDestructibles[triggerEvent.EntityA] = destructibleA;

                    if (triggerEvent.EntityB.Equals(PlayerEntity))
                    {
                        unit.IsHealthUpdated = true;
                        AllUnits[triggerEvent.EntityB] = unit;
                        return;
                    }

                    if (unit.UnitCurrentHitPoints <= 0f && AllDestructibles.HasComponent(triggerEvent.EntityB))
                    {
                        var destructibleB = AllDestructibles[triggerEvent.EntityB];
                        destructibleB.IsDestroyed = true;
                        destructibleB.IsSpawnExplosion = true;
                        destructibleB.ExplosionPosition = AllBoidsAir[triggerEvent.EntityA].Position;
                        AllDestructibles[triggerEvent.EntityB] = destructibleB;
                    }
                }

                else if (AllMissiles.HasComponent(triggerEvent.EntityB) && AllUnits.HasComponent(triggerEvent.EntityA))
                {
                    if (AllMissiles[triggerEvent.EntityB].Origin.Equals(triggerEvent.EntityA)) // Ignore Collision with self
                        return;

                    var missile = AllMissiles[triggerEvent.EntityB];
                    var unit = AllUnits[triggerEvent.EntityA];
                    unit.UnitCurrentHitPoints -= math.clamp((float)WeaponConfig.MissileArmourPenetrationLevel / (float)unit.UnitArmourLevel, 0f, 1f) * WeaponConfig.MissileDamageAmount;
                    AllUnits[triggerEvent.EntityA] = unit;

                    var destructibleB = AllDestructibles[triggerEvent.EntityB];
                    destructibleB.IsDestroyed = true;
                    destructibleB.IsSpawnExplosion = true;
                    destructibleB.ExplosionPosition = AllBoidsAir[triggerEvent.EntityB].Position;
                    AllDestructibles[triggerEvent.EntityB] = destructibleB;

                    if (triggerEvent.EntityA.Equals(PlayerEntity))
                    {
                        unit.IsHealthUpdated = true;
                        AllUnits[triggerEvent.EntityA] = unit;
                        return;
                    }

                    if (unit.UnitCurrentHitPoints <= 0f && AllDestructibles.HasComponent(triggerEvent.EntityA))
                    {
                        var destructibleA = AllDestructibles[triggerEvent.EntityA];
                        destructibleA.IsDestroyed = true;
                        destructibleA.IsSpawnExplosion = true;
                        destructibleA.ExplosionPosition = AllBoidsAir[triggerEvent.EntityB].Position;
                        AllDestructibles[triggerEvent.EntityA] = destructibleA;
                    }
                }

                // Collision Check Between Projectiles and Missiles

                else if (AllProjectiles.HasComponent(triggerEvent.EntityA) && AllMissiles.HasComponent(triggerEvent.EntityB))
                {
                    var destructibleA = AllDestructibles[triggerEvent.EntityA];
                    destructibleA.IsDestroyed = true;
                    AllDestructibles[triggerEvent.EntityA] = destructibleA;

                    var destructibleB = AllDestructibles[triggerEvent.EntityB];
                    destructibleB.IsDestroyed = true;
                    destructibleB.IsSpawnExplosion = true;
                    destructibleB.ExplosionPosition = AllBoidsAir[triggerEvent.EntityB].Position;
                    AllDestructibles[triggerEvent.EntityB] = destructibleB;
                }

                else if (AllProjectiles.HasComponent(triggerEvent.EntityB) && AllMissiles.HasComponent(triggerEvent.EntityA))
                {
                    var destructibleA = AllDestructibles[triggerEvent.EntityA];
                    destructibleA.IsDestroyed = true;
                    destructibleA.IsSpawnExplosion = true;
                    destructibleA.ExplosionPosition = AllBoidsAir[triggerEvent.EntityA].Position;
                    AllDestructibles[triggerEvent.EntityA] = destructibleA;

                    var destructibleB = AllDestructibles[triggerEvent.EntityB];
                    destructibleB.IsDestroyed = true;
                    AllDestructibles[triggerEvent.EntityB] = destructibleB;
                }
            }
        }
    }
}