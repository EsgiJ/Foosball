using UnityEngine;
using Foosball.Rod;

namespace Foosball
{
    public class FootballPlayerController : MonoBehaviour
    {
        [Header("Player Setup")]
        [SerializeField] private float m_PlayerRadius = 1f;
        [SerializeField] private float m_PlayerHeight = 2f;

        [SerializeField] private float m_BallVelocityThreshold = 100f;

        /* References */
        private RodController m_RodController;
        private Rigidbody m_Rigidbody;
        private CapsuleCollider m_Collider;
        private SpriteRenderer m_SpriteRenderer;

        /* Sprite References*/
        [Header("Sprites")]
        [SerializeField] private Sprite m_IdleSprite; 
        [SerializeField] private Sprite m_StunnedSprite;
        [SerializeField] private Sprite m_AttackSprite;
        [SerializeField] private Sprite m_DefenseSprite;

        /** State */
        private bool m_IsTouchingBall = false;

    #region Unity Lifecycle 
        void Start()
        {
            InitializePhysics();
            InitializeCollider();   

            m_SpriteRenderer = gameObject.transform.Find("State_Indicator").GetComponent<SpriteRenderer>();
            m_SpriteRenderer.sprite = m_IdleSprite;
        }

        void Update()
        {
            
        }

        void OnDestroy()
        {
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
            
            m_Rigidbody.isKinematic = true;
            m_Rigidbody.useGravity = false;
            m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }

        private void InitializeCollider()
        {
            m_Collider = GetComponent<CapsuleCollider>();
            if (m_Collider == null)
            {
                m_Collider = gameObject.AddComponent<CapsuleCollider>();
            }
            
            m_Collider.radius = m_PlayerRadius;
            m_Collider.height = m_PlayerHeight;
            m_Collider.isTrigger = true;  
            
            gameObject.tag = "Rod_Player";
        }
    #endregion

    #region Collider
        private void OnTriggerEnter(Collider collision)
        {
            if (collision.gameObject.CompareTag("Ball"))
            {
                BallController ball = collision.gameObject.GetComponent<BallController>();
                Vector3 ballVelocity = ball.GetLinearVelocity();
                
                Debug.Log($"Received ball velocity: {ballVelocity.magnitude}");

                if(ballVelocity.magnitude > m_BallVelocityThreshold && m_RodController.GetState() != RodController.ERodState.DefenseStance)
                {
                    m_RodController.HandleStun(ball);
                    Debug.Log($"{gameObject.name} was stunned by the ball with velocity {ballVelocity.magnitude}");
                }
                else
                {
                    m_IsTouchingBall = true;
                    
                    if (CanAttachBall(ball))
                    {
                        m_RodController.HandleBallContact(ball, transform);
                    }
                    
                    Debug.Log($"{gameObject.name} touched ball");
                }
            }
        }

        private void OnTriggerStay(Collider collision)
        {
            if (collision.gameObject.CompareTag("Ball"))
            {
                BallController ball = collision.gameObject.GetComponent<BallController>();
            
                if (CanAttachBall(ball))
                {
                    m_RodController.HandleBallContact(ball, transform);
                }
            }
        }

        private void OnTriggerExit(Collider collision)
        {
            if (collision.gameObject.CompareTag("Ball"))
            {
                m_IsTouchingBall = false;
                
                Debug.Log($"{gameObject.name} left ball");
            }
        }

        private bool CanAttachBall(BallController ball)
        {
            return ball != null && 
                m_RodController != null && 
                !ball.IsAttachedToRod() && 
                m_RodController.GetState() != RodController.ERodState.Shooting;
        }
    #endregion

    #region Rod Reference

        public void SetRodController(RodController rod)
        {
            m_RodController = rod;
        }
    #endregion

    #region State Management
        public void ChangeStateSprite(RodController.ERodState state)
        {
            switch (state)
            {
                case RodController.ERodState.Idle:
                    m_SpriteRenderer.sprite = m_IdleSprite;
                    break;
                case RodController.ERodState.AttackStance:
                    m_SpriteRenderer.sprite = m_AttackSprite;
                    break;
                case RodController.ERodState.DefenseStance:
                    m_SpriteRenderer.sprite = m_DefenseSprite;
                    break;
                case RodController.ERodState.Stunned:
                    m_SpriteRenderer.sprite = m_StunnedSprite;
                    break;
                case RodController.ERodState.Shooting:
                    m_SpriteRenderer.sprite = m_AttackSprite;
                    break;
            }
        }

    #endregion
    #region Getters
        public RodController GetRodController() => m_RodController;
        public bool IsTouchingBall() => m_IsTouchingBall;
        public Vector3 GetPosition() => transform.position;

    #endregion
    }
}