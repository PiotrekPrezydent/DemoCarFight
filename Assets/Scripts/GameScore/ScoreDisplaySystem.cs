using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace GameScore
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class ScoreDisplaySystem : SystemBase
    {
        int _lastKnownLength = -1;

        protected override void OnCreate() => RequireForUpdate<GameStateTag>();

        protected override void OnUpdate()
        {
            var scoreBuffer = SystemAPI.GetSingletonBuffer<ScoreEvent>(isReadOnly: true);
            if (scoreBuffer.Length == _lastKnownLength) return;
            _lastKnownLength = scoreBuffer.Length;

            var scoreByPlayer = new System.Collections.Generic.Dictionary<int, int>();
            for (var i = 0; i < scoreBuffer.Length; i++)
            {
                var id = scoreBuffer[i].ScoringPlayerId;
                scoreByPlayer.TryGetValue(id, out var current);
                scoreByPlayer[id] = current + 1;
            }

            var localId = SystemAPI.GetSingleton<NetworkId>().Value;
            Debug.Log($"[Score] local player {localId} | " +
                      string.Join("  ", System.Linq.Enumerable.Select(
                          scoreByPlayer, kv => $"player {kv.Key}: {kv.Value}")));
        }
    }
}