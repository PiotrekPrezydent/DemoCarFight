using GameSpawning;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace Bootstrapping.Client
{
    /// <summary>
    /// client side of the handshake,
    /// waits until the server hands out a NetworkId, then flags the connection as in game
    /// and sends GoInGameRequest so the server spawns our player,
    /// stops running by itself once the connection has NetworkStreamInGame
    /// </summary>
    [BurstCompile]                                               // needed on the struct and on every method, burst compiles methods not types
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)] // never created in the server world, so there is no runtime check to pay for
    public partial struct GoInGameClientSystem : ISystem
    {
        // two gates: the subscene has to be loaded, and the connection must not be in game yet
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameSpawner>();
            
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<NetworkId>()
                .WithNone<NetworkStreamInGame>();
            
            state.RequireForUpdate(state.GetEntityQuery(builder));
        }

        // flags the connection and sends the rpc, after which it stops matching its own query
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (id, entity) in SystemAPI.Query<RefRO<NetworkId>>()
                         .WithEntityAccess().WithNone<NetworkStreamInGame>())
            {
                commandBuffer.AddComponent<NetworkStreamInGame>(entity);
                var req = commandBuffer.CreateEntity();
                commandBuffer.AddComponent<GoInGameRequest>(req);
                commandBuffer.AddComponent(req, new SendRpcCommandRequest { TargetConnection = entity });
            }
            commandBuffer.Playback(state.EntityManager);
        }
    }
}