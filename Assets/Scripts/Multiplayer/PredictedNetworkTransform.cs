using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace VoxelSandbox.Multiplayer
{
    /// <summary>
    /// NetworkTransform con predicción cliente y reconciliación servidor.
    /// Cliente: aplica inputs localmente inmediatamente (predicción)
    /// Servidor: valida y corrige, envía estado autoritativo
    /// </summary>
    public class PredictedNetworkTransform : NetworkBehaviour
    {
        [Header("Configuración Predicción")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float reconciliationThreshold = 0.5f; // Distancia max para corrección suave
        [SerializeField] private float hardCorrectionThreshold = 3f; // Teleport si mayor
        [SerializeField] private int bufferSize = 1024;

        [Header("Componentes")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform cameraTransform;

        // Estado
        private uint _currentTick = 0;
        private readonly Queue<PlayerInput> _inputBuffer = new Queue<PlayerInput>();
        private readonly Queue<PlayerState> _stateBuffer = new Queue<PlayerState>();
        private PlayerState _lastAcknowledgedState;
        private Vector3 _velocity;
        private bool _isGrounded;

        // Referencias
        private PlayerMotor _motor;

        public uint CurrentTick => _currentTick;
        public PlayerState LastAcknowledgedState => _lastAcknowledgedState;

        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            _motor = new PlayerMotor(characterController, moveSpeed, jumpForce, gravity);
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                // Cliente dueño: iniciar en posición actual
                _lastAcknowledgedState = new PlayerState
                {
                    Tick = 0,
                    Position = transform.position,
                    Velocity = Vector3.zero,
                    Rotation = transform.rotation,
                    IsGrounded = characterController.isGrounded
                };
            }
        }

        private void FixedUpdate()
        {
            _currentTick++;

            if (IsOwner)
            {
                // CLIENTE: Predicción local
                RunPrediction();
            }
            else if (IsServer)
            {
                // SERVIDOR: Simulación autoritativa + reconciliación
                RunServerSimulation();
            }
            else
            {
                // CLIENTES REMOTOS: Interpolación visual
                RunInterpolation();
            }
        }

        #region Cliente (Owner) - Predicción

        private void RunPrediction()
        {
            // Obtener input actual
            var input = GetCurrentInput();
            input.Tick = _currentTick;

            // Guardar en buffer para reconciliación
            _inputBuffer.Enqueue(input);
            if (_inputBuffer.Count > bufferSize)
                _inputBuffer.Dequeue();

            // Simular movimiento localmente
            var newState = _motor.Simulate(input, _lastAcknowledgedState);

            // Aplicar inmediatamente (predicción)
            transform.position = newState.Position;
            transform.rotation = newState.Rotation;
            _velocity = newState.Velocity;
            _isGrounded = newState.IsGrounded;

            // Enviar input al servidor
            SubmitInputServerRpc(input);
        }

        private PlayerInput GetCurrentInput()
        {
            var input = new PlayerInput();
            
            // Movimiento
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            input.MoveInput = new Vector2(x, z);

            // Acciones
            input.Jump = Input.GetButton("Jump");
            input.Sprint = Input.GetKey(KeyCode.LeftShift);
            input.Crouch = Input.GetKey(KeyCode.LeftControl);

            // Rotación cámara
            if (cameraTransform != null)
                input.LookRotation = cameraTransform.eulerAngles;

            // Acciones de bloque (click izq/der)
            if (Input.GetMouseButtonDown(0)) input.Action = 2; // Break
            else if (Input.GetMouseButtonDown(1)) input.Action = 1; // Place

            return input;
        }

        [ServerRpc]
        private void SubmitInputServerRpc(PlayerInput input)
        {
            // El servidor procesa en RunServerSimulation
        }

        #endregion

        #region Servidor - Simulación + Reconciliación

        private void RunServerSimulation()
        {
            // Procesar inputs recibidos (en cola real usar NetworkQueue)
            // Aquí simplificado: el último input recibido via SubmitInputServerRpc
            
            // Simular con el último input conocido
            // En implementación real, usar NetworkQueue<PlayerInput> por cliente
            
            // Enviar estado autoritativo al cliente dueño
            var currentState = new PlayerState
            {
                Tick = _currentTick,
                Position = transform.position,
                Velocity = _velocity,
                Rotation = transform.rotation,
                IsGrounded = _isGrounded
            };

            _stateBuffer.Enqueue(currentState);
            if (_stateBuffer.Count > bufferSize)
                _stateBuffer.Dequeue();

            // Enviar reconciliación al cliente dueño
            if (IsOwner) // Solo para el dueño (host mode) - en dedicated server usar ClientRpc
            {
                ReconcileClientRpc(currentState);
            }
            else
            {
                // Para clientes no-owner en servidor dedicado
                // Se manejaría via ClientRpc dirigido
            }
        }

        [ClientRpc]
        private void ReconcileClientRpc(PlayerState serverState)
        {
            if (!IsOwner) return; // Solo el dueño se reconcilia

            _lastAcknowledgedState = serverState;

            // Verificar distancia entre posición predicha y autoritativa
            float distance = Vector3.Distance(transform.position, serverState.Position);

            if (distance > hardCorrectionThreshold)
            {
                // Hard correction - teleport
                transform.position = serverState.Position;
                transform.rotation = serverState.Rotation;
                _velocity = serverState.Velocity;
                _isGrounded = serverState.IsGrounded;
                
                // Re-simular inputs pendientes desde el tick del servidor
                ReplayInputsFromTick(serverState.Tick);
                
                Debug.Log($"[PredictedTransform] Hard correction: {distance}m");
            }
            else if (distance > reconciliationThreshold)
            {
                // Soft correction - lerp
                transform.position = Vector3.Lerp(transform.position, serverState.Position, 0.5f);
                transform.rotation = Quaternion.Slerp(transform.rotation, serverState.Rotation, 0.5f);
                
                Debug.Log($"[PredictedTransform] Soft correction: {distance}m");
            }
            // Si distance < threshold: no corrección, predicción válida
        }

        private void ReplayInputsFromTick(uint fromTick)
        {
            var inputsToReplay = new List<PlayerInput>();
            
            foreach (var input in _inputBuffer)
            {
                if (input.Tick > fromTick)
                    inputsToReplay.Add(input);
            }

            var state = _lastAcknowledgedState;
            foreach (var input in inputsToReplay)
            {
                state = _motor.Simulate(input, state);
            }

            // Aplicar estado final re-simulado
            transform.position = state.Position;
            transform.rotation = state.Rotation;
            _velocity = state.Velocity;
            _isGrounded = state.IsGrounded;
        }

        #endregion

        #region Clientes Remotos - Interpolación

        private void RunInterpolation()
        {
            // Interpolar entre estados recibidos del servidor
            // Implementación simplificada: lerp hacia última posición conocida
            if (_stateBuffer.Count > 0)
            {
                var targetState = _stateBuffer.Peek();
                transform.position = Vector3.Lerp(transform.position, targetState.Position, Time.fixedDeltaTime * 15f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetState.Rotation, Time.fixedDeltaTime * 15f);
            }
        }

        #endregion

        /// <summary>
        /// Motor de movimiento del jugador (separado para testabilidad).
        /// </summary>
        private class PlayerMotor
        {
            private readonly CharacterController _controller;
            private readonly float _moveSpeed;
            private readonly float _jumpForce;
            private readonly float _gravity;

            public PlayerMotor(CharacterController controller, float moveSpeed, float jumpForce, float gravity)
            {
                _controller = controller;
                _moveSpeed = moveSpeed;
                _jumpForce = jumpForce;
                _gravity = gravity;
            }

            public PlayerState Simulate(PlayerInput input, PlayerState previousState)
            {
                var state = previousState;
                state.Tick = input.Tick;

                // Rotación
                state.Rotation = Quaternion.Euler(0, input.LookRotation.y, 0);

                // Movimiento horizontal
                Vector3 moveDir = (state.Rotation * new Vector3(input.MoveInput.x, 0, input.MoveInput.y)).normalized;
                float speed = input.Sprint ? _moveSpeed * 1.5f : _moveSpeed;
                
                if (input.Crouch) speed *= 0.5f;

                Vector3 targetVelocity = moveDir * speed;
                targetVelocity.y = state.Velocity.y;

                // Salto
                if (input.Jump && state.IsGrounded)
                {
                    targetVelocity.y = _jumpForce;
                    state.IsGrounded = false;
                }

                // Gravedad
                targetVelocity.y += _gravity * Time.fixedDeltaTime;

                // Mover
                _controller.Move(targetVelocity * Time.fixedDeltaTime);

                state.Position = _controller.transform.position;
                state.Velocity = targetVelocity;
                state.IsGrounded = _controller.isGrounded;

                return state;
            }
        }
    }
}