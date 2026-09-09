using Player.Inputs;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Player.Movement
{
    /// <summary>
    /// turns PlayerInput into PhysicsVelocity,
    /// runs on the server and again for every replayed tick in the client prediction loop,
    /// accelerates towards the target speed instead of assigning it,
    /// above MoveSpeed it only brakes so a collision push survives instead of being cancelled
    /// </summary>
    [UpdateInGroup(typeof(PredictedFixedStepSimulationSystemGroup))] // once per tick on the server, once per replayed tick on the client
    [UpdateBefore(typeof(PhysicsSystemGroup))]                       // netcode moves physics into the group above, velocity must be set before the solver
    [BurstCompile]                                                   // nothing managed here, so the whole system compiles
    public partial struct PlayerMovementSystem : ISystem
    {
        // full speed in 0.125 s keeps steering sharp, a push decays over roughly 0.3 s
        const float MoveSpeed = 5f;
        const float Acceleration = 40f;
        const float Deceleration = 15f;

        // runs many times per frame during rollback, so it must stay deterministic
        // and keep no state between ticks
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
        
        // math has no MoveTowards for float2, the epsilon guards against dividing by zero
        static float2 MoveTowards(float2 current, float2 target, float maxDelta)
        {
            var delta = target - current;
            var dist = math.length(delta);
            return dist <= maxDelta || dist < 1e-5f ? target : current + delta / dist * maxDelta;
        }
    }
}