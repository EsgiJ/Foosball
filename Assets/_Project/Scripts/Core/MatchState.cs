using System;
using Foosball.Data;

namespace Foosball.Core
{
    public class MatchState
    {
        private readonly MatchSettings m_Settings;

        public int HomeScore { get; private set; }
        public int AwayScore { get; private set; }
        public bool IsMatchOver { get; private set; }
        public bool HomeWon { get; private set; }
        public float ElapsedSeconds { get; private set; }

        public event Action<int, int> ScoreChanged;
        public event Action<bool> MatchWon;

        public MatchState(MatchSettings settings)
        {
            m_Settings = settings;
        }

        public void RegisterGoal(bool isHome)
        {
            if (isHome)
                HomeScore++;
            else
                AwayScore++;

            ScoreChanged?.Invoke(HomeScore, AwayScore);

            if (!IsMatchOver && (HomeScore >= m_Settings.WinScore || AwayScore >= m_Settings.WinScore))
            {
                IsMatchOver = true;
                HomeWon = HomeScore > AwayScore;
                MatchWon?.Invoke(HomeWon);
            }
        }

        public void Tick(float deltaTime)
        {
            if (IsMatchOver)
                return;

            ElapsedSeconds += deltaTime;

            if (ElapsedSeconds >= m_Settings.TimeLimitSeconds)
            {
                IsMatchOver = true;
                HomeWon = HomeScore > AwayScore;
                MatchWon?.Invoke(HomeWon);
            }
        }

        public void Reset()
        {
            HomeScore = 0;
            AwayScore = 0;
            IsMatchOver = false;
            HomeWon = false;
            ElapsedSeconds = 0f;
            ScoreChanged?.Invoke(HomeScore, AwayScore);
        }
    }
}
