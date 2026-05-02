using UnityEngine;
using UnityEngine.InputSystem;

namespace Foosball
{
    public class TeamController : MonoBehaviour
    {

    #region Team Properties
        /* Team properties*/
        public string teamName = "Default";
        public int score = 0;
        public RodController[] m_RodControllers;

        private int m_PossedRodIndex = -1;

        /* Action properties*/
        [Header("Input Actions")]
        InputAction m_ChangeRodAction;

    #endregion
    #region Unity Lifecycle
        void Awake()
        {
            InitializeInput();
            m_ChangeRodAction.performed += ctx => SwitchRod();
        }
        void Start()
        {
            DisableAllRodControllers();
            m_RodControllers[0].SetPossessed(true);   
            m_PossedRodIndex = 0; 
        }

        void Update()
        {
            if(m_ChangeRodAction == null)
            {
                Debug.LogWarning("ChangeRod action not found!");
                return;
            }
        }
    #endregion

    #region Initialization
        private void InitializeInput()
        {
            m_ChangeRodAction = InputSystem.actions.FindAction("ChangeRod");
        }
    #endregion

        private void SwitchRod()
        {
            if(m_ChangeRodAction == null)
            {
                Debug.LogWarning("ChangeRod action not found!");
                return;
            }
            int direction = m_RodControllers[m_PossedRodIndex].GetAim().x >= 0 ? 1 : -1;
            int nextRodIndex = (m_PossedRodIndex + direction) % m_RodControllers.Length;
            PossessRod(nextRodIndex);
        }

        private void PossessRod(int rodIndex = -1)
        {
            m_RodControllers[m_PossedRodIndex].SetPossessed(false); 
            m_RodControllers[rodIndex].SetPossessed(true); 
            m_PossedRodIndex = rodIndex; 
        }

        private void DisableAllRodControllers()
        {
            foreach (var rod in m_RodControllers)
            {
                rod.SetPossessed(false);
            }
        }
        public void IncrementScore()
        {
            score++;
        }

        public void ResetScore() => score = 0;
    }
}