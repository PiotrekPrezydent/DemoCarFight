using Unity.NetCode;

namespace Player.Inputs
{
    /// <summary>
    /// wsad state for a single tick,
    /// netcode generates the command buffer and the serializers from this struct,
    /// ints instead of floats so client and server read exactly the same values,
    /// ghost fields so remote players can be predicted too
    /// </summary>
    public struct PlayerInput : IInputComponentData
    {
        // wsad flattened to -1/0/1 per axis, sampled once per tick and replayed from the buffer
        [GhostField] // serialized into the snapshot, without it a remote player is predicted with empty input
        public int Horizontal;
        
        [GhostField]
        public int Vertical;
    }
}