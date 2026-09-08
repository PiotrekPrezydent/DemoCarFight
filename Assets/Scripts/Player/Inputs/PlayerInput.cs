using Unity.NetCode;

namespace Player.Inputs
{
    public struct PlayerInput : IInputComponentData
    {
        [GhostField] 
        public int Horizontal;
        
        [GhostField]
        public int Vertical;
    }
}