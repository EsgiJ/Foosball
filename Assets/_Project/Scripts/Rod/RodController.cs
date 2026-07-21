using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using QuickOutline;

namespace Foosball.Rod
{
    public class RodController : MonoBehaviour
    {
    #region Properties 
        /* Rod General Properties */
        [SerializeField] private RodConfig m_RodConfig;

        [Header("Visual")]
        [SerializeField] private Transform m_VisualWrapper;

        private bool m_IsHomeTeam = true;
        private Quaternion m_StartRotation;

        /* References */
        [Header("Ball Reference")]
        [SerializeField] private GameObject m_FootballPlayer;
        [SerializeField] private BallController m_BallController;

        [Header("Pass")]
        [SerializeField] private float m_PassWindup = 0.3f;   
        [SerializeField] private float m_PassThrow  = 0.5f;   
        [SerializeField] private float m_PassWindupTime = 0.07f;
        [SerializeField] private float m_PassThrowTime  = 0.08f;
        [SerializeField] private float m_PassSettleTime = 0.07f;
        [SerializeField] private bool  m_InvertPassDirection = false;

        private float m_RawAimZ;
        private int   m_HeldPlayerIndex = -1;
        private bool  m_PassLatched = false;
        private bool  m_IsPassing   = false;
        private float m_PassBaseZ;
        private Tween m_PassTween;

        [Header("Defense Dash")]
        [SerializeField] private float m_DashDistance = 20.0f;
        [SerializeField] private float m_DashTime = 0.09f;
        [SerializeField] private float m_DashSettleTime = 0.12f;
        private bool m_IsDashing = false;
        private Tween m_DashTween;
        private InputAction m_DashAction;


        private Gamepad m_Gamepad;
        public void SetGamepad(Gamepad pad) => m_Gamepad = pad;

        private List<GameObject> m_FootballPlayers = new List<GameObject>();
        private BallController m_OwnedBall = null;
        private Rigidbody m_Rigidbody;

        InputAction m_ShootAction;
        InputAction m_MoveAction;
        InputAction m_AimAction ;
        InputAction m_StanceAction;
        InputAction m_PassAction;

        private Vector2 m_AimVector = Vector2.zero;
        private Tween m_CurrentRotationTween;
        private Tween m_WiggleTween;
        private Tween m_StruggleTween;
        private Tween m_OutlineTween;
        private List<Outline> m_Outlines = new();

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
                player.GetComponent<FootballPlayerController>().SetRodController(this);
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
                m_PassAction.started   += OnPassPressed;
                m_DashAction.started   += OnDashPressed;
                AudioManager.Instance?.PlayPossessSwitch();
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

                AimTrajectory.Instance?.Hide(this);

                HideOutline();
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

            if (position.z < m_RodConfig.MinZPos)
                position.z = m_RodConfig.MinZPos;
            else if (position.z > m_RodConfig.MaxZPos)
                position.z = m_RodConfig.MaxZPos;
            else
                position.z += m_MoveAction.ReadValue<float>() * m_RodConfig.MovementSpeed * Time.deltaTime;

            transform.localPosition = position;
        }

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

        private void HandlePassInput(float rawX)
        {
            if (!m_HasBall || m_CurrentRodState != ERodState.AttackStance || m_IsPassing)
            {
                if (Mathf.Abs(rawX) < 0.25f) 
                    m_PassLatched = false;
                return;
            }

            if (Mathf.Abs(rawX) > 0.5f && !m_PassLatched)
            {
                int dir = rawX > 0 ? 1 : -1;
                if (m_InvertPassDirection) 
                    dir = -dir;
                TryPass(dir);
                m_PassLatched = true;          
            }
            else if (Mathf.Abs(rawX) < 0.25f)
            {
                m_PassLatched = false;        
            }
        }

        private void TryPass(int dir)
        {
            if (!m_HasBall || m_OwnedBall == null) 
                return;

            int target = m_HeldPlayerIndex + dir;
            if (target < 0 || target >= m_FootballPlayers.Count) 
                return;   

            m_IsPassing = true;
            AudioManager.Instance?.PlayStanceClick();          
            GameJuiceManager.Instance?.ShakeCamera(0.08f, 0.1f);
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
                .Append(transform.DOLocalMoveZ(z - dir * m_PassWindup, m_PassWindupTime).SetEase(Ease.OutQuad))   
                .AppendCallback(() =>
                {
                    if (m_OwnedBall != null)
                        m_OwnedBall.PassToPlayer(this, fromPlayer, toPlayer, m_PassThrowTime + m_PassSettleTime); 
                    m_HeldPlayerIndex = targetIndex;
                })
                .Append(transform.DOLocalMoveZ(z + dir * m_PassThrow, m_PassThrowTime).SetEase(Ease.OutBack))     
                .Append(transform.DOLocalMoveZ(z, m_PassSettleTime).SetEase(Ease.OutQuad))                        
                .OnComplete(() => m_IsPassing = false);
        }

        private void ReattachBallTo(int index)
        {
            if (m_OwnedBall == null || index < 0 || index >= m_FootballPlayers.Count) 
                return;
            m_OwnedBall.AttachToRod(this, m_FootballPlayers[index].transform);   
            m_HeldPlayerIndex = index;
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

        private void AttachBallToRod(BallController ball, Transform playerTransform)
        {
            ball.AttachToRod(this, playerTransform);
            m_HeldPlayerIndex = m_FootballPlayers.IndexOf(playerTransform.gameObject);
            Debug.Log($"Ball attached to rod via {playerTransform.name}");
        }

        private void OnDashPressed(InputAction.CallbackContext ctx)
        {
            if (m_CurrentRodState != ERodState.DefenseStance || m_IsDashing) return;
            if (m_MoveAction == null) return;

            float axis = m_MoveAction.ReadValue<float>();
            if (Mathf.Abs(axis) < 0.2f) 
                return;       
            DefenseDash(axis > 0 ? 1 : -1);
        }

        private void DefenseDash(int dir)
        {
            m_DashTween?.Kill();
            m_IsDashing = true;

            float baseZ = transform.localPosition.z;
            float target = Mathf.Clamp(baseZ + dir * m_DashDistance, m_RodConfig.MinZPos, m_RodConfig.MaxZPos);

            AudioManager.Instance?.PlayStanceClick();   

            m_DashTween = DOTween.Sequence().SetLink(gameObject)
                .Append(transform.DOLocalMoveZ(target, m_DashTime).SetEase(Ease.OutQuad))        
                .Append(transform.DOLocalMoveZ(target, m_DashSettleTime).SetEase(Ease.OutBack))  
                .OnComplete(() => m_IsDashing = false);
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
            GameEvents.RaiseShootEvent(m_AimVector);
            AudioManager.Instance?.PlayShoot();

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

        private void OnPassPressed(InputAction.CallbackContext context)
        {
            if (m_CurrentRodState == ERodState.Stunned) 
                return;

            if (!m_HasBall || m_CurrentRodState != ERodState.AttackStance || m_IsPassing) 
                return;

            if (Mathf.Abs(m_RawAimZ) < 0.3f) 
                return;     

            int dir = m_RawAimZ > 0 ? 1 : -1;
            if (m_InvertPassDirection) 
            {
                dir = -dir;
            }
            TryPass(dir);
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
            m_DashTween?.Kill();
            m_IsDashing = false;

            ReleaseBall();
            m_CurrentRotationTween = transform
                .DOLocalRotateQuaternion(m_StartRotation, 0.1f)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject);    
        }

        private void OnEnterDefenseStance()
        {
            AudioManager.Instance?.PlayStanceClick();
            m_CurrentRotationTween = RotateRodTo(m_RodConfig.DefenseRotation, m_RodConfig.DefenseStanceDuration, m_RodConfig.DefenseStanceEase);
        }

        private void OnEnterAttackStance()
        {
            m_DashTween?.Kill();
            m_IsDashing = false;

            AudioManager.Instance?.PlayStanceClick();
            float rodRotationBasedOnTeam = m_RodConfig.AttackRotation * (IsHomeTeam() ? 1 : -1);
            m_CurrentRotationTween = RotateRodTo(rodRotationBasedOnTeam, m_RodConfig.AttackStanceDuration, m_RodConfig.AttackStanceEase);
        }

        private void OnEnterShooting()
        {
            ReleaseBall();
            GameJuiceManager.Instance?.ShakeCamera(0.15f, 0.2f);
            m_CurrentRotationTween = RotateRodTo(m_RodConfig.ShootRotation, m_RodConfig.ShootDuration, m_RodConfig.ShootEase)
                .OnComplete(() => SetState(ERodState.Idle));
        }

        private void OnEnterStunned()
        {
            AudioManager.Instance?.PlayStun();

            Quaternion target = m_StartRotation * Quaternion.Euler(0, m_RodConfig.ShootRotation, 0);

            Sequence seq = DOTween.Sequence().SetLink(gameObject);
            seq.Append(transform.DOLocalRotateQuaternion(target, m_RodConfig.StunRotationDuration).SetEase(m_RodConfig.StunEase));
            seq.AppendInterval(Mathf.Max(0f, m_RodConfig.StunDuration - m_RodConfig.StunRotationDuration));
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
            foreach(GameObject player in m_FootballPlayers)
            {
                FootballPlayerController controller = player.GetComponent<FootballPlayerController>();
                controller.ChangeStateSprite(state);
            }
        }
    #endregion

    #region Event Handling
        public void HandleBallContact(BallController ball, Transform contactingPlayer)
        {
            if (m_IsPassing) 
                return;
            m_OwnedBall = ball;
            m_HasBall = true;

            if (m_IsStanceHeld && m_CurrentRodState == ERodState.DefenseStance)
            {
                float s = Mathf.Clamp(ball.GetLinearVelocity().magnitude / 15f, 0.6f, 1.5f);
                VFXManager.Instance?.PlayBlock(contactingPlayer.position, s);

                PlayStruggleEffect(ball);
                ball.StopBall();
                AttachBallToRod(ball, contactingPlayer);
                AudioManager.Instance?.PlayDefenseCatch();
                RumbleManager.Instance?.RumbleBlock(m_Gamepad);

                SetState(ERodState.AttackStance);
            }   
            else if (m_IsStanceHeld && m_CurrentRodState == ERodState.AttackStance)
            {
                AttachBallToRod(ball, contactingPlayer);
            }
        }

        public void HandleStun(BallController ball)
        {
            RumbleManager.Instance?.RumbleStun(m_Gamepad);
            VFXManager.Instance?.PlayStun(ball.transform.position);
            SetState(ERodState.Stunned);
        }

    #endregion

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

        private void ShowAimTrajectory()
        {
            if (AimTrajectory.Instance == null || !m_RodPossessed) 
                return;


            bool shouldShow = m_CurrentRodState == ERodState.AttackStance && m_HasBall && m_OwnedBall != null;

            if (shouldShow)
            {
                AimTrajectory.Instance.Show(this);
                AimTrajectory.Instance.SimulateTrajectory(this, m_OwnedBall.transform.position, m_AimVector);
            }
            else
            {
                AimTrajectory.Instance.Hide(this);
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

            GameJuiceManager.Instance?.ShakeCamera(0.15f, intensity * 0.25f);
            if (intensity > 0.6f)
                GameJuiceManager.Instance?.PauseGame(0.1f);
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
                .SetEase(Ease.OutBack)
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
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    foreach (var o in m_Outlines)
                        if (o != null) o.enabled = false;
                })
                .SetLink(gameObject);
        }

        public void SetPlayerCount(int count)
        {
            m_RodConfig.FootballPlayerCount = Mathf.Clamp(count, 1, 5);
            foreach (var p in m_FootballPlayers) if (p != null) Destroy(p);
            m_FootballPlayers.Clear();
            SpawnFootballPlayers();
        }

        private void ApplyTeamColor()
        {
            var renderers = GetComponentsInChildren<Renderer>(true);

            Color teamColor = IsHomeTeam() ? Color.red : new Color(0.20f, 0.50f, 1.00f);;

            foreach (var r in renderers)
            {
                if (r == null) 
                    continue;

                Material mat = r.material;
                mat.color = teamColor;
            }
        }
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

