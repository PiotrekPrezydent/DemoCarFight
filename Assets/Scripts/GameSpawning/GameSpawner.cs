using Unity.Entities;

namespace GameSpawning
{
    /// <summary>
    /// singleton holding the baked prefabs as entities,
    /// the server reads it when a player joins
    /// </summary>
    public struct GameSpawner : IComponentData
    {
        // baked prefab entities, they carry the Prefab tag so no query sees them
        public Entity RedPlayerPrefab;
        public Entity BluePlayerPrefab;
        public Entity GameStatePrefab;
    }
}