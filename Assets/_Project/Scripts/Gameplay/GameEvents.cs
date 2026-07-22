using UnityEngine;

namespace Foosball.Gameplay
{
    public struct ShootEvent
    {
        public Vector2 Direction;
    }

    public struct GoalEvent
    {
        public bool IsHome;
    }
}
