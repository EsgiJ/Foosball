using System.Collections;
using DG.Tweening;
using UnityEngine;
using Foosball.Data;

namespace Foosball.Presentation
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private AudioConfig m_AudioConfig;

        [Header("Mixer")]
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup m_MusicGroup;
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup m_SfxGroup;

        [Header("Ambient Clips")]
        [SerializeField] private AudioSource m_MusicSource;

        /* Pool state */
        private AudioSource[] m_AudioSourcePool;
        private int m_PoolIndex = 0;

        /* Music state */
        private int m_TrackIndex = -1;
        private Coroutine m_PlaylistRoutine;
        private float m_MusicBaseVolume;
        private Tween m_DuckTween;

    #region Unity Lifecycle
        void Awake()
        {
            m_AudioSourcePool = new AudioSource[m_AudioConfig.PoolSize];
            for (int i = 0; i < m_AudioConfig.PoolSize; i++)
            {
                var go = new GameObject($"AudioPool_{i}");
                go.transform.SetParent(transform);
                m_AudioSourcePool[i] = go.AddComponent<AudioSource>();
                m_AudioSourcePool[i].playOnAwake = false;
                m_AudioSourcePool[i].outputAudioMixerGroup = m_SfxGroup;
            }

            if (m_MusicSource != null)
                m_MusicSource.outputAudioMixerGroup = m_MusicGroup;
        }
    #endregion

    #region Pool
        private AudioSource GetNextSource()
        {
            var source = m_AudioSourcePool[m_PoolIndex];
            m_PoolIndex = (m_PoolIndex + 1) % m_AudioConfig.PoolSize;
            return source;
        }

        public void PlayOneShot(AudioClip clip, float volume = 1f, float pitchMin = 0.95f, float pitchMax = 1.05f)
        {
            if (clip == null)
                return;

            var source = GetNextSource();
            source.pitch = Random.Range(pitchMin, pitchMax);
            source.volume = volume * Random.Range(m_AudioConfig.GlobalVolumeJitterMin, m_AudioConfig.GlobalVolumeJitterMax);
            source.PlayOneShot(clip);
        }

        private void PlayCue(SfxCue cue, float volumeScale = 1f)
        {
            PlayOneShot(cue.Clip, cue.Volume * volumeScale, cue.PitchMin, cue.PitchMax);
        }
    #endregion

    #region SFX
        public void PlayShoot()
        {
            PlayCue(m_AudioConfig.RodSwing);
            PlayCue(m_AudioConfig.BallHit);
        }

        public void PlayStanceClick()
        {
            PlayCue(m_AudioConfig.StanceClick);
        }

        public void PlayMenuButtonClick()
        {
            PlayCue(m_AudioConfig.MenuButton);
        }

        public void PlayToggleReady()
        {
            PlayCue(m_AudioConfig.ToggleReady);
        }

        public void PlaySwapSides()
        {
            PlayCue(m_AudioConfig.SwapSides);
        }

        public void PlayPossessSwitch()
        {
            PlayCue(m_AudioConfig.PossessSwitch);
        }

        public void PlayGoal()
        {
            PlayNetHit();
            DOVirtual.DelayedCall(m_AudioConfig.GoalCrowdCelebrationDelay, PlayCrowdCelebration).SetUpdate(true);
            if (m_AudioConfig.Goal.Clip != null)
            {
                PlayCue(m_AudioConfig.Goal);
            }

            DuckMusic();
        }

        public void PlayStun()
        {
            PlayCue(m_AudioConfig.Stun);
        }

        public void PlayCountdownTick()
        {
            PlayCue(m_AudioConfig.CountdownTick);
        }

        public void PlayCountdownGo()
        {
            PlayCue(m_AudioConfig.CountdownGo);
        }

        public void PlayNetHit()          => PlayCue(m_AudioConfig.NetHit);
        public void PlayCrowdCelebration() => PlayCue(m_AudioConfig.CrowdCelebration);
        public void PlayDefenseCatch()     => PlayCue(m_AudioConfig.DefenseCatch);
        public void PlayWallBounce(float volumeScale = 1f) => PlayCue(m_AudioConfig.WallBounce, Mathf.Clamp01(volumeScale));
    #endregion

    #region Music
        public void PlayMenuMusic()
        {
            if (m_MusicSource != null && m_MusicSource.clip == m_AudioConfig.MenuMusic && m_MusicSource.isPlaying)
                return;

            StopPlaylist();
            PlayMusicClip(m_AudioConfig.MenuMusic, true);
        }

        public void PlayGameplayMusic()
        {
            if (m_AudioConfig.GameplayTracks == null || m_AudioConfig.GameplayTracks.Length == 0)
                return;

            StopPlaylist();
            m_TrackIndex = -1;
            m_PlaylistRoutine = StartCoroutine(PlaylistLoop());
        }

        private IEnumerator PlaylistLoop()
        {
            while (true)
            {
                m_TrackIndex = (m_TrackIndex + 1) % m_AudioConfig.GameplayTracks.Length;
                var clip = m_AudioConfig.GameplayTracks[m_TrackIndex];

                if (clip == null)
                {
                    yield return null;
                    continue;
                }

                PlayMusicClip(clip, false);
                yield return new WaitForSecondsRealtime(clip.length);
            }
        }

        private void PlayMusicClip(AudioClip clip, bool loop)
        {
            if (m_MusicSource == null || clip == null)
                return;
            m_DuckTween?.Kill();
            m_MusicSource.clip = clip;
            m_MusicSource.loop = loop;
            m_MusicSource.volume = 0f;
            m_MusicSource.Play();
            m_MusicBaseVolume = m_AudioConfig.MusicVolume;
            DOTween.To(() => m_MusicSource.volume, v => m_MusicSource.volume = v, m_AudioConfig.MusicVolume, m_AudioConfig.MusicFadeDuration).SetUpdate(true);
        }

        private void StopPlaylist()
        {
            if (m_PlaylistRoutine != null)
            {
                StopCoroutine(m_PlaylistRoutine);
                m_PlaylistRoutine = null;
            }
        }

        public void SetMusicPaused(bool paused)
        {
            if (m_MusicSource == null)
                return;
            if (paused)
                m_MusicSource.Pause();
            else
                m_MusicSource.UnPause();
        }

        public void DuckMusic()
        {
            DuckMusic(m_AudioConfig.GoalDuckVolume, m_AudioConfig.GoalDuckHold, m_AudioConfig.GoalDuckFadeIn, m_AudioConfig.GoalDuckFadeOut);
        }

        public void DuckMusic(float ducked, float hold, float fadeIn, float fadeOut)
        {
            if (m_MusicSource == null)
                return;

            m_DuckTween?.Kill();
            m_DuckTween = DOTween.Sequence()
                .Append(DOTween.To(() => m_MusicSource.volume, v => m_MusicSource.volume = v, ducked, fadeIn))
                .AppendInterval(hold)
                .Append(DOTween.To(() => m_MusicSource.volume, v => m_MusicSource.volume = v, m_MusicBaseVolume, fadeOut))
                .SetUpdate(true);
        }
    #endregion
    }
}
