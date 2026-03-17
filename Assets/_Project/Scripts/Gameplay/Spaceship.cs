using UnityEngine;
using UnityEngine.InputSystem;

namespace AtlasOfStars.Gameplay
{
    /// <summary>
    /// Nave espacial controlada por el jugador (3D, movimiento en plano XY).
    ///
    /// Controles PC:
    ///   W / ↑ / Space  → empuje (consume combustible)
    ///   A / ←          → rotar izquierda
    ///   D / →          → rotar derecha
    ///   S / Shift      → estabilizador: frena la rotación (consume menos combustible)
    ///
    /// Setup en Inspector:
    ///   - Agregar Rigidbody: Use Gravity=false, Drag=0, Angular Drag=0
    ///     Constraints: Freeze Position Z, Freeze Rotation X y Y
    ///   - Agregar GravitySource con Mass muy pequeña (ej. 0.05)
    ///   - El modelo/sprite debe apuntar hacia arriba (eje Y local = frente de la nave)
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Spaceship : MonoBehaviour
    {
        [Header("Propulsión")]
        [SerializeField] private float _thrustForce   = 2f;
        [SerializeField] private float _rotationSpeed = 75f;    // grados / segundo

        [Header("Combustible")]
        [SerializeField] private float _fuelCapacity  = 100f;
        [SerializeField] private float _fuelBurnRate  = 12f;    // unidades / segundo

        [Header("Estabilizador")]
        [SerializeField] private float _stabBurnRate  = 4f;     // combustible/segundo (menos que thrust)
        [SerializeField] private float _stabSpeed     = 8f;     // qué tan rápido frena el giro

        [Header("Gravedad")]
        [SerializeField] private float _gravConstant  = 6f;

        // ---- estado público ----
        public float FuelNormalized  => _fuel / _fuelCapacity;
        public float Speed           => _rb != null ? _rb.linearVelocity.magnitude : 0f;
        public bool  IsThrusting     { get; private set; }
        public bool  IsStabilizing   { get; private set; }
        public bool  HasFuel         => _fuel > 0f;
        public float ThrustForce     => _thrustForce;
        public float GravConstant    => _gravConstant;

        private Rigidbody     _rb;
        private GravitySource _ownSource;
        private float         _fuel;

        // input leído en Update, aplicado en FixedUpdate
        private float _inputRotation;
        private bool  _inputThrust;
        private bool  _inputStabilize;

        // ------------------------------------------------------------------ lifecycle

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity     = false;
            _rb.linearDamping  = 0f;
            _rb.angularDamping = 0f;
            _rb.interpolation  = RigidbodyInterpolation.Interpolate;

            // mantener nave en plano XY, solo rotar en Z
            _rb.constraints = RigidbodyConstraints.FreezePositionZ
                            | RigidbodyConstraints.FreezeRotationX
                            | RigidbodyConstraints.FreezeRotationY;

            _ownSource = GetComponent<GravitySource>();
            _fuel      = _fuelCapacity;
        }

        private void Update()
        {
            ReadInput();
        }

        private void FixedUpdate()
        {
            ApplyRotation();
            ApplyThrust();
            ApplyStabilizer();
            ApplyGravity();
        }

        // ------------------------------------------------------------------ input

        private void ReadInput()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            _inputRotation = 0f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  _inputRotation =  1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) _inputRotation = -1f;

            _inputThrust = kb.wKey.isPressed
                        || kb.upArrowKey.isPressed
                        || kb.spaceKey.isPressed;

            _inputStabilize = kb.sKey.isPressed || kb.leftShiftKey.isPressed;
        }

        private void ApplyRotation()
        {
            if (_inputRotation == 0f) return;

            float angle    = _inputRotation * _rotationSpeed * Time.fixedDeltaTime;
            Quaternion rot = _rb.rotation * Quaternion.Euler(0f, 0f, angle);
            _rb.MoveRotation(rot);
        }

        private void ApplyThrust()
        {
            IsThrusting = _inputThrust && _fuel > 0f;

            if (IsThrusting)
            {
                _rb.AddForce(transform.up * _thrustForce, ForceMode.Force);
                _fuel = Mathf.Max(0f, _fuel - _fuelBurnRate * Time.fixedDeltaTime);
            }
        }

        private void ApplyStabilizer()
        {
            IsStabilizing = _inputStabilize && HasFuel && _rb.angularVelocity != Vector3.zero;

            if (!IsStabilizing) return;

            // frenar la velocidad angular hacia cero suavemente
            _rb.angularVelocity = Vector3.Lerp(_rb.angularVelocity, Vector3.zero, _stabSpeed * Time.fixedDeltaTime);

            // consumir combustible proporcional al giro que queda
            float spinAmount = Mathf.Abs(_rb.angularVelocity.z);
            if (spinAmount > 0.01f)
                _fuel = Mathf.Max(0f, _fuel - _stabBurnRate * Time.fixedDeltaTime);
            else
                _rb.angularVelocity = Vector3.zero; // snap a cero para evitar micro-oscilaciones
        }

        // ------------------------------------------------------------------ gravity

        private void ApplyGravity()
        {
            foreach (var src in GravitySource.All)
            {
                if (src == null || src == _ownSource) continue;

                Vector3 toSource = src.transform.position - transform.position;
                // solo fuerza en XY — ignoramos diferencia en Z
                toSource.z = 0f;

                float distSq   = Mathf.Max(toSource.sqrMagnitude, 0.25f);
                float forceMag = _gravConstant * src.Mass / distSq;

                _rb.AddForce(toSource.normalized * forceMag, ForceMode.Force);
            }
        }
    }
}
