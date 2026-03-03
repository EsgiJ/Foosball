using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BallController : MonoBehaviour
{
    [UnitHeaderInspectable("Shoot Properties")]
    [SerializeField] private AnimationCurve m_ShootAnimationCurve;
    [SerializeField, Min(0f)] private float m_ShootDuration = 0.5f;
    [SerializeField, Min(0f)] private float m_ShootPower = 1000f;
    
    [Header("Collision")]
    [SerializeField] private float m_SphereRadius = 0.6f;

    /* Physics */
    private Rigidbody m_Rigidbody;
    
    /* Attachment */
    private RodController m_AttachedRod = null;
    private Vector3 m_AttachmentOffset = Vector3.zero;
    private bool m_IsAttached = false;

    /* Events */
    public static event Action<Vector3> OnBallImpact;   
#region Unity Lifecycle
    void Start()
    {
        InitializePhysics();

        RodController.OnShootEvent += HandleShoot;
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
        RodController.OnShootEvent -= HandleShoot;
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

    #region Collision
    private void OnCollisionEnter(Collision collision)
    {

    }

    private void OnCollisionExit(Collision collision)
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
    void HandleShoot(Vector2 shootDirection)
    {
        StartCoroutine(ShootCoroutine(shootDirection));
    }
#endregion

#region Couroutines
    IEnumerator ShootCoroutine(Vector2 shootDirection)
    {
        Debug.Log("Starting Shoot Coroutine with direction: " + shootDirection);

        /* Wait for shoot animation to finish */
        yield return new WaitForSeconds(RodController.GetShootAnimationDuration());

        Vector3 shootVector = Vector3.zero;
        
        shootVector.x = shootDirection.x * m_ShootPower;
        shootVector.z = shootDirection.y * m_ShootPower;
        
        m_Rigidbody.AddForce(shootVector);

        /* Detach after the shooting animation is finished*/
        DetachFromRod();
    }
#endregion

#region Getters
    public bool IsAttachedToRod() => m_IsAttached;
    public Vector3 GetLinearVelocity() => m_Rigidbody.linearVelocity;
#endregion
}
