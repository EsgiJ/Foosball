using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Foosball
{
    public class ButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [Header("Scale")]
        [SerializeField] private float m_HoverScale = 1.1f;
        [SerializeField] private float m_ScaleDuration = 0.15f;
        [SerializeField] private Ease m_ScaleEase = Ease.OutBack;

        [Header("Punch")]
        [SerializeField] private float m_PunchAmount = 0.2f;
        [SerializeField] private float m_PunchDuration = 0.3f;
        [SerializeField] private int m_PunchVibrato = 8;
        [SerializeField] private float m_PunchElasticity = 0.7f;

        [Header("Tint")]
        [SerializeField] private Graphic m_TintTarget;                 
        [SerializeField] private Color m_HoverHighlight = Color.white;
        [SerializeField, Range(0f, 1f)] private float m_HoverStrength = 0.25f;
        [SerializeField] private float m_TintDuration = 0.15f;

        [Header("Audio")]
        [SerializeField] private bool m_PlayClickSound = true;

        private Vector3 m_BaseScale;
        private Color m_BaseColor;
        private bool m_Hovered;
        private Tween m_ScaleTween;
        private Tween m_TintTween;

        void Awake()
        {
            m_BaseScale = transform.localScale;
            if (m_TintTarget == null) 
            {
                m_TintTarget = GetComponent<Graphic>();
            }
            if (m_TintTarget != null) 
            {
                m_BaseColor = m_TintTarget.color;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_Hovered = true;
            SetScale(m_BaseScale * m_HoverScale);
            Highlight(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_Hovered = false;
            SetScale(m_BaseScale);
            Highlight(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Punch();
            if (m_PlayClickSound) 
            {
                AudioManager.Instance?.PlayStanceClick();
            }
        }

        private void SetScale(Vector3 target)
        {
            m_ScaleTween?.Kill();
            m_ScaleTween = transform.DOScale(target, m_ScaleDuration)
                .SetEase(m_ScaleEase).SetUpdate(true).SetLink(gameObject);
        }

        private void Punch()
        {
            m_ScaleTween?.Kill();
            transform.localScale = m_Hovered ? m_BaseScale * m_HoverScale : m_BaseScale;
            m_ScaleTween = transform.DOPunchScale(m_BaseScale * m_PunchAmount, m_PunchDuration, m_PunchVibrato, m_PunchElasticity)
                .SetUpdate(true).SetLink(gameObject);
        }

        private void Highlight(bool hovered)
        {
            if (m_TintTarget == null) return;

            Color c = hovered ? Color.Lerp(m_BaseColor, m_HoverHighlight, m_HoverStrength) : m_BaseColor;
            c.a = m_BaseColor.a;

            m_TintTween?.Kill();
            m_TintTween = m_TintTarget.DOColor(c, m_TintDuration).SetUpdate(true).SetLink(gameObject);
        }

        void OnDisable()
        {
            m_ScaleTween?.Kill();
            m_TintTween?.Kill();
            transform.localScale = m_BaseScale;
            m_Hovered = false;
            if (m_TintTarget != null) 
            {
                m_TintTarget.color = m_BaseColor;
            }
        }
    }
}