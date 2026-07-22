using UnityEngine;
using Infrastructure;
using Foosball.Presentation;
using Foosball.Gameplay;

namespace Foosball.App
{
    [DefaultExecutionOrder(-1000)]
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private AudioManager m_AudioManager;
        [SerializeField] private RumbleManager m_RumbleManager;
        [SerializeField] private VFXManager m_VFXManager;
        [SerializeField] private GameJuiceManager m_GameJuiceManager;
        [SerializeField] private AimTrajectory m_AimTrajectory;
        [SerializeField] private GameStateManager m_GameStateManager;

        private readonly EventBus m_EventBus = new EventBus();

        void Awake()
        {
            WireGameManagers();
            WireTeams();
            WireRods();
            WireBalls();
            WireButtons();
            WireSettings();
            WireSetup();
            WireCameras();
            WireInteractiveMenus();
            WireMainMenus();
            WirePauseMenus();
            WireStatePanels();
            WireMatchFeedback();
        }

        private void WireGameManagers()
        {
            foreach (var gameManager in FindObjectsByType<GameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                gameManager.Init(m_GameStateManager, m_EventBus);
        }

        private void WireTeams()
        {
            foreach (var team in FindObjectsByType<TeamController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                team.Init(m_EventBus);
        }

        private void WireRods()
        {
            foreach (var rod in FindObjectsByType<RodController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                rod.Init(m_AimTrajectory, m_GameStateManager, m_EventBus);
        }

        private void WireBalls()
        {
            foreach (var ball in FindObjectsByType<BallController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                ball.Init(m_GameStateManager, m_EventBus);
        }

        private void WireButtons()
        {
            foreach (var button in FindObjectsByType<ButtonJuice>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                button.Init(m_AudioManager);
        }

        private void WireSettings()
        {
            foreach (var settings in FindObjectsByType<SettingsController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                settings.Init(m_RumbleManager, m_GameStateManager);
        }

        private void WireSetup()
        {
            foreach (var setup in FindObjectsByType<SetupController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                setup.Init(m_AudioManager, m_GameStateManager);
        }

        private void WireCameras()
        {
            foreach (var camera in FindObjectsByType<CameraController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                camera.Init(m_GameStateManager);
        }

        private void WireInteractiveMenus()
        {
            foreach (var menu in FindObjectsByType<InteractiveMenu>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                menu.Init(m_GameStateManager);
        }

        private void WireMainMenus()
        {
            foreach (var mainMenu in FindObjectsByType<MainMenuController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                mainMenu.Init(m_GameStateManager);
        }

        private void WirePauseMenus()
        {
            foreach (var pauseMenu in FindObjectsByType<PauseController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                pauseMenu.Init(m_GameStateManager);
        }

        private void WireStatePanels()
        {
            foreach (var panel in FindObjectsByType<StatePanel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                panel.Init(m_GameStateManager);
        }

        private void WireMatchFeedback()
        {
            foreach (var feedback in FindObjectsByType<MatchFeedbackController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                feedback.Init(m_AudioManager, m_VFXManager, m_RumbleManager, m_GameJuiceManager, m_GameStateManager, m_EventBus);
        }
    }
}
