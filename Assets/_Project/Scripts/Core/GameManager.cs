using DG.Tweening;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Foosball
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;

        [Header("Teams")]
        [SerializeField] private TeamController m_HomeTeam;
        [SerializeField] private TeamController m_AwayTeam;

        [Header("Scoreboard")]
        [SerializeField] private TextMeshPro m_HomeScoreText;
        [SerializeField] private TextMeshPro m_AwayScoreText;

        [Header("Ball")]
        [SerializeField] private BallController m_BallController;

        [Header("Kickoff Propoerties")]
        [SerializeField] private float m_KickoffDuration = 3f; 
        Tween m_KickoffTween;

    #region Unity Lifecycle

        public static GameManager Instance
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
            instance = FindObjectsByType<GameManager>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)[0];
            if (instance == null)
            {
                GameObject gameObj = new GameObject();
                gameObj.name = "GameManager";
                instance = gameObj.AddComponent<GameManager>();
                DontDestroyOnLoad(gameObj);
            }
        }

        void Awake()
        {
            if(instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            InitializeTeams();
            FindTheBall();
            SubscribeToEvents();
        }

        void Update()
        {
            
        }
    #endregion

#region Initialization
        private void InitializeTeams()
        {
            ResetGame(); 
            Debug.Log($"[GameManager] Game started: {m_HomeTeam.teamName} {m_AwayTeam.teamName}");
        }

        private void FindTheBall()
        {
            if (m_BallController == null)
            {
                m_BallController = FindObjectsByType<BallController>(FindObjectsSortMode.InstanceID)[0];
            }
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnGoalEvent += HandleGoal;
        }
#endregion
        public void HandleGoal(bool isHome)
        {
            if(isHome)
            {
                m_HomeTeam.IncrementScore();
                Debug.Log($"[GameManager] Home Team({m_HomeTeam.teamName}) scored!");
            }
            else
            {
                m_AwayTeam.IncrementScore();
                Debug.Log($"[GameManager] Away Team({m_AwayTeam.teamName}) scored!");
            }

            GameJuiceManager.Instance?.ChromaticAberrationEffect();
            GameJuiceManager.Instance?.PauseGame(0.1f);          
            GameJuiceManager.Instance?.SlowMotion(0.3f, 0.6f);  
            GameJuiceManager.Instance?.ShakeCamera(0.5f, 0.6f);
            GameJuiceManager.Instance?.VignetteEffect();

            UpdateScoreboard();

            /* update the scoreboard with punch effect */
            var scoreText = isHome ? m_HomeScoreText : m_AwayScoreText;
            scoreText.transform.DOKill();
            scoreText.transform.localScale = Vector3.one;
            scoreText.transform.DOPunchScale(Vector3.one * 0.5f, 0.4f, 8, 0.7f);
            Debug.Log($"[GameManager] {m_HomeTeam.teamName} {m_HomeTeam.score} - {m_AwayTeam.teamName} {m_AwayTeam.score}");
        }

        public void PrepareForKickoff()
        {
            Debug.Log("[GameManager] Preparing for kickoff...");
            
            if (m_BallController != null)
            {
                m_BallController.transform.position = Vector3.zero;
                m_BallController.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                m_BallController.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }
        }

        private void ResetGame()
        {
            m_HomeTeam.ResetScore();
            m_AwayTeam.ResetScore();

            UpdateScoreboard();

            Debug.Log($"[GameManager] Game reset: {m_HomeTeam.teamName} {m_AwayTeam.teamName}");
        }

        private void UpdateScoreboard()
        {
            m_HomeScoreText.text = m_HomeTeam.score.ToString();
            m_AwayScoreText.text = m_AwayTeam.score.ToString();
        }

    #region Getters
        public TeamController GetHomeTeam() => m_HomeTeam;
        public TeamController GetAwayTeam() => m_AwayTeam;
    #endregion
    }
}