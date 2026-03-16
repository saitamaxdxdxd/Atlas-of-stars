using UnityEngine;
using UnityEngine.UI;
using Retropolis.Managers;

namespace Retropolis.Menu
{
    /// <summary>
    /// Panel de ajustes: volumen de música, volumen de SFX y selector de idioma.
    ///
    /// Setup en Inspector:
    ///   - Asignar _panel (el GameObject raíz del panel)
    ///   - Asignar _musicSlider y _sfxSlider
    ///   - Asignar _btnEnglish y _btnSpanish
    ///   - Conectar botón Back → OnBackPressed()
    ///   - Conectar _musicSlider.OnValueChanged → OnMusicVolumeChanged()
    ///   - Conectar _sfxSlider.OnValueChanged   → OnSfxVolumeChanged()
    ///   - Conectar _btnEnglish.onClick          → OnEnglishPressed()
    ///   - Conectar _btnSpanish.onClick          → OnSpanishPressed()
    /// </summary>
    public class SettingsController : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject _panel;

        [Header("Volumen")]
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;

        [Header("Idioma — resaltar el activo cambiando color")]
        [SerializeField] private Button _btnEnglish;
        [SerializeField] private Button _btnSpanish;
        [SerializeField] private Color _colorActive   = Color.white;
        [SerializeField] private Color _colorInactive = new Color(1f, 1f, 1f, 0.4f);

        private void Start()
        {
            // Inicializar sliders con valores guardados
            _musicSlider.value = AudioManager.Instance.MusicVolume;
            _sfxSlider.value   = AudioManager.Instance.SfxVolume;

            RefreshLanguageButtons();
        }

        // ── Panel ─────────────────────────────────────────────

        public void Open()
        {
            _panel.SetActive(true);

            // Sincronizar sliders por si el volumen cambió externamente
            _musicSlider.value = AudioManager.Instance.MusicVolume;
            _sfxSlider.value   = AudioManager.Instance.SfxVolume;

            RefreshLanguageButtons();
        }

        public void Close()
        {
            _panel.SetActive(false);
        }

        // ── Volumen ───────────────────────────────────────────

        public void OnMusicVolumeChanged(float value)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }

        public void OnSfxVolumeChanged(float value)
        {
            AudioManager.Instance.SetSfxVolume(value);
        }

        // ── Idioma ────────────────────────────────────────────

        public void OnEnglishPressed()
        {
            LocalizationManager.Instance.SetLanguage(Language.English);
            RefreshLanguageButtons();
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxButtonClick);
        }

        public void OnSpanishPressed()
        {
            LocalizationManager.Instance.SetLanguage(Language.Spanish);
            RefreshLanguageButtons();
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxButtonClick);
        }

        // ── Back ──────────────────────────────────────────────

        public void OnBackPressed()
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxButtonBack);
            Close();
        }

        // ── Helpers ───────────────────────────────────────────

        private void RefreshLanguageButtons()
        {
            bool isEnglish = LocalizationManager.Instance.CurrentLanguage == Language.English;

            SetButtonColor(_btnEnglish, isEnglish  ? _colorActive : _colorInactive);
            SetButtonColor(_btnSpanish, !isEnglish ? _colorActive : _colorInactive);
        }

        private static void SetButtonColor(Button btn, Color color)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img == null) return;
            img.color = color;
        }
    }
}
