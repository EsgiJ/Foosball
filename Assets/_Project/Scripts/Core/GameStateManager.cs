using System;
using UnityEngine;

namespace Foosball
{
    public enum GameState
    {
        MainMenu,  
        Setup,   
        Countdown,  
        Playing,    
        Goal,
        Paused        
    }

    [DefaultExecutionOrder(-100)]   
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        // (önceki, yeni)
        public event Action<GameState, GameState> OnStateChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

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
        public static bool IsPlaying => Instance != null && Instance.CurrentState == GameState.Playing;
        public GameState PrePauseState { get; private set; } = GameState.Playing;

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
                case GameState.Paused:    return to == GameState.Playing; 
                case GameState.Goal:      return to == GameState.Countdown;
                default: return false;
            }
        }
    }
}