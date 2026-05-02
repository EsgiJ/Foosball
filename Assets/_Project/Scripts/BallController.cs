using System;
using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

namespace Foosball
{
    public class BallController : MonoBehaviour
    {
        [UnitHeaderInspectable("Shoot Properties")]
        [SerializeField] private AnimationCurve m_ShootAnimationCurve;
        [SerializeField, Min(0f)] private float m_ShootPower = 1000f;
        
        [Header("Collision")]
        [SerializeField] private float m_SphereRadius = 0.6f;

        /* Physics */
        private Rigidbody m_Rigidbody;
        
        /* Attachment */
        private RodController m_AttachedRod = null;
        private Vector3 m_AttachmentOffset = Vector3.zero;
        private bool m_IsAttached = false;

        private Tween m_PendingShootTween;
         
    #region Unity Lifecycle
        void Start()
        {
            InitializePhysics();

            GameEvents.OnShootEvent += HandleShoot;
        }

        void Update()
        {
            if (m_IsAttached && m_AttachedRod != null)
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
            }

            if(collision.gameObject.CompareTag("Goal_Trigger_Zone_Away"))
            {
                // true because home scored
                GameEvents.RaiseGoalEvent(true);
            }
        }

        private void OnTriggerExit(Collider collision)
        {

        }
    #endregion

    #region Movement
        private void UpdateAttachedMovement()
        {
            Vector3 rodPos = m_AttachedRod.transform.position;
            
            Vector3 targetPos = rodPos + m_AttachmentOffset;
            
            transform.position = targetPos;
        }
    #endregion

    #region Rod Attachment

        public void AttachToRod(RodController rod)
        {
            m_AttachedRod = rod;
            m_IsAttached = true;
            
            m_AttachmentOffset = transform.position - rod.transform.position;
                    
            Debug.Log($"Ball attached to rod - offset: {m_AttachmentOffset}");
        }

        public void DetachFromRod()
        {
            if (m_IsAttached)
            {
                m_IsAttached = false;
                m_AttachedRod = null;
                m_AttachmentOffset = Vector3.zero;
                
                Debug.Log("Ball detached from rod");
            }
        }

    #endregion

    #region Shoot
        private void HandleShoot(Vector2 shootDirection)
        {
            m_PendingShootTween?.Kill();

            m_PendingShootTween = DOVirtual.DelayedCall(
                RodController.GetShootAnimationDuration(),
                () =>
                {
                    Vector3 shootVector = new Vector3(
                        shootDirection.x * m_ShootPower,
                        0f,
                        shootDirection.y * m_ShootPower
                    );

                    DetachFromRod();
                    m_Rigidbody.AddForce(shootVector);
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