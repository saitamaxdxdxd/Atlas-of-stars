using UnityEngine;
using UnityEngine.InputSystem;
using Retropolis.Managers;

namespace Retropolis.Gameplay
{
    /// <summary>
    /// Conecta los botones de pausa con GameManager.
    /// Escucha el estado del juego para mostrar/ocultar el panel.
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private GameObject _pausePanel;

        private void OnEnable()
        {
            GameManager.OnStateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= OnStateChanged;
        }

        private void Start()
        {
            _pausePanel.SetActive(false);
        }

        private void OnStateChanged(GameState state)
        {
            _pausePanel.SetActive(state == GameState.Paused);
        }

        // Botones → conectar en Inspector
        public void OnPausePressed()   => GameManager.Instance.Pause();
        public void OnResumePressed()  => GameManager.Instance.Resume();
        public void OnRestartPressed() => GameManager.Instance.RestartLevel();
        public void OnExitToMenuPressed() => GameManager.Instance.ExitToMenu();

        // Escape o P → toggle pausa
        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            bool togglePressed = kb.escapeKey.wasPressedThisFrame
                              || kb.pKey.wasPressedThisFrame;

            if (!togglePressed) return;

            if (GameManager.Instance.CurrentState == GameState.Paused)
                GameManager.Instance.Resume();
            else
                GameManager.Instance.Pause();
        }
    }
}
