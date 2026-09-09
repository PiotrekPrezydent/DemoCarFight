using Unity.NetCode;

namespace Bootstrapping
{
    /// <summary>
    /// rpc the client sends once when it is ready to play,
    /// empty on purpose, the fact that it arrived is the whole message,
    /// IRpcCommand is a marker for the source generator - it writes the serializer
    /// and registers the type hash at compile time, no reflection at runtime
    /// </summary>
    public struct GoInGameRequest : IRpcCommand { }
}
