using UnityEngine;

namespace IUHCampus.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(PlayerState))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Speeds")]
        public float walkSpeed = 3.5f;
        public float runSpeed = 6.2f;
        public float rotationSpeed = 12.0f;
        public float gravity = -9.81f;

        [Header("Camera Reference")]
        public Transform cameraTransform;

        private CharacterController m_Controller;
        private PlayerInput m_Input;
        private PlayerState m_State;
        private float m_VerticalVelocity = 0f;

        private void Awake()
        {
            m_Controller = GetComponent<CharacterController>();
            m_Input = GetComponent<PlayerInput>();
            m_State = GetComponent<PlayerState>();

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            if (m_State != null && !m_State.movementEnabled) return;

            Vector2 move = m_Input.MoveInput;
            float currentSpeed = m_Input.IsRunning ? runSpeed : walkSpeed;

            Vector3 moveDirection = Vector3.zero;
            if (cameraTransform != null)
            {
                Vector3 forward = cameraTransform.forward;
                Vector3 right = cameraTransform.right;
                forward.y = 0f;
                right.y = 0f;
                forward.Normalize();
                right.Normalize();

                moveDirection = forward * move.y + right * move.x;
            }
            else
            {
                moveDirection = new Vector3(move.x, 0, move.y);
            }

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            if (m_Controller.isGrounded)
            {
                m_VerticalVelocity = -0.5f;
            }
            else
            {
                m_VerticalVelocity += gravity * Time.deltaTime;
            }

            Vector3 finalMove = moveDirection * currentSpeed + Vector3.up * m_VerticalVelocity;
            m_Controller.Move(finalMove * Time.deltaTime);
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (m_Controller == null) m_Controller = GetComponent<CharacterController>();
            if (m_Controller != null) m_Controller.enabled = false;
            transform.position = position;
            transform.rotation = rotation;
            if (m_Controller != null) m_Controller.enabled = true;
        }
    }
}
