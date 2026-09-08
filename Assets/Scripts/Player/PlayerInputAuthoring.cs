using Unity.Entities;
using UnityEngine;

namespace Player
{
    [DisallowMultipleComponent]
    public class PlayerInputAuthoring : MonoBehaviour
    {
        class PlayerInputBaker : Baker<PlayerInputAuthoring>
        {
            public override void Bake(PlayerInputAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PlayerInput>(entity);
            }
        }
    }
}
