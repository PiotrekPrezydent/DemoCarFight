using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

namespace Bootstrapping
{
    /// <summary>
    /// disables the rpc and ghost protocol hash check between client and server,
    /// needed when both sides are built from a different set of assemblies,
    /// runs once in OnCreate and switches itself off, must happen before the first connection
    /// </summary>
    [BurstCompile] // native code, works here because nothing in this system is managed
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation |
                       WorldSystemFilterFlags.ThinClientSimulation)] // all three worlds, the flag changes rpc encoding so it must match on both sides
    [UpdateInGroup(typeof(InitializationSystemGroup))] // start of the frame
    [CreateAfter(typeof(RpcSystem))] // orders OnCreate, not OnUpdate - RpcSystem creates the RpcCollection we read below
    public partial struct SetRpcSystemDynamicAssemblyListSystem : ISystem
    {
        // sets the flag once and switches the system off, there is no OnUpdate on purpose
        public void OnCreate(ref SystemState state)
        {
            SystemAPI.GetSingletonRW<RpcCollection>().ValueRW.DynamicAssemblyList = true;
            state.Enabled = false;
        }
    }
}
