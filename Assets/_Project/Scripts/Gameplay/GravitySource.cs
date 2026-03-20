using System.Collections.Generic;
using UnityEngine;

namespace AtlasOfStars.Gameplay
{
    /// <summary>
    /// Marca un objeto como fuente de gravedad que curva la SpaceFabric.
    /// Se auto-registra — SpaceFabric itera GravitySource.All.
    /// </summary>
    public class GravitySource : MonoBehaviour
    {
        public static readonly List<GravitySource> All = new();

        [SerializeField] private float _mass = 5f;
        [Tooltip("Override del falloff para esta fuente. 0 = usar el falloff global del SpaceFabricManager.\n" +
                 "Objetos pequeños (asteroides): 0 (global).\n" +
                 "Planetas medianos: 0.001–0.002.\n" +
                 "Planetas gigantes: 0.0005–0.001.\n" +
                 "Estrellas: 0.0002–0.0005.")]
        [SerializeField] private float _falloffOverride = 0f;

        public float Mass           => _mass;
        public float FalloffOverride => _falloffOverride;

        private void OnEnable()  => All.Add(this);
        private void OnDisable() => All.Remove(this);

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, Mathf.Sqrt(_mass));
        }
#endif
    }
}
