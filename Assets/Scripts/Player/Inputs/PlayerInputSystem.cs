using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Player.Inputs
{
    /// <summary>
    /// reads the keyboard once per frame and writes it into PlayerInput,
    /// only for the ghost this client owns,
    /// netcode picks it up from there and ships it to the server,
    /// no [BurstCompile] on purpose - UnityEngine.Input is managed and burst would refuse it,
    /// this is the managed side of the input boundary
    /// </summary>
    [UpdateInGroup(typeof(GhostInputSystemGroup))] // runs right before commands are sent, so the input does not miss its tick
    public partial struct PlayerInputSystem : ISystem
    {
        // no point sampling input before the connection is in game, nothing would carry it
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
        }

        // clears last frame's values first, the component is persistent
        public void OnUpdate(ref SystemState state)
        {
            // Only the locally owned ghost is controlled by this client.
            foreach (var input in SystemAPI.Query<RefRW<PlayerInput>>().WithAll<GhostOwnerIsLocal>())
            {
                input.ValueRW = default;
                if (Input.GetKey(KeyCode.A)) input.ValueRW.Horizontal -= 1;
                if (Input.GetKey(KeyCode.D)) input.ValueRW.Horizontal += 1;
                if (Input.GetKey(KeyCode.S)) input.ValueRW.Vertical -= 1;
                if (Input.GetKey(KeyCode.W)) input.ValueRW.Vertical += 1;
            }
        }
    }
}