using UnityEngine;
using TMPro;
using AtlasOfStars.Gameplay;

namespace AtlasOfStars.UI
{
    /// <summary>
    /// Actualiza el HUD en GameScene:
    ///   - Velocidad de la nave (TMP)
    ///   - Barra + porcentaje de combustible
    ///   - Barra + porcentaje de vida
    ///
    /// Setup en Inspector:
    ///   Spaceship  → referencia a la nave del jugador
    ///   ShipHealth → referencia al componente ShipHealth de la nave
    ///   Cada sección necesita un Image con FillMethod=Horizontal (o Vertical)
    ///   y el tipo Image Type = Filled para usar fillAmount.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Referencias — Nave")]
        [SerializeField] private Spaceship  _spaceship;
        [SerializeField] private ShipHealth _shipHealth;

        [Header("Velocidad")]
        [SerializeField] private TMP_Text _speedText;

        [Header("Combustible")]
        [SerializeField] private TMP_Text _fuelText;

        [Header("Vida")]
        [SerializeField] private TMP_Text _healthText;

        private void Update()
        {
            if (_spaceship == null) return;

            UpdateSpeed();
            UpdateFuel();
            UpdateHealth();
        }

        // ------------------------------------------------------------------ privados

        private void UpdateSpeed()
        {
            if (_speedText == null) return;
            // Velocidad en unidades/s, redondeada a 1 decimal
            _speedText.text = $"{_spaceship.Speed:F1} u/s";
        }

        private void UpdateFuel()
        {
            if (_fuelText == null) return;
            int percent = Mathf.RoundToInt(_spaceship.FuelNormalized * 100f);
            _fuelText.text = $"{percent}%";
        }

        private void UpdateHealth()
        {
            if (_shipHealth == null || _healthText == null) return;
            int percent = Mathf.RoundToInt(_shipHealth.HealthNormalized * 100f);
            _healthText.text = $"{percent}%";
        }
    }
}
