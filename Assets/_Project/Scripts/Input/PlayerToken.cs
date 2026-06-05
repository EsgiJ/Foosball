using UnityEngine;
using UnityEngine.InputSystem;

namespace Foosball
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerToken : MonoBehaviour
    {
        public PlayerInput PlayerInput { get; private set; }
        public int PlayerIndex => PlayerInput.playerIndex;

        void Awake()
        {
            PlayerInput = GetComponent<PlayerInput>();
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable()  => PlayerRegistry.Register(this);
        void OnDisable() => PlayerRegistry.Unregister(this);
    }
}
