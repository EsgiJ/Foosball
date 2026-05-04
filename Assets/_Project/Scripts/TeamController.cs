using System;
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
        private bool m_IsHomeTeam = true;
        private int m_PossedRodIndex = -1;

        /* Action properties*/
        [Header("Input Actions")]
        InputAction m_ChangeRodAction;
        InputAction m_ShakeTableAction;

        [SerializeField] private BallController m_BallController;
        [SerializeField] private float m_BallStartingNudge = 0.5f;

    #endregion
    #region Unity Lifecycle
        void Awake()
        {
            InitializeInput();
            m_ChangeRodAction.performed += ctx => SwitchRod();
            m_ShakeTableAction.performed += ctx => ShakeTheTable();
        }
        void Start()
        {
            DisableAllRodControllers();
            SetTeamForRodControllers();
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
            m_ShakeTableAction = InputSystem.actions.FindAction("ShakeTable");
        }
    #endregion

        private void ShakeTheTable()
        {
            GameJuiceManager.Instance?.ShakeCamera(0.5f, 0.5f);

            if (m_BallController != null)
            {
                Vector3 randomDir = new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    0f,
                    UnityEngine.Random.Range(-1f, 1f)
                ).normalized;

                m_BallController.GetComponent<Rigidbody>().AddForce(
                    randomDir * m_BallStartingNudge,
                    ForceMode.Impulse
                );
            }
        }
        private void SwitchRod()
        {
            if(m_ChangeRodAction == null)
            {
                Debug.LogWarning("ChangeRod action not found!");
                return;
            }
            int direction = m_RodControllers[m_PossedRodIndex].GetAim().x >= 0 ? 1 : -1;
            int nextRodIndex = (m_PossedRodIndex + direction + m_RodControllers.Length) % m_RodControllers.Length;
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

        private void SetTeamForRodControllers()
        {
            foreach (var rod in m_RodControllers)
            {
                rod.SetIsHomeTeam(IsHomeTeam());
            }
        }

        public void IncrementScore()
        {
            score++;
        }

        public void ResetScore() => score = 0;

        public bool IsHomeTeam() => m_IsHomeTeam;
        public void SetIsHomeTeam(bool isHome) => m_IsHomeTeam = isHome;
    }
}