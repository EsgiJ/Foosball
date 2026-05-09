using UnityEngine;
using DG.Tweening;

namespace Foosball.Rod
{
    [CreateAssetMenu(fileName = "RodConfig", menuName = "Foosball/Rod Config")]
    public class RodConfig : ScriptableObject
    {
        [Header("Movement")]
        [Min(0f)] public float MovementSpeed = 5f;
        public float MinZPos = -1.5f;
        public float MaxZPos = 1.5f;
        [Min(0f)] public float UsableExtent = 12f;
        [Range(1, 5)] public int FootballPlayerCount = 3;

        [Header("Stance Rotations")]
        public float DefenseRotation = 0f;
        public float AttackRotation = 30f;
        public float ShootRotation = -180f;

        [Header("Rotation Animations")]
        [Min(0f)] public float ShootDuration = 0.2f;
        public Ease ShootEase = Ease.OutQuad;

        [Min(0f)] public float DefenseStanceDuration = 0.2f;
        public Ease DefenseStanceEase = Ease.OutQuad;

        [Min(0f)] public float AttackStanceDuration = 0.2f;
        public Ease AttackStanceEase = Ease.OutQuad;

        [Min(0f)] public float StunRotationDuration = 0.2f;
        public Ease StunEase = Ease.OutBounce;
        [Min(0f)] public float StunDuration = 2f;

        [Header("Wiggle Effect")]
        public float WigglePunchAmount = 0.15f;
        public float WigglePunchRotation = 10f;
        public float WiggleDuration = 0.4f;
        public int WiggleVibrato = 8;
        public float WiggleElasticity = 0.5f;

        [Header("Struggle Effect")]
        public Vector3 StrugglePunchAngle = new Vector3(0f, 0f, 8f);
        public float StruggleDuration = 0.5f;
        public int StruggleVibrato = 4;
        public float StruggleElasticity = 0.6f;

        [Header("Outline")]
        public Color OutlineColor = Color.yellow;
        public float OutlineWidth = 6f;
        public float OutlineFadeDuration = 0.2f;
    }
}