using UnityEngine;
using Retropolis.Managers;

namespace Retropolis.Gameplay
{
    /// <summary>
    /// Panel de Game Over. Se muestra automáticamente cuando GameManager → GameOver.
    ///
    /// Setup en Inspector:
    ///   - Asignar _panel (el GameObject raíz del panel, empieza desactivado)
    ///   - Conectar Btn_Retry  → OnRetryPressed()
    ///   - Conectar Btn_Menu   → OnMenuPressed()
    /// </summary>
    public class GameOverController : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;

        private void OnEnable()
        {
            GameManager.OnStateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
                Show();
        }

        private void Show()
        {
            _panel.SetActive(true);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxLevelFail);
        }

        // ── Botones ───────────────────────────────────────────

        public void OnRetryPressed()
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxButtonClick);
            _panel.SetActive(false);
            GameManager.Instance.RestartLevel();
        }

        public void OnMenuPressed()
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxButtonBack);
            _panel.SetActive(false);
            GameManager.Instance.ExitToMenu();
        }
    }
}
