using UnityEngine;
using System;

public static class GameEvents
{
    /* Events */
    public static event Action<Vector3> OnBallImpact;
    public static event Action<Vector2> OnShootEvent;
    public static event Action<bool>    OnGoalEvent;
    /* Raise Helpers */
    public static void RaiseBallImpact(Vector3 vector3) =>  OnBallImpact?.Invoke(vector3);
    public static void RaiseShootEvent(Vector2 vector2) =>  OnShootEvent?.Invoke(vector2);
    public static void RaiseGoalEvent(bool isHome) =>       OnGoalEvent?.Invoke(isHome);         // Whether the one who scores is home or away

    /* Clear all subscribers */
    public static void ClearAll()
    {
        OnBallImpact = null;
        OnShootEvent = null;
        OnGoalEvent = null;
    }
}
