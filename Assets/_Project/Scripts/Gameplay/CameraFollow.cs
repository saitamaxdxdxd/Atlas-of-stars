using UnityEngine;

namespace AtlasOfStars.Gameplay
{
    /// <summary>
    /// Sigue al target en XY manteniendo el offset de posición y rotación actuales.
    /// Agregar a la Main Camera en GameScene y asignar la Spaceship como target.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float     _smoothSpeed = 5f;

        private Vector3 _offset;

        private void Awake()
        {
            if (_target == null)
            {
                var ship = FindFirstObjectByType<Spaceship>();
                if (ship != null) _target = ship.transform;
            }
        }

        private void Start()
        {
            if (_target == null) return;
            _offset = transform.position - _target.position;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desired = _target.position + _offset;
            transform.position = Vector3.Lerp(transform.position, desired, _smoothSpeed * Time.deltaTime);
        }
    }
}
