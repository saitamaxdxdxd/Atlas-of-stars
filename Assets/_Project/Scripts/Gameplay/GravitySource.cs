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
        public float Mass => _mass;

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
