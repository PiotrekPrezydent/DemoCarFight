using Unity.Entities;
using UnityEngine;

namespace Player.Movement
{
    /// <summary>
    /// editor side of CarSpec, tuned while playing,
    /// defaults fit a car around 7 units long on a 50 unit arena
    /// </summary>
    [DisallowMultipleComponent] // guards against baking the same component onto the entity twice
    public class CarSpecAuthoring : MonoBehaviour
    {
        public float MaxSpeed           = 22f;
        public float ReverseSpeedFactor = 0.4f;
        public float Acceleration       = 4f;
        public float Braking            = 8f;
        public float RollingResistance  = 1.2f;

        public float TurnRate            = 2.4f;
        public float SteerReferenceSpeed = 5f;
        public float SteerResponse       = 12f;
        public float SpinDamping         = 3f;

        public float CorneringSpeedFactor = 0.8f;
        public float LateralGrip          = 9f;
        public float DriftGrip            = 2.5f;

        class CarSpecBaker : Baker<CarSpecAuthoring>
        {
            public override void Bake(CarSpecAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CarSpec
                {
                    MaxSpeed             = authoring.MaxSpeed,
                    ReverseSpeedFactor   = authoring.ReverseSpeedFactor,
                    Acceleration         = authoring.Acceleration,
                    Braking              = authoring.Braking,
                    RollingResistance    = authoring.RollingResistance,
                    TurnRate             = authoring.TurnRate,
                    SteerReferenceSpeed  = authoring.SteerReferenceSpeed,
                    SteerResponse        = authoring.SteerResponse,
                    SpinDamping          = authoring.SpinDamping,
                    CorneringSpeedFactor = authoring.CorneringSpeedFactor,
                    LateralGrip          = authoring.LateralGrip,
                    DriftGrip            = authoring.DriftGrip
                });
            }
        }
    }
}
