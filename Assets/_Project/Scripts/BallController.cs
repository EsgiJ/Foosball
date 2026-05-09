using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

using Foosball.Rod;

namespace Foosball
{
    public class BallController : MonoBehaviour
    {
        [UnitHeaderInspectable("Shoot Properties")]
        [SerializeField, Min(0f)] private float m_ShootPower = 10f;
        
        [Header("Collision")]
        [SerializeField] private float m_SphereRadius = 0.6f;

        /* Physics */
        private Rigidbody m_Rigidbody;
        
        /* Attachment */
        [SerializeField] private float m_AttachmentDuration = 0.1f;

        private Tween m_AttachToRodTween;
        private RodController m_AttachedRod = null;
        private Transform m_AttachedPlayerTransform = null;
        private Vector3 m_AttachmentOffset = Vector3.zero;
        private bool m_IsAttached = false;
        private Tween m_PendingShootTween;

        private float m_LastAttachedRodShootDuration = 0f;

    #region Unity Lifecycle
        void Start()
        {
            InitializePhysics();

            GameEvents.OnShootEvent += HandleShoot;
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
            GameEvents.OnShootEvent -= HandleShoot;
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
            
            collider.radius = m_SphereRadius;
            collider.isTrigger = false;
            
            gameObject.tag = "Ball";
        }
        #endregion

    #region Collider
        private void OnTriggerEnter(Collider collision)
        {
            if(collision.gameObject.CompareTag("Goal_Trigger_Zone_Home"))
            {
                // false because away scored
                GameEvents.RaiseGoalEvent(false);
                AudioManager.Instance?.PlayGoal();
            }

            if(collision.gameObject.CompareTag("Goal_Trigger_Zone_Away"))
            {
                // true because home scored
                GameEvents.RaiseGoalEvent(true);
                AudioManager.Instance?.PlayGoal();
            }
        }

        private void OnTriggerExit(Collider collision)
        {

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
                rod.IsHomeTeam() ? 0.25f : -0.25f,
                0f,
                0f
            );

            Vector3 targetPosition = playerTransform.position + m_AttachmentOffset;
            targetPosition.y = transform.position.y;

            m_AttachToRodTween?.Kill();
            m_AttachToRodTween = transform.DOMove(targetPosition, m_AttachmentDuration).SetLink(gameObject);

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

    #region Shoot
        private void HandleShoot(Vector2 shootDirection)
        {
            m_PendingShootTween?.Kill();

            m_PendingShootTween = DOVirtual.DelayedCall(
                m_LastAttachedRodShootDuration,
                () =>
                {
                    Vector3 shootVector = new Vector3(
                        shootDirection.x * m_ShootPower,
                        0f,
                        shootDirection.y * m_ShootPower
                    );

                    DetachFromRod();
                    m_Rigidbody.AddForce(shootVector, ForceMode.Impulse);
                }
            ).SetLink(gameObject);
        }

        public void SimulateShoot(Vector2 shootDirection)
        {
            HandleShoot(shootDirection);
        }
    #endregion

    #region Getters
        public bool IsAttachedToRod() => m_IsAttached;
        public Vector3 GetLinearVelocity() => m_Rigidbody.linearVelocity;
    #endregion
    }
}