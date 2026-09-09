using GameSpawning;
using Unity.Entities;

namespace GameScore
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GameStateSpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state) => state.RequireForUpdate<GameSpawner>();

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false; // run once
            var prefab = SystemAPI.GetSingleton<GameSpawner>().GameStatePrefab;
            state.EntityManager.Instantiate(prefab);
        }
    }
}