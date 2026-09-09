using Player;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

namespace GameScore
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct FallDetectionSystem : ISystem
    {
        // the platform surface is at y = 0, so this is a clear fall and not a bump
        const float FallThresholdY = -8f;

        // kept in sync with GoInGameServerSystem - see the comments there
        const float SpawnDistanceFromCenter = 12f;
        const float SpawnHeight = 0.6f;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameStateTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Collect the network ids of all currently alive players (exactly two here).
            var players = new NativeList<int>(2, Allocator.Temp);
            foreach (var owner in SystemAPI.Query<RefRO<GhostOwner>>().WithAll<PlayerTag>())
                players.Add(owner.ValueRO.NetworkId);

            // Re-fetch the buffer here, after any structural change, never cache it.
            var scoreBuffer = SystemAPI.GetSingletonBuffer<ScoreEvent>();

            foreach (var (transform, velocity, owner) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRW<PhysicsVelocity>, RefRO<GhostOwner>>()
                         .WithAll<PlayerTag>())
            {
                if (transform.ValueRO.Position.y > FallThresholdY) continue;

                var fallenId = owner.ValueRO.NetworkId;
                // With exactly two players the opponent is simply "the other one".
                foreach (var candidate in players)
                {
                    if (candidate == fallenId) continue;
                    scoreBuffer.Add(new ScoreEvent { ScoringPlayerId = candidate, FallenPlayerId = fallenId });
                    break;
                }

                // Respawn facing the arena again, and clear the velocity - otherwise the car
                // reappears with the speed it had while falling and drops straight off again.
                transform.ValueRW = LocalTransform.FromPositionRotation(
                    SpawnPositionFor(fallenId), SpawnRotationFor(fallenId));
                velocity.ValueRW = default;
            }

            players.Dispose();
        }

        static float3 SpawnPositionFor(int networkId) => new float3(
            networkId % 2 == 1 ? -SpawnDistanceFromCenter : SpawnDistanceFromCenter,
            SpawnHeight,
            0f);

        static quaternion SpawnRotationFor(int networkId) =>
            networkId % 2 == 1 ? quaternion.identity : quaternion.RotateY(math.PI);
    }
}