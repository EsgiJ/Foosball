using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Infrastructure;
using Foosball.Data;
using Foosball.Core;

namespace Foosball.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        [Header("Teams")]
        [SerializeField] private TeamController m_HomeTeam;
        [SerializeField] private TeamController m_AwayTeam;

        [Header("Scoreboard")]
        [SerializeField] private TextMeshPro m_HomeScoreText;
        [SerializeField] private TextMeshPro m_AwayScoreText;

        [Header("Ball")]
        [SerializeField] private BallController m_BallController;

        [Header("Match Settings")]
        [SerializeField] private MatchSettings m_MatchSettings;

        [Header("Kickoff Properties")]
        [SerializeField] private TextMeshPro m_CountdownText;
        private Vector3 m_CountdownOriginalScale;
        private Tween m_KickoffTween;
        private Tween m_KickoffBallMoveTween;
        private Tween m_GoalWaitTween;

        [Header("UI")]
        [SerializeField] private TextMeshPro m_HomeTeamScoreUI;
        [SerializeField] private TextMeshPro m_AwayTeamScoreUI;
        [SerializeField] private TextMeshPro m_ScoreDashUI;

        /* Injected dependencies */
        private GameStateManager m_GameStateManager;
        private EventBus m_EventBus;
        private MatchState m_MatchState;

    #region Unity Lifecycle
        public void Init(GameStateManager gameStateManager, EventBus eventBus)
        {
            m_GameStateManager = gameStateManager;
            m_EventBus = eventBus;
        }

        void Awake()
        {
            m_MatchState = new MatchState(m_MatchSettings);
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
            this.Subscribe<GoalEvent>(m_EventBus, HandleGoal);
            m_GameStateManager.OnStateChanged += HandleStateChanged;
        }
    #endregion

    #region Goal Handling
        public void HandleGoal(GoalEvent goalEvent)
        {
            bool isHome = goalEvent.IsHome;

            if(m_GameStateManager.CurrentState != GameState.Playing)
            {
                Debug.LogWarning("[GameManager] Goal scored while not in Playing state, ignoring.");
                return;
            }

            m_MatchState.RegisterGoal(isHome);

            if(isHome)
            {
                Debug.Log($"[GameManager] Home Team({m_HomeTeam.teamName}) scored!");
            }
            else
            {
                Debug.Log($"[GameManager] Away Team({m_AwayTeam.teamName}) scored!");
            }

            var scorer   = isHome ? m_HomeTeam.AssignedGamepad : m_AwayTeam.AssignedGamepad;
            var conceder = isHome ? m_AwayTeam.AssignedGamepad : m_HomeTeam.AssignedGamepad;
            m_EventBus?.Publish(new GoalFeedbackEvent { Scorer = scorer, Conceder = conceder });

            UpdateScoreboard();

            /* Update the scoreboard with punch effect */
            var scoreText = isHome ? m_HomeScoreText : m_AwayScoreText;
            scoreText.transform.DOKill();
            scoreText.transform.localScale = Vector3.one;
            scoreText.transform.DOPunchScale(Vector3.one * 0.5f, 0.4f, 8, 0.7f).SetLink(scoreText.gameObject);
            Debug.Log($"[GameManager] {m_HomeTeam.teamName} {m_MatchState.HomeScore} - {m_AwayTeam.teamName} {m_MatchState.AwayScore}");

            m_GameStateManager.GoToGoal();
            m_GoalWaitTween?.Kill();
            m_GoalWaitTween = DOVirtual.DelayedCall(m_MatchSettings.GoalWaitDuration, () => m_GameStateManager.StartCountdown())
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        private void ResetGame()
        {
            m_MatchState.Reset();

            UpdateScoreboard();

            Debug.Log($"[GameManager] Game reset: {m_HomeTeam.teamName} {m_AwayTeam.teamName}");
        }

        private void UpdateScoreboard()
        {
            m_HomeScoreText.text = m_MatchState.HomeScore.ToString();
            m_AwayScoreText.text = m_MatchState.AwayScore.ToString();

            m_HomeTeamScoreUI.text = m_MatchState.HomeScore.ToString();
            m_AwayTeamScoreUI.text = m_MatchState.AwayScore.ToString();
        }
    #endregion

    #region Kickoff Sequence
        public void StartKickoffSequence()
        {
            m_KickoffTween?.Kill();
            if (m_BallController != null)
                m_BallController.ResetBall();

            // SetUpdate(true) to not get affected by the slow mo
            DG.Tweening.Sequence seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

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
                    m_KickoffBallMoveTween?.Kill();
                    m_KickoffBallMoveTween = m_BallController.transform.DOMove(Vector3.zero, m_MatchSettings.KickoffBallMoveDuration)
                        .SetEase(Ease.InOutQuad)
                        .SetUpdate(true)
                        .SetLink(m_BallController.gameObject);
                }
            });

            int[] numbers = {3, 2, 1};

            for(int i = 0; i < numbers.Length; i++)
            {
                int captured = i;
                seq.AppendCallback(() => ShowCountdownNumber(numbers[captured].ToString()));
                seq.JoinCallback(() => m_EventBus?.Publish(new CountdownTickEvent()));
                seq.AppendInterval(m_MatchSettings.CountdownTickInterval);
            }

            seq.AppendCallback(() => ShowCountdownNumber("GO!", true));
            seq.JoinCallback(() => m_EventBus?.Publish(new CountdownGoEvent()));

            seq.AppendInterval(m_MatchSettings.CountdownGoHoldDuration);

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
                m_GameStateManager.StartPlaying();
            });

            m_KickoffTween = seq;
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
                .SetUpdate(true)
                .SetLink(m_CountdownText.gameObject);

            var color = m_CountdownText.color;
            color.a = 0f;
            m_CountdownText.color = color;
            m_CountdownText.DOKill();
            m_CountdownText.DOFade(1f, 0.2f).SetUpdate(true).SetLink(m_CountdownText.gameObject);

            if (isFinal)
            {
                m_CountdownText.transform
                    .DOPunchRotation(new Vector3(0, 0, 10f), 0.4f, 8, 0.7f)
                    .SetUpdate(true)
                    .SetLink(m_CountdownText.gameObject);
            }
        }
    #endregion

    #region State Change Handling
        private void HandleStateChanged(GameState previous, GameState next)
        {
            bool wasFrozen = previous == GameState.Paused || previous == GameState.Settings;
            bool isFrozen  = next == GameState.Paused || next == GameState.Settings;

            if (isFrozen)       Time.timeScale = 0f;
            else if (wasFrozen) Time.timeScale = 1f;

            switch (next)
            {
                case GameState.Countdown:
                    DisableInput();
                    BeginControl();
                    StartKickoffSequence();
                    break;

                case GameState.Playing:
                    EnableInput();
                    if (previous != GameState.Paused)
                        NudgeBall();
                    break;

                case GameState.Paused:
                    DisableInput();
                    Time.timeScale = 0f;
                    break;

                case GameState.Goal:
                    DisableInput();
                    break;

                case GameState.Setup:
                    EndControl();
                    break;

                case GameState.MainMenu:
                    DisableInput();
                    EndControl();
                    ResetGame();
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
            m_BallController.GetComponent<Rigidbody>().AddForce(randomDir * m_MatchSettings.StartingNudgeForce, ForceMode.Impulse);
        }
    #endregion

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
