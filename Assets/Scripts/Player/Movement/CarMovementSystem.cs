using Player.Inputs;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Player.Movement
{
    /// <summary>
    /// classic car handling: w and s are throttle and reverse, a and d turn the car,
    /// steering scales with speed so a standing car cannot pivot on the spot,
    /// sideways grip drops while turning, which is what makes it drift
    /// </summary>
    [UpdateInGroup(typeof(PredictedFixedStepSimulationSystemGroup))] // once per tick on the server, once per replayed tick on the client
    [UpdateBefore(typeof(PhysicsSystemGroup))]                       // velocity has to be set before the solver runs
    [BurstCompile]                                                   // nothing managed here, so the whole system compiles
    public partial struct CarMovementSystem : ISystem
    {
        // the model is 7.15 long on X and the front wheels sit at +X, so the nose is local +X,
        // change to math.forward() if the model is ever rotated to the usual convention
        static readonly float3 LocalNose = new float3(1f, 0f, 0f);

        const float Epsilon = 0.001f;

        // runs many times per frame during rollback, so it must stay deterministic
        // and keep no state between ticks
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            // WithAll<Simulate>() is required: in the prediction loop only ghosts
            // that are actually being simulated on this tick have it enabled.
            foreach (var (input, transform, velocity, mass, spec) in
                     SystemAPI.Query<RefRO<PlayerInput>, RefRO<LocalTransform>,
                                     RefRW<PhysicsVelocity>, RefRO<PhysicsMass>, RefRO<CarSpec>>()
                         .WithAll<Simulate>())
            {
                var rotation = transform.ValueRO.Rotation;

                var nose = math.mul(rotation, LocalNose);
                nose.y = 0f;
                nose = math.normalizesafe(nose, new float3(1f, 0f, 0f));
                var side = math.cross(math.up(), nose);

                var linear    = velocity.ValueRW.Linear;
                var alongNose = math.dot(linear, nose);
                var sideways  = math.dot(linear, side);
                var vertical  = linear.y;

                var steer    = input.ValueRO.Horizontal;
                var throttle = input.ValueRO.Vertical;

                // --- steering -------------------------------------------------
                var yawRate = velocity.ValueRW
                    .GetAngularVelocityWorldSpace(mass.ValueRO, rotation).y;

                var steering = steer != 0;
                var yawTarget = 0f;

                if (steering)
                {
                    // no grip on the tyres without motion, and reversing flips the direction
                    // the car swings, the same way it does in a real car
                    var steerStrength = math.saturate(
                        math.abs(alongNose) / math.max(spec.ValueRO.SteerReferenceSpeed, Epsilon));

                    yawTarget = steer * spec.ValueRO.TurnRate * steerStrength * math.sign(alongNose);
                }

                var yawRateOfChange = steering ? spec.ValueRO.SteerResponse : spec.ValueRO.SpinDamping;
                yawRate = math.lerp(yawRate, yawTarget, 1f - math.exp(-yawRateOfChange * deltaTime));

                velocity.ValueRW.SetAngularVelocityWorldSpace(
                    mass.ValueRO, rotation, new float3(0f, yawRate, 0f));

                // --- throttle -------------------------------------------------
                var targetSpeed = throttle > 0
                    ? spec.ValueRO.MaxSpeed
                    : throttle < 0
                        ? -spec.ValueRO.MaxSpeed * spec.ValueRO.ReverseSpeedFactor
                        : 0f;

                var cornering = math.saturate(math.abs(yawRate) / math.max(spec.ValueRO.TurnRate, Epsilon));
                targetSpeed *= math.lerp(1f, spec.ValueRO.CorneringSpeedFactor, cornering);

                float rate;
                if (throttle == 0)
                    rate = spec.ValueRO.RollingResistance;                  // coast to a stop
                else if (alongNose * targetSpeed < 0f)
                    rate = spec.ValueRO.Braking;                            // throttle fights the motion
                else if (math.abs(alongNose) > math.abs(targetSpeed))
                    rate = spec.ValueRO.RollingResistance;                  // pushed past top speed, let it carry
                else
                    rate = spec.ValueRO.Acceleration;

                alongNose = math.lerp(alongNose, targetSpeed, 1f - math.exp(-rate * deltaTime));

                // --- grip -----------------------------------------------------
                // grip falls off in a corner, this line is the drift
                var grip = math.lerp(spec.ValueRO.LateralGrip, spec.ValueRO.DriftGrip, cornering);
                sideways *= math.exp(-grip * deltaTime);

                velocity.ValueRW.Linear = nose * alongNose
                                          + side * sideways
                                          + new float3(0f, vertical, 0f);
            }
        }
    }
}
