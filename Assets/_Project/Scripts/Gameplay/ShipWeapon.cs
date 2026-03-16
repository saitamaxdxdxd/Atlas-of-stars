using UnityEngine;
using UnityEngine.InputSystem;

namespace AtlasOfStars.Gameplay
{
    /// <summary>
    /// Arma de la nave: dispara proyectiles desde el muzzle.
    ///
    /// Controles PC:
    ///   Click izquierdo  → disparar
    ///   F                → disparar (alternativa teclado)
    ///
    /// Setup:
    ///   1. Agregar este componente al mismo GameObject que Spaceship.
    ///   2. Crear un GameObject hijo en la punta de la nave, llamarlo "Muzzle".
    ///      Asegurarse de que su eje Y local apunte hacia el frente de la nave.
    ///   3. Asignar el Muzzle en el Inspector.
    ///   4. Crear un prefab con el componente Projectile y asignarlo en ProjectilePrefab.
    /// </summary>
    public class ShipWeapon : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform  _muzzle;

        [Header("Disparo")]
        [SerializeField] private float _fireRate    = 0.25f;  // segundos entre disparos
        [SerializeField] private float _recoilForce = 1.5f;   // impulso hacia atrás al disparar (0 = sin retroceso)

        private Rigidbody _rb;
        private float     _nextFireTime;

        // ------------------------------------------------------------------ lifecycle

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (WantsToFire() && Time.time >= _nextFireTime)
                Fire();
        }

        // ------------------------------------------------------------------ input

        private bool WantsToFire()
        {
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.isPressed) return true;

            var kb = Keyboard.current;
            if (kb != null && kb.fKey.isPressed) return true;

            return false;
        }

        // ------------------------------------------------------------------ fire

        private void Fire()
        {
            if (_projectilePrefab == null || _muzzle == null) return;

            _nextFireTime = Time.time + _fireRate;
            Instantiate(_projectilePrefab, _muzzle.position, _muzzle.rotation);

            // retroceso: impulso opuesto a la dirección de disparo
            if (_rb != null && _recoilForce > 0f)
                _rb.AddForce(-_muzzle.up * _recoilForce, ForceMode.Impulse);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_muzzle == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_muzzle.position, 0.1f);
            Gizmos.DrawRay(_muzzle.position, _muzzle.up * 0.5f);
        }
#endif
    }
}
