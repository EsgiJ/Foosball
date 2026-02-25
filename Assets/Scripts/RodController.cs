using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

public class RodController : MonoBehaviour
{
    /* Action propesties*/
    InputAction m_ShootAction;

    /* Transform propesties*/

    /* Animation properties */
    [SerializeField] private AnimationCurve m_RotationCurve;
    [SerializeField, Min(0f)] private float m_RotationDuration = 0.5f;

#region Unity Lifecycle
    void Start()
    {
        m_ShootAction = InputSystem.actions.FindAction("Shoot");

        m_ShootAction.started += OnShoot;
    }

    void Update()
    {

    }

    void OnDestroy()
    {
        m_ShootAction.started -= OnShoot;
    }
#endregion

#region Shoot
    private void OnShoot(InputAction.CallbackContext context)
    {
        Debug.Log("Shoot action triggered!");
        StartCoroutine(RotateWithAnimationCurve());
    }

    IEnumerator RotateWithAnimationCurve()
    {
        float elapsed = 0f;
        Quaternion startRotation = transform.localRotation;
        Quaternion m_shootTargetRotation = startRotation * Quaternion.Euler(0, 180f, 0);

        while(elapsed < m_RotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / m_RotationDuration;

            float curveValue = m_RotationCurve.Evaluate(t);

            transform.localRotation = Quaternion.Lerp(startRotation, m_shootTargetRotation, curveValue);

            yield return null;
        }
    }
#endregion
}
