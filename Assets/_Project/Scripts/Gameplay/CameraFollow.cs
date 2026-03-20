using UnityEngine;

namespace AtlasOfStars.Gameplay
{
    /// <summary>
    /// Cámara que sigue al jugador (tag "Player").
    /// Perspectiva con tilt en X para ver la profundidad de los pozos gravitacionales.
    ///
    /// Setup en escena:
    ///   1. Agregar este script a la Main Camera.
    ///   2. Dejar _target vacío — se auto-asigna buscando el tag "Player".
    ///   3. Ajustar _height y _tiltAngle según el efecto visual deseado.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("Si se deja vacío, busca automáticamente el GameObject con tag Player.")]
        [SerializeField] private Transform _target;

        [Header("Offset desde el jugador")]
        [Tooltip("Cuánto sube la cámara en Y sobre la nave.")]
        [SerializeField] private float _offsetY = 25f;
        [Tooltip("Cuánto retrocede la cámara en Z (debe ser negativo para verse desde atrás).")]
        [SerializeField] private float _offsetZ = -30f;
        [Tooltip("Suavidad del seguimiento. Valores altos = más pegada al jugador.")]
        [SerializeField] private float _smoothSpeed = 6f;

        [Header("Ángulo")]
        [Tooltip("Tilt en X. 45–55° = pozos gravitacionales bien visibles.")]
        [SerializeField, Range(0f, 90f)] private float _tiltAngle = 50f;

        private Vector3 _velocity;

        // ------------------------------------------------------------------ lifecycle

        private void Start()
        {
            transform.rotation = Quaternion.Euler(_tiltAngle, 0f, 0f);
            TryFindPlayer();
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                TryFindPlayer();
                return;
            }

            transform.position = Vector3.SmoothDamp(
                transform.position,
                GetTargetPosition(),
                ref _velocity,
                1f / _smoothSpeed
            );
        }

        // ------------------------------------------------------------------ private

        private void TryFindPlayer()
        {
            if (_target != null) return;
            var player = GameObject.FindWithTag("Player");
            if (player == null) return;
            _target = player.transform;
            transform.position = GetTargetPosition();
        }

        private Vector3 GetTargetPosition() =>
            new Vector3(_target.position.x, _target.position.y + _offsetY, _target.position.z + _offsetZ);
    }
}
