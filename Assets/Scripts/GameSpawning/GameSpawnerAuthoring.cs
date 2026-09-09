using Unity.Entities;
using UnityEngine;

namespace GameSpawning
{
    /// <summary>
    /// editor side of GameSpawner, lets us drag the prefabs in the inspector,
    /// baking turns the GameObject references into entity references
    /// </summary>
    [DisallowMultipleComponent] // two of these would bake onto the same entity twice and throw
    public class GameSpawnerAuthoring : MonoBehaviour
    {
        // dragged in the inspector as GameObjects, baking turns them into entity references
        public GameObject RedPlayerPrefab;
        public GameObject BluePlayerPrefab;
        public GameObject GameStatePrefab;

        class GameSpawnerBaker : Baker<GameSpawnerAuthoring>
        {
            // None on the spawner itself, it has no position and never renders
            public override void Bake(GameSpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new GameSpawner
                {
                    RedPlayerPrefab = GetEntity(authoring.RedPlayerPrefab, TransformUsageFlags.Dynamic),
                    BluePlayerPrefab = GetEntity(authoring.BluePlayerPrefab, TransformUsageFlags.Dynamic),
                    GameStatePrefab = GetEntity(authoring.GameStatePrefab, TransformUsageFlags.None)
                });
            }
        }
    }
}