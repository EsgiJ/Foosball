using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class GameJuiceManager : MonoBehaviour
{
    private static GameJuiceManager instance;

    [Header("Camera")]
    [SerializeField] private Transform m_CameraTransform;

    [Header("Pause")]
    [SerializeField] private float m_DefaultHitPauseDuration = 1.0f;

    [Header("Slow Motion")]
    [SerializeField] private float m_SlowMoScale = 0.25f;
    [SerializeField] private float m_SlowMoEaseIn = 0.1f;
    [SerializeField] private float m_SlowMoHold = 0.4f;
    [SerializeField] private float m_SlowMoEaseOut = 0.5f;

    [Header("Screen Shake")]
    [SerializeField] private float m_DefaultShakeDuration = 0.2f;
    [SerializeField] private float m_DefaultShakeStrength = 0.3f;
    [SerializeField] private int m_DefaultShakeVibrato = 12;

    private Tween m_TimeScaleTween;
    private Vector3 m_CameraBasePos;
    private Tween m_ShakeTween;
#region Unity Lifecycle

    public static GameJuiceManager Instance
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
        instance = FindObjectsByType<GameJuiceManager>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)[0];
        if (instance == null)
        {
            GameObject gameObj = new GameObject();
            gameObj.name = "GameJuiceManager";
            instance = gameObj.AddComponent<GameJuiceManager>();
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

        if (m_CameraTransform == null && Camera.main != null)
            m_CameraTransform = Camera.main.transform;

        if (m_CameraTransform != null)
            m_CameraBasePos = m_CameraTransform.localPosition;
    }
#endregion

#region Game Juice

    public void PauseGame(float duration = -1f)
    {
        if (duration < 0f) 
            duration = m_DefaultHitPauseDuration;

        m_TimeScaleTween?.Kill();
        Time.timeScale = 0f;

        m_TimeScaleTween = DOVirtual.DelayedCall(duration, () => Time.timeScale = 1f, false)
            .SetUpdate(true); 
    }

    public void ShakeCamera(float duration = -1f, float strength = -1f)
    {
        if (m_CameraTransform == null) return;
        if (duration < 0f) duration = m_DefaultShakeDuration;
        if (strength < 0f) strength = m_DefaultShakeStrength;

        m_ShakeTween?.Kill(true); 
        m_CameraTransform.localPosition = m_CameraBasePos;

        m_ShakeTween = m_CameraTransform
            .DOShakePosition(duration, strength, m_DefaultShakeVibrato, 90f, false, true)
            .SetUpdate(true) 
            .SetLink(gameObject);
    }

    public void SlowMotion(float? targetScale = null, float? hold = null)
    {
        float scale = targetScale ?? m_SlowMoScale;
        float holdDur = hold ?? m_SlowMoHold;

        m_TimeScaleTween?.Kill();

        DG.Tweening.Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(DOTween.To(() => Time.timeScale, x => Time.timeScale = x, scale, m_SlowMoEaseIn).SetEase(Ease.OutQuad));
        seq.AppendInterval(holdDur);
        seq.Append(DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, m_SlowMoEaseOut).SetEase(Ease.InQuad));
        seq.OnKill(() => Time.timeScale = 1f); 

        m_TimeScaleTween = seq;

        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
#endregion
}
