using Unity.Entities;
using UnityEngine;

namespace GameSpawning
{
    [DisallowMultipleComponent]
    public class GameSpawnerAuthoring : MonoBehaviour
    {
        public GameObject RedPlayerPrefab;
        public GameObject BluePlayerPrefab;
        public GameObject GameStatePrefab;

        class GameSpawnerBaker : Baker<GameSpawnerAuthoring>
        {
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