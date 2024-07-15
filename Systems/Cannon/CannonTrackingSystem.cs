using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    partial struct CannonTrackingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WeaponConfigData>();
            state.RequireForUpdate<TargetClosestToCenter>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var weaponConfigData = SystemAPI.GetSingleton<WeaponConfigData>();
            var closestTarget = SystemAPI.GetSingleton<TargetClosestToCenter>();

            foreach (var (cannon, shootCommand, transform, targetPosition) in SystemAPI.Query<RefRW<Cannon>, RefRW<CannonPlayerShootCommand>, RefRO<LocalTransform>, RefRO<TargetPosition>>().WithAll<PlayerTag>())
            {
                var turretTransform = SystemAPI.GetComponentRW<LocalTransform>(cannon.ValueRO.Turret);
                var barrelTransform = SystemAPI.GetComponentRW<LocalTransform>(cannon.ValueRO.Barrel);
                var turretPosition = transform.ValueRO.Position + cannon.ValueRO.TurretOffset.z * transform.ValueRO.Forward();

                // Do Nothing if Target is close to Self
                if (math.distance(transform.ValueRO.Position, targetPosition.ValueRO.Value) < 1f)   
                    continue;

                float3 trackPosition = float3.zero;

                if (!closestTarget.Entity.Equals(Entity.Null) || closestTarget.DistanceFromCenter < 1f)
                {
                    float3 trackingOffset = float3.zero;

                    if (math.length(closestTarget.Velocity) > 0)
                        trackingOffset = math.distance(targetPosition.ValueRO.Value.xz, transform.ValueRO.Position.xz) / 10f * math.normalize(closestTarget.Velocity);

                    trackPosition = closestTarget.Position + trackingOffset;

                    if (trackPosition.y < 0.1f) // Compensate for ships sitting directly on the sea level
                        trackPosition.y = 0.5f;
                }
                else
                    trackPosition = targetPosition.ValueRO.Value;

                var targetElevation = trackPosition.y;
                trackPosition.y = 0f;

                // Turret Rotation
                var desiredRotation = math.mul(quaternion.LookRotation(trackPosition - turretPosition, math.up()), 
                    math.inverse(transform.ValueRO.Rotation));  // Subtract ship rotation from the target direction to get desired rotation
                
                var angleBetweeen = SignedAngleBetween(turretTransform.ValueRO.Forward(), math.forward(desiredRotation));

                if (math.abs(angleBetweeen) > math.EPSILON)
                    turretTransform.ValueRW = turretTransform.ValueRW.RotateY((angleBetweeen > 0 ? 1 : -1) 
                        * cannon.ValueRO.TurretRotateSpeedRadiansPerSecond * SystemAPI.Time.DeltaTime);

                // Barrel Elevation
                var distance = math.distance(trackPosition, turretPosition);
                var g = SystemAPI.GetSingleton<WeaponConfigData>().GravityConstant;
                var b = math.square(weaponConfigData.CannonShellMoveSpeedMetersPerSecond);
                var barrelElevation = math.atan((b - math.sqrt(math.square(b) - g * (g * math.square(distance)
                     + 2f * (targetElevation - cannon.ValueRO.BarrelOffset.y) * b))) / (g * distance)); // solve for angle given distance and velocity, highschool physics ftw!! 
                barrelElevation = math.clamp(90 - math.degrees(barrelElevation), 45f, 100f);
                barrelTransform.ValueRW.Rotation = quaternion.Euler(new float3(math.radians(barrelElevation), 0f, 0f));

                var barrelWorldTransform = state.EntityManager.GetComponentData<LocalToWorld>(cannon.ValueRO.Barrel);
                shootCommand.ValueRW.BarrelPosition = barrelWorldTransform.Position;
                shootCommand.ValueRW.BarrelRotation = barrelWorldTransform.Rotation;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        
        }

        float SignedAngleBetween(float3 from, float3 to)
        {
            float angle = math.acos(math.dot(math.normalize(from), math.normalize(to)));
            float3 cross = math.cross(from, to);
            angle *= math.sign(math.dot(math.up(), cross));
            return math.degrees(angle);
        }
    }
}
