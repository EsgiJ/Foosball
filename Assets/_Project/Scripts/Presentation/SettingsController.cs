using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using Foosball.Gameplay;

namespace Foosball.Presentation
{
    public class SettingsController : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject m_Panel;
        [SerializeField] private Button m_BackButton;

        [Header("Audio")]
        [SerializeField] private AudioMixer m_Mixer;
        [SerializeField] private Slider m_MasterSlider;
        [SerializeField] private Slider m_MusicSlider;
        [SerializeField] private Slider m_SfxSlider;

        [Header("Gamepad")]
        [SerializeField] private Toggle m_RumbleToggle;

        [Header("Controls Help (optional)")]
        [SerializeField] private GameObject m_ControlsPanel;
        [SerializeField] private Button m_ControlsButton;
        [SerializeField] private Button m_ControlsCloseButton;
        [SerializeField] private TMP_Text m_ControlsText;

        /* Injected dependencies */
        private RumbleManager m_RumbleManager;
        private GameStateManager m_GameStateManager;

        private const string K_Master = "vol_master";
        private const string K_Music  = "vol_music";
        private const string K_Sfx    = "vol_sfx";
        private const string K_Rumble = "rumble_on";

    #region Unity Lifecycle
        public void Init(RumbleManager rumbleManager, GameStateManager gameStateManager)
        {
            m_RumbleManager = rumbleManager;
            m_GameStateManager = gameStateManager;
        }

        void Awake()
        {
            float master = PlayerPrefs.GetFloat(K_Master, 0.9f);
            float music  = PlayerPrefs.GetFloat(K_Music,  0.7f);
            float sfx    = PlayerPrefs.GetFloat(K_Sfx,    0.9f);

            if (m_MasterSlider) { m_MasterSlider.SetValueWithoutNotify(master); m_MasterSlider.onValueChanged.AddListener(SetMaster); }
            if (m_MusicSlider)  { m_MusicSlider.SetValueWithoutNotify(music);   m_MusicSlider.onValueChanged.AddListener(SetMusic); }
            if (m_SfxSlider)    { m_SfxSlider.SetValueWithoutNotify(sfx);       m_SfxSlider.onValueChanged.AddListener(SetSfx); }

            ApplyMaster(master); ApplyMusic(music); ApplySfx(sfx);

            if (m_BackButton) m_BackButton.onClick.AddListener(() => m_GameStateManager?.CloseSettings());
            if (m_ControlsButton)      m_ControlsButton.onClick.AddListener(OpenControls);
            if (m_ControlsCloseButton) m_ControlsCloseButton.onClick.AddListener(CloseControls);

            if (m_Panel)         m_Panel.SetActive(false);
            if (m_ControlsPanel) m_ControlsPanel.SetActive(false);

            FillControlsText();
        }

        void Start()
        {
            bool rumble = PlayerPrefs.GetInt(K_Rumble, 1) == 1;
            if (m_RumbleToggle) { m_RumbleToggle.SetIsOnWithoutNotify(rumble); m_RumbleToggle.onValueChanged.AddListener(SetRumble); }
            m_RumbleManager?.SetEnabled(rumble);
        }
    #endregion

    #region Controls Panel
        private void OpenControls()  { if (m_ControlsPanel) m_ControlsPanel.SetActive(true); }
        private void CloseControls() { if (m_ControlsPanel) m_ControlsPanel.SetActive(false); }

        private void FillControlsText()
        {
            if (m_ControlsText == null) return;
            m_ControlsText.text =
                "<b>P1 — Klavye (Sol)</b>\n" +
                "Hareket / Nişan: W A S D\n" +
                "Şut: Space\n" +
                "Duruş (savunma/atak): Left Shift\n" +
                "Çubuk değiştir: Q\n" +
                "Masayı salla: E\n\n" +
                "<b>P2 — Klavye (Sağ)</b>\n" +
                "Hareket / Nişan: Ok tuşları\n" +
                "Şut: Right Shift\n" +
                "Duruş: Right Ctrl\n" +
                "Çubuk değiştir: ]\n" +
                "Masayı salla: [\n\n" +
                "<b>Gamepad</b>\n" +
                "Hareket / Nişan: Sol analog\n" +
                "Şut: A / Cross\n" +
                "Duruş: Sağ tetik (RT)\n" +
                "Çubuk değiştir: RB\n" +
                "Masayı salla: Y / Triangle\n" +
                "Duraklat: Start";
        }
    #endregion

    #region Audio Sliders
        private void SetMaster(float v) { ApplyMaster(v); PlayerPrefs.SetFloat(K_Master, v); }
        private void SetMusic(float v)  { ApplyMusic(v);  PlayerPrefs.SetFloat(K_Music, v); }
        private void SetSfx(float v)    { ApplySfx(v);    PlayerPrefs.SetFloat(K_Sfx, v); }

        private void ApplyMaster(float v) => SetMixer("MasterVolume", v);
        private void ApplyMusic(float v)  => SetMixer("MusicVolume", v);
        private void ApplySfx(float v)    => SetMixer("SfxVolume", v);

        private void SetMixer(string param, float value01)
        {
            if (m_Mixer == null) return;
            float db = value01 <= 0.0001f ? -80f : Mathf.Log10(value01) * 20f;
            m_Mixer.SetFloat(param, db);
        }
    #endregion

    #region Gamepad Rumble
        private void SetRumble(bool on)
        {
            PlayerPrefs.SetInt(K_Rumble, on ? 1 : 0);
            m_RumbleManager?.SetEnabled(on);
        }
    #endregion
    }
}
