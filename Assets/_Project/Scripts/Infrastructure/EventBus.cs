using System;
using System.Collections.Generic;

namespace Infrastructure
{
    public sealed class EventBus
    {
        private readonly Dictionary<Type, Delegate> m_Handlers = new();

        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            m_Handlers.TryGetValue(typeof(T), out var existing);
            m_Handlers[typeof(T)] = Delegate.Combine(existing, handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            if (!m_Handlers.TryGetValue(typeof(T), out var existing))
                return;

            var remaining = Delegate.Remove(existing, handler);
            if (remaining == null)
                m_Handlers.Remove(typeof(T));
            else
                m_Handlers[typeof(T)] = remaining;
        }

        public void Publish<T>(T payload) where T : struct
        {
            if (m_Handlers.TryGetValue(typeof(T), out var existing) && existing is Action<T> handler)
                handler.Invoke(payload);
        }
    }
}
