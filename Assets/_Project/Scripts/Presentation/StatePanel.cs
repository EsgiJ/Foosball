using System.Collections.Generic;
using UnityEngine;
using Foosball.Gameplay;

namespace Foosball.Presentation
{
    public class StatePanel : MonoBehaviour
    {
        [SerializeField] private GameObject m_Content;
        [SerializeField] private List<GameState> m_VisibleInStates = new();

        private GameStateManager m_GameStateManager;

        public void Init(GameStateManager gameStateManager)
        {
            m_GameStateManager = gameStateManager;
        }

        void OnEnable()
        {
            var gsm = m_GameStateManager;
            if (gsm == null)
            {
                Debug.LogError("[StatePanel] GameStateManager is null!");
                return;
            }

            gsm.OnStateChanged += HandleStateChanged;
            Apply(gsm.CurrentState);
        }

        void OnDisable()
        {
            if (m_GameStateManager != null)
                m_GameStateManager.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState previous, GameState current) => Apply(current);

        private void Apply(GameState current)
        {
            if (m_Content != null)
            {
                m_Content.SetActive(m_VisibleInStates.Contains(current));
            }
        }
    }
}