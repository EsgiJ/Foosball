using DG.Tweening;
using UnityEngine;

namespace Foosball
{
    public class VFXManager : MonoBehaviour
    {
        private static VFXManager instance;
        public static VFXManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var found = FindObjectsByType<VFXManager>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
                    if (found.Length > 0) instance = found[0];
                }
                return instance;
            }
        }

        [Header("Hit Effect Materials")]
        [SerializeField] private Material m_GoalMat;
        [SerializeField] private Material m_WallHitMat;
        [SerializeField] private Material m_StunMat;
        [SerializeField] private Material m_BlockMat;

        [Header("Flash Animasyonu")]
        [SerializeField] private float m_BaseSize = 1.5f;
        [SerializeField] private float m_Duration = 0.35f;
        [SerializeField] private float m_StartScale = 0.3f;   
        [SerializeField] private float m_EndScale = 1f;       

        void Awake()
        {
            if (instance == null) 
            {
                instance = this;
            }   
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void Play(Material mat, Vector3 position, float scale = 1f)
        {
            if (mat == null) 
                return;

            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(go.GetComponent<Collider>());            

            var mr = go.GetComponent<MeshRenderer>();
            mr.material = new Material(mat);                 
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;

            go.transform.position = position;
            FaceCamera(go.transform);

            float target = m_BaseSize * scale;
            var mat2 = mr.material;

            Sequence seq = DOTween.Sequence().SetLink(go).SetUpdate(true);
            go.transform.localScale = Vector3.one * target * m_StartScale;
            seq.Join(go.transform.DOScale(target * m_EndScale, m_Duration).SetEase(Ease.OutQuad));
            string colorProp = mat2.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
            Color c = mat2.GetColor(colorProp);
            seq.Join(DOTween.To(() => mat2.GetColor(colorProp).a,
                                a => { c.a = a; mat2.SetColor(colorProp, c); },
                                0f, m_Duration).SetEase(Ease.InQuad));

            seq.OnComplete(() => Destroy(go));
        }

        private void FaceCamera(Transform t)
        {
            if (Camera.main == null) 
                return;
            t.rotation = Quaternion.LookRotation(t.position - Camera.main.transform.position);
        }

        public void PlayGoal(Vector3 pos)              => Play(m_GoalMat, pos, 1.4f);
        public void PlayWallHit(Vector3 pos, float s)  => Play(m_WallHitMat, pos, s);
        public void PlayStun(Vector3 pos)              => Play(m_StunMat, pos, 1f);
        public void PlayBlock(Vector3 pos, float s)    => Play(m_BlockMat, pos, s);
    }
}