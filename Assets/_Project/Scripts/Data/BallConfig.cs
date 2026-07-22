using UnityEngine;
using UnityEngine.Serialization;

namespace Foosball.Data
{
    [CreateAssetMenu(fileName = "BallConfig", menuName = "Foosball/Ball Config")]
    public class BallConfig : ScriptableObject
    {
        [Header("Shoot")]
        [SerializeField, FormerlySerializedAs("ShootPower"), Min(0f)] private float m_ShootPower = 10f;
        public float ShootPower => m_ShootPower;

        [Header("Collision")]
        [SerializeField, FormerlySerializedAs("SphereRadius"), Min(0f)] private float m_SphereRadius = 0.6f;
        public float SphereRadius => m_SphereRadius;

        [Header("Attachment")]
        [SerializeField, FormerlySerializedAs("AttachmentDuration"), Min(0f)] private float m_AttachmentDuration = 0.1f;
        public float AttachmentDuration => m_AttachmentDuration;
        [SerializeField, FormerlySerializedAs("AttachmentSideOffset")] private float m_AttachmentSideOffset = 0.25f;
        public float AttachmentSideOffset => m_AttachmentSideOffset;

        [Header("Wall Bounce")]
        [SerializeField, FormerlySerializedAs("WallBounceMinImpact"), Min(0f)] private float m_WallBounceMinImpact = 1f;
        public float WallBounceMinImpact => m_WallBounceMinImpact;
        [SerializeField, FormerlySerializedAs("WallBounceMaxImpact"), Min(0f)] private float m_WallBounceMaxImpact = 20f;
        public float WallBounceMaxImpact => m_WallBounceMaxImpact;
        [SerializeField, FormerlySerializedAs("WallHitVFXScaleMin")] private float m_WallHitVFXScaleMin = 0.6f;
        public float WallHitVFXScaleMin => m_WallHitVFXScaleMin;
        [SerializeField, FormerlySerializedAs("WallHitVFXScaleMax")] private float m_WallHitVFXScaleMax = 1.4f;
        public float WallHitVFXScaleMax => m_WallHitVFXScaleMax;
    }
}
