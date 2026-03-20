using UnityEngine;

namespace AtlasOfStars.Gameplay
{
    /// <summary>
    /// Gestiona un grid 3×3 de SpaceFabricChunks siempre centrado en el jugador.
    /// Recicla chunks automáticamente cuando el jugador cambia de celda.
    ///
    /// Anti-mareo a alta velocidad:
    ///   - Parallax: el grid se mueve a _parallaxFactor × velocidad de la nave (no 1:1)
    ///     Esto reduce la velocidad aparente de los hilos en pantalla.
    ///   - Fade: los hilos se atenúan cuando la nave supera _fadeStartSpeed.
    ///     A _fadeEndSpeed el grid es casi invisible. Vuelve al frenar.
    ///
    /// Setup en escena:
    ///   1. Crear un GameObject vacío, agregar este componente.
    ///   2. Asignar _fabricMaterial: Material con shader Unlit, Cull Off, color blanco.
    ///      El material DEBE tener Surface Type = Transparent (para el fade de alpha).
    ///      Para glow: usar HDR color + Bloom en post-processing.
    ///   3. _player se auto-asigna desde Spaceship si se deja vacío.
    ///   4. La cámara debe tener rotación X entre 20–35° para ver la profundidad de los pozos.
    ///   5. Eliminar el GameObject con el antiguo SpaceFabric.cs de la escena.
    /// </summary>
    public class SpaceFabricManager : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private Transform _player;
        [SerializeField] private Material  _fabricMaterial;

        [Header("Grid")]
        [Tooltip("Tamaño de cada chunk en unidades de mundo. 3×3 chunks = 3× esta cobertura por eje.")]
        [SerializeField] private float _chunkSize    = 60f;
        [Tooltip("Hilos por dirección por chunk. 15–20 = aireado, 30–40 = denso.")]
        [SerializeField] private int   _threadCount  = 20;
        [Tooltip("Segmentos por hilo. Más = curva más suave en los pozos gravitacionales.")]
        [SerializeField] private int   _segmentCount = 40;

        [Header("Hilos")]
        [Tooltip("Medio grosor del hilo en unidades de mundo. 0.04–0.08 = rango recomendado.")]
        [SerializeField] private float _threadHalfWidth = 0.06f;
        [Tooltip("Separación Z entre hilos H y V para el efecto de entrelazado.")]
        [SerializeField] private float _interlaceOffset = 0.05f;

        [Header("Deformación gravitacional")]
        [Tooltip("Velocidad de decaimiento. 0.004–0.005 = pozos amplios y dramáticos.")]
        [SerializeField] private float _falloff  = 0.004f;
        [Tooltip("Profundidad máxima en unidades de mundo. Debe ser >= Mass del objeto más grande.")]
        [SerializeField] private float _maxDepth = 30f;

        [Header("Anti-mareo — Parallax")]
        [Tooltip("Fracción de la velocidad de la nave a la que se mueve el grid. " +
                 "1 = sigue exacto (máximo estrobo). 0.5 = mueve a la mitad (menos estrobo). " +
                 "Recomendado: 0.6–0.75.")]
        [SerializeField, Range(0.1f, 1f)] private float _parallaxFactor = 0.7f;

        [Header("Anti-mareo — Fade")]
        [Tooltip("Brillo de los hilos. 1 = blanco puro, 0.3 = tenue.")]
        [SerializeField, Range(0f, 1f)] private float _fadeMaxBrightness = 0.4f;

        private SpaceFabricChunk[] _chunks;
        private Vector2Int         _currentChunkCoord;
        private Vector2            _fabricCenter;       // centro real del grid (con parallax)
        private float              _currentBrightness = 1f;

        // nombre del property en el shader (URP Unlit = "_BaseColor", Built-in = "_Color")
        private static readonly int ShaderColor = Shader.PropertyToID("_BaseColor");

        // ------------------------------------------------------------------ lifecycle

        private void Start()
        {
            if (_player == null)
            {
                Debug.LogWarning("SpaceFabricManager: _player no asignado.");
            }

            _fabricCenter      = new Vector2(_player.position.x, _player.position.y);
            _currentChunkCoord = ToChunkCoord(_fabricCenter);

            _chunks = new SpaceFabricChunk[9];
            for (int i = 0; i < 9; i++)
            {
                var go = new GameObject($"FabricChunk_{i}");
                go.transform.SetParent(transform, false);

                _chunks[i] = new SpaceFabricChunk(
                    go.transform, _fabricMaterial,
                    _chunkSize, _threadCount, _segmentCount,
                    _threadHalfWidth, _interlaceOffset,
                    _falloff, _maxDepth
                );
            }

            LayoutGrid(_currentChunkCoord);

            // Deformación inicial: evita que la tela aparezca "rota" en el primer frame
            // antes de que LateUpdate la actualice.
            var sources = GravitySource.All;
            foreach (var chunk in _chunks)
                chunk.Deform(sources);
        }

        private void LateUpdate()
        {
            if (_player == null) return;

            UpdateParallax();
            UpdateFade();

            Vector2Int coord = ToChunkCoord(_fabricCenter);
            if (coord != _currentChunkCoord)
            {
                _currentChunkCoord = coord;
                LayoutGrid(coord);
            }

            var sources = GravitySource.All;
            foreach (var chunk in _chunks)
                chunk.Deform(sources);
        }

        // ------------------------------------------------------------------ anti-mareo

        /// <summary>
        /// El grid sigue al player al _parallaxFactor de su velocidad.
        /// A 0.7: si la nave se mueve 10u, el grid se mueve 7u → los hilos cruzan
        /// la pantalla un 30% más lento → menos estrobo.
        /// </summary>
        private void UpdateParallax()
        {
            Vector2 playerPos  = new Vector2(_player.position.x, _player.position.y);
            Vector2 delta      = playerPos - _fabricCenter;
            _fabricCenter     += delta * _parallaxFactor;
        }

        /// <summary>
        /// Atenúa los hilos cuando la nave va rápido.
        /// Usa el alpha del color del material compartido.
        /// </summary>
        private void UpdateFade()
        {
            // Speed se reconectará cuando Spaceship se reconstruya
            float targetBrightness = _fadeMaxBrightness;

            _currentBrightness = targetBrightness;

            float b = _currentBrightness;
            _fabricMaterial.SetColor(ShaderColor, new Color(b, b, b, 1f));
        }

        // ------------------------------------------------------------------ grid

        private void LayoutGrid(Vector2Int center)
        {
            int idx = 0;
            for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                var coord  = center + new Vector2Int(dx, dy);
                var origin = new Vector2(coord.x * _chunkSize, coord.y * _chunkSize);
                _chunks[idx++].SetPosition(origin);
            }
        }

        private Vector2Int ToChunkCoord(Vector2 pos) =>
            new Vector2Int(
                Mathf.FloorToInt(pos.x / _chunkSize),
                Mathf.FloorToInt(pos.y / _chunkSize)
            );

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_chunks == null) return;
            Gizmos.color = new Color(0.4f, 0.4f, 1f, 0.2f);
            foreach (var chunk in _chunks)
            {
                var origin = chunk.WorldOrigin;
                var center = new Vector3(origin.x + _chunkSize * 0.5f,
                                        origin.y + _chunkSize * 0.5f, 0f);
                Gizmos.DrawWireCube(center, new Vector3(_chunkSize, _chunkSize, 0.1f));
            }
        }
#endif
    }
}
