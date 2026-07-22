using System;
using UnityEngine;

namespace Infrastructure
{
    public static class EventBusExtensions
    {
        public static void Subscribe<T>(this MonoBehaviour owner, EventBus bus, Action<T> handler) where T : struct
        {
            bus.Subscribe(handler);

            if (!owner.TryGetComponent<EventBusUnsubscriber>(out var unsubscriber))
            {
                unsubscriber = owner.gameObject.AddComponent<EventBusUnsubscriber>();
                unsubscriber.hideFlags = HideFlags.HideInInspector;
            }

            unsubscriber.Track(() => bus.Unsubscribe(handler));
        }
    }
}
