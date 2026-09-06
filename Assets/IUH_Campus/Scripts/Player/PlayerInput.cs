using UnityEngine;

namespace IUHCampus.Player
{
    public class PlayerInput : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsRunning { get; private set; }
        public bool InteractTriggered { get; private set; }
        public bool JumpTriggered { get; private set; }
        public bool EscapeTriggered { get; private set; }

        private void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            MoveInput = new Vector2(h, v).normalized;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            LookInput = new Vector2(mouseX, mouseY);

            IsRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            InteractTriggered = Input.GetKeyDown(KeyCode.E);
            JumpTriggered = Input.GetKeyDown(KeyCode.Space);
            EscapeTriggered = Input.GetKeyDown(KeyCode.Escape);
        }
    }
}
