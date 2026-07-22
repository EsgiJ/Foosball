using UnityEngine;
using UnityEngine.Serialization;

namespace Foosball.Data
{
    [System.Serializable]
    public class SfxCue
    {
        [SerializeField, FormerlySerializedAs("Clip")] private AudioClip m_Clip;
        public AudioClip Clip => m_Clip;

        [SerializeField, FormerlySerializedAs("Volume"), Min(0f)] private float m_Volume = 1f;
        public float Volume => m_Volume;

        [SerializeField, FormerlySerializedAs("PitchMin"), Range(0f, 2f)] private float m_PitchMin = 0.95f;
        public float PitchMin => m_PitchMin;

        [SerializeField, FormerlySerializedAs("PitchMax"), Range(0f, 2f)] private float m_PitchMax = 1.05f;
        public float PitchMax => m_PitchMax;

        public SfxCue() { }

        public SfxCue(float volume, float pitchMin, float pitchMax)
        {
            m_Volume = volume;
            m_PitchMin = pitchMin;
            m_PitchMax = pitchMax;
        }
    }

    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Foosball/Audio Config")]
    public class AudioConfig : ScriptableObject
    {
        [Header("Pool")]
        [SerializeField, FormerlySerializedAs("PoolSize"), Min(1)] private int m_PoolSize = 10;
        public int PoolSize => m_PoolSize;
        [Tooltip("Extra per-shot volume jitter applied on top of each cue's own volume.")]
        [SerializeField, FormerlySerializedAs("GlobalVolumeJitterMin"), Range(0f, 1f)] private float m_GlobalVolumeJitterMin = 0.9f;
        public float GlobalVolumeJitterMin => m_GlobalVolumeJitterMin;
        [SerializeField, FormerlySerializedAs("GlobalVolumeJitterMax"), Range(0f, 1f)] private float m_GlobalVolumeJitterMax = 1f;
        public float GlobalVolumeJitterMax => m_GlobalVolumeJitterMax;

        [Header("Ball Hit")]
        [SerializeField, FormerlySerializedAs("BallHit")] private SfxCue m_BallHit = new SfxCue(0.8f, 0.95f, 1.05f);
        public SfxCue BallHit => m_BallHit;

        [Header("Rod")]
        [SerializeField, FormerlySerializedAs("RodSwing")] private SfxCue m_RodSwing = new SfxCue(0.8f, 0.95f, 1.05f);
        public SfxCue RodSwing => m_RodSwing;
        [SerializeField, FormerlySerializedAs("StanceClick")] private SfxCue m_StanceClick = new SfxCue(0.5f, 0.95f, 1.05f);
        public SfxCue StanceClick => m_StanceClick;
        [SerializeField, FormerlySerializedAs("PossessSwitch")] private SfxCue m_PossessSwitch = new SfxCue(0.4f, 0.95f, 1.05f);
        public SfxCue PossessSwitch => m_PossessSwitch;

        [Header("General Game")]
        [SerializeField, FormerlySerializedAs("NetHit")] private SfxCue m_NetHit = new SfxCue(0.9f, 0.97f, 1.03f);
        public SfxCue NetHit => m_NetHit;
        [SerializeField, FormerlySerializedAs("CrowdCelebration")] private SfxCue m_CrowdCelebration = new SfxCue(0.85f, 0.98f, 1.02f);
        public SfxCue CrowdCelebration => m_CrowdCelebration;
        [SerializeField, FormerlySerializedAs("Goal")] private SfxCue m_Goal = new SfxCue(0.5f, 0.95f, 1.05f);
        public SfxCue Goal => m_Goal;
        [SerializeField, FormerlySerializedAs("Stun")] private SfxCue m_Stun = new SfxCue(0.7f, 0.9f, 1.1f);
        public SfxCue Stun => m_Stun;
        [SerializeField, FormerlySerializedAs("CountdownTick")] private SfxCue m_CountdownTick = new SfxCue(0.6f, 1f, 1f);
        public SfxCue CountdownTick => m_CountdownTick;
        [SerializeField, FormerlySerializedAs("CountdownGo")] private SfxCue m_CountdownGo = new SfxCue(1f, 1f, 1f);
        public SfxCue CountdownGo => m_CountdownGo;
        [SerializeField, FormerlySerializedAs("MenuButton")] private SfxCue m_MenuButton = new SfxCue(0.5f, 0.95f, 1.05f);
        public SfxCue MenuButton => m_MenuButton;
        [SerializeField, FormerlySerializedAs("ToggleReady")] private SfxCue m_ToggleReady = new SfxCue(0.5f, 0.95f, 1.05f);
        public SfxCue ToggleReady => m_ToggleReady;
        [SerializeField, FormerlySerializedAs("SwapSides")] private SfxCue m_SwapSides = new SfxCue(0.5f, 0.95f, 1.05f);
        public SfxCue SwapSides => m_SwapSides;
        [SerializeField, FormerlySerializedAs("DefenseCatch")] private SfxCue m_DefenseCatch = new SfxCue(0.7f, 0.95f, 1.05f);
        public SfxCue DefenseCatch => m_DefenseCatch;
        [Tooltip("Volume is multiplied by the impact-scaled volume passed in at runtime.")]
        [SerializeField, FormerlySerializedAs("WallBounce")] private SfxCue m_WallBounce = new SfxCue(0.6f, 0.9f, 1.1f);
        public SfxCue WallBounce => m_WallBounce;

        [Header("Goal Event Timing")]
        [SerializeField, FormerlySerializedAs("GoalCrowdCelebrationDelay"), Min(0f)] private float m_GoalCrowdCelebrationDelay = 0.15f;
        public float GoalCrowdCelebrationDelay => m_GoalCrowdCelebrationDelay;
        [SerializeField, FormerlySerializedAs("GoalDuckVolume"), Min(0f)] private float m_GoalDuckVolume = 0.2f;
        public float GoalDuckVolume => m_GoalDuckVolume;
        [SerializeField, FormerlySerializedAs("GoalDuckHold"), Min(0f)] private float m_GoalDuckHold = 1.5f;
        public float GoalDuckHold => m_GoalDuckHold;
        [SerializeField, FormerlySerializedAs("GoalDuckFadeIn"), Min(0f)] private float m_GoalDuckFadeIn = 0.1f;
        public float GoalDuckFadeIn => m_GoalDuckFadeIn;
        [SerializeField, FormerlySerializedAs("GoalDuckFadeOut"), Min(0f)] private float m_GoalDuckFadeOut = 0.5f;
        public float GoalDuckFadeOut => m_GoalDuckFadeOut;

        [Header("Music")]
        [SerializeField, FormerlySerializedAs("MenuMusic")] private AudioClip m_MenuMusic;
        public AudioClip MenuMusic => m_MenuMusic;
        [SerializeField, FormerlySerializedAs("GameplayTracks")] private AudioClip[] m_GameplayTracks;
        public AudioClip[] GameplayTracks => m_GameplayTracks;
        [SerializeField, FormerlySerializedAs("MusicVolume"), Range(0f, 1f)] private float m_MusicVolume = 0.6f;
        public float MusicVolume => m_MusicVolume;
        [SerializeField, FormerlySerializedAs("MusicFadeDuration"), Min(0f)] private float m_MusicFadeDuration = 1f;
        public float MusicFadeDuration => m_MusicFadeDuration;
    }
}
