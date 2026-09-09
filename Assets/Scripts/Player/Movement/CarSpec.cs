using Unity.Entities;

namespace Player.Movement
{
    /// <summary>
    /// tuning values for one car, baked from the inspector so every prefab can drive differently,
    /// not a ghost field - identical on both sides because it comes from the same prefab
    /// </summary>
    public struct CarSpec : IComponentData
    {
        public float MaxSpeed;
        public float ReverseSpeedFactor;   // top speed in reverse, as a fraction of MaxSpeed
        public float Acceleration;
        public float Braking;              // used when the throttle fights the current direction
        public float RollingResistance;    // how fast it coasts to a stop with no throttle

        public float TurnRate;             // rad/s at full lock and full speed
        public float SteerReferenceSpeed;  // speed at which steering reaches full strength
        public float SteerResponse;        // how fast the yaw follows the steering input
        public float SpinDamping;          // how fast a collision spin dies out

        public float CorneringSpeedFactor; // speed multiplier at full lock, 1 disables it
        public float LateralGrip;
        public float DriftGrip;            // sideways damping mid-turn, low value slides
    }
}
