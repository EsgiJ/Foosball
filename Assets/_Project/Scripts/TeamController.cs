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

        /* Action properties*/
        [Header("Input Actions")]
        InputAction m_ChangeRodAction;
        InputAction m_ShakeTableAction;

        [Header("Input")]
        [SerializeField] private InputActionAsset m_BaseActions;          
        [SerializeField] private string m_DefaultScheme = "KeyboardLeft"; // Home: KeyboardLeft, Away: KeyboardRight

        private InputActionAsset m_Actions;
        public string CurrentScheme { get; private set; }

        [SerializeField] private BallController m_BallController;
        [SerializeField] private float m_BallStartingNudge = 0.5f;

    #endregion
    #region Unity Lifecycle
        void Awake()
        {
            AssignScheme(m_DefaultScheme);
        }

        public void AssignScheme(string schemeName)
        {
            UnsubscribeTeamActions();

            InputActionAsset baseAsset = m_BaseActions != null ? m_BaseActions : InputSystem.actions;
            if (m_Actions != null) 
            { 
                m_Actions.Disable(); 
                Destroy(m_Actions); 
            }

            m_Actions = Instantiate(baseAsset);                

            var scheme = m_Actions.FindControlScheme(schemeName);
            if (scheme.HasValue)
            {
                m_Actions.bindingMask = InputBinding.MaskByGroup(scheme.Value.bindingGroup);
            }
            else
            {
                Debug.LogWarning($"[TeamController] '{schemeName}' no control scheme found");
            }

            CurrentScheme = schemeName;

            m_ChangeRodAction  = m_Actions.FindAction("ChangeRod");
            m_ShakeTableAction = m_Actions.FindAction("ShakeTable");
            SubscribeTeamActions();

            if (m_RodControllers != null)
            {
                foreach (var rod in m_RodControllers)
                {
                    if (rod != null) 
                        rod.InitializeInput(m_Actions);
                }
            }

            m_Actions.Disable();  
        }

        private void SubscribeTeamActions()
        {
            if (m_ChangeRodAction != null)  
            {
                m_ChangeRodAction.performed  += OnChangeRod;
            }
            if (m_ShakeTableAction != null) 
            {
                m_ShakeTableAction.performed += OnShakeTable;
            }
        }
        private void UnsubscribeTeamActions()
        {
            if (m_ChangeRodAction != null)  
            {
                m_ChangeRodAction.performed  -= OnChangeRod;
            }
            if (m_ShakeTableAction != null) 
            {
                m_ShakeTableAction.performed -= OnShakeTable;
            }
        }
        void Start()
        {
            SetTeamForRodControllers();
            DisableAllRodControllers();
            m_PossedRodIndex = -1;
        }

        void Update()
        {
            if(m_ChangeRodAction == null)
            {
                Debug.LogWarning("ChangeRod action not found!");
                return;
            }
        }

        void OnDestroy()
        {
            UnsubscribeTeamActions();
            if (m_Actions != null) 
            { 
                m_Actions.Disable(); 
                Destroy(m_Actions); 
            }
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

        public void ApplyFormation(int[] counts)
        {
                Debug.Log($"[{teamName}] ApplyFormation: {string.Join(",", counts)} | rod count: {m_RodControllers.Length}");

            for (int i = 0; i < m_RodControllers.Length && i < counts.Length; i++)
            {
                if (m_RodControllers[i] != null)
                {
                    m_RodControllers[i].SetPlayerCount(counts[i]);
                }
            }
        }
        private void OnChangeRod(InputAction.CallbackContext ctx)  => SwitchRod();
        private void OnShakeTable(InputAction.CallbackContext ctx) => ShakeTheTable();

        public void EnableInput()  => m_Actions?.FindActionMap("Game")?.Enable();
        public void DisableInput() => m_Actions?.FindActionMap("Game")?.Disable();

        public void ResetScore() => score = 0;

        public bool IsHomeTeam() => m_IsHomeTeam;
        public void SetIsHomeTeam(bool isHome) => m_IsHomeTeam = isHome;

        public InputActionAsset Actions => m_Actions;
    }
}