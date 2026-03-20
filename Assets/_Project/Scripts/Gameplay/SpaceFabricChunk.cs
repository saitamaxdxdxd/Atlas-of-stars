using System.Collections.Generic;
using UnityEngine;

namespace AtlasOfStars.Gameplay
{
    /// <summary>
    /// Un chunk de la tela del espacio-tiempo.
    /// Contiene dos meshes de quad strips: hilos horizontales y verticales entrelazados.
    /// No es MonoBehaviour — SpaceFabricManager lo instancia y gestiona.
    /// </summary>
    public class SpaceFabricChunk
    {
        public Vector2 WorldOrigin { get; private set; }

        private readonly Mesh      _hMesh;           // hilos horizontales
        private readonly Mesh      _vMesh;           // hilos verticales
        private readonly Vector3[] _hVerts;
        private readonly Vector3[] _vVerts;

        private readonly float _chunkSize;
        private readonly int   _threadCount;
        private readonly int   _segmentCount;
        private readonly float _halfWidth;
        private readonly float _interlaceOffset;
        private readonly float _falloff;
        private readonly float _maxDepth;

        // ------------------------------------------------------------------ constructor

        public SpaceFabricChunk(
            Transform parent, Material material,
            float chunkSize, int threadCount, int segmentCount,
            float halfWidth, float interlaceOffset,
            float falloff, float maxDepth)
        {
            _chunkSize       = chunkSize;
            _threadCount     = threadCount;
            _segmentCount    = segmentCount;
            _halfWidth       = halfWidth;
            _interlaceOffset = interlaceOffset;
            _falloff         = falloff;
            _maxDepth        = maxDepth;

            _hMesh = BuildStaticMesh("FabricH", threadCount, segmentCount);
            _vMesh = BuildStaticMesh("FabricV", threadCount, segmentCount);

            CreateRendererGO("H", parent, _hMesh, material);
            CreateRendererGO("V", parent, _vMesh, material);

            int vertsPerThread = (_segmentCount + 1) * 2;
            _hVerts = new Vector3[_threadCount * vertsPerThread];
            _vVerts = new Vector3[_threadCount * vertsPerThread];
        }

        // ------------------------------------------------------------------ public API

        public void SetPosition(Vector2 worldOrigin)
        {
            WorldOrigin = worldOrigin;
        }

        /// <summary>
        /// Actualiza la deformación gravitacional de todos los vértices del chunk.
        /// Llamar desde LateUpdate del manager.
        /// </summary>
        public void Deform(List<GravitySource> sources)
        {
            float threadStep = _chunkSize / _threadCount;   // espaciado entre hilos
            float segStep    = _chunkSize / _segmentCount;  // espaciado entre segmentos
            int   vpt        = (_segmentCount + 1) * 2;     // vértices por hilo

            // Hilos horizontales — corren en X, grosor en Y
            for (int t = 0; t < _threadCount; t++)
            {
                float wy    = WorldOrigin.y + t * threadStep;
                int   vBase = t * vpt;

                for (int s = 0; s <= _segmentCount; s++)
                {
                    float wx    = WorldOrigin.x + s * segStep;
                    float depth = CalcDepth(wx, wy, sources);
                    int   vi    = vBase + s * 2;

                    _hVerts[vi]     = new Vector3(wx, wy - _halfWidth, depth);
                    _hVerts[vi + 1] = new Vector3(wx, wy + _halfWidth, depth);
                }
            }

            // Hilos verticales — corren en Y, grosor en X
            // Hilos pares van en Z+ respecto a los horizontales, impares en Z-
            // Esto crea el efecto de entrelazado al cruzarse con los horizontales
            for (int t = 0; t < _threadCount; t++)
            {
                float wx        = WorldOrigin.x + t * threadStep;
                float interlace = (t % 2 == 0) ? _interlaceOffset : -_interlaceOffset;
                int   vBase     = t * vpt;

                for (int s = 0; s <= _segmentCount; s++)
                {
                    float wy    = WorldOrigin.y + s * segStep;
                    float depth = CalcDepth(wx, wy, sources) + interlace;
                    int   vi    = vBase + s * 2;

                    _vVerts[vi]     = new Vector3(wx - _halfWidth, wy, depth);
                    _vVerts[vi + 1] = new Vector3(wx + _halfWidth, wy, depth);
                }
            }

            _hMesh.vertices = _hVerts;
            _vMesh.vertices = _vVerts;
            _hMesh.RecalculateBounds();
            _vMesh.RecalculateBounds();
        }

        // ------------------------------------------------------------------ private

        private float CalcDepth(float wx, float wy, List<GravitySource> sources)
        {
            float depth = 0f;
            foreach (var src in sources)
            {
                if (src == null) continue;
                float dx      = wx - src.transform.position.x;
                float dy      = wy - src.transform.position.y;
                float falloff = src.FalloffOverride > 0f ? src.FalloffOverride : _falloff;
                depth += src.Mass / (1f + (dx * dx + dy * dy) * falloff);
            }
            return Mathf.Min(depth, _maxDepth);
        }

        /// <summary>
        /// Construye la topología estática del mesh (triángulos).
        /// Los vértices se actualizan cada frame en Deform(), los índices nunca cambian.
        /// Layout por hilo: [bottom_s0, top_s0, bottom_s1, top_s1, ..., bottom_sN, top_sN]
        /// </summary>
        private static Mesh BuildStaticMesh(string name, int threadCount, int segmentCount)
        {
            var mesh = new Mesh { name = name };
            mesh.MarkDynamic();

            int vpt     = (segmentCount + 1) * 2;
            var indices = new int[threadCount * segmentCount * 6];
            int t       = 0;

            for (int thread = 0; thread < threadCount; thread++)
            {
                int vBase = thread * vpt;
                for (int s = 0; s < segmentCount; s++)
                {
                    int v0 = vBase + s * 2;
                    int v1 = v0 + 1;
                    int v2 = v0 + 2;
                    int v3 = v0 + 3;

                    // dos triángulos por quad (winding CCW visto desde +Z)
                    indices[t++] = v0; indices[t++] = v1; indices[t++] = v2;
                    indices[t++] = v2; indices[t++] = v1; indices[t++] = v3;
                }
            }

            // vértices placeholder para que Unity no se queje antes del primer Deform()
            mesh.vertices  = new Vector3[threadCount * vpt];
            mesh.triangles = indices;
            return mesh;
        }

        private static void CreateRendererGO(string name, Transform parent, Mesh mesh, Material material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().mesh = mesh;

            var mr = go.AddComponent<MeshRenderer>();
            mr.material          = material;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows    = false;
        }
    }
}
