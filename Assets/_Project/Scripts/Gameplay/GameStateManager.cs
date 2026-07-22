using System;
using UnityEngine;

namespace Foosball.Gameplay
{
    public enum GameState
    {
        MainMenu,
        Setup,
        Countdown,
        Playing,
        Goal,
        Paused,
        Settings
    }

    public class GameStateManager : MonoBehaviour
    {
        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        public GameState PrePauseState { get; private set; } = GameState.Playing;
        public bool IsPlaying => CurrentState == GameState.Playing;

        // (previous, next)
        public event Action<GameState, GameState> OnStateChanged;

        public void ChangeState(GameState next)
        {
            if (next == CurrentState) return;

            if (!IsTransitionAllowed(CurrentState, next))
            {
                Debug.LogWarning($"[GameState] invalid transition {CurrentState} -> {next}, ignoring");
                return;
            }

            GameState previous = CurrentState;
            CurrentState = next;
            Debug.Log($"[GameState] {previous} -> {next}");
            OnStateChanged?.Invoke(previous, next);
        }

        public void Pause()
        {
            if (CurrentState == GameState.Paused) return;
            PrePauseState = CurrentState;
            ChangeState(GameState.Paused);
        }

        public void Resume()
        {
            if (CurrentState != GameState.Paused) return;
            ChangeState(PrePauseState);
        }

        public void GoToMainMenu()  => ChangeState(GameState.MainMenu);
        public void GoToSetup()     => ChangeState(GameState.Setup);
        public void StartCountdown()=> ChangeState(GameState.Countdown);
        public void StartPlaying()  => ChangeState(GameState.Playing);
        public void GoToGoal()      => ChangeState(GameState.Goal);
        public void GoToSettings()  => ChangeState(GameState.Settings);
        public void CloseSettings() => ChangeState(GameState.Paused);

        private bool IsTransitionAllowed(GameState from, GameState to)
        {
            // can transition to main menu from anywhere
            if (to == GameState.MainMenu) return true;

            switch (from)
            {
                case GameState.MainMenu:  return to == GameState.Setup;
                case GameState.Setup:     return to == GameState.Countdown;
                case GameState.Countdown: return to == GameState.Playing;
                case GameState.Playing:   return to == GameState.Goal || to == GameState.Paused;
                case GameState.Paused:    return to == GameState.Playing || to == GameState.Settings || to == GameState.MainMenu;
                case GameState.Goal:      return to == GameState.Countdown;
                case GameState.Settings:  return to == GameState.MainMenu || to == GameState.Paused;
                default: return false;
            }
        }
    }
}
