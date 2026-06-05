using UnityEngine;
using UnityEngine.InputSystem;

using Foosball.Rod;

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

        [SerializeField] private int m_PlayerIndex = 0;   // Home = 0, Away = 1
        private PlayerInput m_PlayerInput;
        private InputActionAsset m_Actions;

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
            ResolveInputSource();
            InitializeInput();
            m_ChangeRodAction.performed += ctx => SwitchRod();
            m_ShakeTableAction.performed += ctx => ShakeTheTable();
        }
        void Start()
        {
            InitializeRodInput();
            SetTeamForRodControllers();
            DisableAllRodControllers();
            m_PossedRodIndex = -1;
            DisableInput();
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
    private void InitializeRodInput()
    {
        foreach (var rod in m_RodControllers)
            rod.InitializeInput(m_Actions);
    }
    private void ResolveInputSource()
    {
        PlayerToken token = PlayerRegistry.GetByIndex(m_PlayerIndex);
        if (token != null)
        {
            m_PlayerInput = token.PlayerInput;
            m_Actions = m_PlayerInput.actions;
        }
        else
        {
            Debug.LogWarning($"[TeamController] index {m_PlayerIndex} no token found, global input fallback");
            m_Actions = InputSystem.actions;
        }
    }

    private void InitializeInput()
    {
        m_ChangeRodAction  = m_Actions.FindAction("ChangeRod");
        m_ShakeTableAction = m_Actions.FindAction("ShakeTable");
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

        public void BeginControl()
        {
            if (m_PossedRodIndex >= 0) 
                return;          
            m_RodControllers[0].SetPossessed(true);
            m_PossedRodIndex = 0;
        }

        public void EndControl()
        {
            DisableAllRodControllers();
            m_PossedRodIndex = -1;
        }
        private void SwitchRod()
        {
            if (m_PossedRodIndex < 0) 
                return;
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

        public void EnableInput()
        {
            if (m_PlayerInput != null) 
            {
                m_PlayerInput.ActivateInput();
            }
            else
            {
                m_Actions?.FindActionMap("Game")?.Enable();
            }
        }

        public void DisableInput()
        {
            if (m_PlayerInput != null) 
            {
                m_PlayerInput.DeactivateInput();
            }
            else
            {
                m_Actions?.FindActionMap("Game")?.Disable();
            }
        }

        public void ResetScore() => score = 0;

        public bool IsHomeTeam() => m_IsHomeTeam;
        public void SetIsHomeTeam(bool isHome) => m_IsHomeTeam = isHome;
    }
}