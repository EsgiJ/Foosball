using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Foosball.Presentation
{
    public class RumbleManager : MonoBehaviour
    {
        [Header("Global")]
        [SerializeField] private bool m_RumbleEnabled = true;

        private readonly Dictionary<Gamepad, Coroutine> m_Active = new();

        void OnDisable() => StopAll();
        void OnApplicationFocus(bool focus) { if (!focus) StopAll(); }   
        void OnApplicationPause(bool paused) { if (paused) StopAll(); }

        public void SetEnabled(bool on)
        {
            m_RumbleEnabled = on;
            if (!on) StopAll();
        }

        public void Rumble(Gamepad pad, float low, float high, float duration)
        {
            if (!m_RumbleEnabled || pad == null) return;

            if (m_Active.TryGetValue(pad, out var co) && co != null)
                StopCoroutine(co);

            m_Active[pad] = StartCoroutine(RumbleRoutine(pad, low, high, duration));
        }

        private IEnumerator RumbleRoutine(Gamepad pad, float low, float high, float duration)
        {
            pad.SetMotorSpeeds(low, high);
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            pad.SetMotorSpeeds(0f, 0f);
            m_Active.Remove(pad);
        }

        public void StopAll()
        {
            foreach (var kv in m_Active)
            {
                if (kv.Value != null) StopCoroutine(kv.Value);
                kv.Key?.SetMotorSpeeds(0f, 0f);
            }
            m_Active.Clear();
            foreach (var g in Gamepad.all)
            {
                g.SetMotorSpeeds(0f, 0f);
            }
        }

        public void RumbleHit(Gamepad pad)   => Rumble(pad, 0.15f, 0.25f, 0.07f);  
        public void RumbleBlock(Gamepad pad) => Rumble(pad, 0.35f, 0.60f, 0.12f);  
        public void RumbleStun(Gamepad pad)  => Rumble(pad, 0.60f, 0.90f, 0.25f); 

        public void RumbleGoal(Gamepad scorer, Gamepad conceder)
        {
            Rumble(scorer,   0.50f, 0.80f, 0.40f);
        }
    }
}