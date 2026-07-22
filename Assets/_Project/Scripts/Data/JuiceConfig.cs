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
}
