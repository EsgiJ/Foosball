using UnityEngine;

public class FootballPlayerController : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private float m_PlayerRadius = 1f;
    [SerializeField] private float m_PlayerHeight = 1.5f;

    /* References */
    private RodController m_RodController;
    private Rigidbody m_Rigidbody;
    private CapsuleCollider m_Collider;

    /** State */
    private bool m_IsTouchingBall = false;

#region Unity Lifecycle 
    void Start()
    {
        InitializePhysics();
        InitializeCollider();    
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
            m_IsTouchingBall = true;
            
            BallController ball = collision.gameObject.GetComponent<BallController>();
            if (CanAttachBall(ball))
            {
                m_RodController.HandleBallContact(ball);
            }
            {
                m_RodController.HandleBallContact(ball);
            }
            
            Debug.Log($"{gameObject.name} touched ball");
        }
    }

    private void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            m_IsTouchingBall = true;
            
            BallController ball = collision.gameObject.GetComponent<BallController>();
            if (CanAttachBall(ball))
            {
                m_RodController.HandleBallContact(ball);
            }
            
            Debug.Log($"{gameObject.name} touched ball");
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

#region Getters
    public RodController GetRodController() => m_RodController;
    public bool IsTouchingBall() => m_IsTouchingBall;
    public Vector3 GetPosition() => transform.position;

#endregion
}
