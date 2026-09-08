using Player.Inputs;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Player.Movement
{
    [UpdateInGroup(typeof(PredictedFixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    public partial struct PlayerMovementSystem : ISystem
    {
        const float MoveSpeed = 5f;

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // WithAll<Simulate>() is required: in the prediction loop only ghosts
            // that are actually being simulated on this tick have it enabled.
            foreach (var (input, velocity) in
                     SystemAPI.Query<RefRO<PlayerInput>, RefRW<PhysicsVelocity>>()
                         .WithAll<Simulate>())
            {
                var move = math.normalizesafe(new float3(input.ValueRO.Horizontal, 0f, input.ValueRO.Vertical));
                var linear = velocity.ValueRW.Linear;
                linear.xz = (move * MoveSpeed).xz;   // keep gravity on the Y axis
                velocity.ValueRW.Linear = linear;
            }
        }
    }
}