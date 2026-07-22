using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    internal sealed class EventBusUnsubscriber : MonoBehaviour
    {
        private readonly List<Action> m_Teardowns = new();

        internal void Track(Action teardown) => m_Teardowns.Add(teardown);

        private void OnDestroy()
        {
            for (int i = 0; i < m_Teardowns.Count; i++)
                m_Teardowns[i]();
            m_Teardowns.Clear();
        }
    }
}
