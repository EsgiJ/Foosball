using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AimTrajectory : MonoBehaviour
{
    private static AimTrajectory instance;

    [Header("Scene Propesties")]
    [SerializeField] private Transform m_FoosballTableObstaclesParent;
    private Scene m_SimulateScene;
    private PhysicsScene m_PhysicsScene;


    [Header("Ghost Ball")]
    [SerializeField] private GameObject m_GhostBallPrefab;
    private Rigidbody m_GhostBallRigidbody;
    private Transform m_GhostBallTransform;

    [Header("Trajectory Properties")]
    [SerializeField] private LineRenderer m_LineRenderer;
    [SerializeField] private int m_MaxPhysicsFrameIterations = 100;
    [SerializeField] private float m_ShootPower = 1000f;
    [SerializeField] private float m_UpdateInterval = 0.05f;

    [Header("Visual")]
    [SerializeField] private float m_FadeDuration = 0.15f;

    private float m_LastUpdateTime = -999f;
    private bool m_IsVisible = false;
    private Tween m_FadeTween;

#region Unity Lifecycle
    public static AimTrajectory Instance
    {
        get
        {
            if(instance == null)
            {
                SetupInstance();
            }
            return instance;
        }
    }

    private static void SetupInstance()
    {
        instance = FindObjectsByType<AimTrajectory>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)[0];
        if (instance == null)
        {
            GameObject gameObj = new GameObject();
            gameObj.name = "AimTrajectory";
            instance = gameObj.AddComponent<AimTrajectory>();
            DontDestroyOnLoad(gameObj);
        }
    }

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CreatePhysicsScene();
        SpawnGhostBall();
        m_LineRenderer.positionCount = 0;
        m_LineRenderer.enabled = false;
        m_LineRenderer.textureMode = LineTextureMode.Tile;
    }

#endregion

    private void CreatePhysicsScene()
    {
        m_SimulateScene = SceneManager.CreateScene("AimTrajectorySimulationScene", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        m_PhysicsScene = m_SimulateScene.GetPhysicsScene();
        m_FoosballTableObstaclesParent = GameObject.Find("FoosballTable").transform;
        foreach(Transform obj in m_FoosballTableObstaclesParent)
        {
            var ghostObj = Instantiate(obj.gameObject, obj.position, obj.rotation);
            if(ghostObj.TryGetComponent<Renderer>(out Renderer renderer))
            {
                renderer.enabled = false;
            }
            SceneManager.MoveGameObjectToScene(ghostObj, m_SimulateScene);
        }
    }

    private void SpawnGhostBall()
    {
        Debug.Log($"SpawnGhostBall called. Prefab: {m_GhostBallPrefab}");
        if (m_GhostBallPrefab == null)
        {
            Debug.LogError("GhostBallPrefab atanmadı!");
            return;
        }

        var ghost = Instantiate(m_GhostBallPrefab);
        ghost.name = "GhostBall_Trajectory";

        SceneManager.MoveGameObjectToScene(ghost, m_SimulateScene);

        m_GhostBallRigidbody = ghost.GetComponent<Rigidbody>();
        m_GhostBallTransform = ghost.transform;

        m_GhostBallRigidbody.linearVelocity = Vector3.zero;
        m_GhostBallRigidbody.angularVelocity = Vector3.zero;
        ghost.SetActive(false);
    }

    public void SimulateTrajectory(Vector3 startPos, Vector2 shootDirection)
    {
        if (shootDirection.sqrMagnitude < 0.01f)
        {
            Hide();
            return;
        }

        Show();

        // to not update every frame
        if (Time.time - m_LastUpdateTime < m_UpdateInterval) return;
        m_LastUpdateTime = Time.time;

        m_GhostBallTransform.gameObject.SetActive(true);
        m_GhostBallTransform.position = startPos;
        m_GhostBallRigidbody.linearVelocity = Vector3.zero;
        m_GhostBallRigidbody.angularVelocity = Vector3.zero;

        Vector3 shootVector = new Vector3(shootDirection.x, 0f, shootDirection.y) * m_ShootPower;
        m_GhostBallRigidbody.AddForce(shootVector);

        m_LineRenderer.positionCount = m_MaxPhysicsFrameIterations + 1;
        m_LineRenderer.SetPosition(0, startPos);

        for(int i = 0; i < m_MaxPhysicsFrameIterations; i++)
        {
            m_PhysicsScene.Simulate(Time.fixedDeltaTime);
            m_LineRenderer.SetPosition(i + 1, m_GhostBallTransform.position);
        }

        m_GhostBallRigidbody.linearVelocity = Vector3.zero;
        m_GhostBallRigidbody.angularVelocity = Vector3.zero;
        m_GhostBallTransform.gameObject.SetActive(false);
    }

    public void Show()
    {
        if (m_IsVisible) return;
        m_IsVisible = true;
        m_LineRenderer.enabled = true;

        m_FadeTween?.Kill();
        m_FadeTween = DOVirtual.Float(0f, 1f, m_FadeDuration,
            v =>
            {
                var c = m_LineRenderer.startColor; c.a = v;
                m_LineRenderer.startColor = c;
                var c2 = m_LineRenderer.endColor; c2.a = v * 0.3f;
                m_LineRenderer.endColor = c2;
            }).SetLink(gameObject);
    }

    public void Hide()
    {
        if (!m_IsVisible) return;
        m_IsVisible = false;

        m_FadeTween?.Kill();
        m_FadeTween = DOVirtual.Float(1f, 0f, m_FadeDuration,
            v =>
            {
                var c = m_LineRenderer.startColor; c.a = v;
                m_LineRenderer.startColor = c;
                var c2 = m_LineRenderer.endColor; c2.a = v * 0.3f;
                m_LineRenderer.endColor = c2;
            })
            .OnComplete(() =>
            {
                m_LineRenderer.enabled = false;
                m_LineRenderer.positionCount = 0;
            })
            .SetLink(gameObject);
    }
}
