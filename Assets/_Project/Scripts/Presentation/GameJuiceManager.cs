using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameJuiceManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private JuiceConfig m_JuiceConfig;

    [Header("Camera")]
    [SerializeField] private Transform m_CameraTransform;

    /* Tween handles */
    private Tween m_TimeScaleTween;
    private Vector3 m_CameraBasePos;
    private Tween m_ShakeTween;
    private Tween m_ChromaticAberrationTween;
    private Tween m_VignetteTween;

#region Unity Lifecycle
    void Awake()
    {
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

#region Time Scale Effects
    public void PauseGame(float duration = -1f)
    {
        if (duration < 0f)
            duration = m_JuiceConfig.DefaultPauseDuration;

        m_TimeScaleTween?.Kill();
        Time.timeScale = 0f;

        m_TimeScaleTween = DOVirtual.DelayedCall(duration, () => Time.timeScale = 1f, false)
            .SetUpdate(true);
    }

    public void ShakeCamera(float duration = -1f, float strength = -1f)
    {
        if (m_CameraTransform == null)
            return;
        if (duration < 0f) duration = m_JuiceConfig.DefaultShakeDuration;
        if (strength < 0f) strength = m_JuiceConfig.DefaultShakeStrength;

        m_ShakeTween?.Kill(true);

        m_ShakeTween = m_CameraTransform
            .DOShakePosition(duration, strength, m_JuiceConfig.DefaultShakeVibrato, m_JuiceConfig.ShakeRandomness, m_JuiceConfig.ShakeSnapping, m_JuiceConfig.ShakeFadeOut)
            .SetUpdate(true)
            .SetLink(gameObject);
    }

    public void SlowMotion(float? targetScale = null, float? hold = null)
    {
        float scale = targetScale ?? m_JuiceConfig.SlowMoScale;
        float holdDur = hold ?? m_JuiceConfig.SlowMoHold;

        m_TimeScaleTween?.Kill();

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(DOTween.To(() => Time.timeScale, x => Time.timeScale = x, scale, m_JuiceConfig.SlowMoEaseIn).SetEase(Ease.OutQuad));
        seq.AppendInterval(holdDur);
        seq.Append(DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, m_JuiceConfig.SlowMoEaseOut).SetEase(Ease.InQuad));
        seq.OnKill(() => Time.timeScale = 1f);

        m_TimeScaleTween = seq;

        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
#endregion

#region Post-Processing Effects
    private VolumeProfile GetVolumeProfile()
    {
        var volumeObj = GameObject.Find("Global Volume");
        return volumeObj != null ? volumeObj.GetComponent<Volume>().profile : null;
    }

    public void ChromaticAberrationEffect(float? intensity = null, float? riseDuration = null, float? hold = null, float? fallDuration = null)
    {
        float targetIntensity = intensity ?? m_JuiceConfig.TargetChromaticAberrationIntensity;
        float rise = riseDuration ?? m_JuiceConfig.ChromaticAberrationEaseIn;
        float holdDur = hold ?? m_JuiceConfig.ChromaticAberrationHold;
        float fall = fallDuration ?? m_JuiceConfig.ChromaticAberrationEaseOut;

        VolumeProfile profile = GetVolumeProfile();
        ChromaticAberration chromaticAberration;
        profile.TryGet(out chromaticAberration);

        m_ChromaticAberrationTween?.Kill();

        Sequence seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        seq.Append(DOTween.To(() => chromaticAberration.intensity.value, x => chromaticAberration.intensity.value = x, targetIntensity, m_JuiceConfig.ChromaticAberrationEaseIn).SetEase(Ease.OutQuad));
        seq.AppendInterval(holdDur);
        seq.Append(DOTween.To(() => chromaticAberration.intensity.value, x  => chromaticAberration.intensity.value = x, m_JuiceConfig.DefaultChromaticAberrationIntensity, m_JuiceConfig.ChromaticAberrationEaseOut).SetEase(Ease.InQuad));
        seq.OnKill(() => chromaticAberration.intensity.value = m_JuiceConfig.DefaultChromaticAberrationIntensity);

        m_ChromaticAberrationTween = seq;
    }

    public void VignetteEffect(float? intensity = null, float? smoothness = null, float? riseDuration = null, float? hold = null, float? fallDuration = null)
    {
        float targetIntensity = intensity ?? m_JuiceConfig.TargetVignetteIntensity;
        float rise = riseDuration ?? m_JuiceConfig.VignetteEaseIn;
        float holdDur = hold ?? m_JuiceConfig.VignetteHold;
        float fall = fallDuration ?? m_JuiceConfig.VignetteEaseOut;
        float targetSmoothness = smoothness ?? m_JuiceConfig.TargetVignetteSmoothness;

        VolumeProfile profile = GetVolumeProfile();
        Vignette vignette;
        profile.TryGet(out vignette);

        m_VignetteTween?.Kill();
        Sequence seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
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
                            m_JuiceConfig.DefaultVignetteIntensity, fall).SetEase(Ease.InQuad));

        seq.Join(DOTween.To(() => vignette.smoothness.value,
                            x => vignette.smoothness.value = x,
                            m_JuiceConfig.DefaultVignetteSmoothness, fall).SetEase(Ease.InQuad));

        seq.AppendCallback(() => vignette.rounded.value = false);

        seq.OnKill(() =>
        {
            vignette.rounded.value = false;
            vignette.intensity.value = m_JuiceConfig.DefaultVignetteIntensity;
            vignette.smoothness.value = m_JuiceConfig.DefaultVignetteSmoothness;
        });

        m_VignetteTween = seq;
    }

    public void SetVignetteIntensity(float intensity) => SetVignetteIntensity(intensity, m_JuiceConfig.VignetteRestoreDuration);

    public void SetVignetteIntensity(float intensity, float duration)
    {
        VolumeProfile profile = GetVolumeProfile();
        if (profile == null)
            return;
        if (!profile.TryGet(out Vignette vignette))
            return;

        m_VignetteTween?.Kill();
        m_VignetteTween = DOTween.To(() => vignette.intensity.value,
            x => vignette.intensity.value = x, intensity, duration)
            .SetUpdate(true).SetLink(gameObject);
    }

    public void RestoreVignette() => RestoreVignette(m_JuiceConfig.VignetteRestoreDuration);
    public void RestoreVignette(float duration) => SetVignetteIntensity(m_JuiceConfig.DefaultVignetteIntensity, duration);
#endregion
}
