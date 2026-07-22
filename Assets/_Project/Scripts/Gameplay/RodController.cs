using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using QuickOutline;
using Infrastructure;
using Foosball.Data;

namespace Foosball.Gameplay
{
    public class RodController : MonoBehaviour
    {
        public enum ERodState
        {
            Idle,
            DefenseStance,
            AttackStance,
            Shooting,
            Stunned
        }

    #region Fields
        [Header("Config")]
        [SerializeField] private RodConfig m_RodConfig;

        [Header("Visual")]
        [SerializeField] private Transform m_VisualWrapper;

        [Header("Ball Reference")]
        [SerializeField] private GameObject m_FootballPlayer;
        [SerializeField] private BallController m_BallController;

        [Header("Pass")]
        [SerializeField] private bool  m_InvertPassDirection = false;

        /* Injected dependencies */
        private AimTrajectory m_AimTrajectory;
        private GameStateManager m_GameStateManager;
        private EventBus m_EventBus;

        /* Team & rotation state */
        private bool m_IsHomeTeam = true;
        private Quaternion m_StartRotation;

        /* Input actions */
        private InputAction m_ShootAction;
        private InputAction m_MoveAction;
        private InputAction m_AimAction;
        private InputAction m_StanceAction;
        private InputAction m_PassAction;
        private InputAction m_DashAction;
        private Gamepad m_Gamepad;

        /* Ball ownership */
        private List<GameObject> m_FootballPlayers = new List<GameObject>();
        private BallController m_OwnedBall = null;
        private Rigidbody m_Rigidbody;

        /* Pass state */
        private float m_RawAimZ;
        private int   m_HeldPlayerIndex = -1;
        private bool  m_IsPassing   = false;
        private float m_PassBaseZ;
        private Tween m_PassTween;

        /* Dash state */
        private bool m_IsDashing = false;
        private Tween m_DashTween;

        /* Aim & visual state */
        private Vector2 m_AimVector = Vector2.zero;
        private Tween m_CurrentRotationTween;
        private Tween m_WiggleTween;
        private Tween m_StruggleTween;
        private Tween m_OutlineTween;
        private List<Outline> m_Outlines = new();

        /* Rod state */
        private ERodState m_CurrentRodState = ERodState.Idle;
        private bool m_IsStanceHeld = false;
        private bool m_HasBall = false;
        public bool m_RodPossessed = false;
    #endregion

    #region Unity Lifecycle
        public void Init(AimTrajectory aimTrajectory, GameStateManager gameStateManager, EventBus eventBus)
        {
            m_AimTrajectory = aimTrajectory;
            m_GameStateManager = gameStateManager;
            m_EventBus = eventBus;
        }

        void Awake()
        {

        }

        void Start()
        {
            InitializePhysics();
            SpawnFootballPlayers();
            CollectOutlines();

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
                if (!m_IsPassing && !m_IsDashing)
                {
                    HandleMovement();
                }
                HandleAim();
                ShowAimTrajectory();
            }
        }

        void OnDestroy()
        {
            m_ShootAction.started -= OnShootPressed;
            m_StanceAction.started -= OnStancePressed;
            m_StanceAction.canceled -= OnStanceReleased;
            m_PassAction.started   -= OnPassPressed;
            m_DashAction.started   -= OnDashPressed;
        }
    #endregion

    #region Initialization
        public void InitializeInput(InputActionAsset actions)
        {
            m_ShootAction  = actions.FindAction("Shoot");
            m_MoveAction   = actions.FindAction("MoveRod");
            m_AimAction    = actions.FindAction("Aim");
            m_StanceAction = actions.FindAction("Stance");
            m_PassAction   = actions.FindAction("Pass");
            m_DashAction = actions.FindAction("Dash");
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
            Transform parent = m_VisualWrapper != null ? m_VisualWrapper : transform;

            for (int i = 0; i < m_RodConfig.FootballPlayerCount; i++)
            {
                Vector3 spawnPosition = CalculateFootballPlayerPosition(i);
                GameObject player = Instantiate(m_FootballPlayer, spawnPosition, m_FootballPlayer.transform.rotation);

                player.transform.SetParent(parent, true);

                m_FootballPlayers.Add(player);
                var playerController = player.GetComponent<FootballPlayerController>();
                playerController.SetRodController(this);
                playerController.Init(m_GameStateManager);
            }
            ApplyTeamColor();
        }

        Vector3 CalculateFootballPlayerPosition(int index)
        {
            float spacing = m_RodConfig.UsableExtent / (m_RodConfig.FootballPlayerCount + 1);
            float zOffset = (index + 1) * spacing - (m_RodConfig.UsableExtent / 2f);
            return transform.position + new Vector3(0f, 0f, zOffset);
        }

        private void CollectOutlines()
        {
            m_Outlines.Clear();

            var outlines = GetComponentsInChildren<Outline>(includeInactive: true);
            outlines.Append(GetComponent<Outline>());

            foreach (var o in outlines)
            {
                m_Outlines.Add(o);
                o.OutlineColor = m_RodConfig.OutlineColor;
                o.OutlineWidth = 0f;
                o.enabled = false;
            }
        }

        private void ApplyTeamColor()
        {
            var renderers = GetComponentsInChildren<Renderer>(true);

            Color teamColor = IsHomeTeam() ? m_RodConfig.HomeColor : m_RodConfig.AwayColor;

            foreach (var r in renderers)
            {
                if (r == null)
                    continue;

                Material mat = r.material;
                mat.color = teamColor;
            }
        }
    #endregion

    #region Possession
        public void SetGamepad(Gamepad pad) => m_Gamepad = pad;

        public void SetPossessed(bool possessed)
        {
            m_RodPossessed = possessed;

            if (possessed)
            {
                m_ShootAction.started += OnShootPressed;
                m_StanceAction.started += OnStancePressed;
                m_StanceAction.canceled += OnStanceReleased;
                m_PassAction.started   += OnPassPressed;
                m_DashAction.started   += OnDashPressed;
                m_EventBus?.Publish(new PossessSwitchEvent());
                PlayWiggleEffect();
                ShowOutline();
            }
            else
            {
                m_ShootAction.started -= OnShootPressed;
                m_StanceAction.started -= OnStancePressed;
                m_StanceAction.canceled -= OnStanceReleased;
                m_PassAction.started   -= OnPassPressed;
                m_DashAction.started   -= OnDashPressed;

                m_AimTrajectory?.Hide(this);

                HideOutline();
            }
        }

        private void PlayWiggleEffect()
        {
            if (m_VisualWrapper == null) return;

            m_WiggleTween?.Kill(true);

            Quaternion baseRot = Quaternion.identity;
            Quaternion punchTarget = baseRot * Quaternion.Euler(0, m_RodConfig.WigglePunchRotation, 0);

            Sequence seq = DOTween.Sequence().SetLink(gameObject);

            seq.Join(m_VisualWrapper.DOPunchPosition(
                new Vector3(0f, 0f, m_RodConfig.WigglePunchAmount),
                m_RodConfig.WiggleDuration,
                m_RodConfig.WiggleVibrato,
                m_RodConfig.WiggleElasticity
            ));

            seq.Join(m_VisualWrapper.DOPunchRotation(
                new Vector3(0, m_RodConfig.WigglePunchRotation, 0),
                m_RodConfig.WiggleDuration,
                m_RodConfig.WiggleVibrato,
                m_RodConfig.WiggleElasticity
            ));

            m_WiggleTween = seq;
        }

        private void ShowOutline()
        {
            m_OutlineTween?.Kill();

            foreach (var o in m_Outlines)
            {
                if (o != null) o.enabled = true;
            }

            m_OutlineTween = DOVirtual.Float(0f, m_RodConfig.OutlineWidth, m_RodConfig.OutlineFadeDuration,
                v =>
                {
                    foreach (var o in m_Outlines)
                        if (o != null) o.OutlineWidth = v;
                })
                .SetEase(m_RodConfig.OutlineShowEase)
                .SetLink(gameObject);
        }

        private void HideOutline()
        {
            m_OutlineTween?.Kill();

            m_OutlineTween = DOVirtual.Float(m_RodConfig.OutlineWidth, 0f, m_RodConfig.OutlineFadeDuration,
                v =>
                {
                    foreach (var o in m_Outlines)
                        if (o != null) o.OutlineWidth = v;
                })
                .SetEase(m_RodConfig.OutlineHideEase)
                .OnComplete(() =>
                {
                    foreach (var o in m_Outlines)
                        if (o != null) o.enabled = false;
                })
                .SetLink(gameObject);
        }
    #endregion

    #region Movement
        private void HandleMovement()
        {
            if(m_MoveAction == null)
            {
                Debug.LogWarning("MoveRod action not found!");
                return;
            }

            Vector3 position = transform.localPosition;

            if (position.z < m_RodConfig.MinZPos)
                position.z = m_RodConfig.MinZPos;
            else if (position.z > m_RodConfig.MaxZPos)
                position.z = m_RodConfig.MaxZPos;
            else
                position.z += m_MoveAction.ReadValue<float>() * m_RodConfig.MovementSpeed * Time.deltaTime;

            transform.localPosition = position;
        }
    #endregion

    #region Aim & Shoot
        private void HandleAim()
        {
            if(m_AimAction == null)
            {
                Debug.LogWarning("Aim action not found!");
                return;
            }

            Vector2 raw = m_AimAction.ReadValue<Vector2>();
            m_RawAimZ = raw.y;

            m_AimVector = raw;
            m_AimVector.x = IsHomeTeam() ? 1f : -1f;
        }

        private void ShowAimTrajectory()
        {
            if (m_AimTrajectory == null || !m_RodPossessed)
                return;

            bool shouldShow = m_CurrentRodState == ERodState.AttackStance && m_HasBall && m_OwnedBall != null;

            if (shouldShow)
            {
                m_AimTrajectory.Show(this);
                m_AimTrajectory.SimulateTrajectory(this, m_OwnedBall.transform.position, m_AimVector);
            }
            else
            {
                m_AimTrajectory.Hide(this);
            }
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
            m_EventBus?.Publish(new ShootEvent { Direction = m_AimVector });

            m_HasBall = false;
        }
    #endregion

    #region Pass
        private void OnPassPressed(InputAction.CallbackContext context)
        {
            if (m_CurrentRodState == ERodState.Stunned)
                return;

            if (!m_HasBall || m_CurrentRodState != ERodState.AttackStance || m_IsPassing)
                return;

            if (Mathf.Abs(m_RawAimZ) < m_RodConfig.PassInputDeadzone)
                return;

            int dir = m_RawAimZ > 0 ? 1 : -1;
            if (m_InvertPassDirection)
            {
                dir = -dir;
            }
            TryPass(dir);
        }

        private void TryPass(int dir)
        {
            if (!m_HasBall || m_OwnedBall == null)
                return;

            int target = m_HeldPlayerIndex + dir;
            if (target < 0 || target >= m_FootballPlayers.Count)
                return;

            m_IsPassing = true;
            m_EventBus?.Publish(new PassAttemptedEvent());
            PlayPassFlick(dir, target);
        }

        private void PlayPassFlick(int dir, int targetIndex)
        {
            m_PassTween?.Kill();
            m_PassBaseZ = transform.localPosition.z;
            float z = m_PassBaseZ;

            Transform fromPlayer = m_FootballPlayers[m_HeldPlayerIndex].transform;
            Transform toPlayer   = m_FootballPlayers[targetIndex].transform;

            m_PassTween = DOTween.Sequence().SetLink(gameObject)
                .Append(transform.DOLocalMoveZ(z - dir * m_RodConfig.PassWindupDistance, m_RodConfig.PassWindupTime).SetEase(m_RodConfig.PassWindupEase))
                .AppendCallback(() =>
                {
                    if (m_OwnedBall != null)
                        m_OwnedBall.PassToPlayer(this, fromPlayer, toPlayer, m_RodConfig.PassThrowTime + m_RodConfig.PassSettleTime);
                    m_HeldPlayerIndex = targetIndex;
                })
                .Append(transform.DOLocalMoveZ(z + dir * m_RodConfig.PassThrowDistance, m_RodConfig.PassThrowTime).SetEase(m_RodConfig.PassThrowEase))
                .Append(transform.DOLocalMoveZ(z, m_RodConfig.PassSettleTime).SetEase(m_RodConfig.PassSettleEase))
                .OnComplete(() => m_IsPassing = false);
        }

        private void CancelPass()
        {
            if (!m_IsPassing)
                return;
            m_PassTween?.Kill();
            var p = transform.localPosition;
            p.z = m_PassBaseZ;
            transform.localPosition = p;
            m_IsPassing = false;
        }
    #endregion

    #region Dash
        private void OnDashPressed(InputAction.CallbackContext ctx)
        {
            if (m_CurrentRodState != ERodState.DefenseStance || m_IsDashing) return;
            if (m_MoveAction == null) return;

            float axis = m_MoveAction.ReadValue<float>();
            if (Mathf.Abs(axis) < m_RodConfig.DashInputDeadzone)
                return;
            DefenseDash(axis > 0 ? 1 : -1);
        }

        private void DefenseDash(int dir)
        {
            m_DashTween?.Kill();
            m_IsDashing = true;

            float baseZ = transform.localPosition.z;
            float target = Mathf.Clamp(baseZ + dir * m_RodConfig.DashDistance, m_RodConfig.MinZPos, m_RodConfig.MaxZPos);

            m_EventBus?.Publish(new DashAttemptedEvent());

            m_DashTween = DOTween.Sequence().SetLink(gameObject)
                .Append(transform.DOLocalMoveZ(target, m_RodConfig.DashTime).SetEase(m_RodConfig.DashMoveEase))
                .Append(transform.DOLocalMoveZ(target, m_RodConfig.DashSettleTime).SetEase(m_RodConfig.DashSettleEase))
                .OnComplete(() => m_IsDashing = false);
        }
    #endregion

    #region Rod State Machine
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

        private void OnEnterIdle()
        {
            m_DashTween?.Kill();
            m_IsDashing = false;

            ReleaseBall();
            m_CurrentRotationTween = transform
                .DOLocalRotateQuaternion(m_StartRotation, m_RodConfig.IdleReturnDuration)
                .SetEase(m_RodConfig.IdleReturnEase)
                .SetLink(gameObject);
        }

        private void OnEnterDefenseStance()
        {
            m_EventBus?.Publish(new StanceChangedEvent());
            m_CurrentRotationTween = RotateRodTo(m_RodConfig.DefenseRotation, m_RodConfig.DefenseStanceDuration, m_RodConfig.DefenseStanceEase);
        }

        private void OnEnterAttackStance()
        {
            m_DashTween?.Kill();
            m_IsDashing = false;

            m_EventBus?.Publish(new StanceChangedEvent());
            float rodRotationBasedOnTeam = m_RodConfig.AttackRotation * (IsHomeTeam() ? 1 : -1);
            m_CurrentRotationTween = RotateRodTo(rodRotationBasedOnTeam, m_RodConfig.AttackStanceDuration, m_RodConfig.AttackStanceEase);
        }

        private void OnEnterShooting()
        {
            ReleaseBall();
            m_CurrentRotationTween = RotateRodTo(m_RodConfig.ShootRotation, m_RodConfig.ShootDuration, m_RodConfig.ShootEase)
                .OnComplete(() => SetState(ERodState.Idle));
        }

        private void OnEnterStunned()
        {
            Quaternion target = m_StartRotation * Quaternion.Euler(0, m_RodConfig.ShootRotation, 0);

            Sequence seq = DOTween.Sequence().SetLink(gameObject);
            seq.Append(transform.DOLocalRotateQuaternion(target, m_RodConfig.StunRotationDuration).SetEase(m_RodConfig.StunEase));
            seq.AppendInterval(Mathf.Max(0f, m_RodConfig.StunDuration - m_RodConfig.StunRotationDuration));
            seq.OnComplete(() => SetState(ERodState.Idle));

            m_CurrentRotationTween = seq;
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
            foreach(GameObject player in m_FootballPlayers)
            {
                FootballPlayerController controller = player.GetComponent<FootballPlayerController>();
                controller.ChangeStateSprite(state);
            }
        }
    #endregion

    #region Ball Contact & Stun
        public void HandleBallContact(BallController ball, Transform contactingPlayer)
        {
            if (m_IsPassing)
                return;
            m_OwnedBall = ball;
            m_HasBall = true;

            if (m_IsStanceHeld && m_CurrentRodState == ERodState.DefenseStance)
            {
                Vector3 ballVelocity = ball.GetLinearVelocity();
                float s = Mathf.Clamp(ballVelocity.magnitude / m_RodConfig.BlockVfxSpeedNormalizer, m_RodConfig.BlockVfxScaleMin, m_RodConfig.BlockVfxScaleMax);

                PlayStruggleEffect(ball);
                ball.StopBall();
                AttachBallToRod(ball, contactingPlayer);

                m_EventBus?.Publish(new BlockEvent
                {
                    Position = contactingPlayer.position,
                    VfxScale = s,
                    BallVelocityX = ballVelocity.x,
                    BallSpeed = ballVelocity.magnitude,
                    Gamepad = m_Gamepad
                });

                SetState(ERodState.AttackStance);
            }
            else if (m_IsStanceHeld && m_CurrentRodState == ERodState.AttackStance)
            {
                AttachBallToRod(ball, contactingPlayer);
            }
        }

        public void HandleStun(BallController ball)
        {
            m_EventBus?.Publish(new RodStunnedEvent { Position = ball.transform.position, Gamepad = m_Gamepad });
            SetState(ERodState.Stunned);
        }

        public void ReleaseBall()
        {
            CancelPass();

            if (m_OwnedBall != null)
            {
                m_OwnedBall.DetachFromRod();
                m_OwnedBall = null;
                m_HasBall = false;
                m_HeldPlayerIndex = -1;

                Debug.Log("Ball released from rod");
            }
        }

        private void AttachBallToRod(BallController ball, Transform playerTransform)
        {
            ball.AttachToRod(this, playerTransform);
            m_HeldPlayerIndex = m_FootballPlayers.IndexOf(playerTransform.gameObject);
            Debug.Log($"Ball attached to rod via {playerTransform.name}");
        }

        private void PlayStruggleEffect(BallController ball)
        {
            if (m_VisualWrapper == null) return;

            m_StruggleTween?.Kill();

            Vector3 ballVelocity = ball.GetLinearVelocity();
            if (Mathf.Abs(ballVelocity.x) < 0.1f) return;

            float direction = Mathf.Sign(ballVelocity.x);
            float intensity = Mathf.Clamp01(ballVelocity.magnitude / 30f);

            Vector3 scaledPunch = m_RodConfig.StrugglePunchAngle * direction * intensity;

            m_StruggleTween = m_VisualWrapper.DOPunchRotation(
                scaledPunch,
                m_RodConfig.StruggleDuration,
                m_RodConfig.StruggleVibrato,
                m_RodConfig.StruggleElasticity
            ).SetLink(gameObject);
        }
    #endregion

    #region Team Setup
        public void SetPlayerCount(int count)
        {
            m_RodConfig.FootballPlayerCount = Mathf.Clamp(count, 1, 5);
            foreach (var p in m_FootballPlayers) if (p != null) Destroy(p);
            m_FootballPlayers.Clear();
            SpawnFootballPlayers();
        }
    #endregion

    #region Getters
        public ERodState GetState() => m_CurrentRodState;
        public float GetShootAnimationDuration() => m_RodConfig.ShootDuration;

        public Vector2 GetAim()
        {
            if(m_AimAction == null)
            {
                Debug.LogWarning("Aim action not found!");
                return Vector2.zero;
            }
            return m_AimAction.ReadValue<Vector2>();
        }

        public bool IsHomeTeam() => m_IsHomeTeam;
        public void SetIsHomeTeam(bool isHome) => m_IsHomeTeam = isHome;
    #endregion
    }
}
