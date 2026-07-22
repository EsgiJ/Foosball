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
        [SerializeField] private Ease m_OutlineShowEase = Ease.OutBack;
        public Ease OutlineShowEase => m_OutlineShowEase;
        [SerializeField] private Ease m_OutlineHideEase = Ease.InQuad;
        public Ease OutlineHideEase => m_OutlineHideEase;

        [Header("Pass")]
        [SerializeField, Min(0f)] private float m_PassWindupDistance = 0.3f;
        public float PassWindupDistance => m_PassWindupDistance;
        [SerializeField, Min(0f)] private float m_PassThrowDistance = 0.5f;
        public float PassThrowDistance => m_PassThrowDistance;
        [SerializeField, Min(0f)] private float m_PassWindupTime = 0.07f;
        public float PassWindupTime => m_PassWindupTime;
        [SerializeField, Min(0f)] private float m_PassThrowTime = 0.08f;
        public float PassThrowTime => m_PassThrowTime;
        [SerializeField, Min(0f)] private float m_PassSettleTime = 0.07f;
        public float PassSettleTime => m_PassSettleTime;
        [SerializeField] private Ease m_PassWindupEase = Ease.OutQuad;
        public Ease PassWindupEase => m_PassWindupEase;
        [SerializeField] private Ease m_PassThrowEase = Ease.OutBack;
        public Ease PassThrowEase => m_PassThrowEase;
        [SerializeField] private Ease m_PassSettleEase = Ease.OutQuad;
        public Ease PassSettleEase => m_PassSettleEase;
        [SerializeField, Min(0f)] private float m_PassInputDeadzone = 0.3f;
        public float PassInputDeadzone => m_PassInputDeadzone;

        [Header("Dash")]
        [SerializeField, Min(0f)] private float m_DashDistance = 20.0f;
        public float DashDistance => m_DashDistance;
        [SerializeField, Min(0f)] private float m_DashTime = 0.09f;
        public float DashTime => m_DashTime;
        [SerializeField, Min(0f)] private float m_DashSettleTime = 0.12f;
        public float DashSettleTime => m_DashSettleTime;
        [SerializeField] private Ease m_DashMoveEase = Ease.OutQuad;
        public Ease DashMoveEase => m_DashMoveEase;
        [SerializeField] private Ease m_DashSettleEase = Ease.OutBack;
        public Ease DashSettleEase => m_DashSettleEase;
        [SerializeField, Min(0f)] private float m_DashInputDeadzone = 0.2f;
        public float DashInputDeadzone => m_DashInputDeadzone;

        [Header("Idle Return")]
        [SerializeField, Min(0f)] private float m_IdleReturnDuration = 0.1f;
        public float IdleReturnDuration => m_IdleReturnDuration;
        [SerializeField] private Ease m_IdleReturnEase = Ease.OutQuad;
        public Ease IdleReturnEase => m_IdleReturnEase;

        [Header("Team Colors")]
        [SerializeField] private Color m_HomeColor = Color.red;
        public Color HomeColor => m_HomeColor;
        [SerializeField] private Color m_AwayColor = new Color(0.20f, 0.50f, 1.00f);
        public Color AwayColor => m_AwayColor;

        [Header("Block Feedback")]
        [SerializeField, Min(0.0001f), Tooltip("Ball speed is divided by this to normalize the block-flash VFX scale (distinct from JuiceConfig's shake-scale normalizer).")] private float m_BlockVfxSpeedNormalizer = 15f;
        public float BlockVfxSpeedNormalizer => m_BlockVfxSpeedNormalizer;
        [SerializeField] private float m_BlockVfxScaleMin = 0.6f;
        public float BlockVfxScaleMin => m_BlockVfxScaleMin;
        [SerializeField] private float m_BlockVfxScaleMax = 1.5f;
        public float BlockVfxScaleMax => m_BlockVfxScaleMax;

        [Header("Stun Velocity")]
        [SerializeField, Min(0f), Tooltip("Ball speed above which a football player figure gets stunned on contact.")] private float m_StunVelocityThreshold = 100f;
        public float StunVelocityThreshold => m_StunVelocityThreshold;

        [Header("Player Collider")]
        [SerializeField, Min(0f)] private float m_PlayerRadius = 1f;
        public float PlayerRadius => m_PlayerRadius;
        [SerializeField, Min(0f)] private float m_PlayerHeight = 2f;
        public float PlayerHeight => m_PlayerHeight;
    }
}
