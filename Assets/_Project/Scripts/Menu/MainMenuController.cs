using UnityEngine;
using Retropolis.Core;
using Retropolis.Managers;

namespace Retropolis.Menu
{
    /// <summary>
    /// Controla la pantalla principal del menú.
    /// Conectar botones de Unity UI a los métodos públicos.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private SettingsController _settings;

        private void Start()
        {
            AudioManager.Instance.PlayPlaylist(AudioManager.Instance.musicMainMenu);
        }

        // Botón Play → conectar en Inspector
        public void OnPlayPressed()
        {
            SceneLoader.Instance.Load(SceneNames.LevelSelect);
        }

        // Botón Settings → conectar en Inspector
        public void OnSettingsPressed()
        {
            _settings.Open();
        }

        // Botón Quit
        public void OnQuitPressed()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
