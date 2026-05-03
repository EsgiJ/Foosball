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

        [Header("Ball Hit Clips")]
        [SerializeField] private AudioClip m_BallHitClip; 

        [Header("Rod Clips")]
        [SerializeField] private AudioClip m_RodSwingClip;
        [SerializeField] private AudioClip m_StanceClickClip;
        [SerializeField] private AudioClip m_PossessSwitchClip;

        [Header("General Game Clips")]
        [SerializeField] private AudioClip m_GoalClip;
        [SerializeField] private AudioClip m_StunClip;
        [SerializeField] private AudioClip m_CountdownTickClip;
        [SerializeField] private AudioClip m_CountdownGoClip;

        [Header("Ambient Clips")]
        [SerializeField] private AudioClip m_CrowdAmbientClip;
        [SerializeField] private AudioSource m_MusicSource;

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
                }
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
        }

        public void PlayStanceClick()
        {
            PlayOneShot(m_StanceClickClip, 0.5f, 0.95f, 1.05f);
        }

        public void PlayPossessSwitch()
        {
            PlayOneShot(m_PossessSwitchClip, 0.4f, 0.95f, 1.05f);
        }

        public void PlayGoal()
        {
            PlayOneShot(m_GoalClip, 1f, 1f, 1f);

            if (m_MusicSource != null)
            {
                DOTween.Sequence()
                    .Append(DOTween.To(() => m_MusicSource.volume, x => m_MusicSource.volume = x, 0.2f, 0.1f))
                    .AppendInterval(1.5f)
                    .Append(DOTween.To(() => m_MusicSource.volume, x => m_MusicSource.volume = x, 1f, 0.5f))
                    .SetUpdate(true);
            }
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
#endregion
    }
}