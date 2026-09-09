using Unity.Entities;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// puts PlayerTag on the baked player prefab
    /// </summary>
    [DisallowMultipleComponent] // guards against baking the same component onto the entity twice
    public class PlayerAuthoring : MonoBehaviour
    {
        class PlayerBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PlayerTag>(entity);
            }
        }
    }
}