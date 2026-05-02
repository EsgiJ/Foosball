using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GameJuiceManager : MonoBehaviour
{
    private static GameJuiceManager instance;

    [Header("Camera")]
    [SerializeField] private Transform m_CameraTransform;

    [Header("Pause")]
    [SerializeField] private float m_DefaultPauseDuration = 1.0f;

    [Header("Slow Motion")]
    [SerializeField] private float m_SlowMoScale = 0.25f;
    [SerializeField] private float m_SlowMoEaseIn = 0.1f;
    [SerializeField] private float m_SlowMoHold = 0.4f;
    [SerializeField] private float m_SlowMoEaseOut = 0.5f;
    private Tween m_TimeScaleTween;

    [Header("Screen Shake")]
    [SerializeField] private float m_DefaultShakeDuration = 0.2f;
    [SerializeField] private float m_DefaultShakeStrength = 0.3f;
    [SerializeField] private int m_DefaultShakeVibrato = 12;
    private Vector3 m_CameraBasePos;
    private Tween m_ShakeTween;

    [Header("Chromatic Aberration Effect")]
    [SerializeField] private float m_DefaultChromaticAberrationIntensity = 0.5f;
    [SerializeField] private float m_TargetChromaticAberrationIntensity = 1f;
    [SerializeField] private float m_ChromaticAberrationEaseIn = 0.1f;
    [SerializeField] private float m_ChromaticAberrationHold = 0.4f;
    [SerializeField] private float m_ChromaticAberrationEaseOut = 0.5f;
    private Tween m_ChromaticAberrationTween;

    [Header("Vignette Effect")]
    [SerializeField] private float m_DefaultVignetteIntensity = 0.6f;
    [SerializeField] private float m_TargetVignetteIntensity = 1f;
    [SerializeField] private float m_DefaultVignetteSmoothness = 0.2f;
    [SerializeField] private float m_TargetVignetteSmoothness = 1f;
    [SerializeField] private float m_VignetteEaseIn = 0.1f;
    [SerializeField] private float m_VignetteHold = 0.4f;
    [SerializeField] private float m_VignetteEaseOut = 0.5f;
    private Tween m_VignetteTween;

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

    void OnDestroy()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
#endregion

#region Game Juice

    public void PauseGame(float duration = -1f)
    {
        if (duration < 0f) 
            duration = m_DefaultPauseDuration;

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

    public void ChromaticAberrationEffect(float? intensity = null, float? riseDuration = null, float? hold = null, float? fallDuration = null)
    {
        float targetIntensity = intensity ?? m_TargetChromaticAberrationIntensity;
        float rise = riseDuration ?? m_ChromaticAberrationEaseIn;
        float holdDur = hold ?? m_ChromaticAberrationHold;
        float fall = fallDuration ?? m_ChromaticAberrationEaseOut;

        UnityEngine.Rendering.VolumeProfile profile = GameObject.Find("Global Volume").GetComponent<UnityEngine.Rendering.Volume>().profile;
        UnityEngine.Rendering.Universal.ChromaticAberration chromaticAberration;

        profile.TryGet(out chromaticAberration);
        
        m_ChromaticAberrationTween?.Kill();

        DG.Tweening.Sequence seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        seq.Append(DOTween.To(() => chromaticAberration.intensity.value, x => chromaticAberration.intensity.value = x, targetIntensity, m_ChromaticAberrationEaseIn).SetEase(Ease.OutQuad));
        seq.AppendInterval(holdDur);
        seq.Append(DOTween.To(() => chromaticAberration.intensity.value, x  => chromaticAberration.intensity.value = x, m_DefaultChromaticAberrationIntensity, m_ChromaticAberrationEaseOut).SetEase(Ease.InQuad));
        seq.OnKill(() => chromaticAberration.intensity.value = m_DefaultChromaticAberrationIntensity); 

        m_ChromaticAberrationTween = seq;
    }

    public void VignetteEffect(float? intensity = null, float? smoothness = null, float? riseDuration = null, float? hold = null, float? fallDuration = null)
    {
        float targetIntensity = intensity ?? m_TargetVignetteIntensity;
        float rise = riseDuration ?? m_VignetteEaseIn;
        float holdDur = hold ?? m_VignetteHold;
        float fall = fallDuration ?? m_VignetteEaseOut;
        float targetSmoothness = smoothness ?? m_TargetVignetteSmoothness;

        UnityEngine.Rendering.VolumeProfile profile = GameObject.Find("Global Volume").GetComponent<UnityEngine.Rendering.Volume>().profile;
        UnityEngine.Rendering.Universal.Vignette vignette;

        profile.TryGet(out vignette);
        
        m_VignetteTween?.Kill();
        DG.Tweening.Sequence seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        seq.AppendCallback(() => vignette.rounded.value = true);

        seq.Append(DOTween.To(() => vignette.intensity.value,
                            x => vignette.intensity.value = x,
                            targetIntensity, rise).SetEase(Ease.OutQuad));

        seq.Join(DOTween.To(() => vignette.smoothness.value,
                            x => vignette.smoothness.value = x,
                            targetSmoothness, rise).SetEase(Ease.OutQuad));

        seq.AppendInterval(holdDur);

        seq.Append(DOTween.To(() => vignette.intensity.value,
                            x => vignette.intensity.value = x,
                            m_DefaultVignetteIntensity, fall).SetEase(Ease.InQuad));

        seq.Join(DOTween.To(() => vignette.smoothness.value,
                            x => vignette.smoothness.value = x,
                            m_DefaultVignetteSmoothness, fall).SetEase(Ease.InQuad));

        seq.AppendCallback(() => vignette.rounded.value = false);

        seq.OnKill(() =>
        {
            vignette.rounded.value = false;
            vignette.intensity.value = m_DefaultVignetteIntensity;
            vignette.smoothness.value = m_DefaultVignetteSmoothness;
        });


        m_VignetteTween = seq;
    }
#endregion
}
