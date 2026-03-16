using UnityEngine;

namespace AtlasOfStars.Gameplay
{
    /// <summary>
    /// Genera un grid de malla que simula la tela del espacio-tiempo.
    /// Los vértices se deforman en Z hacia los GravitySource presentes en escena.
    ///
    /// Setup:
    ///   1. Crear un GameObject vacío en GameScene, agregar este componente.
    ///   2. Asignar un Material con un shader Unlit + textura de grid (o color sólido).
    ///   3. La cámara debe ser Perspective, con rotación X ~20-35° para ver la profundidad.
    ///   4. Colocar este GameObject detrás de todo (Z positivo si cámara mira -Z).
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class SpaceFabric : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private int   _resolution = 50;    // vértices por lado
        [SerializeField] private float _size       = 30f;   // tamaño en unidades de mundo

        [Header("Deformación")]
        [SerializeField] private float _maxDepth = 5f;      // máximo desplazamiento en Z
        [SerializeField] private float _falloff  = 1.2f;    // cuánto de rápido decrece la influencia

        private Mesh      _mesh;
        private Vector3[] _baseVertices;   // posiciones originales (sin deformar)
        private Vector3[] _vertices;       // posiciones deformadas (escritas al mesh)

        // ------------------------------------------------------------------ lifecycle

        private void Awake()
        {
            BuildMesh();
        }

        private void LateUpdate()
        {
            DeformMesh();
        }

        // ------------------------------------------------------------------ mesh

        private void BuildMesh()
        {
            _mesh = new Mesh { name = "SpaceFabric" };
            _mesh.MarkDynamic(); // optimiza escrituras frecuentes
            GetComponent<MeshFilter>().mesh = _mesh;

            int side      = _resolution + 1;
            int vertCount = side * side;

            _baseVertices = new Vector3[vertCount];
            _vertices     = new Vector3[vertCount];
            var uvs       = new Vector2[vertCount];
            var tris      = new int[_resolution * _resolution * 6];

            float step = _size / _resolution;
            float half = _size * 0.5f;

            // vértices
            for (int y = 0; y <= _resolution; y++)
            {
                for (int x = 0; x <= _resolution; x++)
                {
                    int i = y * side + x;
                    _baseVertices[i] = new Vector3(x * step - half, y * step - half, 0f);
                    uvs[i]           = new Vector2((float)x / _resolution, (float)y / _resolution);
                }
            }

            // triángulos
            int t = 0;
            for (int y = 0; y < _resolution; y++)
            {
                for (int x = 0; x < _resolution; x++)
                {
                    int bl = y * side + x;
                    tris[t++] = bl;          tris[t++] = bl + side;     tris[t++] = bl + 1;
                    tris[t++] = bl + 1;      tris[t++] = bl + side;     tris[t++] = bl + side + 1;
                }
            }

            _mesh.vertices  = _baseVertices;
            _mesh.uv        = uvs;
            _mesh.triangles = tris;
            _mesh.RecalculateNormals();

            System.Array.Copy(_baseVertices, _vertices, vertCount);
        }

        // ------------------------------------------------------------------ deformation

        private void DeformMesh()
        {
            var sources = GravitySource.All;

            for (int i = 0; i < _baseVertices.Length; i++)
            {
                _vertices[i] = _baseVertices[i];

                if (sources.Count == 0) continue;

                Vector3 worldPos = transform.TransformPoint(_baseVertices[i]);
                float   depth    = 0f;

                foreach (var src in sources)
                {
                    if (src == null) continue;

                    // distancia en XY (ignoramos Z del astro para calcular influencia)
                    float dx   = worldPos.x - src.transform.position.x;
                    float dy   = worldPos.y - src.transform.position.y;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    // pozo gravitacional: influencia = masa / (1 + dist² * falloff)
                    depth += src.Mass / (1f + dist * dist * _falloff);
                }

                _vertices[i].z = Mathf.Min(depth, _maxDepth);
            }

            _mesh.vertices = _vertices;
            _mesh.RecalculateNormals();
        }
    }
}
