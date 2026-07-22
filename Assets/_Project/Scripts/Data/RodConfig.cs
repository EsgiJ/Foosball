using UnityEngine;
using UnityEngine.Serialization;
using DG.Tweening;

namespace Foosball.Data
{
    [CreateAssetMenu(fileName = "RodConfig", menuName = "Foosball/Rod Config")]
    public class RodConfig : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField, FormerlySerializedAs("MovementSpeed"), Min(0f)] private float m_MovementSpeed = 5f;
        public float MovementSpeed => m_MovementSpeed;
        [SerializeField, FormerlySerializedAs("MinZPos")] private float m_MinZPos = -1.5f;
        public float MinZPos => m_MinZPos;
        [SerializeField, FormerlySerializedAs("MaxZPos")] private float m_MaxZPos = 1.5f;
        public float MaxZPos => m_MaxZPos;
        [SerializeField, FormerlySerializedAs("UsableExtent"), Min(0f)] private float m_UsableExtent = 12f;
        public float UsableExtent => m_UsableExtent;
        [SerializeField, FormerlySerializedAs("FootballPlayerCount"), Range(1, 5)] private int m_FootballPlayerCount = 3;
        public int FootballPlayerCount { get => m_FootballPlayerCount; set => m_FootballPlayerCount = value; }

        [Header("Stance Rotations")]
        [SerializeField, FormerlySerializedAs("DefenseRotation")] private float m_DefenseRotation = 0f;
        public float DefenseRotation => m_DefenseRotation;
        [SerializeField, FormerlySerializedAs("AttackRotation")] private float m_AttackRotation = 30f;
        public float AttackRotation => m_AttackRotation;
        [SerializeField, FormerlySerializedAs("ShootRotation")] private float m_ShootRotation = -180f;
        public float ShootRotation => m_ShootRotation;

        [Header("Rotation Animations")]
        [SerializeField, FormerlySerializedAs("ShootDuration"), Min(0f)] private float m_ShootDuration = 0.2f;
        public float ShootDuration => m_ShootDuration;
        [SerializeField, FormerlySerializedAs("ShootEase")] private Ease m_ShootEase = Ease.OutQuad;
        public Ease ShootEase => m_ShootEase;

        [SerializeField, FormerlySerializedAs("DefenseStanceDuration"), Min(0f)] private float m_DefenseStanceDuration = 0.2f;
        public float DefenseStanceDuration => m_DefenseStanceDuration;
        [SerializeField, FormerlySerializedAs("DefenseStanceEase")] private Ease m_DefenseStanceEase = Ease.OutQuad;
        public Ease DefenseStanceEase => m_DefenseStanceEase;

        [SerializeField, FormerlySerializedAs("AttackStanceDuration"), Min(0f)] private float m_AttackStanceDuration = 0.2f;
        public float AttackStanceDuration => m_AttackStanceDuration;
        [SerializeField, FormerlySerializedAs("AttackStanceEase")] private Ease m_AttackStanceEase = Ease.OutQuad;
        public Ease AttackStanceEase => m_AttackStanceEase;

        [SerializeField, FormerlySerializedAs("StunRotationDuration"), Min(0f)] private float m_StunRotationDuration = 0.2f;
        public float StunRotationDuration => m_StunRotationDuration;
        [SerializeField, FormerlySerializedAs("StunEase")] private Ease m_StunEase = Ease.OutBounce;
        public Ease StunEase => m_StunEase;
        [SerializeField, FormerlySerializedAs("StunDuration"), Min(0f)] private float m_StunDuration = 2f;
        public float StunDuration => m_StunDuration;

        [Header("Wiggle Effect")]
        [SerializeField, FormerlySerializedAs("WigglePunchAmount")] private float m_WigglePunchAmount = 0.15f;
        public float WigglePunchAmount => m_WigglePunchAmount;
        [SerializeField, FormerlySerializedAs("WigglePunchRotation")] private float m_WigglePunchRotation = 10f;
        public float WigglePunchRotation => m_WigglePunchRotation;
        [SerializeField, FormerlySerializedAs("WiggleDuration")] private float m_WiggleDuration = 0.4f;
        public float WiggleDuration => m_WiggleDuration;
        [SerializeField, FormerlySerializedAs("WiggleVibrato")] private int m_WiggleVibrato = 8;
        public int WiggleVibrato => m_WiggleVibrato;
        [SerializeField, FormerlySerializedAs("WiggleElasticity")] private float m_WiggleElasticity = 0.5f;
        public float WiggleElasticity => m_WiggleElasticity;

        [Header("Struggle Effect")]
        [SerializeField, FormerlySerializedAs("StrugglePunchAngle")] private Vector3 m_StrugglePunchAngle = new Vector3(0f, 0f, 8f);
        public Vector3 StrugglePunchAngle => m_StrugglePunchAngle;
        [SerializeField, FormerlySerializedAs("StruggleDuration")] private float m_StruggleDuration = 0.5f;
        public float StruggleDuration => m_StruggleDuration;
        [SerializeField, FormerlySerializedAs("StruggleVibrato")] private int m_StruggleVibrato = 4;
        public int StruggleVibrato => m_StruggleVibrato;
        [SerializeField, FormerlySerializedAs("StruggleElasticity")] private float m_StruggleElasticity = 0.6f;
        public float StruggleElasticity => m_StruggleElasticity;

        [Header("Outline")]
        [SerializeField, FormerlySerializedAs("OutlineColor")] private Color m_OutlineColor = Color.yellow;
        public Color OutlineColor => m_OutlineColor;
        [SerializeField, FormerlySerializedAs("OutlineWidth")] private float m_OutlineWidth = 6f;
        public float OutlineWidth => m_OutlineWidth;
        [SerializeField, FormerlySerializedAs("OutlineFadeDuration")] private float m_OutlineFadeDuration = 0.2f;
        public float OutlineFadeDuration => m_OutlineFadeDuration;
    }
}
