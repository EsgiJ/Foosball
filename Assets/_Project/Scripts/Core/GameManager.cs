using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

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
        [SerializeField] private float m_GoalWaitDuration = 1f;
        [SerializeField] private TextMeshPro m_CountdownText;
        [SerializeField] private float m_BallStartingNudge = 0.5f;
        private Vector3 m_CountdownOriginalScale;

        private Tween m_KickoffTween;

        [Header("UI")]
        [SerializeField] private TextMeshPro m_HomeTeamScoreUI;
        [SerializeField] private TextMeshPro m_AwayTeamScoreUI;
        [SerializeField] private TextMeshPro m_ScoreDashUI;

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
            }
            else
            {
                Destroy(gameObject);
            }

            InitializeTeams();
        }

        void Start()
        {
            FindTheBall();
            SubscribeToEvents();
            if (m_CountdownText != null)
                m_CountdownOriginalScale = m_CountdownText.transform.localScale;
        }

        void Update()
        {
            
        }
    #endregion

#region Initialization
        private void InitializeTeams()
        {
            m_HomeTeam.SetIsHomeTeam(true);
            m_AwayTeam.SetIsHomeTeam(false);
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
            GameStateManager.Instance.OnStateChanged += HandleStateChanged;
        }
#endregion
        public void HandleGoal(bool isHome)
        {
            if(GameStateManager.Instance.CurrentState != GameState.Playing)
            {
                Debug.LogWarning("[GameManager] Goal scored while not in Playing state, ignoring.");
                return;
            }

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

            /* Update the scoreboard with punch effect */
            var scoreText = isHome ? m_HomeScoreText : m_AwayScoreText;
            scoreText.transform.DOKill();
            scoreText.transform.localScale = Vector3.one;
            scoreText.transform.DOPunchScale(Vector3.one * 0.5f, 0.4f, 8, 0.7f);
            Debug.Log($"[GameManager] {m_HomeTeam.teamName} {m_HomeTeam.score} - {m_AwayTeam.teamName} {m_AwayTeam.score}");

            GameStateManager.Instance.GoToGoal();
            DOVirtual.DelayedCall(m_GoalWaitDuration,() => GameStateManager.Instance.StartCountdown()).SetUpdate(true);
        }

        public void StartKickoffSequence()
        {
            m_KickoffTween?.Kill();
            if (m_BallController != null)
                m_BallController.ResetBall();
            
            // SetUpdate(true) to not get affected by the slow mo 
            DG.Tweening.Sequence seq = DOTween.Sequence().SetUpdate(true);

            seq.AppendCallback(() =>
            {
                m_HomeTeamScoreUI.gameObject.SetActive(true);
                m_AwayTeamScoreUI.gameObject.SetActive(true);
                m_ScoreDashUI.gameObject.SetActive(true);
            });

            seq.AppendCallback(() =>
            {
                if (m_BallController != null)
                {
                    m_BallController.transform.DOMove(Vector3.zero, 0.5f)
                        .SetEase(Ease.InOutQuad)
                        .SetUpdate(true);
                }
            });

            int[] numbers = {3, 2, 1};

            for(int i = 0; i < numbers.Length; i++)
            {
                int captured = i;
                seq.AppendCallback(() => ShowCountdownNumber(numbers[captured].ToString()));
                seq.JoinCallback(() => AudioManager.Instance?.PlayCountdownTick());
                seq.AppendInterval(1f);
            }
            
            seq.AppendCallback(() => ShowCountdownNumber("GO!", true));
            seq.JoinCallback(() => AudioManager.Instance?.PlayCountdownGo());

            seq.AppendInterval(0.4f);

            seq.AppendCallback(() =>
            {
                m_HomeTeamScoreUI.gameObject.SetActive(false);
                m_AwayTeamScoreUI.gameObject.SetActive(false);
                m_ScoreDashUI.gameObject.SetActive(false);
            });

            seq.AppendCallback(() =>
            {
                if (m_CountdownText != null) 
                {
                    m_CountdownText.gameObject.SetActive(false);
                }
                GameStateManager.Instance.StartPlaying();
            });

            m_KickoffTween = seq;
        }

        private void HandleStateChanged(GameState previous, GameState next)
        {
            switch (next)
            {
                case GameState.Countdown:
                    DisableInput();
                    BeginControl();
                    StartKickoffSequence();
                    break;
                case GameState.Playing:
                    EnableInput();
                    NudgeBall();
                    break;
                case GameState.Goal:
                    DisableInput();
                    break;
                case GameState.Setup:
                    GameJuiceManager.Instance?.VignetteEffect();
                    EndControl();
                    break;    
                case GameState.MainMenu:
                    DisableInput();
                    EndControl();
                    break;
            }
        }

        private void BeginControl()
        {
            m_HomeTeam.BeginControl();
            m_AwayTeam.BeginControl();
        }

        private void EndControl()
        {
            m_HomeTeam.EndControl();
            m_AwayTeam.EndControl();
        }

        private void NudgeBall()
        {
            if (m_BallController == null) 
                return;

            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            m_BallController.GetComponent<Rigidbody>().AddForce(randomDir * m_BallStartingNudge, ForceMode.Impulse);
        }

        private void ShowCountdownNumber(string countdownText, bool isFinal = false)
        {
            if(m_CountdownText  == null)
                return;

            m_CountdownText.gameObject.SetActive(true);
            m_CountdownText.text = countdownText;

            m_CountdownText.transform.DOKill();
            m_CountdownText.transform.localScale = m_CountdownOriginalScale * 0.3f;
            Vector3 targetScale = isFinal ? m_CountdownOriginalScale * 1.5f : m_CountdownOriginalScale;

            m_CountdownText.transform
                .DOScale(targetScale, 0.3f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);

            var color = m_CountdownText.color;
            color.a = 0f;
            m_CountdownText.color = color;
            m_CountdownText.DOFade(1f, 0.2f).SetUpdate(true);

            if (isFinal)
            {
                m_CountdownText.transform
                    .DOPunchRotation(new Vector3(0, 0, 10f), 0.4f, 8, 0.7f)
                    .SetUpdate(true);
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
    
            m_HomeTeamScoreUI.text = m_HomeTeam.score.ToString();
            m_AwayTeamScoreUI.text = m_AwayTeam.score.ToString();
        }

#region Input
    private void EnableInput()
    {
        m_HomeTeam.EnableInput();
        m_AwayTeam.EnableInput();
    }

    private void DisableInput()
    {
        m_HomeTeam.DisableInput();
        m_AwayTeam.DisableInput();
    }
#endregion

#region Getters
        public TeamController GetHomeTeam() => m_HomeTeam;
        public TeamController GetAwayTeam() => m_AwayTeam;
#endregion
    }
}