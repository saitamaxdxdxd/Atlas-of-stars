using UnityEngine;

namespace AtlasOfStars.Gameplay
{
    /// <summary>
    /// Instancia el sistema solar completo al cargar la partida.
    ///
    /// Distancias desde el centro de masa (CM):
    ///   Estrella A/B   → 20–30u   (sistema binario)
    ///   Helio          →  120u    (tutorial, cercano)
    ///   [belt menor]   →  200u    (zona para cinturón de asteroides futuro)
    ///   Ferrón         →  300u    (minería)
    ///   [belt mayor]   →  500–700u (zona para cinturón principal futuro)
    ///   Nexus          →  900u    (comercio)
    ///   Umber          → 1600u    (misterio)
    ///   Ceniza         → 2800u    (muerto, casi inaccesible)
    ///   [Planeta6/7]   → 4500u+  (slots opcionales)
    ///
    /// Satélites: cada planeta tiene un PlanetSetup con array de MoonSetup.
    /// Puedes asignar 0–10 lunas por planeta desde el Inspector.
    /// </summary>
    public class WorldGenerator : MonoBehaviour
    {
        // ------------------------------------------------------------------ structs

        [System.Serializable]
        public struct MoonSetup
        {
            public GameObject prefab;
            [Tooltip("Distancia al planeta padre.")]
            public float radius;
            [Tooltip("Segundos por vuelta. Negativo = horario.")]
            public float period;
            [Tooltip("Ángulo inicial en grados.")]
            public float startAngle;
        }

        [System.Serializable]
        public struct PlanetSetup
        {
            public GameObject prefab;
            public MoonSetup[] moons;
        }

        // ------------------------------------------------------------------ inspector

        [Header("Estrellas")]
        [SerializeField] private GameObject _starAPrefab;
        [SerializeField] private GameObject _starBPrefab;

        [Header("Planetas — en orden orbital")]
        [SerializeField] private PlanetSetup _helio;
        [SerializeField] private PlanetSetup _ferron;
        [SerializeField] private PlanetSetup _nexus;
        [SerializeField] private PlanetSetup _umber;
        [SerializeField] private PlanetSetup _ceniza;

        [Header("Planetas extra opcionales (dejar vacío si no se usan)")]
        [SerializeField] private PlanetSetup _planet6;
        [SerializeField] private PlanetSetup _planet7;

        [Header("Nave")]
        [SerializeField] private GameObject _shipPrefab;

        [Header("Escala global del sistema")]
        [Tooltip("Multiplica todos los radios orbitales. 1 = valores base.")]
        [SerializeField] private float _systemScale = 1f;

        // ------------------------------------------------------------------ lifecycle

        private void Start() => BuildSystem();

        // ------------------------------------------------------------------ build

        private void BuildSystem()
        {
            var cm = new GameObject("CenterOfMass");
            cm.transform.SetParent(transform);
            cm.transform.position = Vector3.zero;
            var cmT = cm.transform;

            // Sistema binario — las dos estrellas orbitan el CM entre sí
            SpawnStar(_starAPrefab, "Estrella A", cmT, radius: 20f, period:  120f, startAngle:   0f);
            SpawnStar(_starBPrefab, "Estrella B", cmT, radius: 30f, period:  120f, startAngle: 180f);

            // Planetas
            // Gaps crecen ~2–2.5× entre cada par (como un sistema solar real).
            // Zonas comentadas marcan dónde irán cinturones de asteroides.
            SpawnPlanet(_helio,  "Helio",   cmT, radius:  120f, period:   280f, startAngle:  45f);
            // ~200u — belt menor (futuro)
            SpawnPlanet(_ferron, "Ferrón",  cmT, radius:  300f, period:   600f, startAngle: 120f);
            // ~500–700u — belt principal (futuro)
            SpawnPlanet(_nexus,  "Nexus",   cmT, radius:  900f, period:  1500f, startAngle: 200f);
            SpawnPlanet(_umber,  "Umber",   cmT, radius: 1600f, period:  3000f, startAngle: 310f);
            SpawnPlanet(_ceniza, "Ceniza",  cmT, radius: 2800f, period:  6500f, startAngle: 260f);

            if (_planet6.prefab != null)
                SpawnPlanet(_planet6, "Coloso", cmT, radius: 4500f, period: 12000f, startAngle:  80f);
            if (_planet7.prefab != null)
                SpawnPlanet(_planet7, "Limbo",  cmT, radius: 7000f, period: 22000f, startAngle: 155f);

            SpawnShip();
        }

        // ------------------------------------------------------------------ helpers

        private void SpawnStar(GameObject prefab, string bodyName,
                               Transform center, float radius, float period, float startAngle)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"WorldGenerator: prefab '{bodyName}' no asignado.");
                return;
            }
            var go      = Instantiate(prefab, transform);
            go.name     = bodyName;
            var orbital = go.GetComponent<OrbitalBody>() ?? go.AddComponent<OrbitalBody>();
            orbital.Configure(center, radius * _systemScale, period, startAngle);
        }

        private void SpawnPlanet(PlanetSetup setup, string bodyName,
                                 Transform center, float radius, float period, float startAngle)
        {
            if (setup.prefab == null)
            {
                Debug.LogWarning($"WorldGenerator: prefab '{bodyName}' no asignado.");
                return;
            }
            var go      = Instantiate(setup.prefab, transform);
            go.name     = bodyName;
            var orbital = go.GetComponent<OrbitalBody>() ?? go.AddComponent<OrbitalBody>();
            orbital.Configure(center, radius * _systemScale, period, startAngle);

            // Satélites
            if (setup.moons == null) return;
            foreach (var moon in setup.moons)
            {
                if (moon.prefab == null) continue;
                var moonGO      = Instantiate(moon.prefab, transform);
                moonGO.name     = $"Luna de {bodyName}";
                var moonOrbital = moonGO.GetComponent<OrbitalBody>() ?? moonGO.AddComponent<OrbitalBody>();
                moonOrbital.Configure(go.transform, moon.radius * _systemScale, moon.period, moon.startAngle);
            }
        }

        private void SpawnShip()
        {
            if (_shipPrefab == null) return;
            var ship = Instantiate(_shipPrefab, new Vector3(150f, 0f, 0f), Quaternion.identity);
            ship.name = "Spaceship";
        }
    }
}
