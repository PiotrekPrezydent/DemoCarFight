using Unity.Entities;

namespace Player
{
    /// <summary>
    /// marks an entity as a player, holds no data, only used for queries
    /// </summary>
    public struct PlayerTag : IComponentData { }
}