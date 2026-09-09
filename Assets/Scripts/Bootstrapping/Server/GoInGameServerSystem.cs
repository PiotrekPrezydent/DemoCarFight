using GameSpawning;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Bootstrapping.Server
{
    /// <summary>
    /// server side of the handshake,
    /// on GoInGameRequest flags the connection as in game and spawns a cube for it,
    /// red for odd network ids, blue for even, placed on opposite sides of the platform,
    /// the cube is linked to the connection so it is destroyed when the player leaves
    /// </summary>
    [BurstCompile]                                               // needed on the struct and on every method, burst compiles methods not types
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)] // mirror of the client system, this one flag is the whole difference
    public partial struct GoInGameServerSystem : ISystem
    {
        // random access by entity instead of iteration, has to be refreshed every frame
        ComponentLookup<NetworkId> _networkIdFromEntity;

        // caches the lookup and gates the system on the spawner and on an incoming rpc
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

        // spawns one cube per connection that asked to go in game, then eats the rpc entity
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
                
                // Spawn players on opposite sides of the platform, nose pointing at each other.
                commandBuffer.SetComponent(player, LocalTransform.FromPositionRotation(
                    SpawnPositionFor(networkId), SpawnRotationFor(networkId)));

                // Destroy the player entity when the connection is closed.
                commandBuffer.AppendToBuffer(connection, new LinkedEntityGroup { Value = player });
                commandBuffer.DestroyEntity(requestEntity);
            }
            commandBuffer.Playback(state.EntityManager);
        }

        // The arena is 50 wide and a car is about 7 long, so the two of them start
        // roughly three car lengths apart with the whole platform between them.
        const float SpawnDistanceFromCenter = 12f;

        // Just above the platform surface, which sits at y = 0. The car drops the last
        // few centimetres and settles, which also proves gravity is running.
        const float SpawnHeight = 0.6f;

        // odd ids start on the west side, even ids on the east side
        static float3 SpawnPositionFor(int networkId) => new float3(
            networkId % 2 == 1 ? -SpawnDistanceFromCenter : SpawnDistanceFromCenter,
            SpawnHeight,
            0f);

        // The nose points along local +X, so the car on the west side needs no rotation
        // and the one on the east side is turned around to face it.
        static quaternion SpawnRotationFor(int networkId) =>
            networkId % 2 == 1 ? quaternion.identity : quaternion.RotateY(math.PI);
    }
}