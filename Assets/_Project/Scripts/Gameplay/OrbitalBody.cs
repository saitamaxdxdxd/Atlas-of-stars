using UnityEngine;

namespace AtlasOfStars.Gameplay
{
    /// <summary>
    /// Mueve un GameObject en órbita circular alrededor de un punto o Transform.
    /// No usa física — posición calculada directamente cada FixedUpdate.
    ///
    /// Para satélites: apuntar _center al Transform del planeta padre.
    /// Como el planeta también usa OrbitalBody, el satélite orbita en cadena correctamente.
    /// </summary>
    public class OrbitalBody : MonoBehaviour
    {
        [Header("Órbita")]
        [Tooltip("Punto central de la órbita. Si es null, usa la posición del mundo (0,0,0).")]
        [SerializeField] private Transform _center;

        [Tooltip("Distancia al centro en unidades de mundo.")]
        [SerializeField] private float _radius = 100f;

        [Tooltip("Segundos por vuelta completa. Negativo = sentido horario.")]
        [SerializeField] private float _period = 200f;

        [Tooltip("Ángulo inicial en grados. Sirve para separar planetas en su órbita.")]
        [SerializeField] private float _startAngle = 0f;

        private float _currentAngle;

        // ------------------------------------------------------------------ public API

        /// <summary>
        /// Configura la órbita en runtime (usado por WorldGenerator al instanciar prefabs).
        /// </summary>
        public void Configure(Transform center, float radius, float period, float startAngle)
        {
            _center      = center;
            _radius      = radius;
            _period      = period;
            _startAngle  = startAngle;
            _currentAngle = startAngle * Mathf.Deg2Rad;
            ApplyPosition();
        }

        // ------------------------------------------------------------------ lifecycle

        private void Awake()
        {
            _currentAngle = _startAngle * Mathf.Deg2Rad;
            ApplyPosition();
        }

        private void FixedUpdate()
        {
            if (_period == 0f) return;

            _currentAngle += (2f * Mathf.PI / _period) * Time.fixedDeltaTime;
            ApplyPosition();
        }

        // ------------------------------------------------------------------ private

        private void ApplyPosition()
        {
            Vector3 centerPos = _center != null ? _center.position : Vector3.zero;
            float x = centerPos.x + Mathf.Cos(_currentAngle) * _radius;
            float y = centerPos.y + Mathf.Sin(_currentAngle) * _radius;
            transform.position = new Vector3(x, y, transform.position.z);
        }

        // ------------------------------------------------------------------ editor

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 centerPos = _center != null ? _center.position : Vector3.zero;

            Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.3f);
            DrawCircle(centerPos, _radius, 64);

            // línea del radio en ángulo inicial
            float a = _startAngle * Mathf.Deg2Rad;
            Vector3 startPos = centerPos + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * _radius;
            Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.6f);
            Gizmos.DrawLine(centerPos, startPos);
        }

        private static void DrawCircle(Vector3 center, float radius, int segments)
        {
            float step = 2f * Mathf.PI / segments;
            Vector3 prev = center + new Vector3(radius, 0f, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float a = i * step;
                Vector3 next = center + new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }
#endif
    }
}
