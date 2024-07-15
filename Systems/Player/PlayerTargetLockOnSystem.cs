using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    partial struct PlayerTargetLockOnSystem : ISystem
    {
        TargetType GetTargetType(UnitType unitType)
        {
            switch (unitType)
            {
                case UnitType.Carrier:
                case UnitType.Battleship:
                case UnitType.Destroyer:
                case UnitType.ShoreBattery:
                case UnitType.USV:
                    return TargetType.Ground;

                case UnitType.UAV:
                case UnitType.Fighter:
                case UnitType.Missile:
                    return TargetType.Air;

                default:
                    return TargetType.None;
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<InputMousePosition>();
            state.RequireForUpdate<CameraPosition>();
            state.RequireForUpdate<WeaponConfigData>();
            state.RequireForUpdate<LockOnConfigData>();
            state.RequireForUpdate<TargetClosestToCenter>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var lockOnConfigData = SystemAPI.GetSingleton<LockOnConfigData>();
            var weaponConfigData = SystemAPI.GetSingleton<WeaponConfigData>();
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            var mousePosition = SystemAPI.GetSingleton<InputMousePosition>();
            NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);
            var direction = mousePosition.OriginPosition - mousePosition.EndPosition;
            var distance = math.distance(mousePosition.EndPosition, mousePosition.OriginPosition);
            var collisionFilter = new CollisionFilter
            {
                BelongsTo = 1 << 6,
                CollidesWith = 1 << 9
            };
            TargetClosestToCenter targetClosestToCenter = new TargetClosestToCenter
            {
                Entity = Entity.Null,
                Position = float3.zero,
                Velocity = float3.zero,
                DistanceFromCenter = float.MaxValue,
                IsTargetUpdated = false
            };

            collisionWorld.SphereCastAll(mousePosition.EndPosition - 2.0f * direction, lockOnConfigData.CIWSLockOnRangeInMeters, direction, 10f,
                ref hits, collisionFilter, QueryInteraction.Default);

            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    var distanceFromCenter = math.length(math.cross(-direction, hit.Position - mousePosition.OriginPosition));

                    if (distanceFromCenter < targetClosestToCenter.DistanceFromCenter
                        && SystemAPI.HasComponent<LocalToWorld>(hit.Entity))
                    {
                        targetClosestToCenter.Entity = hit.Entity;
                        targetClosestToCenter.Position = SystemAPI.GetComponent<LocalToWorld>(hit.Entity).Position;
                        targetClosestToCenter.DistanceFromCenter = distanceFromCenter;
                        targetClosestToCenter.IsTargetUpdated = true;

                        if (SystemAPI.HasComponent<BoidAir>(hit.Entity))
                            targetClosestToCenter.Velocity = SystemAPI.GetComponent<BoidAir>(hit.Entity).Velocity;

                        if (SystemAPI.HasComponent<BoidSurface>(hit.Entity))
                            targetClosestToCenter.Velocity = SystemAPI.GetComponent<BoidSurface>(hit.Entity).Velocity;
                    }
                }
            }

            SystemAPI.SetSingleton(targetClosestToCenter);

            collisionWorld.SphereCastAll(mousePosition.EndPosition - 2.0f * direction, lockOnConfigData.MissileLockOnRangeInMeters, direction, 10f,
                ref hits, collisionFilter, QueryInteraction.Default);
            var targetBuffer = SystemAPI.GetSingletonBuffer<CurrentTarget>();

            if (hits.Length <= 0)
            {
                if (targetBuffer.Length > 0)    // Remove targets that are already acquired
                {
                    foreach (var target in targetBuffer)
                    {
                        if (state.EntityManager.HasComponent<Unit>(target.TargetEntity))
                        {

                            if (state.EntityManager.HasBuffer<LinkedEntityGroup>(target.TargetEntity))
                            {
                                var linkedEntities = state.EntityManager.GetBuffer<LinkedEntityGroup>(target.TargetEntity);

                                foreach (var linkedEntity in linkedEntities)
                                    if (state.EntityManager.HasComponent<URPMaterialPropertyBaseColor>(linkedEntity.Value))
                                        state.EntityManager.SetComponentData(linkedEntity.Value,
                                            new URPMaterialPropertyBaseColor { Value = target.PreviousColour });
                            }
                            else if (state.EntityManager.HasComponent<URPMaterialPropertyBaseColor>(target.TargetEntity))
                                state.EntityManager.SetComponentData(target.TargetEntity,
                                    new URPMaterialPropertyBaseColor { Value = target.PreviousColour });
                        }
                        else if (state.EntityManager.HasComponent<URPMaterialPropertyBaseColor>(target.TargetEntity))
                            state.EntityManager.SetComponentData(target.TargetEntity,
                                new URPMaterialPropertyBaseColor { Value = target.PreviousColour });

                    }
                    targetBuffer.Clear();
                }
                return;
            }

            if (!targetBuffer.IsEmpty)      // Check if target are already acquired, process existing targets
            {
                NativeArray<Entity> hitEntities = new NativeArray<Entity>(hits.Length, Allocator.Temp);

                for (int i = 0; i < hits.Length; i++)
                    hitEntities[i] = hits[i].Entity;

                for (int i = 0; i < targetBuffer.Length; i++)
                {
                    if (hitEntities.Contains(targetBuffer[i].TargetEntity))
                    {
                        if (targetBuffer[i].LockOnTimer > lockOnConfigData.MissileLockOnTimeInSeconds)
                            continue;

                        var temp = targetBuffer[i];
                        temp.LockOnTimer += SystemAPI.Time.DeltaTime;

                        if (temp.LockOnTimer > lockOnConfigData.MissileLockOnTimeInSeconds)
                            temp.IsLockedOn = true;

                        targetBuffer[i] = temp;
                    }
                    else
                    {
                        if (state.EntityManager.HasBuffer<LinkedEntityGroup>(targetBuffer[i].TargetEntity))
                        {
                            var linkedEntities = state.EntityManager.GetBuffer<LinkedEntityGroup>(targetBuffer[i].TargetEntity);

                            foreach (var linkedEntity in linkedEntities)
                                if (state.EntityManager.HasComponent<URPMaterialPropertyBaseColor>(linkedEntity.Value))
                                    state.EntityManager.SetComponentData(linkedEntity.Value,
                                        new URPMaterialPropertyBaseColor { Value = targetBuffer[i].PreviousColour });
                        }
                        else if (state.EntityManager.HasComponent<URPMaterialPropertyBaseColor>(targetBuffer[i].TargetEntity))
                            state.EntityManager.SetComponentData(targetBuffer[i].TargetEntity,
                                new URPMaterialPropertyBaseColor { Value = targetBuffer[i].PreviousColour });
                        targetBuffer.RemoveAt(i);
                    }
                }
            }

            foreach (var hit in hits)
            {
                if (targetBuffer.Length > 0)
                {
                    for (int i = 0; i < targetBuffer.Length; i++)
                        if (targetBuffer[i].TargetEntity.Equals(hit.Entity))
                            continue;
                }

                if (!state.EntityManager.HasComponent<TargetableTag>(hit.Entity))
                    continue;

                TargetType targetType = TargetType.None;
                float4 targetColour;
                int targetID = -1;
                float3 targetPosition = float3.zero;

                if (state.EntityManager.HasComponent<Unit>(hit.Entity))
                {
                    var unit = state.EntityManager.GetComponentData<Unit>(hit.Entity);
                    targetType = GetTargetType(unit.UnitType);
                    targetColour = unit.Colour;
                    targetID = unit.ID;

                    if (state.EntityManager.HasComponent<LocalTransform>(hit.Entity))
                        targetPosition = state.EntityManager.GetComponentData<LocalToWorld>(hit.Entity).Position;

                    if (state.EntityManager.HasBuffer<LinkedEntityGroup>(hit.Entity))
                    {
                        var linkedEntities = state.EntityManager.GetBuffer<LinkedEntityGroup>(hit.Entity);

                        foreach (var linkedEntity in linkedEntities)
                            if (state.EntityManager.HasComponent<URPMaterialPropertyBaseColor>(linkedEntity.Value))
                                state.EntityManager.SetComponentData(linkedEntity.Value,
                                    new URPMaterialPropertyBaseColor { Value = new float4(1f) });
                    }
                    else if (state.EntityManager.HasComponent<URPMaterialPropertyBaseColor>(hit.Entity))
                        state.EntityManager.SetComponentData(hit.Entity,
                            new URPMaterialPropertyBaseColor { Value = new float4(5f) });
                }
                else
                {
                    targetType = TargetType.Air;    // If target is not a unit, it's a missile
                    targetColour = state.EntityManager.GetComponentData<Missile>(hit.Entity).Colour;

                    if (state.EntityManager.HasComponent<URPMaterialPropertyBaseColor>(hit.Entity))
                        state.EntityManager.SetComponentData(hit.Entity,
                            new URPMaterialPropertyBaseColor { Value = new float4(10f) });
                }

                targetBuffer.Add(new CurrentTarget
                {
                    TargetEntity = hit.Entity,
                    TargetType = targetType,
                    TargetID = targetID,
                    TargetPosition = targetPosition,
                    LockOnTimer = 0f,
                    IsLockedOn = false,
                    PreviousColour = targetColour,
                });

            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        
        }
    }
}
