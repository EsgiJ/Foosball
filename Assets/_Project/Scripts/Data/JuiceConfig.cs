using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "JuiceConfig", menuName = "Foosball/Juice Config")]
public class JuiceConfig : ScriptableObject
{
    [Header("Pause")]
    [SerializeField, FormerlySerializedAs("DefaultPauseDuration"), Min(0f)] private float m_DefaultPauseDuration = 1.0f;
    public float DefaultPauseDuration => m_DefaultPauseDuration;

    [Header("Slow Motion")]
    [SerializeField, FormerlySerializedAs("SlowMoScale"), Range(0f, 1f)] private float m_SlowMoScale = 0.25f;
    public float SlowMoScale => m_SlowMoScale;
    [SerializeField, FormerlySerializedAs("SlowMoEaseIn"), Min(0f)] private float m_SlowMoEaseIn = 0.1f;
    public float SlowMoEaseIn => m_SlowMoEaseIn;
    [SerializeField, FormerlySerializedAs("SlowMoHold"), Min(0f)] private float m_SlowMoHold = 0.4f;
    public float SlowMoHold => m_SlowMoHold;
    [SerializeField, FormerlySerializedAs("SlowMoEaseOut"), Min(0f)] private float m_SlowMoEaseOut = 0.5f;
    public float SlowMoEaseOut => m_SlowMoEaseOut;

    [Header("Screen Shake")]
    [SerializeField, FormerlySerializedAs("DefaultShakeDuration"), Min(0f)] private float m_DefaultShakeDuration = 0.2f;
    public float DefaultShakeDuration => m_DefaultShakeDuration;
    [SerializeField, FormerlySerializedAs("DefaultShakeStrength"), Min(0f)] private float m_DefaultShakeStrength = 0.3f;
    public float DefaultShakeStrength => m_DefaultShakeStrength;
    [SerializeField, FormerlySerializedAs("DefaultShakeVibrato"), Min(0)] private int m_DefaultShakeVibrato = 12;
    public int DefaultShakeVibrato => m_DefaultShakeVibrato;
    [SerializeField, FormerlySerializedAs("ShakeRandomness"), Min(0f)] private float m_ShakeRandomness = 90f;
    public float ShakeRandomness => m_ShakeRandomness;
    [SerializeField, FormerlySerializedAs("ShakeSnapping")] private bool m_ShakeSnapping = false;
    public bool ShakeSnapping => m_ShakeSnapping;
    [SerializeField, FormerlySerializedAs("ShakeFadeOut")] private bool m_ShakeFadeOut = true;
    public bool ShakeFadeOut => m_ShakeFadeOut;

    [Header("Chromatic Aberration")]
    [SerializeField, FormerlySerializedAs("DefaultChromaticAberrationIntensity"), Range(0f, 1f)] private float m_DefaultChromaticAberrationIntensity = 0.5f;
    public float DefaultChromaticAberrationIntensity => m_DefaultChromaticAberrationIntensity;
    [SerializeField, FormerlySerializedAs("TargetChromaticAberrationIntensity"), Range(0f, 1f)] private float m_TargetChromaticAberrationIntensity = 1f;
    public float TargetChromaticAberrationIntensity => m_TargetChromaticAberrationIntensity;
    [SerializeField, FormerlySerializedAs("ChromaticAberrationEaseIn"), Min(0f)] private float m_ChromaticAberrationEaseIn = 0.1f;
    public float ChromaticAberrationEaseIn => m_ChromaticAberrationEaseIn;
    [SerializeField, FormerlySerializedAs("ChromaticAberrationHold"), Min(0f)] private float m_ChromaticAberrationHold = 0.4f;
    public float ChromaticAberrationHold => m_ChromaticAberrationHold;
    [SerializeField, FormerlySerializedAs("ChromaticAberrationEaseOut"), Min(0f)] private float m_ChromaticAberrationEaseOut = 0.5f;
    public float ChromaticAberrationEaseOut => m_ChromaticAberrationEaseOut;

    [Header("Vignette")]
    [SerializeField, FormerlySerializedAs("DefaultVignetteIntensity"), Range(0f, 1f)] private float m_DefaultVignetteIntensity = 0.6f;
    public float DefaultVignetteIntensity => m_DefaultVignetteIntensity;
    [SerializeField, FormerlySerializedAs("TargetVignetteIntensity"), Range(0f, 1f)] private float m_TargetVignetteIntensity = 1f;
    public float TargetVignetteIntensity => m_TargetVignetteIntensity;
    [SerializeField, FormerlySerializedAs("DefaultVignetteSmoothness"), Range(0f, 1f)] private float m_DefaultVignetteSmoothness = 0.2f;
    public float DefaultVignetteSmoothness => m_DefaultVignetteSmoothness;
    [SerializeField, FormerlySerializedAs("TargetVignetteSmoothness"), Range(0f, 1f)] private float m_TargetVignetteSmoothness = 1f;
    public float TargetVignetteSmoothness => m_TargetVignetteSmoothness;
    [SerializeField, FormerlySerializedAs("VignetteEaseIn"), Min(0f)] private float m_VignetteEaseIn = 0.1f;
    public float VignetteEaseIn => m_VignetteEaseIn;
    [SerializeField, FormerlySerializedAs("VignetteHold"), Min(0f)] private float m_VignetteHold = 0.4f;
    public float VignetteHold => m_VignetteHold;
    [SerializeField, FormerlySerializedAs("VignetteEaseOut"), Min(0f)] private float m_VignetteEaseOut = 0.5f;
    public float VignetteEaseOut => m_VignetteEaseOut;
    [SerializeField, FormerlySerializedAs("VignetteRestoreDuration"), Min(0f)] private float m_VignetteRestoreDuration = 0.3f;
    public float VignetteRestoreDuration => m_VignetteRestoreDuration;

    [Header("Goal Feedback")]
    [SerializeField, Min(0f)] private float m_GoalPauseDuration = 0.1f;
    public float GoalPauseDuration => m_GoalPauseDuration;
    [SerializeField, Range(0f, 1f)] private float m_GoalSlowMoScale = 0.3f;
    public float GoalSlowMoScale => m_GoalSlowMoScale;
    [SerializeField, Min(0f)] private float m_GoalSlowMoHold = 0.6f;
    public float GoalSlowMoHold => m_GoalSlowMoHold;
    [SerializeField, Min(0f)] private float m_GoalShakeDuration = 0.5f;
    public float GoalShakeDuration => m_GoalShakeDuration;
    [SerializeField, Min(0f)] private float m_GoalShakeStrength = 0.6f;
    public float GoalShakeStrength => m_GoalShakeStrength;

    [Header("Shoot Feedback")]
    [SerializeField, Min(0f)] private float m_ShootShakeDuration = 0.15f;
    public float ShootShakeDuration => m_ShootShakeDuration;
    [SerializeField, Min(0f)] private float m_ShootShakeStrength = 0.2f;
    public float ShootShakeStrength => m_ShootShakeStrength;

    [Header("Pass Feedback")]
    [SerializeField, Min(0f)] private float m_PassShakeDuration = 0.08f;
    public float PassShakeDuration => m_PassShakeDuration;
    [SerializeField, Min(0f)] private float m_PassShakeStrength = 0.1f;
    public float PassShakeStrength => m_PassShakeStrength;

    [Header("Stun Feedback")]
    [SerializeField, Min(0f)] private float m_StunShakeDuration = 0.3f;
    public float StunShakeDuration => m_StunShakeDuration;
    [SerializeField, Min(0f)] private float m_StunShakeStrength = 0.5f;
    public float StunShakeStrength => m_StunShakeStrength;
    [SerializeField, Min(0f)] private float m_StunPauseDuration = 0.08f;
    public float StunPauseDuration => m_StunPauseDuration;

    [Header("Block Feedback")]
    [SerializeField, Min(0f), Tooltip("Ball velocity X below this magnitude is treated as no meaningful block.")] private float m_BlockVelocityDeadzone = 0.1f;
    public float BlockVelocityDeadzone => m_BlockVelocityDeadzone;
    [SerializeField, Min(0.0001f), Tooltip("Ball speed is divided by this to normalize into a 0-1 intensity.")] private float m_BlockSpeedNormalizer = 30f;
    public float BlockSpeedNormalizer => m_BlockSpeedNormalizer;
    [SerializeField, Min(0f)] private float m_BlockShakeDuration = 0.15f;
    public float BlockShakeDuration => m_BlockShakeDuration;
    [SerializeField, Min(0f)] private float m_BlockShakeMaxStrength = 0.25f;
    public float BlockShakeMaxStrength => m_BlockShakeMaxStrength;
    [SerializeField, Range(0f, 1f), Tooltip("Normalized intensity above which a block also pauses the game briefly.")] private float m_BlockPauseThreshold = 0.6f;
    public float BlockPauseThreshold => m_BlockPauseThreshold;
    [SerializeField, Min(0f)] private float m_BlockPauseDuration = 0.1f;
    public float BlockPauseDuration => m_BlockPauseDuration;

    [Header("Table Feedback")]
    [SerializeField, Min(0f)] private float m_TableShakeDuration = 0.5f;
    public float TableShakeDuration => m_TableShakeDuration;
    [SerializeField, Min(0f)] private float m_TableShakeStrength = 0.5f;
    public float TableShakeStrength => m_TableShakeStrength;
}
