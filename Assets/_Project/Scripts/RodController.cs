using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class RodController : MonoBehaviour
{
#region Properties 
    /* Rod General Properties */
    [Header("Rod Properties")]
    [SerializeField, Min(0f)] private float m_MovementSpeed = 5f;
    [SerializeField] private float m_MinZPos = -3.0f;
    [SerializeField] private float m_MaxZPos = 3.0f;
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
    [SerializeField, Min(0f)] private static float m_ShootRotationDuration = 0.2f;
    [SerializeField] private Ease m_ShootEase = Ease.OutQuad;

    [SerializeField, Min(0f)] private static float m_StunRotationDuration = 0.2f;
    [SerializeField] private Ease m_StunEase = Ease.OutBounce;
    [SerializeField, Min(0f)] private static float m_StunDuration = 2f;

    [SerializeField, Min(0f)] private static float m_DefenseStanceRotationDuration = 0.2f;
    [SerializeField] private Ease m_DefenseStanceEase = Ease.OutQuad;

    [SerializeField, Min(0f)] private static float m_AttackStanceRotationDuration = 0.2f;
    [SerializeField] private Ease m_AttackStanceEase = Ease.OutQuad;

    private Tween m_CurrentRotationTween;

    /* Events */
    public static event Action<Vector2> OnShootEvent;

    /* State */
    public enum ERodState
    {
        Idle,
        DefenseStance,
        AttackStance,
        Shooting,
        Stunned
    }

    /* Current Rod State Properties*/
    private ERodState m_CurrentRodState = ERodState.Idle;
    private bool m_IsStanceHeld = false;
    private bool m_HasBall = false;

    public bool m_RodPossessed = false;
#endregion

#region Unity Lifecycle
    void Awake()
    {
        // This was necessary to ensure that we initialize input before any team tries to possess the rod 
        InitializeInput();
    }

    void Start()
    {
        InitializePhysics();
        SpawnFootballPlayers();

        if (m_BallController == null)
        {
            m_BallController = FindObjectsByType<BallController>(FindObjectsSortMode.InstanceID)[0];
        }

        m_StartRotation = transform.localRotation;
    }

    void Update()
    {
        if(m_CurrentRodState != ERodState.Stunned && m_RodPossessed)
        {
            HandleMovement();
            HandleAim();
            ShowAimTrajectory();
        }
    }

    void OnDestroy()
    {
        m_ShootAction.started -= OnShootPressed;
        m_StanceAction.started -= OnStancePressed;
        m_StanceAction.canceled -= OnStanceReleased;
    }
#endregion

#region Initialization

    private void InitializeInput()
    {
        m_ShootAction = InputSystem.actions.FindAction("Shoot");
        m_MoveAction = InputSystem.actions.FindAction("MoveRod");
        m_AimAction = InputSystem.actions.FindAction("Aim");
        m_StanceAction = InputSystem.actions.FindAction("Stance");
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
    public void SetPossessed(bool possessed)
    {
        m_RodPossessed = possessed;

    if (possessed)
    {
        m_ShootAction.started += OnShootPressed;
        m_StanceAction.started += OnStancePressed;
        m_StanceAction.canceled += OnStanceReleased;
    }
    else
    {
        m_ShootAction.started -= OnShootPressed;
        m_StanceAction.started -= OnStancePressed;
        m_StanceAction.canceled -= OnStanceReleased;
        AimTrajectory.Instance?.Hide();
    }
    }

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
        m_CurrentRotationTween?.Kill();
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
        m_CurrentRotationTween = transform
            .DOLocalRotateQuaternion(m_StartRotation, 0.1f)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);    
    }

    private void OnEnterDefenseStance()
    {
        m_CurrentRotationTween = RotateRodTo(m_RodDefenseStanceRotation, m_DefenseStanceRotationDuration, m_DefenseStanceEase);
    }

    private void OnEnterAttackStance()
    {
        m_CurrentRotationTween = RotateRodTo(m_RodAttackStanceRotation, m_AttackStanceRotationDuration, m_AttackStanceEase);
    }

    private void OnEnterShooting()
    {
        ReleaseBall();
        GameJuiceManager.Instance?.ShakeCamera(0.15f, 0.2f);
        m_CurrentRotationTween = RotateRodTo(m_RodShootRotation, m_ShootRotationDuration, m_ShootEase)
            .OnComplete(() => SetState(ERodState.Idle));
    }

    private void OnEnterStunned()
    {
        Quaternion target = m_StartRotation * Quaternion.Euler(0, m_RodShootRotation, 0);

        Sequence seq = DOTween.Sequence().SetLink(gameObject);
        seq.Append(transform.DOLocalRotateQuaternion(target, m_StunRotationDuration).SetEase(m_StunEase));
        seq.AppendInterval(Mathf.Max(0f, m_StunDuration - m_StunRotationDuration));
        seq.OnComplete(() => SetState(ERodState.Idle));

        m_CurrentRotationTween = seq;

        GameJuiceManager.Instance?.ShakeCamera(0.3f, 0.5f);
        GameJuiceManager.Instance?.PauseGame(0.08f);
    }

    private Tween RotateRodTo(float angle, float duration, Ease ease)
    {
        Quaternion target = m_StartRotation * Quaternion.Euler(0, angle, 0);
        return transform
            .DOLocalRotateQuaternion(target, duration)
            .SetEase(ease)
            .SetLink(gameObject);
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

    private void ShowAimTrajectory()
    {
        if (AimTrajectory.Instance == null) return;

        if (!m_RodPossessed) return;

        bool shouldShow = m_CurrentRodState == ERodState.AttackStance
                    && m_HasBall
                    && m_OwnedBall != null
                    && m_AimVector.sqrMagnitude > 0.01f;

        if (shouldShow)
            AimTrajectory.Instance.SimulateTrajectory(m_OwnedBall.transform.position, m_AimVector);
        else
            AimTrajectory.Instance.Hide();
    }
#region Getters
    public ERodState GetState() => m_CurrentRodState;
    public static float GetShootAnimationDuration() => m_ShootRotationDuration;
    public Vector2 GetAim()
    {
        if(m_AimAction == null)
        {
            Debug.LogWarning("Aim action not found!");
            return Vector2.zero;
        }
        return m_AimAction.ReadValue<Vector2>();
    }
#endregion
}
