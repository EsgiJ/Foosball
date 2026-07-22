using UnityEngine;
using UnityEngine.InputSystem;
using Foosball.Gameplay;

namespace Foosball.Presentation
{
    public class InteractiveMenu : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera m_Camera;
        [SerializeField] private BallController m_Ball;
        private Rigidbody m_BallRigidbody;

        [Header("Ball Steering")]
        [SerializeField] private float m_SteerForce = 6f;
        [SerializeField] private float m_MaxBallSpeed = 8f;
        [SerializeField] private float m_BallDamping = 1.5f;

        private bool m_Active;
        private GameStateManager m_GameStateManager;

    #region Unity Lifecycle
        public void Init(GameStateManager gameStateManager)
        {
            m_GameStateManager = gameStateManager;
        }

        void Start()
        {
            if (m_Camera == null)
            {
                m_Camera = Camera.main;
            }
            if (m_Ball == null)
            {
                m_Ball = FindFirstObjectByType<BallController>();
            }
            if (m_Ball != null)
            {
                m_BallRigidbody = m_Ball.GetComponent<Rigidbody>();
            }

            if (m_GameStateManager != null)
            {
                m_GameStateManager.OnStateChanged += HandleStateChanged;
                m_Active = m_GameStateManager.CurrentState == GameState.MainMenu;
            }
        }

        void OnDestroy()
        {
            if (m_GameStateManager != null)
                m_GameStateManager.OnStateChanged -= HandleStateChanged;
        }

        void FixedUpdate()
        {
            if (!m_Active)
                return;
            if (m_BallRigidbody == null || m_Camera == null || Mouse.current == null)
                return;

            Vector3 toCursor = MouseOnTablePlane() - m_BallRigidbody.position;
            toCursor.y = 0f;
            m_BallRigidbody.AddForce(toCursor.normalized * m_SteerForce, ForceMode.Acceleration);

            Vector3 v = m_BallRigidbody.linearVelocity;
            v -= v * m_BallDamping * Time.fixedDeltaTime;
            if (v.magnitude > m_MaxBallSpeed) v = v.normalized * m_MaxBallSpeed;
            m_BallRigidbody.linearVelocity = v;
        }
    #endregion

    #region State Handling
        private void HandleStateChanged(GameState previous, GameState next)
        {
            m_Active = next == GameState.MainMenu;
            if (!m_Active && m_Ball != null)
            {
                m_Ball.StopBall();
            }
        }
    #endregion

    #region Ball Steering
        private Vector3 MouseOnTablePlane()
        {
            float planeY = m_BallRigidbody != null ? m_BallRigidbody.position.y : 0f;
            Plane plane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));
            Ray ray = m_Camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            return plane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : m_BallRigidbody.position;
        }
    #endregion
    }
}
