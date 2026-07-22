using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Foosball.Gameplay;

namespace Foosball.Presentation
{
    public class PauseController : MonoBehaviour
    {
        [Header("Pause Panel")]
        [SerializeField] private GameObject m_PausePanel;
        [SerializeField] private Button m_ResumeButton;
        [SerializeField] private Button m_SettingsButton;
        [SerializeField] private Button m_MainMenuButton;
        [SerializeField] private Button m_QuitButton;

        [Header("Settings")]
        [SerializeField] private SettingsController m_Settings;

        private GameStateManager m_GameStateManager;

    #region Unity Lifecycle
        public void Init(GameStateManager gameStateManager)
        {
            m_GameStateManager = gameStateManager;
        }

        void Awake()
        {
            if (m_ResumeButton)   m_ResumeButton.onClick.AddListener(OnResume);
            if (m_MainMenuButton) m_MainMenuButton.onClick.AddListener(OnQuitToMenu);
            if (m_QuitButton)     m_QuitButton.onClick.AddListener(OnQuitGame);
            if (m_SettingsButton && m_Settings != null)
                m_SettingsButton.onClick.AddListener(() => m_GameStateManager?.GoToSettings());

            if (m_PausePanel) m_PausePanel.SetActive(false);
        }

        void OnEnable()
        {
            var gsm = m_GameStateManager;
            if (gsm == null) return;
            gsm.OnStateChanged += HandleState;
            if (m_PausePanel) m_PausePanel.SetActive(gsm.CurrentState == GameState.Paused);
        }

        void OnDisable()
        {
            if (m_GameStateManager != null)
                m_GameStateManager.OnStateChanged -= HandleState;
        }

        void Update()
        {
            if (!TogglePressed()) return;

            var gsm = m_GameStateManager;
            if (gsm == null) return;

            if (gsm.CurrentState == GameState.Playing)      gsm.Pause();
            else if (gsm.CurrentState == GameState.Paused)  gsm.Resume();
        }
    #endregion

    #region Button Handlers
        private void OnResume()     => m_GameStateManager?.Resume();
        private void OnQuitToMenu() => m_GameStateManager?.GoToMainMenu();
        private void OnQuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    #endregion

    #region Input
        private bool TogglePressed()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                return true;
            foreach (var g in Gamepad.all)
                if (g.startButton.wasPressedThisFrame) return true;
            return false;
        }
    #endregion

    #region State Handling
        private void HandleState(GameState previous, GameState next)
        {
            bool paused = next == GameState.Paused;
            if (m_PausePanel) m_PausePanel.SetActive(paused);

            if (paused && m_ResumeButton != null)
                UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(m_ResumeButton.gameObject);
        }
    #endregion
    }
}
