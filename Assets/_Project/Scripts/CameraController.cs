using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Foosball
{
    public class CameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform m_Camera;     
        [SerializeField] private Transform m_MenuPose;   
        [SerializeField] private Transform m_Ball;      

        [Header("Blend")]
        [SerializeField] private float m_BlendDuration = 0.8f;
        [SerializeField] private Ease m_BlendEase = Ease.InOutCubic;

        [Header("Menu Tilt (mouse'a göre)")]
        [SerializeField] private float m_MenuTiltAmount = 4f;
        [SerializeField] private float m_MenuTiltLerp = 5f;

        [Header("Game Tilt (topa göre)")]
        [SerializeField] private float m_TiltAmount = 4f;    
        [SerializeField] private float m_TiltPerUnit = 1f;
        [SerializeField] private float m_TiltLerp = 4f;

        private Vector3 m_GamePosePosition;
        private Quaternion m_GamePoseRotation;
        private Quaternion m_MenuPoseRotation;
        private bool m_InMenuPose;
        private bool m_ApplyMenuTilt;
        private bool m_ApplyGameTilt;
        private Tween m_BlendTween;

        void Start()
        {
            if (m_Camera == null && Camera.main != null) 
            {
                m_Camera = Camera.main.transform;
            }
            if (m_Camera == null) 
            { 
                Debug.LogError("[CameraController] camera not found."); 
                enabled = false; 
                return; 
            }
            if (m_Ball == null)   
            {
                m_Ball = FindFirstObjectByType<BallController>().gameObject.transform;
            }

            m_GamePosePosition = m_Camera.localPosition;
            m_GamePoseRotation = m_Camera.localRotation;

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged += HandleStateChanged;
                ApplyStateInstant(GameStateManager.Instance.CurrentState);
            }
        }

        void OnDestroy()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
        }

        void LateUpdate()
        {
            if (m_ApplyMenuTilt) 
            { 
                ApplyMenuTilt(); 
                return; 
            }
            if (m_ApplyGameTilt) 
            { 
                ApplyGameTilt(); 
            }
        }

        private void ApplyMenuTilt()
        {
            if (Mouse.current == null) 
                return;

            Vector2 p = Mouse.current.position.ReadValue();
            Vector2 norm = new Vector2(
                (p.x / Screen.width)  * 2f - 1f,
                (p.y / Screen.height) * 2f - 1f);

            Quaternion target = m_MenuPoseRotation * Quaternion.Euler(-norm.y * m_MenuTiltAmount, norm.x * m_MenuTiltAmount, 0f);
            m_Camera.localRotation = Quaternion.Slerp(m_Camera.localRotation, target, m_MenuTiltLerp * Time.deltaTime);
        }

        private void ApplyGameTilt()
        {
            if (m_Ball == null) 
                return;

            float tiltX = Mathf.Clamp(-m_Ball.position.z * m_TiltPerUnit, -m_TiltAmount, m_TiltAmount);
            float tilty = Mathf.Clamp(m_Ball.position.x * m_TiltPerUnit, -m_TiltAmount, m_TiltAmount);
            Quaternion target = m_GamePoseRotation * Quaternion.Euler(tiltX, tilty, 0f);

            m_Camera.localRotation = Quaternion.Slerp(m_Camera.localRotation, target, m_TiltLerp * Time.deltaTime);
        }

        private bool IsMenuState(GameState s) => s == GameState.MainMenu || s == GameState.Setup;

        private void ApplyStateInstant(GameState s)
        {
            m_InMenuPose = IsMenuState(s);
            m_ApplyMenuTilt = false;
            m_ApplyGameTilt = false;

            if (m_InMenuPose)
            {
                GetMenuLocalPose(out var pos, out var rot);
                m_MenuPoseRotation = rot;
                m_Camera.localPosition = pos;
                m_Camera.localRotation = rot;
                m_ApplyMenuTilt = true;
            }
            else
            {
                m_Camera.localPosition = m_GamePosePosition;
                m_Camera.localRotation = m_GamePoseRotation;
                m_ApplyGameTilt = s == GameState.Playing;
            }
        }

        private void HandleStateChanged(GameState previous, GameState next)
        {
            bool targetMenu = IsMenuState(next);

            if (targetMenu != m_InMenuPose)          
            {
                m_InMenuPose = targetMenu;
                m_ApplyMenuTilt = false;
                m_ApplyGameTilt = false;

                if (targetMenu)
                {
                    GetMenuLocalPose(out _, out var rot);
                    m_MenuPoseRotation = rot;
                }

                BlendToPose(targetMenu, blendPosition: true, onComplete: targetMenu ? () => m_ApplyMenuTilt = true : (System.Action)null);
                return;
            }

            if (targetMenu)
            {
                m_ApplyMenuTilt = true;              
            }
            else if (next == GameState.Playing)
            {
                m_ApplyGameTilt = true;              
            }
            else
            {
                m_ApplyGameTilt = false;             
                BlendToPose(false, blendPosition: false, onComplete: null);
            }
        }

        private void BlendToPose(bool menu, bool blendPosition, System.Action onComplete)
        {
            m_BlendTween?.Kill();

            Vector3 pos; Quaternion rot;
            if (menu) 
            {
                GetMenuLocalPose(out pos, out rot);
            }
            else 
            { 
                pos = m_GamePosePosition; 
                rot = m_GamePoseRotation; 
            }

            Sequence seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

            if (blendPosition)
                seq.Join(m_Camera.DOLocalMove(pos, m_BlendDuration).SetEase(m_BlendEase));

            seq.Join(m_Camera.DOLocalRotateQuaternion(rot, m_BlendDuration).SetEase(m_BlendEase));

            if (onComplete != null) 
            {
                seq.OnComplete(() => onComplete());
            }
            m_BlendTween = seq;
        }

        private void GetMenuLocalPose(out Vector3 pos, out Quaternion rot)
        {
            if (m_MenuPose == null) 
            { 
                pos = m_GamePosePosition; 
                rot = m_GamePoseRotation; 
                return; 
            }

            Transform parent = m_Camera.parent;
            if (parent != null)
            {
                pos = parent.InverseTransformPoint(m_MenuPose.position);
                rot = Quaternion.Inverse(parent.rotation) * m_MenuPose.rotation;
            }
            else
            {
                pos = m_MenuPose.position;
                rot = m_MenuPose.rotation;
            }
        }
    }
}