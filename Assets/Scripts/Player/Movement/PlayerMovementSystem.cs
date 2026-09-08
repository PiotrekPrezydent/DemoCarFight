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
        const float Acceleration = 40f;
        const float Deceleration = 15f;

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

                var current = linear.xz;
                var target = (move * MoveSpeed).xz;
                var speed = math.length(current);
                var hasInput = math.lengthsq(move) > 0f;
                
                var rate = hasInput && speed <= MoveSpeed ? Acceleration : Deceleration;

                linear.xz = MoveTowards(current, target, rate * SystemAPI.Time.DeltaTime);
                velocity.ValueRW.Linear = linear;

            }
        }
        
        static float2 MoveTowards(float2 current, float2 target, float maxDelta)
        {
            var delta = target - current;
            var dist = math.length(delta);
            return dist <= maxDelta || dist < 1e-5f ? target : current + delta / dist * maxDelta;
        }
    }
}