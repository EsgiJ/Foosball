using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Foosball
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

        void Awake()
        {
            if (m_ResumeButton)   m_ResumeButton.onClick.AddListener(OnResume);
            if (m_MainMenuButton) m_MainMenuButton.onClick.AddListener(OnQuitToMenu);
            if (m_QuitButton)     m_QuitButton.onClick.AddListener(OnQuitGame);
            if (m_SettingsButton && m_Settings != null)
                m_SettingsButton.onClick.AddListener(() => GameStateManager.Instance?.GoToSettings());

            if (m_PausePanel) m_PausePanel.SetActive(false);
        }

        void OnEnable()
        {
            var gsm = GameStateManager.Instance;
            if (gsm == null) return;
            gsm.OnStateChanged += HandleState;
            if (m_PausePanel) m_PausePanel.SetActive(gsm.CurrentState == GameState.Paused);
        }

        void OnDisable()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= HandleState;
        }

        void Update()
        {
            if (!TogglePressed()) return;

            var gsm = GameStateManager.Instance;
            if (gsm == null) return;

            if (gsm.CurrentState == GameState.Playing)      gsm.Pause();
            else if (gsm.CurrentState == GameState.Paused)  gsm.Resume();
        }

        private bool TogglePressed()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                return true;
            foreach (var g in Gamepad.all)
                if (g.startButton.wasPressedThisFrame) return true;
            return false;
        }

        private void HandleState(GameState previous, GameState next)
        {
            bool paused = next == GameState.Paused;
            if (m_PausePanel) m_PausePanel.SetActive(paused);

            if (paused && m_ResumeButton != null)
                UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(m_ResumeButton.gameObject);
        }

        private void OnResume()     => GameStateManager.Instance?.Resume();
        private void OnQuitToMenu() => GameStateManager.Instance?.GoToMainMenu();  
        private void OnQuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}