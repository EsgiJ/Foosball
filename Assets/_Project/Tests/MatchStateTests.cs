using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Foosball.Core;
using Foosball.Data;

namespace Foosball.Tests
{
    public class MatchStateTests
    {
        private const int WinScore = 3;
        private const float TimeLimitSeconds = 10f;

        private MatchSettings m_Settings;
        private MatchState m_State;

        [SetUp]
        public void SetUp()
        {
            m_Settings = CreateSettings(WinScore, TimeLimitSeconds);
            m_State = new MatchState(m_Settings);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_Settings);
        }

        private static MatchSettings CreateSettings(int winScore, float timeLimitSeconds)
        {
            var settings = ScriptableObject.CreateInstance<MatchSettings>();
            SetPrivateField(settings, "m_WinScore", winScore);
            SetPrivateField(settings, "m_TimeLimitSeconds", timeLimitSeconds);
            return settings;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            typeof(MatchSettings)
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(target, value);
        }

    #region Score Increments
        [Test]
        public void RegisterGoal_Home_IncrementsHomeScoreOnly()
        {
            m_State.RegisterGoal(isHome: true);

            Assert.AreEqual(1, m_State.HomeScore);
            Assert.AreEqual(0, m_State.AwayScore);
        }

        [Test]
        public void RegisterGoal_Away_IncrementsAwayScoreOnly()
        {
            m_State.RegisterGoal(isHome: false);

            Assert.AreEqual(0, m_State.HomeScore);
            Assert.AreEqual(1, m_State.AwayScore);
        }

        [Test]
        public void RegisterGoal_MultipleGoals_AccumulatesPerSide()
        {
            m_State.RegisterGoal(isHome: true);
            m_State.RegisterGoal(isHome: true);
            m_State.RegisterGoal(isHome: false);

            Assert.AreEqual(2, m_State.HomeScore);
            Assert.AreEqual(1, m_State.AwayScore);
        }

        [Test]
        public void RegisterGoal_RaisesScoreChangedEvent_WithCurrentScores()
        {
            int? raisedHome = null;
            int? raisedAway = null;
            m_State.ScoreChanged += (home, away) => { raisedHome = home; raisedAway = away; };

            m_State.RegisterGoal(isHome: true);

            Assert.AreEqual(1, raisedHome);
            Assert.AreEqual(0, raisedAway);
        }
    #endregion

    #region Win Detection
        [Test]
        public void RegisterGoal_BelowWinScore_DoesNotEndMatch()
        {
            for (int i = 0; i < WinScore - 1; i++)
                m_State.RegisterGoal(isHome: true);

            Assert.IsFalse(m_State.IsMatchOver);
        }

        [Test]
        public void RegisterGoal_HomeReachesWinScore_EndsMatchWithHomeWon()
        {
            for (int i = 0; i < WinScore; i++)
                m_State.RegisterGoal(isHome: true);

            Assert.IsTrue(m_State.IsMatchOver);
            Assert.IsTrue(m_State.HomeWon);
        }

        [Test]
        public void RegisterGoal_AwayReachesWinScore_EndsMatchWithHomeWonFalse()
        {
            for (int i = 0; i < WinScore; i++)
                m_State.RegisterGoal(isHome: false);

            Assert.IsTrue(m_State.IsMatchOver);
            Assert.IsFalse(m_State.HomeWon);
        }

        [Test]
        public void RegisterGoal_RaisesMatchWonEvent_WhenThresholdReached()
        {
            bool? raisedHomeWon = null;
            m_State.MatchWon += homeWon => raisedHomeWon = homeWon;

            for (int i = 0; i < WinScore; i++)
                m_State.RegisterGoal(isHome: true);

            Assert.AreEqual(true, raisedHomeWon);
        }

        [Test]
        public void RegisterGoal_AfterMatchOver_StillIncrementsScoreButDoesNotRefireMatchWon()
        {
            int matchWonCount = 0;
            m_State.MatchWon += _ => matchWonCount++;

            for (int i = 0; i < WinScore; i++)
                m_State.RegisterGoal(isHome: true);

            m_State.RegisterGoal(isHome: true);

            Assert.AreEqual(WinScore + 1, m_State.HomeScore);
            Assert.AreEqual(1, matchWonCount);
        }
    #endregion

    #region Time Expiry
        [Test]
        public void Tick_BelowTimeLimit_DoesNotEndMatch()
        {
            m_State.Tick(TimeLimitSeconds - 1f);

            Assert.IsFalse(m_State.IsMatchOver);
        }

        [Test]
        public void Tick_AccumulatesElapsedSecondsAcrossCalls()
        {
            m_State.Tick(2f);
            m_State.Tick(3f);

            Assert.AreEqual(5f, m_State.ElapsedSeconds);
        }

        [Test]
        public void Tick_ReachesTimeLimit_EndsMatch()
        {
            m_State.Tick(TimeLimitSeconds);

            Assert.IsTrue(m_State.IsMatchOver);
        }

        [Test]
        public void Tick_TimeExpiresWithHigherHomeScore_HomeWon()
        {
            m_State.RegisterGoal(isHome: true);

            m_State.Tick(TimeLimitSeconds);

            Assert.IsTrue(m_State.IsMatchOver);
            Assert.IsTrue(m_State.HomeWon);
        }

        [Test]
        public void Tick_RaisesMatchWonEvent_WhenTimeExpires()
        {
            bool raised = false;
            m_State.MatchWon += _ => raised = true;

            m_State.Tick(TimeLimitSeconds);

            Assert.IsTrue(raised);
        }

        [Test]
        public void Tick_AfterMatchOver_DoesNotAdvanceElapsedSecondsFurther()
        {
            m_State.Tick(TimeLimitSeconds);
            float elapsedAtMatchEnd = m_State.ElapsedSeconds;

            m_State.Tick(5f);

            Assert.AreEqual(elapsedAtMatchEnd, m_State.ElapsedSeconds);
        }
    #endregion

    #region Reset
        [Test]
        public void Reset_ClearsScores()
        {
            m_State.RegisterGoal(isHome: true);
            m_State.RegisterGoal(isHome: false);

            m_State.Reset();

            Assert.AreEqual(0, m_State.HomeScore);
            Assert.AreEqual(0, m_State.AwayScore);
        }

        [Test]
        public void Reset_ClearsMatchOverAndHomeWon()
        {
            for (int i = 0; i < WinScore; i++)
                m_State.RegisterGoal(isHome: true);

            m_State.Reset();

            Assert.IsFalse(m_State.IsMatchOver);
            Assert.IsFalse(m_State.HomeWon);
        }

        [Test]
        public void Reset_ClearsElapsedSeconds()
        {
            m_State.Tick(4f);

            m_State.Reset();

            Assert.AreEqual(0f, m_State.ElapsedSeconds);
        }

        [Test]
        public void Reset_RaisesScoreChangedEvent_WithZeroScores()
        {
            m_State.RegisterGoal(isHome: true);

            int? raisedHome = null;
            int? raisedAway = null;
            m_State.ScoreChanged += (home, away) => { raisedHome = home; raisedAway = away; };

            m_State.Reset();

            Assert.AreEqual(0, raisedHome);
            Assert.AreEqual(0, raisedAway);
        }

        [Test]
        public void Reset_AllowsMatchToBeWonAgainAfterPreviousMatchEnded()
        {
            for (int i = 0; i < WinScore; i++)
                m_State.RegisterGoal(isHome: true);
            m_State.Reset();

            for (int i = 0; i < WinScore; i++)
                m_State.RegisterGoal(isHome: false);

            Assert.IsTrue(m_State.IsMatchOver);
            Assert.IsFalse(m_State.HomeWon);
        }
    #endregion
    }
}
