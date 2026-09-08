using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Player.Inputs
{
    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    public partial struct PlayerInputSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
        }

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