using UnityEngine;

namespace AtlasOfStars.Gameplay
{
    public enum CelestialBodyType { Star, Planet, Moon, AsteroidField, DeadPlanet }

    /// <summary>
    /// Datos de un cuerpo celeste. Se combina con OrbitalBody y GravitySource en el mismo GO.
    ///
    /// Zona de influencia: radio en el que se genera contenido procedural (derelictos, pickups).
    /// Los eventos OnPlayerEnterInfluence / OnPlayerExitInfluence los dispara este componente
    /// cuando la nave entra o sale de esa zona — otros sistemas los escuchan.
    /// </summary>
    public class CelestialBody : MonoBehaviour
    {
        [Header("Identidad")]
        [SerializeField] private string           _bodyName = "Sin nombre";
        [SerializeField] private CelestialBodyType _type    = CelestialBodyType.Planet;
        [TextArea(2, 4)]
        [SerializeField] private string _description = "";

        [Header("Colisión y zona")]
        [Tooltip("Radio de colisión mortal (chocar = muerte instantánea para planetas/estrellas).")]
        [SerializeField] private float _collisionRadius = 10f;
        [Tooltip("Radio de zona de influencia para spawn de contenido procedural.")]
        [SerializeField] private float _influenceRadius = 60f;

        public string            BodyName        => _bodyName;
        public CelestialBodyType Type            => _type;
        public string            Description     => _description;
        public float             CollisionRadius => _collisionRadius;
        public float             InfluenceRadius => _influenceRadius;

        // Eventos que otros sistemas escuchan (SolarCycleManager, WorldGenerator, etc.)
        public event System.Action<CelestialBody> OnPlayerEnterInfluence;
        public event System.Action<CelestialBody> OnPlayerExitInfluence;

        private Transform _player;
        private bool      _playerInside;

        // ------------------------------------------------------------------ lifecycle

        private void Start()
        {
            // Se reconectará cuando Spaceship se reconstruya
            // Por ahora busca cualquier GameObject con tag "Player"
            var player = GameObject.FindWithTag("Player");
            if (player != null) _player = player.transform;
        }

        private void Update()
        {
            if (_player == null) return;

            float dist    = Vector2.Distance(transform.position, _player.position);
            bool  inside  = dist <= _influenceRadius;

            if (inside && !_playerInside)
            {
                _playerInside = true;
                OnPlayerEnterInfluence?.Invoke(this);
            }
            else if (!inside && _playerInside)
            {
                _playerInside = false;
                OnPlayerExitInfluence?.Invoke(this);
            }
        }

        // ------------------------------------------------------------------ editor

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // radio de colisión — rojo
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, _collisionRadius);

            // zona de influencia — amarillo
            Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, _influenceRadius);
        }
#endif
    }
}
