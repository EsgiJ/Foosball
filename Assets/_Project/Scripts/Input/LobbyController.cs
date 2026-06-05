using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Foosball
{
    [RequireComponent(typeof(PlayerInputManager))]
    public class LobbyController : MonoBehaviour
    {
        [SerializeField] private string m_GameSceneName = "SCB_Game";

        private PlayerInputManager m_Manager;

        void Awake()
        {
            m_Manager = GetComponent<PlayerInputManager>();

            foreach (var token in PlayerRegistry.Tokens.ToArray())
            {
                if (token != null)
                {
                    Destroy(token.gameObject);
                }
            }
            PlayerRegistry.Clear();
        }

        void OnEnable()  => m_Manager.onPlayerJoined += HandlePlayerJoined;
        void OnDisable() => m_Manager.onPlayerJoined -= HandlePlayerJoined;

        private void HandlePlayerJoined(PlayerInput player)
        {
            string deviceName = player.devices.Count > 0 ? player.devices[0].displayName : "unknown";
            Debug.Log($"[Lobby] Player {player.playerIndex} joined with {deviceName}. " + $"({m_Manager.playerCount}/{m_Manager.maxPlayerCount})");

            if (m_Manager.playerCount >= m_Manager.maxPlayerCount)
            {
                m_Manager.DisableJoining();
                SceneManager.LoadScene(m_GameSceneName);
            }
        }
    }
}
