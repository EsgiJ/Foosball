using UnityEngine;
using UnityEngine.InputSystem;

namespace Foosball.Gameplay
{
    public struct ShootEvent
    {
        public Vector2 Direction;
    }

    public struct GoalEvent
    {
        public bool IsHome;
        public Vector3 Position;
    }

    public struct GoalFeedbackEvent
    {
        public Gamepad Scorer;
        public Gamepad Conceder;
    }

    public struct PossessSwitchEvent
    {
    }

    public struct StanceChangedEvent
    {
    }

    public struct PassAttemptedEvent
    {
    }

    public struct DashAttemptedEvent
    {
    }

    public struct RodStunnedEvent
    {
        public Vector3 Position;
        public Gamepad Gamepad;
    }

    public struct BlockEvent
    {
        public Vector3 Position;
        public float VfxScale;
        public float BallVelocityX;
        public float BallSpeed;
        public Gamepad Gamepad;
    }

    public struct WallBounceEvent
    {
        public Vector3 Position;
        public float Volume;
        public float VfxScale;
    }

    public struct CountdownTickEvent
    {
    }

    public struct CountdownGoEvent
    {
    }

    public struct TableShakenEvent
    {
    }
}
