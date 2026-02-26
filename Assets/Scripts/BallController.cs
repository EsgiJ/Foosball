using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BallController : MonoBehaviour
{

    [Header("Ball Properties")]
    private bool m_IsShot = false;

    [UnitHeaderInspectable("Animation Properties")]
    [SerializeField]
    private AnimationCurve m_ShootAnimationCurve;

    [SerializeField]
    private float m_ShootDuration = 1f;
    
    [SerializeField, Min(0f)] 
    private float m_ShootPower = 10f;
    
#region Unity Lifecycle
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = true;
    }

    void Update()
    {
        
    }
    
    void OnEnable()
    {
        RodController.OnShootEvent += HandleShoot;
    }

    void OnDisable()
    {
        RodController.OnShootEvent -= HandleShoot;
    }
    
#endregion

#region Collider
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Rod_Player"))
        {
            Debug.Log("Ball hit football player");
            m_IsShot = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.CompareTag("Rod_Player"))
        {
            Debug.Log("Ball left football player");
            m_IsShot = false;
        }
    }
#endregion

#region Shoot
    void HandleShoot(Vector2 shootDirection)
    {
        if(!m_IsShot)
        {
            Debug.Log("Ball is not shot yet!");
            return;
        }
        Debug.Log("Ball is shot!");
        StartCoroutine(ShootCoroutine(shootDirection));
    }
#endregion

#region Couroutines
    IEnumerator ShootCoroutine(Vector2 shootDirection)
    {
        Debug.Log("Starting Shoot Coroutine with direction: " + shootDirection);
        float elapsed = 0;
        while(elapsed < m_ShootDuration)
        {            
            Vector3 position = transform.localPosition;
            
            elapsed += Time.deltaTime;
            float t = elapsed / m_ShootDuration;
            float curveValue = m_ShootAnimationCurve.Evaluate(t);

            position.x += shootDirection.x * curveValue * m_ShootPower * Time.deltaTime;
            position.z += shootDirection.y * curveValue * m_ShootPower * Time.deltaTime;
            
            Debug.Log($"Elapsed: {elapsed:F2}, Curve Value: {curveValue:F2}, New Position: {position}");
            GetComponent<Rigidbody>().MovePosition(position);
            yield return null;
        }
    }
#endregion
}
