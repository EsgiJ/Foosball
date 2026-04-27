using UnityEngine;
using UnityEngine.InputSystem;

public class TeamController : MonoBehaviour
{

#region Team Properties
    /* Team properties*/
    public string teamName = "Default";
    public int score = 0;
    public RodController[] rodControllers;

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
        rodControllers[0].SetPossessed(true);    
    }

    void Update()
    {
        if(m_ChangeRodAction == null)
        {
            Debug.LogWarning("ChangeRod action not found!");
            return;
        }

        m_ChangeRodAction.performed += ctx => SwitchRod();
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
        
    }

    private void DisableAllRodControllers()
    {
        foreach (var rod in rodControllers)
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
