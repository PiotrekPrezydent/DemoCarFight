using Unity.NetCode;

namespace Player.Inputs
{
    public struct PlayerInput : IInputComponentData
    {
        public int Horizontal;
        public int Vertical;
    }
}