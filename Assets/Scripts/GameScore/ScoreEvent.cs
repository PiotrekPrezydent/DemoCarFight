using Unity.Entities;
using Unity.NetCode;

namespace GameScore
{
    // One element per fall. Intentionally redundant (a counter would do) —
    // the task requires the score history to live in a DynamicBuffer.
    [InternalBufferCapacity(16)]
    public struct ScoreEvent : IBufferElementData
    {
        [GhostField] 
        public int ScoringPlayerId;  // NetworkId of the player who gained the point
        [GhostField] 
        public int FallenPlayerId;   // NetworkId of the player who fell
    }
}