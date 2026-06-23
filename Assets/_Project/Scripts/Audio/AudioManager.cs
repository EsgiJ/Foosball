using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

namespace Foosball
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Pools")]
        [SerializeField] private int m_PoolSize = 10;
        private AudioSource[] m_AudioSourcePool;
        private int m_PoolIndex = 0;

        [Header("Mixer")]
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup m_MusicGroup;
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup m_SfxGroup;

        [Header("Ball Hit Clips")]
        [SerializeField] private AudioClip m_BallHitClip; 

        [Header("Rod Clips")]
        [SerializeField] private AudioClip m_RodSwingClip;
        [SerializeField] private AudioClip m_StanceClickClip;
        [SerializeField] private AudioClip m_PossessSwitchClip;

        [Header("General Game Clips")]
        [SerializeField] private AudioClip m_NetHitClip;         
        [SerializeField] private AudioClip m_CrowdCelebrationClip;
        [SerializeField] private AudioClip m_GoalClip;
        [SerializeField] private AudioClip m_StunClip;
        [SerializeField] private AudioClip m_CountdownTickClip;
        [SerializeField] private AudioClip m_CountdownGoClip;
        [SerializeField] private AudioClip m_MenuButtonClip;
        [SerializeField] private AudioClip m_ToggleReadyClip;
        [SerializeField] private AudioClip m_SwapSidesClip;
        [SerializeField] private AudioClip m_DefenseCatchClip;   
        [SerializeField] private AudioClip m_WallBounceClip;

        [Header("Ambient Clips")]
        [SerializeField] private AudioSource m_MusicSource;

        [Header("Music")]
        [SerializeField] private AudioClip m_MenuMusic;
        [SerializeField] private AudioClip[] m_GameplayTracks;
        [SerializeField] private float m_MusicVolume = 0.6f;
        [SerializeField] private float m_MusicFadeDuration = 1f;

        private int m_TrackIndex = -1;
        private Coroutine m_PlaylistRoutine;
        private float m_MusicBaseVolume;
        private Tween m_DuckTween;

        private static AudioManager instance;

    #region Unity Lifecycle

            public static AudioManager Instance
            {
                get
                {
                    if(instance == null)
                    {
                        SetupInstance();
                    }
                    return instance;
                }
            }

            private static void SetupInstance()
            {
                instance = FindObjectsByType<AudioManager>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)[0];
                if (instance == null)
                {
                    GameObject gameObj = new GameObject();
                    gameObj.name = "AudioManager";
                    instance = gameObj.AddComponent<AudioManager>();
                    DontDestroyOnLoad(gameObj);
                }
            }

            void Awake()
            {
                if(instance == null)
                {
                    instance = this;
                }
                else
                {
                    Destroy(gameObject);
                }

                m_AudioSourcePool = new AudioSource[m_PoolSize];
                for (int i = 0; i < m_PoolSize; i++)
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

            void Start()
            {
            }

        #endregion

        private AudioSource GetNextSource()
        {
            var source = m_AudioSourcePool[m_PoolIndex];
            m_PoolIndex = (m_PoolIndex + 1) % m_PoolSize;
            return source;
        }

#region General Purpose
        public void PlayOneShot(AudioClip clip, float volume = 1f, float pitchMin = 0.95f, float pitchMax = 1.05f)
        {
            if (clip == null) 
                return;

            var source = GetNextSource();
            source.pitch = Random.Range(pitchMin, pitchMax);
            source.volume = volume * Random.Range(0.9f, 1f);
            source.PlayOneShot(clip);
        }
#endregion

#region Game Spesific Methods
        public void PlayShoot()
        {
            PlayOneShot(m_RodSwingClip, 0.8f, 0.95f, 1.05f);
            PlayOneShot(m_BallHitClip, 0.8f, 0.95f, 1.05f);
        }

        public void PlayStanceClick()
        {
            PlayOneShot(m_StanceClickClip, 0.5f, 0.95f, 1.05f);
        }

        public void PlayMenuButtonClick()
        {
            PlayOneShot(m_MenuButtonClip, 0.5f, 0.95f, 1.05f);
        }
    
        public void PlayToggleReady()
        {
            PlayOneShot(m_ToggleReadyClip, 0.5f, 0.95f, 1.05f);
        }

        public void PlaySwapSides()
        {
            PlayOneShot(m_SwapSidesClip, 0.5f, 0.95f, 1.05f);
        }

        public void PlayPossessSwitch()
        {
            PlayOneShot(m_PossessSwitchClip, 0.4f, 0.95f, 1.05f);
        }

        public void PlayGoal()
        {
            PlayNetHit();                                                    
            DOVirtual.DelayedCall(0.15f, PlayCrowdCelebration).SetUpdate(true);
            if (m_GoalClip != null) 
            {
                PlayOneShot(m_GoalClip, 0.5f);          
            }
            
            DuckMusic();  
        }

        public void PlayStun()
        {
            PlayOneShot(m_StunClip, 0.7f, 0.9f, 1.1f);
        }

        public void PlayCountdownTick()
        {
            PlayOneShot(m_CountdownTickClip, 0.6f, 1f, 1f);
        }

        public void PlayCountdownGo()
        {
            PlayOneShot(m_CountdownGoClip, 1f, 1f, 1f);
        }

        public void PlayMenuMusic()
        {
            if (m_MusicSource != null && m_MusicSource.clip == m_MenuMusic && m_MusicSource.isPlaying)
                return;                          

            StopPlaylist();
            PlayMusicClip(m_MenuMusic, true);
        }

        public void PlayGameplayMusic()
        {
            if (m_GameplayTracks == null || m_GameplayTracks.Length == 0) 
                return;
                
            StopPlaylist();
            m_TrackIndex = -1;
            m_PlaylistRoutine = StartCoroutine(PlaylistLoop());
        }

        private IEnumerator PlaylistLoop()
        {
            while (true)
            {
                m_TrackIndex = (m_TrackIndex + 1) % m_GameplayTracks.Length;
                var clip = m_GameplayTracks[m_TrackIndex];

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
            m_MusicBaseVolume = m_MusicVolume;
            DOTween.To(() => m_MusicSource.volume, v => m_MusicSource.volume = v, m_MusicVolume, m_MusicFadeDuration).SetUpdate(true);
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

        public void DuckMusic(float ducked = 0.2f, float hold = 1.5f, float fadeIn = 0.1f, float fadeOut = 0.5f)
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

        public void PlayNetHit()          => PlayOneShot(m_NetHitClip, 0.9f, 0.97f, 1.03f);
        public void PlayCrowdCelebration() => PlayOneShot(m_CrowdCelebrationClip, 0.85f, 0.98f, 1.02f);

        public void PlayDefenseCatch() => PlayOneShot(m_DefenseCatchClip, 0.7f, 0.95f, 1.05f);
        public void PlayWallBounce(float volumeScale = 1f) => PlayOneShot(m_WallBounceClip, 0.6f * Mathf.Clamp01(volumeScale), 0.9f, 1.1f);
#endregion
    }
}