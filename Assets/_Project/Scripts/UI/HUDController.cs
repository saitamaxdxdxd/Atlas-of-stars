using UnityEngine;
using TMPro;

namespace AtlasOfStars.UI
{
    /// <summary>
    /// HUD in-game. Pendiente de reconectar cuando Spaceship se reconstruya.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private TMP_Text _speedText;
        [SerializeField] private TMP_Text _fuelText;
        [SerializeField] private TMP_Text _healthText;
    }
}
