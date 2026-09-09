using Unity.Entities;
using UnityEngine;

namespace GameScore
{
    /// <summary>
    /// editor side of the game state ghost,
    /// puts the tag and the empty score buffer on the entity at bake time,
    /// the buffer has to exist here or netcode will not replicate it
    /// </summary>
    [DisallowMultipleComponent] // guards against baking the same component onto the entity twice
    public class GameStateAuthoring : MonoBehaviour
    {
        class GameStateBaker : Baker<GameStateAuthoring>
        {
            // None because the game state entity has no position and never renders
            public override void Bake(GameStateAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent<GameStateTag>(entity);
                AddBuffer<ScoreEvent>(entity);
            }
        }
    }
}