using System.Collections.Generic;
using UnityEngine;

namespace Foosball
{
    public class StatePanel : MonoBehaviour
    {
        [SerializeField] private GameObject m_Content;                 
        [SerializeField] private List<GameState> m_VisibleInStates = new();

        void OnEnable()
        {
            var gsm = GameStateManager.Instance;
            if (gsm == null) 
            {
                Debug.LogError("[StatePanel] GameStateManager.Instance is null!");
                return;
            }
            
            gsm.OnStateChanged += HandleStateChanged;
            Apply(gsm.CurrentState);
        }

        void OnDisable()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
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