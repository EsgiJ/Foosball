using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RodController : MonoBehaviour
{
#region Properties 

    /* Rod General Properties */
    [Header("Rod Properties")]
    [SerializeField, Min(0f)] private float m_MovementSpeed = 5f;
    [SerializeField, Min(0f)] private float m_MinZPos = -1.5f;
    [SerializeField, Min(0f)] private float m_MaxZPos = 1.5f;
    [SerializeField, Min(0f)] private float m_RodUsableExtent = 6f;
    [SerializeField, Range(1, 5)] private int m_FootballPlayerCount = 3;

    [SerializeField] private float m_RodDefenseStanceRotation = 0f;
    [SerializeField] private float m_RodAttackStanceRotation = 30f;
    [SerializeField] private float m_RodShootRotation = -180f;

    private Quaternion m_StartRotation;

    /* References */
    [Header("Ball Reference")]
    [SerializeField] private GameObject footballPlayer;
    private List<GameObject> footballPlayers = new List<GameObject>();
    [SerializeField] private BallController m_BallController;
    private BallController m_OwnedBall = null;
    private Rigidbody m_Rigidbody;

    /* Action properties*/
    [Header("Animation Properties")]
    InputAction m_ShootAction;
    InputAction m_MoveAction;
    InputAction m_AimAction ;
    InputAction m_StanceAction;

    private Vector2 m_AimVector = Vector2.zero;

    /* Animation properties */
    [Header("Animation Properties")]
    [SerializeField] private AnimationCurve m_ShootRotationCurve;
    [SerializeField, Min(0f)] private static float m_ShootRotationDuration = 0.2f;

    [SerializeField] private AnimationCurve m_StunRotationCurve; 
    [SerializeField, Min(0f)] private static float m_StunRotationDuration = 0.2f;
    [SerializeField, Min(0f)] private static float m_StunDuration = 2f;

    [SerializeField] private AnimationCurve m_DefenseStanceRotationCurve; 
    [SerializeField, Min(0f)] private static float m_DefenseStanceRotationDuration = 0.2f;

    [SerializeField] private AnimationCurve m_AttackStanceRotationCurve; 
    [SerializeField, Min(0f)] private static float m_AttackStanceRotationDuration = 0.2f;

    /*Events*/
    public static event Action<Vector2> OnShootEvent;

    /*State*/
    public enum ERodState
    {
        Idle,
        DefenseStance,
        AttackStance,
        Shooting,
        Stunned
    }

    private ERodState m_CurrentRodState = ERodState.Idle;
    private bool m_IsStanceHeld = false;
    private bool m_HasBall = false;
#endregion

#region Unity Lifecycle
    void Start()
    {
        InitializeInput();
        InitializePhysics();
        SpawnFootballPlayers();

        if (m_BallController == null)
        {
            m_BallController = FindObjectsByType<BallController>(FindObjectsSortMode.None)[0];
        }

        m_StartRotation = transform.localRotation;
    }

    void Update()
    {
        if(m_CurrentRodState != ERodState.Stunned)
        {
            HandleMovement();
            HandleAim();
        }
    }

    void OnDestroy()
    {
        if (m_ShootAction != null) m_ShootAction.started -= OnShootPressed;
        if (m_StanceAction != null)
        {
            m_StanceAction.started -= OnStancePressed;
            m_StanceAction.canceled -= OnStanceReleased;
        }
    }
#endregion

#region Initialization

    private void InitializeInput()
    {
        m_ShootAction = InputSystem.actions.FindAction("Shoot");
        m_MoveAction = InputSystem.actions.FindAction("MoveRod");
        m_AimAction = InputSystem.actions.FindAction("Aim");
        m_StanceAction = InputSystem.actions.FindAction("Stance");

        if (m_ShootAction != null)
            m_ShootAction.started += OnShootPressed;

        if (m_StanceAction != null)
        {
            m_StanceAction.started += OnStancePressed;
            m_StanceAction.canceled += OnStanceReleased;
        }
    }

    private void InitializePhysics()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        if (m_Rigidbody == null)
        {
            m_Rigidbody = gameObject.AddComponent<Rigidbody>();
        }
        
        m_Rigidbody.isKinematic = true;
        m_Rigidbody.useGravity = false;
    }

    void SpawnFootballPlayers()
    {
        for(int i = 0; i < m_FootballPlayerCount; i++)
        {
            Vector3 spawnPosition = CalculateFootballPlayerPosition(i);
            GameObject player = Instantiate(footballPlayer, spawnPosition, footballPlayer.transform.rotation);
            player.transform.SetParent(transform, true);
            footballPlayers.Add(player);
            player.GetComponent<FootballPlayerController>().SetRodController(this);
        }
    }

    Vector3 CalculateFootballPlayerPosition(int index)
    {
        float spacing = m_RodUsableExtent / (m_FootballPlayerCount + 1);
        float zOffset = (index + 1) * spacing - (m_RodUsableExtent / 2f);
        return transform.position + new Vector3(0f, 0f, zOffset);
    }
#endregion

#region Input Handling
    private void HandleMovement()
    {        
        if(m_MoveAction == null)
        {
            Debug.LogWarning("MoveRod action not found!");
            return;
        }

        Vector3 position = transform.localPosition;

        if (position.z < m_MinZPos)
            position.z = m_MinZPos;
        else if (position.z > m_MaxZPos)
            position.z = m_MaxZPos;
        else
            position.z += m_MoveAction.ReadValue<float>() * m_MovementSpeed * Time.deltaTime;

        transform.localPosition = position;
    }

    private void HandleAim()
    {
        if(m_AimAction == null)
        {
            Debug.LogWarning("Aim action not found!");
            return;
        }
        m_AimVector = m_AimAction.ReadValue<Vector2>();
    }

    private void OnShootPressed(InputAction.CallbackContext context)
    {
        if(m_CurrentRodState == ERodState.Stunned)
        {
            Debug.Log("Cannot shoot while stunned!");
            return;
        }

        if(!m_HasBall || m_CurrentRodState != ERodState.AttackStance)
        {
            Debug.Log("Cannot shoot: either no ball or not in attack stance!");
            return;
        }

        SetState(ERodState.Shooting);
        OnShootEvent?.Invoke(m_AimVector);

        m_HasBall = false;
    }

    private void OnStancePressed(InputAction.CallbackContext context)
    {
        if(m_CurrentRodState == ERodState.Stunned)
        {
            Debug.Log("Cannot change stance while stunned!");
            return;
        }

        m_IsStanceHeld = true;

        if(m_HasBall && m_CurrentRodState != ERodState.Shooting)
            SetState(ERodState.AttackStance);
        else
            SetState(ERodState.DefenseStance);
    }

    private void OnStanceReleased(InputAction.CallbackContext context)
    {
        if(m_CurrentRodState == ERodState.Stunned)
        {
            Debug.Log("Cannot change stance while stunned!");
            return;
        }

        m_IsStanceHeld = false;

        SetState(ERodState.Idle);
        Debug.Log("RodState changed to Idle");
    }
#endregion

#region State Management
    private void SetState(ERodState newRodState)
    {
        if(m_CurrentRodState == newRodState)
        return;

        m_CurrentRodState = newRodState;
        Debug.Log("RodState changed to " + newRodState);

        ChangeSpritesForEachPlayer(newRodState);
        switch(m_CurrentRodState)
        {
            case ERodState.Idle:
                OnEnterIdle();
                break;
            case ERodState.DefenseStance:
                OnEnterDefenseStance();
                break;
            case ERodState.AttackStance:
                OnEnterAttackStance();
                break;
            case ERodState.Shooting:
                OnEnterShooting();
                break;
            case ERodState.Stunned:
                OnEnterStunned();
                break;
        }
    }

    private void OnEnterIdle()
    {
        ReleaseBall();
        transform.localRotation = m_StartRotation;
    }

    private void OnEnterDefenseStance()
    {
        StartCoroutine(DefenseStateCoroutine());
    }

    private void OnEnterAttackStance()
    {
        StartCoroutine(AttackStateCoroutine());
    }

    private void OnEnterShooting()
    {
        StartCoroutine(ShootingStateCoroutine());
    }

    private void OnEnterStunned()
    {
        StartCoroutine(StunStateCoroutine());
    }

    private void ChangeSpritesForEachPlayer(ERodState state)
    {
        foreach(GameObject player in footballPlayers)
        {
            FootballPlayerController controller = player.GetComponent<FootballPlayerController>();
            controller.ChangeStateSprite(state);
        }
    }
#endregion

#region Coroutines
    IEnumerator StateAnimationCoroutine(AnimationCurve curve, float rotationDuration, float rotationAngle)
    {
        float elapsed = 0f;
        Quaternion m_TargetRotation = m_StartRotation * Quaternion.Euler(0, rotationAngle, 0);

        while(elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotationDuration;
            float curveValue = curve.Evaluate(t);
            transform.localRotation = Quaternion.Lerp(m_StartRotation, m_TargetRotation, curveValue);
            yield return null;
        }

        transform.localRotation = m_TargetRotation;
    }

    private IEnumerator DefenseStateCoroutine()
    {
        yield return StartCoroutine(StateAnimationCoroutine(m_DefenseStanceRotationCurve, m_DefenseStanceRotationDuration, m_RodDefenseStanceRotation));
    }

    private IEnumerator AttackStateCoroutine()
    {
        yield return StartCoroutine(StateAnimationCoroutine(m_AttackStanceRotationCurve, m_AttackStanceRotationDuration, m_RodAttackStanceRotation));
    }

    private IEnumerator ShootingStateCoroutine()
    {
        ReleaseBall();
        yield return StartCoroutine(StateAnimationCoroutine(m_ShootRotationCurve, m_ShootRotationDuration, m_RodShootRotation));
        SetState(ERodState.Idle);
    }

    private IEnumerator StunStateCoroutine()
    {
        yield return StartCoroutine(StateAnimationCoroutine(m_StunRotationCurve, m_StunRotationDuration, m_RodShootRotation));

        float waitTime = Mathf.Max(0f, m_StunDuration - m_StunRotationDuration);
        yield return new WaitForSeconds(waitTime);

        SetState(ERodState.Idle);
    }
#endregion

#region Event Handling
    public void HandleBallContact(BallController ball)
    {
        m_OwnedBall = ball;
        m_HasBall = true;   

        if (m_IsStanceHeld && m_CurrentRodState == ERodState.DefenseStance)
        {
            AttachBallToRod(ball);
        }
        else if (m_IsStanceHeld && m_CurrentRodState == ERodState.AttackStance)
        {
            AttachBallToRod(ball);
        }
    }

    public void HandleStun(BallController ball)
    {
        SetState(ERodState.Stunned);
    }

#endregion

    private void AttachBallToRod(BallController ball)
    {
        ball.AttachToRod(this);
        
        Debug.Log("Ball attached to rod");
    }

    public void ReleaseBall()
    {
        if (m_OwnedBall != null)
        {
            m_OwnedBall.DetachFromRod();
            m_OwnedBall = null;
            m_HasBall = false;
            
            Debug.Log("Ball released from rod");
        }
    }

#region Getters
    public ERodState GetState() => m_CurrentRodState;
    public static float GetShootAnimationDuration() => m_ShootRotationDuration;
#endregion
}
