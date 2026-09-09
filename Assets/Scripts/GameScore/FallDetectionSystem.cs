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
        const float FallThresholdY = -5f;

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

                // Respawn: reset both transform and velocity, otherwise the cube keeps falling.
                transform.ValueRW = LocalTransform.FromPosition(SpawnPositionFor(fallenId));
                velocity.ValueRW = default;
            }

            players.Dispose();
        }

        static float3 SpawnPositionFor(int networkId) =>
            new float3(networkId % 2 == 1 ? -2f : 2f, 1f, 0f);
    }
}