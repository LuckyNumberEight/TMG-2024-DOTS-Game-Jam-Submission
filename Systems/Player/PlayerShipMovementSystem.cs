using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace LuckyNumber8.TMG2024DOTSJAM
{
    partial struct PlayerShipMovementSystem : ISystem
    {
        EntityQuery moveShipQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<InputMoveDirection>();
            state.RequireForUpdate<PlayerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerMove = state.EntityManager.GetComponentData<MoveComponent>(playerEntity);
            playerMove.Direction = SystemAPI.GetSingleton<InputMoveDirection>().Value;
            SystemAPI.SetComponent(playerEntity, playerMove);

            new MovePlayerShipJob { DeltaTime = SystemAPI.Time.DeltaTime}.Schedule();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        
        }

        [BurstCompile]
        partial struct MovePlayerShipJob : IJobEntity
        {
            public float DeltaTime;

            static float SignedAngleBetween(float3 from, float3 to)
            {
                float angle = math.acos(math.dot(math.normalize(from), math.normalize(to)));
                float3 cross = math.cross(from, to);
                angle *= math.sign(math.dot(math.up(), cross));
                return math.degrees(angle);
            }

            void Execute(ref LocalTransform transform, ref MoveComponent move)
            {
                if (math.lengthsq(move.Direction) < float.Epsilon)     // do nothing if no direction is given
                    return;

                var angleBetween = SignedAngleBetween(transform.Forward(), move.Direction);

                if (math.abs(angleBetween) > float.Epsilon)
                    transform = transform.RotateY((angleBetween > 0 ? 1 : -1) * move.TurnSpeed * DeltaTime);

                move.Velocity = DeltaTime * move.MoveSpeed * transform.Forward();
                transform.Position += move.Velocity;
            }
        }
    }
}
