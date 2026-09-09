using Unity.Entities;
using UnityEngine;

namespace Player.Inputs
{
    /// <summary>
    /// puts PlayerInput on the baked player prefab,
    /// it has to be there at bake time or netcode will not generate the command stream
    /// </summary>
    [DisallowMultipleComponent] // guards against baking the same component onto the entity twice
    public class PlayerInputAuthoring : MonoBehaviour
    {
        class PlayerInputBaker : Baker<PlayerInputAuthoring>
        {
            // has to happen at bake time, adding PlayerInput later generates no command stream
            public override void Bake(PlayerInputAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PlayerInput>(entity);
            }
        }
    }
}
