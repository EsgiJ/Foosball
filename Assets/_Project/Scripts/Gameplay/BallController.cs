using DG.Tweening;
using UnityEngine;

using Infrastructure;
using Foosball.Data;

namespace Foosball.Gameplay
{
    public class BallController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private BallConfig m_BallConfig;

        /* Injected dependencies */
        private GameStateManager m_GameStateManager;
        private EventBus m_EventBus;

        /* Physics */
        private Rigidbody m_Rigidbody;

        /* Rod attachment state */
        private Tween m_AttachToRodTween;
        private RodController m_AttachedRod = null;
        private Transform m_AttachedPlayerTransform = null;
        private Vector3 m_AttachmentOffset = Vector3.zero;
        private bool m_IsAttached = false;
        private float m_LastAttachedRodShootDuration = 0f;

        /* Pass state */
        private Tween m_PassTravelTween;
        private float m_PassT;

        /* Shoot state */
        private Tween m_PendingShootTween;

    #region Unity Lifecycle
        public void Init(GameStateManager gameStateManager, EventBus eventBus)
        {
            m_GameStateManager = gameStateManager;
            m_EventBus = eventBus;

            this.Subscribe<ShootEvent>(m_EventBus, HandleShoot);
        }

        void Start()
        {
            InitializePhysics();
        }

        void Update()
        {
            if (m_IsAttached && m_AttachedPlayerTransform  != null)
            {
                UpdateAttachedMovement();
            }
        }

        void OnDestroy()
        {
            m_PendingShootTween?.Kill();
        }
    #endregion

    #region Initialization
        private void InitializePhysics()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            if (m_Rigidbody == null)
            {
                m_Rigidbody = gameObject.AddComponent<Rigidbody>();
            }

            m_Rigidbody.isKinematic = false;
            m_Rigidbody.useGravity = true;
            m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

            SphereCollider collider = GetComponent<SphereCollider>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<SphereCollider>();
            }

            collider.radius = m_BallConfig.SphereRadius;
            collider.isTrigger = false;

            gameObject.tag = "Ball";
        }
    #endregion

    #region Collider
        private void OnTriggerEnter(Collider collision)
        {
            if (m_GameStateManager == null || !m_GameStateManager.IsPlaying)
                return;
            if(collision.gameObject.CompareTag("Goal_Trigger_Zone_Home"))
            {
                // false because away scored
                m_EventBus?.Publish(new GoalEvent { IsHome = false, Position = transform.position });
            }

            if(collision.gameObject.CompareTag("Goal_Trigger_Zone_Away"))
            {
                // true because home scored
                m_EventBus?.Publish(new GoalEvent { IsHome = true, Position = transform.position });
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Wall") || m_GameStateManager == null || !m_GameStateManager.IsPlaying)
                return;

            float impact = collision.relativeVelocity.magnitude;
            if (impact < m_BallConfig.WallBounceMinImpact)
                return;

            float vol = Mathf.Clamp01(impact / m_BallConfig.WallBounceMaxImpact);
            ContactPoint c = collision.GetContact(0);

            m_EventBus?.Publish(new WallBounceEvent
            {
                Position = c.point,
                Volume = vol,
                VfxScale = Mathf.Lerp(m_BallConfig.WallHitVFXScaleMin, m_BallConfig.WallHitVFXScaleMax, vol)
            });
        }
    #endregion

    #region Movement
        private void UpdateAttachedMovement()
        {
            if (m_AttachedPlayerTransform == null) return;

            Vector3 targetPos = m_AttachedPlayerTransform.position + m_AttachmentOffset;
            targetPos.y = transform.position.y;
            transform.position = targetPos;
        }
    #endregion

    #region Rod Attachment
        public void AttachToRod(RodController rod, Transform playerTransform)
        {
            m_AttachedRod = rod;
            m_AttachedPlayerTransform = playerTransform;
            m_IsAttached = true;

            m_LastAttachedRodShootDuration = rod.GetShootAnimationDuration();

            m_AttachmentOffset = new Vector3(
                rod.IsHomeTeam() ? m_BallConfig.AttachmentSideOffset : -m_BallConfig.AttachmentSideOffset,
                0f,
                0f
            );

            Vector3 targetPosition = playerTransform.position + m_AttachmentOffset;
            targetPosition.y = transform.position.y;

            m_AttachToRodTween?.Kill();
            m_AttachToRodTween = transform.DOMove(targetPosition, m_BallConfig.AttachmentDuration).SetLink(gameObject);

            m_Rigidbody.linearVelocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;

            Debug.Log($"Ball attached to player {playerTransform.name}");
        }

        public void DetachFromRod()
        {
            if (m_IsAttached)
            {
                m_IsAttached = false;
                m_AttachedRod = null;
                m_AttachedPlayerTransform = null;
                m_AttachmentOffset = Vector3.zero;

                Debug.Log("Ball detached from rod");
            }
        }

        public void PassToPlayer(RodController rod, Transform fromPlayer, Transform toPlayer, float duration)
        {
            m_AttachToRodTween?.Kill();
            m_PassTravelTween?.Kill();

            m_AttachedRod = rod;
            m_IsAttached = true;
            m_AttachedPlayerTransform = null;
            m_LastAttachedRodShootDuration = rod.GetShootAnimationDuration();

            float offsetX = rod.IsHomeTeam() ? m_BallConfig.AttachmentSideOffset : -m_BallConfig.AttachmentSideOffset;
            m_PassT = 0f;

            m_PassTravelTween = DOTween.To(() => m_PassT, x => m_PassT = x, 1f, duration)
                .SetEase(Ease.InOutQuad)
                .OnUpdate(() =>
                {
                    Vector3 pos = Vector3.Lerp(fromPlayer.position, toPlayer.position, m_PassT);
                    pos.x += offsetX;
                    pos.y = transform.position.y;
                    transform.position = pos;
                })
                .OnComplete(() =>
                {
                    m_AttachmentOffset = new Vector3(offsetX, 0f, 0f);
                    m_AttachedPlayerTransform = toPlayer;
                })
                .SetLink(gameObject);
        }
    #endregion

    #region Shoot
        private void HandleShoot(ShootEvent shootEvent)
        {
            Vector2 shootDirection = shootEvent.Direction;

            m_PendingShootTween?.Kill();

            m_PendingShootTween = DOVirtual.DelayedCall(
                m_LastAttachedRodShootDuration,
                () =>
                {
                    Vector3 shootVector = new Vector3(
                        shootDirection.x * m_BallConfig.ShootPower,
                        0f,
                        shootDirection.y * m_BallConfig.ShootPower
                    );

                    DetachFromRod();
                    m_Rigidbody.AddForce(shootVector, ForceMode.Impulse);
                }
            ).SetLink(gameObject);
        }

        public void SimulateShoot(Vector2 shootDirection)
        {
            HandleShoot(new ShootEvent { Direction = shootDirection });
        }
    #endregion

    #region Ball Control
        public void ResetBall()
        {
            m_PendingShootTween?.Kill();

            if (m_IsAttached)
                DetachFromRod();

            m_Rigidbody.linearVelocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;
            transform.position = Vector3.zero;
        }

        public void StopBall()
        {
            m_PendingShootTween?.Kill();

            m_Rigidbody.linearVelocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;
        }
    #endregion

    #region Getters
        public bool IsAttachedToRod() => m_IsAttached;
        public Vector3 GetLinearVelocity() => m_Rigidbody.linearVelocity;
    #endregion
    }
}
