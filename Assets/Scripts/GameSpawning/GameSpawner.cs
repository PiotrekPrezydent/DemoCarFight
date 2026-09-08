using Unity.Entities;

namespace GameSpawnering
{
    public struct GameSpawner : IComponentData
    {
        public Entity RedPlayerPrefab;
        public Entity BluePlayerPrefab;
        public Entity GameStatePrefab;
    }
}