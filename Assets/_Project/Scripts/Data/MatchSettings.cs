using UnityEngine;
using UnityEngine.Serialization;

namespace Foosball.Data
{
    public enum ServeRule
    {
        Random,
        LoserServes,
        AlternateEachGoal
    }

    [CreateAssetMenu(fileName = "MatchSettings", menuName = "Foosball/Match Settings")]
    public class MatchSettings : ScriptableObject
    {
        [Header("Kickoff Timing")]
        [SerializeField, FormerlySerializedAs("GoalWaitDuration"), Min(0f)] private float m_GoalWaitDuration = 1f;
        public float GoalWaitDuration => m_GoalWaitDuration;
        [SerializeField, FormerlySerializedAs("CountdownTickInterval"), Min(0f)] private float m_CountdownTickInterval = 1f;
        public float CountdownTickInterval => m_CountdownTickInterval;
        [SerializeField, FormerlySerializedAs("CountdownGoHoldDuration"), Min(0f)] private float m_CountdownGoHoldDuration = 0.4f;
        public float CountdownGoHoldDuration => m_CountdownGoHoldDuration;
        [SerializeField, FormerlySerializedAs("KickoffBallMoveDuration"), Min(0f)] private float m_KickoffBallMoveDuration = 0.5f;
        public float KickoffBallMoveDuration => m_KickoffBallMoveDuration;
        [SerializeField, FormerlySerializedAs("StartingNudgeForce"), Min(0f)] private float m_StartingNudgeForce = 0.5f;
        public float StartingNudgeForce => m_StartingNudgeForce;

        [Header("Match Rules")]
        [Tooltip("Not yet wired to gameplay - reserved for a future match-rules stage.")]
        [SerializeField, FormerlySerializedAs("WinScore"), Min(1)] private int m_WinScore = 5;
        public int WinScore => m_WinScore;
        [Tooltip("Not yet wired to gameplay - reserved for a future match-rules stage.")]
        [SerializeField, FormerlySerializedAs("TimeLimitSeconds"), Min(0f)] private float m_TimeLimitSeconds = 300f;
        public float TimeLimitSeconds => m_TimeLimitSeconds;
        [Tooltip("Not yet wired to gameplay - reserved for a future match-rules stage.")]
        [SerializeField, FormerlySerializedAs("BallCount"), Min(1)] private int m_BallCount = 1;
        public int BallCount => m_BallCount;
        [Tooltip("Not yet wired to gameplay - reserved for a future match-rules stage.")]
        [SerializeField, FormerlySerializedAs("ServeRule")] private ServeRule m_ServeRule = ServeRule.Random;
        public ServeRule ServeRule => m_ServeRule;
    }
}
