using UnityEngine;
using Infrastructure;
using Foosball.Gameplay;

namespace Foosball.Presentation
{
    public class MatchFeedbackController : MonoBehaviour
    {
        private AudioManager m_AudioManager;
        private VFXManager m_VFXManager;
        private RumbleManager m_RumbleManager;
        private GameJuiceManager m_GameJuiceManager;
        private GameStateManager m_GameStateManager;
        private EventBus m_EventBus;

    #region Unity Lifecycle
        public void Init(AudioManager audioManager, VFXManager vfxManager, RumbleManager rumbleManager, GameJuiceManager gameJuiceManager, GameStateManager gameStateManager, EventBus eventBus)
        {
            m_AudioManager = audioManager;
            m_VFXManager = vfxManager;
            m_RumbleManager = rumbleManager;
            m_GameJuiceManager = gameJuiceManager;
            m_GameStateManager = gameStateManager;
            m_EventBus = eventBus;

            this.Subscribe<GoalEvent>(m_EventBus, HandleGoal);
            this.Subscribe<GoalFeedbackEvent>(m_EventBus, HandleGoalFeedback);
            this.Subscribe<ShootEvent>(m_EventBus, HandleShoot);
            this.Subscribe<PossessSwitchEvent>(m_EventBus, HandlePossessSwitch);
            this.Subscribe<StanceChangedEvent>(m_EventBus, HandleStanceChanged);
            this.Subscribe<PassAttemptedEvent>(m_EventBus, HandlePassAttempted);
            this.Subscribe<DashAttemptedEvent>(m_EventBus, HandleDashAttempted);
            this.Subscribe<RodStunnedEvent>(m_EventBus, HandleRodStunned);
            this.Subscribe<BlockEvent>(m_EventBus, HandleBlock);
            this.Subscribe<WallBounceEvent>(m_EventBus, HandleWallBounce);
            this.Subscribe<CountdownTickEvent>(m_EventBus, HandleCountdownTick);
            this.Subscribe<CountdownGoEvent>(m_EventBus, HandleCountdownGo);
            this.Subscribe<TableShakenEvent>(m_EventBus, HandleTableShaken);

            if (m_GameStateManager != null)
                m_GameStateManager.OnStateChanged += HandleStateChanged;
        }

        void Start()
        {
            m_AudioManager?.PlayMenuMusic();
        }

        void OnDestroy()
        {
            if (m_GameStateManager != null)
                m_GameStateManager.OnStateChanged -= HandleStateChanged;
        }
    #endregion

    #region Goal
        private void HandleGoal(GoalEvent evt)
        {
            m_AudioManager?.PlayGoal();
            m_VFXManager?.PlayGoal(evt.Position);
        }

        private void HandleGoalFeedback(GoalFeedbackEvent evt)
        {
            m_RumbleManager?.RumbleGoal(evt.Scorer, evt.Conceder);
            m_GameJuiceManager?.PlayGoalPunch();
        }
    #endregion

    #region Rod Actions
        private void HandleShoot(ShootEvent evt)
        {
            m_AudioManager?.PlayShoot();
            m_GameJuiceManager?.PlayShootShake();
        }

        private void HandlePossessSwitch(PossessSwitchEvent evt)
        {
            m_AudioManager?.PlayPossessSwitch();
        }

        private void HandleStanceChanged(StanceChangedEvent evt)
        {
            m_AudioManager?.PlayStanceClick();
        }

        private void HandlePassAttempted(PassAttemptedEvent evt)
        {
            m_AudioManager?.PlayStanceClick();
            m_GameJuiceManager?.PlayPassShake();
        }

        private void HandleDashAttempted(DashAttemptedEvent evt)
        {
            m_AudioManager?.PlayStanceClick();
        }

        private void HandleRodStunned(RodStunnedEvent evt)
        {
            m_AudioManager?.PlayStun();
            m_VFXManager?.PlayStun(evt.Position);
            m_RumbleManager?.RumbleStun(evt.Gamepad);
            m_GameJuiceManager?.PlayStunPunch();
        }

        private void HandleBlock(BlockEvent evt)
        {
            m_VFXManager?.PlayBlock(evt.Position, evt.VfxScale);
            m_AudioManager?.PlayDefenseCatch();
            m_RumbleManager?.RumbleBlock(evt.Gamepad);
            m_GameJuiceManager?.PlayBlockPunch(evt.BallVelocityX, evt.BallSpeed);
        }
    #endregion

    #region Ball
        private void HandleWallBounce(WallBounceEvent evt)
        {
            m_AudioManager?.PlayWallBounce(evt.Volume);
            m_VFXManager?.PlayWallHit(evt.Position, evt.VfxScale);
        }
    #endregion

    #region Kickoff
        private void HandleCountdownTick(CountdownTickEvent evt)
        {
            m_AudioManager?.PlayCountdownTick();
        }

        private void HandleCountdownGo(CountdownGoEvent evt)
        {
            m_AudioManager?.PlayCountdownGo();
        }
    #endregion

    #region Table
        private void HandleTableShaken(TableShakenEvent evt)
        {
            m_GameJuiceManager?.PlayTableShake();
        }
    #endregion

    #region State-Driven Music & Rumble
        private void HandleStateChanged(GameState previous, GameState next)
        {
            bool isFrozen = next == GameState.Paused || next == GameState.Settings;
            m_AudioManager?.SetMusicPaused(isFrozen);

            switch (next)
            {
                case GameState.Countdown:
                    if (previous == GameState.Setup)
                        m_AudioManager?.PlayGameplayMusic();
                    break;

                case GameState.Paused:
                    m_RumbleManager?.StopAll();
                    m_AudioManager?.SetMusicPaused(true);
                    break;

                case GameState.Setup:
                    m_GameJuiceManager?.VignetteEffect();
                    m_AudioManager?.PlayMenuMusic();
                    break;

                case GameState.MainMenu:
                    m_AudioManager?.PlayMenuMusic();
                    break;
            }
        }
    #endregion
    }
}
