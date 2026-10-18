using UnityEngine;
using UnityEngine.InputSystem;

namespace VoxelSandbox.Player
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public Vector2 Move { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool InteractPressed { get; private set; }

        public void OnMove(InputAction.CallbackContext ctx) => Move = ctx.ReadValue<Vector2>();
        public void OnJump(InputAction.CallbackContext ctx) { if (ctx.performed) JumpPressed = true; }
        public void OnInteract(InputAction.CallbackContext ctx) { if (ctx.performed) InteractPressed = true; }

        void LateUpdate()
        {
            JumpPressed = false;
            InteractPressed = false;
        }
    }
}
