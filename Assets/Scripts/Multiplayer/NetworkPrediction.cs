using Unity.Netcode;
using UnityEngine;

namespace VoxelSandbox.Multiplayer
{
    /// <summary>
    /// Estructura de input del jugador para predicción y reconciliación.
    /// </summary>
    public struct PlayerInput : INetworkSerializable, IEquatable<PlayerInput>
    {
        public uint Tick;
        public Vector2 MoveInput;      // WASD / joystick
        public bool Jump;
        public bool Sprint;
        public bool Crouch;
        public Vector3 LookRotation;   // Euler angles cámara
        public byte Action;            // 0=nada, 1=place, 2=break, 3=interact
        public Vector3Int TargetBlockPos; // Para place/break

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref MoveInput);
            serializer.SerializeValue(ref Jump);
            serializer.SerializeValue(ref Sprint);
            serializer.SerializeValue(ref Crouch);
            serializer.SerializeValue(ref LookRotation);
            serializer.SerializeValue(ref Action);
            serializer.SerializeValue(ref TargetBlockPos);
        }

        public bool Equals(PlayerInput other)
        {
            return Tick == other.Tick &&
                   MoveInput.Equals(other.MoveInput) &&
                   Jump == other.Jump &&
                   Sprint == other.Sprint &&
                   Crouch == other.Crouch &&
                   LookRotation.Equals(other.LookRotation) &&
                   Action == other.Action &&
                   TargetBlockPos.Equals(other.TargetBlockPos);
        }

        public override bool Equals(object obj) => obj is PlayerInput other && Equals(other);
        public override int GetHashCode() => Tick.GetHashCode();
    }

    /// <summary>
    /// Estado del jugador para reconciliación.
    /// </summary>
    public struct PlayerState : INetworkSerializable, IEquatable<PlayerState>
    {
        public uint Tick;
        public Vector3 Position;
        public Vector3 Velocity;
        public Quaternion Rotation;
        public bool IsGrounded;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Velocity);
            serializer.SerializeValue(ref Rotation);
            serializer.SerializeValue(ref IsGrounded);
        }

        public bool Equals(PlayerState other)
        {
            return Tick == other.Tick &&
                   Position.Equals(other.Position) &&
                   Velocity.Equals(other.Velocity) &&
                   Rotation.Equals(other.Rotation) &&
                   IsGrounded == other.IsGrounded;
        }
    }
}