using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Foosball
{
    public class SetupController : MonoBehaviour
    {
        [Header("Teams (sides)")]
        [SerializeField] private TeamController m_LeftTeam;    // Home
        [SerializeField] private TeamController m_RightTeam;   // Away

        [Header("Formations")]
        [SerializeField] private List<Formation> m_Formations = new()
        {
            new Formation { label = "3-3-3", rodCounts = new[] { 3, 3, 3 } },
            new Formation { label = "4-3-2", rodCounts = new[] { 4, 3, 2 } },
            new Formation { label = "2-3-4", rodCounts = new[] { 2, 3, 4 } },
            new Formation { label = "3-4-2", rodCounts = new[] { 3, 4, 2 } }
        };

        [Header("UI Left side")]
        [SerializeField] private TMP_Text m_LeftPlayerLabel;
        [SerializeField] private Image m_LeftDeviceIcon;
        [SerializeField] private TMP_Text m_LeftFormationText;
        [SerializeField] private GameObject m_LeftReadyMark;
        [SerializeField] private RectTransform[] m_LeftRodSlots;   

        [Header("UI Right side")]
        [SerializeField] private TMP_Text m_RightPlayerLabel;
        [SerializeField] private Image m_RightDeviceIcon;
        [SerializeField] private TMP_Text m_RightFormationText;
        [SerializeField] private GameObject m_RightReadyMark;
        [SerializeField] private RectTransform[] m_RightRodSlots;

        [Header("UI Shared")]
        [SerializeField] private GameObject m_PlayerIconPrefab;    
        [SerializeField] private Sprite m_KeyboardIcon;
        [SerializeField] private Sprite m_GamepadIcon;
        [SerializeField] private Color m_P1Color = Color.red;
        [SerializeField] private Color m_P2Color = new Color(0.2f, 0.5f, 1f);

        [Header("Input")]
        [SerializeField] private float m_AxisThreshold = 0.5f;

        private class PlayerSetup
        {
            public int id;
            public Color color;
            public string baseScheme;   // P1: KeyboardLeft, P2: KeyboardRight
            public bool useGamepad;
            public Gamepad pad;
            public int side;            // 0 = left, 1 = right
            public int formationIndex;
            public bool ready;
            public string Scheme => useGamepad ? "Gamepad" : baseScheme;
        }

        private PlayerSetup[] m_Players;
        private readonly bool[] m_AxisLatched = new bool[2];

    #region Lifecycle
        void Awake()
        {
            m_Players = new[]
            {
                new PlayerSetup { id = 1, color = m_P1Color, baseScheme = "KeyboardLeft",  side = 0 },
                new PlayerSetup { id = 2, color = m_P2Color, baseScheme = "KeyboardRight", side = 1 },
            };
        }

        void OnEnable()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged += HandleState;

            if (GameStateManager.Instance.CurrentState == GameState.Setup)
                EnterSetup();
        }

        void OnDisable()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= HandleState;
        }

        void Update()
        {
            if (GameStateManager.Instance == null ||
                GameStateManager.Instance.CurrentState != GameState.Setup) return;

            PollSide(0, m_LeftTeam);
            PollSide(1, m_RightTeam);
            DetectGamepad();
        }

        private void HandleState(GameState previous, GameState next)
        {
            if (next == GameState.Setup) EnterSetup();
        }
    #endregion

    #region Setup flow
        private void EnterSetup()
        {
            foreach (var p in m_Players) 
            {
                p.ready = false;
            }
            m_AxisLatched[0] = m_AxisLatched[1] = false;
            ApplySchemesAndEnable();
            RefreshAll();
        }

        private void ApplySchemesAndEnable()
        {
            var pl = GetPlayerOnSide(0);
            var pr = GetPlayerOnSide(1);
            m_LeftTeam.AssignScheme(pl.Scheme,  pl.useGamepad ? pl.pad : null);
            m_RightTeam.AssignScheme(pr.Scheme, pr.useGamepad ? pr.pad : null);
            m_LeftTeam.EnableInput();
            m_RightTeam.EnableInput();
        }

        private void PollSide(int side, TeamController team)
        {
            if (team == null || team.Actions == null) 
                return;

            var aim = team.Actions.FindAction("Aim");
            float x = aim != null ? aim.ReadValue<Vector2>().x : 0f;

            if (Mathf.Abs(x) > m_AxisThreshold && !m_AxisLatched[side])
            {
                CycleFormation(side, x > 0 ? 1 : -1);
                m_AxisLatched[side] = true;
            }
            else if (Mathf.Abs(x) < m_AxisThreshold * 0.5f)
            {
                m_AxisLatched[side] = false;
            }

            if (team.Actions.FindAction("Shoot")?.WasPressedThisFrame() == true)
                ToggleReady(side);

            if (team.Actions.FindAction("ChangeRod")?.WasPressedThisFrame() == true)
                SwapSides();
        }

        private void CycleFormation(int side, int dir)
        {
            var p = GetPlayerOnSide(side);
            int count = m_Formations.Count;
            p.formationIndex = (p.formationIndex + dir + count) % count;
            AudioManager.Instance?.PlayMenuButtonClick();
            RefreshSide(side);
        }

        private void ToggleReady(int side)
        {
            var p = GetPlayerOnSide(side);
            p.ready = !p.ready;
            AudioManager.Instance?.PlayToggleReady();
            RefreshSide(side);

            if (m_Players[0].ready && m_Players[1].ready)
                ApplyAndStart();
        }

        private void SwapSides()
        {
            foreach (var p in m_Players) 
                p.side = 1 - p.side;
            AudioManager.Instance?.PlaySwapSides();
            ApplySchemesAndEnable();
            RefreshAll();
        }

        private void DetectGamepad()
        {
            foreach (var pad in Gamepad.all)
            {
                if (!AnyButtonPressed(pad)) 
                    continue;

                bool taken = false;
                foreach (var p in m_Players) 
                {
                    if (p.pad == pad) 
                        taken = true;
                }
                
                if (taken) 
                    continue;

                foreach (var p in m_Players)
                {
                    if (!p.useGamepad)
                    {
                        p.useGamepad = true;
                        p.pad = pad;
                        ApplySchemesAndEnable();
                        RefreshAll();
                        return;
                    }
                }
            }
        }

        private bool AnyButtonPressed(Gamepad pad)
        {
            return pad.buttonSouth.wasPressedThisFrame || pad.buttonEast.wasPressedThisFrame ||
                   pad.buttonWest.wasPressedThisFrame  || pad.buttonNorth.wasPressedThisFrame ||
                   pad.startButton.wasPressedThisFrame || pad.leftShoulder.wasPressedThisFrame ||
                   pad.rightShoulder.wasPressedThisFrame;
        }

        private void ApplyAndStart()
        {
            ApplyFormation(0, m_LeftTeam);
            ApplyFormation(1, m_RightTeam);
            GameStateManager.Instance.StartCountdown();
        }

        private void ApplyFormation(int side, TeamController team)
        {
            var f = m_Formations[GetPlayerOnSide(side).formationIndex];
            team.ApplyFormation(f.rodCounts);
        }
    #endregion

    #region UI
        private void RefreshAll()
        {
            RefreshSide(0);
            RefreshSide(1);
        }

        private void RefreshSide(int side)
        {
            var p = GetPlayerOnSide(side);
            bool left = side == 0;

            var label = left ? m_LeftPlayerLabel : m_RightPlayerLabel;
            var icon  = left ? m_LeftDeviceIcon  : m_RightDeviceIcon;
            var ftext = left ? m_LeftFormationText : m_RightFormationText;
            var ready = left ? m_LeftReadyMark   : m_RightReadyMark;
            var slots = left ? m_LeftRodSlots    : m_RightRodSlots;

            if (label) 
            { 
                label.text = "P" + p.id; 
                label.color = p.color; 
            }
            if (icon)  
            {
                icon.sprite = p.useGamepad ? m_GamepadIcon : m_KeyboardIcon;
                icon.color = p.color;
            }
            if (ftext) 
            { 
                ftext.text = m_Formations[p.formationIndex].label;
            }
            if (ready) 
            { 
                ready.SetActive(p.ready);
            }

            RefreshRodPreview(slots, m_Formations[p.formationIndex].rodCounts, p.color);
        }

        private void RefreshRodPreview(RectTransform[] slots, int[] counts, Color color)
        {
            if (slots == null || m_PlayerIconPrefab == null) 
                return;

            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot == null) 
                    continue;

                for (int c = slot.childCount - 1; c >= 0; c--)
                    Destroy(slot.GetChild(c).gameObject);

                int n = i < counts.Length ? counts[i] : 0;
                for (int k = 0; k < n; k++)
                {
                    var icon = Instantiate(m_PlayerIconPrefab, slot);
                    var img = icon.GetComponent<Image>();
                    if (img != null) img.color = color;
                }
            }
        }
    #endregion

    #region Helpers
        private TeamController TeamForSide(int side) => side == 0 ? m_LeftTeam : m_RightTeam;
        private PlayerSetup GetPlayerOnSide(int side) => m_Players[0].side == side ? m_Players[0] : m_Players[1];
    #endregion
    }
}