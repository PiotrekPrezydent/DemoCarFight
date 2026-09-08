using Unity.Entities;

namespace Components
{
    public struct GameSpawner : IComponentData
    {
        public Entity RedPlayerPrefab;
        public Entity BluePlayerPrefab;
        public Entity GameStatePrefab;
    }
}