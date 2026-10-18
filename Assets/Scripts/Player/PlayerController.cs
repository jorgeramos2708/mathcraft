using UnityEngine;

namespace VoxelSandbox.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public float speed = 5f;
        public float gravity = -9.81f;
        private CharacterController _controller;
        private Vector3 _velocity;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            Vector3 move = transform.right * x + transform.forward * z;
            _controller.Move(move.normalized * speed * Time.deltaTime);

            _velocity.y += gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);

            if (_controller.isGrounded && _velocity.y < 0)
                _velocity.y = -2f;
        }
    }
}
