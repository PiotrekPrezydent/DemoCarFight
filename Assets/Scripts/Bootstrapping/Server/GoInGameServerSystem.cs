using GameSpawnering;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Systems.GoInGame
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GoInGameServerSystem : ISystem
    {
        ComponentLookup<NetworkId> _networkIdFromEntity;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameSpawner>();
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<GoInGameRequest>()
                .WithAll<ReceiveRpcCommandRequest>();
            state.RequireForUpdate(state.GetEntityQuery(builder));
            _networkIdFromEntity = state.GetComponentLookup<NetworkId>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var spawner = SystemAPI.GetSingleton<GameSpawner>();
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
            _networkIdFromEntity.Update(ref state);

            foreach (var (request, requestEntity) in
                     SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>()
                         .WithAll<GoInGameRequest>().WithEntityAccess())
            {
                var connection = request.ValueRO.SourceConnection;
                commandBuffer.AddComponent<NetworkStreamInGame>(connection);

                var networkId = _networkIdFromEntity[connection].Value;
                
                var prefab = networkId % 2 == 1 ? spawner.RedPlayerPrefab : spawner.BluePlayerPrefab;
                var player = commandBuffer.Instantiate(prefab);
                
                commandBuffer.SetComponent(player, new GhostOwner { NetworkId = networkId });
                
                // Spawn players on opposite sides of the platform.
                commandBuffer.SetComponent(player, LocalTransform.FromPosition(SpawnPositionFor(networkId)));

                // Destroy the player entity when the connection is closed.
                commandBuffer.AppendToBuffer(connection, new LinkedEntityGroup { Value = player });
                commandBuffer.DestroyEntity(requestEntity);
            }
            commandBuffer.Playback(state.EntityManager);
        }

        static float3 SpawnPositionFor(int networkId) => new float3(
            networkId % 2 == 1 ? 
                -2f : 
                2f, 1f, 0f);
    }
}