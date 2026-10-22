using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using VoxelSandbox.Multiplayer;

namespace VoxelSandbox.Multiplayer.Tests
{
    public class PlayerInputTests
    {
        [Test]
        public void PlayerInput_SerializeDeserialize_RoundTrip()
        {
            var input = new PlayerInput
            {
                Tick = 123,
                MoveInput = new Vector2(0.5f, -0.3f),
                Jump = true,
                Sprint = false,
                Crouch = true,
                LookRotation = new Vector3(10, 180, 0),
                Action = 1,
                TargetBlockPos = new Vector3Int(5, 10, 15)
            };

            // Serializar
            var writer = new FastBufferWriter(256, Allocator.Temp);
            input.NetworkSerialize(writer);

            // Deserializar
            var reader = new FastBufferReader(writer, Allocator.Temp);
            var deserialized = new PlayerInput();
            deserialized.NetworkSerialize(reader);

            Assert.AreEqual(input.Tick, deserialized.Tick);
            Assert.AreEqual(input.MoveInput, deserialized.MoveInput);
            Assert.AreEqual(input.Jump, deserialized.Jump);
            Assert.AreEqual(input.Sprint, deserialized.Sprint);
            Assert.AreEqual(input.Crouch, deserialized.Crouch);
            Assert.AreEqual(input.LookRotation, deserialized.LookRotation);
            Assert.AreEqual(input.Action, deserialized.Action);
            Assert.AreEqual(input.TargetBlockPos, deserialized.TargetBlockPos);

            writer.Dispose();
            reader.Dispose();
        }

        [Test]
        public void PlayerInput_Equality_Works()
        {
            var a = new PlayerInput { Tick = 1, MoveInput = Vector2.one };
            var b = new PlayerInput { Tick = 1, MoveInput = Vector2.one };
            var c = new PlayerInput { Tick = 2, MoveInput = Vector2.one };

            Assert.IsTrue(a.Equals(b));
            Assert.IsFalse(a.Equals(c));
        }
    }

    public class PlayerStateTests
    {
        [Test]
        public void PlayerState_SerializeDeserialize_RoundTrip()
        {
            var state = new PlayerState
            {
                Tick = 456,
                Position = new Vector3(10.5f, 5.2f, -3.1f),
                Velocity = new Vector3(1, 2, 3),
                Rotation = Quaternion.Euler(0, 90, 0),
                IsGrounded = true
            };

            var writer = new FastBufferWriter(256, Allocator.Temp);
            state.NetworkSerialize(writer);

            var reader = new FastBufferReader(writer, Allocator.Temp);
            var deserialized = new PlayerState();
            deserialized.NetworkSerialize(reader);

            Assert.AreEqual(state.Tick, deserialized.Tick);
            Assert.AreEqual(state.Position, deserialized.Position);
            Assert.AreEqual(state.Velocity, deserialized.Velocity);
            Assert.AreEqual(state.Rotation, deserialized.Rotation);
            Assert.AreEqual(state.IsGrounded, deserialized.IsGrounded);

            writer.Dispose();
            reader.Dispose();
        }
    }

    public class PredictedNetworkTransformTests
    {
        private GameObject _playerGO;
        private PredictedNetworkTransform _predictedTransform;
        private CharacterController _controller;

        [SetUp]
        public void Setup()
        {
            _playerGO = new GameObject("TestPlayer");
            _controller = _playerGO.AddComponent<CharacterController>();
            _controller.height = 1.8f;
            _controller.radius = 0.3f;
            _controller.center = new Vector3(0, 0.9f, 0);

            _predictedTransform = _playerGO.AddComponent<PredictedNetworkTransform>();
            
            // Configurar via reflection campos privados
            var moveSpeedField = typeof(PredictedNetworkTransform).GetField("moveSpeed", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            moveSpeedField.SetValue(_predictedTransform, 5f);

            var jumpForceField = typeof(PredictedNetworkTransform).GetField("jumpForce", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            jumpForceField.SetValue(_predictedTransform, 8f);
        }

        [TearDown]
        public void Teardown()
        {
            if (_playerGO != null)
                Object.DestroyImmediate(_playerGO);
        }

        [Test]
        public void PredictedNetworkTransform_InitialState_Valid()
        {
            Assert.AreEqual(0, _predictedTransform.CurrentTick);
        }

        [Test]
        public void PlayerMotor_Simulate_MovesForward()
        {
            // Test del motor interno via reflection
            var motorType = typeof(PredictedNetworkTransform).GetNestedType("PlayerMotor", 
                System.Reflection.BindingFlags.NonPublic);
            
            var motor = System.Activator.CreateInstance(motorType, 
                new object[] { _controller, 5f, 8f, -20f });

            var simulateMethod = motorType.GetMethod("Simulate", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            var input = new PlayerInput
            {
                Tick = 1,
                MoveInput = new Vector2(0, 1), // Forward
                LookRotation = Vector3.zero
            };

            var initialState = new PlayerState
            {
                Tick = 0,
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Rotation = Quaternion.identity,
                IsGrounded = true
            };

            var newState = (PlayerState)simulateMethod.Invoke(motor, new object[] { input, initialState });

            // Debería moverse en Z+
            Assert.Greater(newState.Position.z, 0);
            Assert.AreEqual(1, newState.Tick);
        }

        [Test]
        public void PlayerMotor_Simulate_Jump_AppliesVelocity()
        {
            var motorType = typeof(PredictedNetworkTransform).GetNestedType("PlayerMotor", 
                System.Reflection.BindingFlags.NonPublic);

            var motor = System.Activator.CreateInstance(motorType, 
                new object[] { _controller, 5f, 8f, -20f });

            var simulateMethod = motorType.GetMethod("Simulate", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            var input = new PlayerInput
            {
                Tick = 1,
                MoveInput = Vector2.zero,
                Jump = true,
                LookRotation = Vector3.zero
            };

            var initialState = new PlayerState
            {
                Tick = 0,
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Rotation = Quaternion.identity,
                IsGrounded = true
            };

            var newState = (PlayerState)simulateMethod.Invoke(motor, new object[] { input, initialState });

            // Velocidad Y debería ser jumpForce
            Assert.AreEqual(8f, newState.Velocity.y, 0.01f);
            Assert.IsFalse(newState.IsGrounded);
        }

        [Test]
        public void PlayerMotor_Simulate_Gravity_Applied()
        {
            var motorType = typeof(PredictedNetworkTransform).GetNestedType("PlayerMotor", 
                System.Reflection.BindingFlags.NonPublic);

            var motor = System.Activator.CreateInstance(motorType, 
                new object[] { _controller, 5f, 8f, -20f });

            var simulateMethod = motorType.GetMethod("Simulate", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            var input = new PlayerInput
            {
                Tick = 1,
                MoveInput = Vector2.zero,
                Jump = false,
                LookRotation = Vector3.zero
            };

            var initialState = new PlayerState
            {
                Tick = 0,
                Position = new Vector3(0, 10, 0), // En el aire
                Velocity = Vector3.zero,
                Rotation = Quaternion.identity,
                IsGrounded = false
            };

            var newState = (PlayerState)simulateMethod.Invoke(motor, new object[] { input, initialState });

            // Gravedad debería reducir Y velocity
            Assert.Less(newState.Velocity.y, 0);
        }
    }

    public class ChunkManagerTests
    {
        [Test]
        public void ChunkManager_WorldToChunkPos_Works()
        {
            // Chunk.Size = 16
            var pos1 = new Vector3(0, 0, 0);
            var chunkPos1 = ChunkManager.WorldToChunkPos(pos1);
            Assert.AreEqual(new Vector3Int(0, 0, 0), chunkPos1);

            var pos2 = new Vector3(15.9f, 15.9f, 15.9f);
            var chunkPos2 = ChunkManager.WorldToChunkPos(pos2);
            Assert.AreEqual(new Vector3Int(0, 0, 0), chunkPos2);

            var pos3 = new Vector3(16, 0, 0);
            var chunkPos3 = ChunkManager.WorldToChunkPos(pos3);
            Assert.AreEqual(new Vector3Int(1, 0, 0), chunkPos3);

            var pos4 = new Vector3(-1, 0, 0);
            var chunkPos4 = ChunkManager.WorldToChunkPos(pos4);
            Assert.AreEqual(new Vector3Int(-1, 0, 0), chunkPos4);
        }

        [Test]
        public void ChunkManager_ChunkToWorldPos_CenterOfChunk()
        {
            var chunkPos = new Vector3Int(0, 0, 0);
            var worldPos = ChunkManager.ChunkToWorldPos(chunkPos);
            Assert.AreEqual(new Vector3(8, 8, 8), worldPos); // Centro del chunk 0,0,0

            var chunkPos2 = new Vector3Int(1, 0, 0);
            var worldPos2 = ChunkManager.ChunkToWorldPos(chunkPos2);
            Assert.AreEqual(new Vector3(24, 8, 8), worldPos2); // Centro chunk 1,0,0
        }
    }

    public class ClientChunkManagerTests
    {
        [Test]
        public void ClientChunkManager_WorldToLocalInChunk_Works()
        {
            var managerGO = new GameObject("TestClientChunkManager");
            var manager = managerGO.AddComponent<ClientChunkManager>();

            // Test via reflection
            var method = typeof(ClientChunkManager).GetMethod("WorldToLocalInChunk", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var worldPos1 = new Vector3Int(0, 0, 0);
            var local1 = (Vector3Int)method.Invoke(manager, new object[] { worldPos1 });
            Assert.AreEqual(new Vector3Int(0, 0, 0), local1);

            var worldPos2 = new Vector3Int(15, 15, 15);
            var local2 = (Vector3Int)method.Invoke(manager, new object[] { worldPos2 });
            Assert.AreEqual(new Vector3Int(15, 15, 15), local2);

            var worldPos3 = new Vector3Int(16, 0, 0);
            var local3 = (Vector3Int)method.Invoke(manager, new object[] { worldPos3 });
            Assert.AreEqual(new Vector3Int(0, 0, 0), local3);

            var worldPos4 = new Vector3Int(31, 20, 10);
            var local4 = (Vector3Int)method.Invoke(manager, new object[] { worldPos4 });
            Assert.AreEqual(new Vector3Int(15, 4, 10), local4);

            Object.DestroyImmediate(managerGO);
        }
    }
}