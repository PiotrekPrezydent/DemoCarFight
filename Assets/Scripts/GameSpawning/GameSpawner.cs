using Unity.Entities;

namespace GameSpawning
{
    public struct GameSpawner : IComponentData
    {
        public Entity RedPlayerPrefab;
        public Entity BluePlayerPrefab;
        public Entity GameStatePrefab;
    }
}