using UnityEngine;
using Retropolis.Managers;

namespace Retropolis.Gameplay
{
    // TEMPORAL — borrar antes de release
    public class TestButtons : MonoBehaviour
    {
        public void TriggerGameOver()     => GameManager.Instance.GameOver();
        public void TriggerLevelComplete() => GameManager.Instance.LevelComplete();
    }
}
