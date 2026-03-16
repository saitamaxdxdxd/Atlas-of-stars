using UnityEngine;

namespace AtlasOfStars.Gameplay
{
    /// <summary>
    /// Proyectil básico: viaja en línea recta en la dirección que apuntaba la nave al disparar.
    /// Se destruye solo al acabar su vida útil o al colisionar.
    ///
    /// Setup del prefab:
    ///   - GameObject con este componente + cualquier visual (sprite, mesh, trail)
    ///   - Agregar Collider (ej. SphereCollider con IsTrigger=true) para detectar impactos
    ///   - Rigidbody se configura automáticamente por código
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float _speed    = 25f;
        [SerializeField] private float _lifetime = 4f;

        private void Awake()
        {
            var rb = GetComponent<Rigidbody>();
            rb.useGravity     = false;
            rb.linearDamping  = 0f;
            rb.angularDamping = 0f;
            rb.interpolation  = RigidbodyInterpolation.Interpolate;
            rb.constraints    = RigidbodyConstraints.FreezePositionZ
                              | RigidbodyConstraints.FreezeRotationX
                              | RigidbodyConstraints.FreezeRotationY;

            // impulso inicial en la dirección que apunta la nave al disparar
            rb.linearVelocity = transform.up * _speed;

            Destroy(gameObject, _lifetime);
        }

        private void OnTriggerEnter(Collider other)
        {
            // ignorar la propia nave (el muzzle puede estar dentro del collider del ship)
            if (other.GetComponent<Spaceship>() != null) return;

            // TODO: aplicar daño, efectos, etc.
            Destroy(gameObject);
        }
    }
}
