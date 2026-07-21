using UnityEngine;

namespace Foosball
{
    [RequireComponent(typeof(Rigidbody))]
    public class BallTrail : MonoBehaviour
    {
        [SerializeField] private TrailRenderer m_Trail;

        [Header("Speed Interval")]
        [SerializeField] private float m_MinSpeed = 0f;    
        [SerializeField] private float m_MaxSpeed = 5f;   
        [Header("Trail Length")]
        [SerializeField] private float m_MinTime = 0.05f;
        [SerializeField] private float m_MaxTime = 0.35f;

        [Header("Transition Smoothing")]
        [SerializeField] private float m_Lerp = 10f;

        private Rigidbody m_Rb;
        private float m_BaseStartWidth;

        void Awake()
        {
            m_Rb = GetComponent<Rigidbody>();
            if (m_Trail == null) 
            {
                m_Trail = GetComponent<TrailRenderer>();
            }
            if (m_Trail != null) 
            {
                m_BaseStartWidth = m_Trail.startWidth;
            }
        }

        void Update()
        {
            if (m_Trail == null) 
                return;

            float speed = m_Rb.linearVelocity.magnitude;
            float t = Mathf.InverseLerp(m_MinSpeed, m_MaxSpeed, speed);   

            float targetTime = Mathf.Lerp(m_MinTime, m_MaxTime, t);
            m_Trail.time = Mathf.Lerp(m_Trail.time, targetTime, m_Lerp * Time.deltaTime);

            m_Trail.emitting = speed > m_MinSpeed;

            m_Trail.startWidth = m_BaseStartWidth * Mathf.Lerp(0.4f, 1f, t);
        }
    }
}