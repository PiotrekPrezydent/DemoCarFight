using Unity.NetCode;

namespace Player
{
    public struct PlayerInput : IInputComponentData
    {
        public int Horizontal;
        public int Vertical;
    }
}