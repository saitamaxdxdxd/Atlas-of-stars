using UnityEngine;

namespace AtlasOfStars.Gameplay
{
    /// <summary>
    /// Dibuja dos líneas de predicción de trayectoria fijas a la nave:
    ///   - Línea azul   : trayectoria actual (sin empuje, solo gravedad + inercia)
    ///   - Línea naranja: trayectoria si se aplica empuje en la dirección que apunta la nave
    ///
    /// Toggle: Tab activa/desactiva ambas líneas.
    /// Cada línea también se puede ocultar individualmente desde el Inspector.
    /// </summary>
    [RequireComponent(typeof(Spaceship), typeof(Rigidbody))]
    public class TrajectoryPredictor : MonoBehaviour
    {
        [Header("Simulación")]
        [SerializeField] private int   _steps    = 120;
        [SerializeField] private float _stepSize = 0.06f;

        [Header("Trayectoria actual (azul)")]
        [SerializeField] private bool  _showCoast  = true;
        [SerializeField] private Color _coastColor = new Color(0.3f, 0.8f, 1f, 0.5f);
        [SerializeField] private float _coastWidth = 0.03f;

        [Header("Trayectoria con empuje (naranja)")]
        [SerializeField] private bool  _showThrust  = true;
        [SerializeField] private Color _thrustColor = new Color(1f, 0.55f, 0.1f, 0.85f);
        [SerializeField] private float _thrustWidth = 0.04f;

        private Spaceship    _ship;
        private Rigidbody    _rb;
        private LineRenderer _coastLine;
        private LineRenderer _thrustLine;
        private Vector3[]    _points;
        private bool         _visible = true;

        // estado físico cacheado en FixedUpdate para evitar oscilación por interpolación
        private Vector3 _cachedVelocity;
        private Vector3 _cachedPosition;
        private Vector3 _cachedForward;

        // ------------------------------------------------------------------ lifecycle

        private void Awake()
        {
            _ship   = GetComponent<Spaceship>();
            _rb     = GetComponent<Rigidbody>();
            _points = new Vector3[_steps];

            _coastLine  = CreateLine("Trajectory_Coast",  _coastColor,  _coastWidth);
            _thrustLine = CreateLine("Trajectory_Thrust", _thrustColor, _thrustWidth);
        }

        private void FixedUpdate()
        {
            _cachedVelocity   = _rb.linearVelocity;
            _cachedVelocity.z = 0f;
            _cachedPosition   = transform.position;
            _cachedForward    = transform.up;
            _cachedForward.z  = 0f;
        }

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current.tabKey.wasPressedThisFrame)
                SetVisible(!_visible);
        }

        private void LateUpdate()
        {
            _coastLine.enabled  = _visible && _showCoast;
            _thrustLine.enabled = _visible && _showThrust;

            if (_coastLine.enabled)  SimulateLine(_coastLine,  applyThrust: false);
            if (_thrustLine.enabled) SimulateLine(_thrustLine, applyThrust: true);
        }

        public void SetVisible(bool value)
        {
            _visible = value;
        }

        // ------------------------------------------------------------------ simulation

        private void SimulateLine(LineRenderer line, bool applyThrust)
        {
            Vector3 pos     = _cachedPosition;
            Vector3 vel     = _cachedVelocity;
            float   mass    = _rb.mass;
            Vector3 forward = _cachedForward;

            bool canThrust = applyThrust && _ship.HasFuel;

            for (int i = 0; i < _steps; i++)
            {
                Vector3 totalForce = Vector3.zero;
                foreach (var src in GravitySource.All)
                {
                    if (src == null) continue;
                    Vector3 toSrc = src.transform.position - pos;
                    toSrc.z = 0f;
                    float distSq = Mathf.Max(toSrc.sqrMagnitude, 0.25f);
                    totalForce += toSrc.normalized * (_ship.GravConstant * src.Mass / distSq);
                }

                if (canThrust)
                    totalForce += forward * _ship.ThrustForce;

                vel += (totalForce / mass) * _stepSize;
                pos += vel * _stepSize;

                _points[i] = pos;
            }

            line.SetPositions(_points);
        }

        // ------------------------------------------------------------------ helpers

        private LineRenderer CreateLine(string goName, Color color, float width)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;

            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace  = true;
            lr.positionCount  = _steps;
            lr.startWidth     = width;
            lr.endWidth       = 0f;
            lr.numCapVertices = 4;
            lr.material       = new Material(Shader.Find("Sprites/Default"));
            lr.startColor     = color;
            lr.endColor       = new Color(color.r, color.g, color.b, 0f);

            return lr;
        }
    }
}
