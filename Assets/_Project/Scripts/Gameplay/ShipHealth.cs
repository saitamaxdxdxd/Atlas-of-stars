using UnityEngine;
using System;

namespace AtlasOfStars.Gameplay
{
    /// <summary>
    /// Gestiona la vida de la nave (0–100 %).
    /// Agregar al mismo GameObject que Spaceship.
    /// Llamar TakeDamage(amount) desde colisiones u otros sistemas.
    /// </summary>
    public class ShipHealth : MonoBehaviour
    {
        [Header("Vida")]
        [SerializeField] private float _maxHealth = 100f;

        public float HealthNormalized => _health / _maxHealth;
        public float Health           => _health;
        public float MaxHealth        => _maxHealth;
        public bool  IsDead           => _health <= 0f;

        public event Action OnDied;
        public event Action<float> OnHealthChanged;  // normalizado 0–1

        private float _health;

        private void Awake()
        {
            _health = _maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f) return;

            _health = Mathf.Max(0f, _health - amount);
            OnHealthChanged?.Invoke(HealthNormalized);

            if (IsDead)
                OnDied?.Invoke();
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;

            _health = Mathf.Min(_maxHealth, _health + amount);
            OnHealthChanged?.Invoke(HealthNormalized);
        }
    }
}
