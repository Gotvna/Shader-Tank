using UnityEngine;

namespace Tanks.Complete
{
    public class ShellExplosion : MonoBehaviour
    {
        public GameObject m_ExplosionPrefab;
        public LayerMask m_TankMask;

        [HideInInspector] public float m_MaxLifeTime = 2f;
        [HideInInspector] public float m_MaxDamage = 100f;
        [HideInInspector] public float m_ExplosionForce = 1000f;
        [HideInInspector] public float m_ExplosionRadius = 5f;
        [HideInInspector] public float m_VisualEffectLifetime = 2f;

        private void Start()
        {
            Destroy(gameObject, m_MaxLifeTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_TankMask);

            foreach (Collider col in colliders)
            {
                Rigidbody targetRigidbody = col.GetComponent<Rigidbody>();
                if (!targetRigidbody) continue;

                targetRigidbody.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius);

                TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();
                if (!targetHealth) continue;

                float damage = CalculateDamage(targetRigidbody.position);
                targetHealth.TakeDamage(damage);
            }

            if (m_ExplosionPrefab != null)
            {
                GameObject explosion = Instantiate(m_ExplosionPrefab, transform.position, transform.rotation);
                explosion.transform.SetParent(null);

                Vector3 spawnPos = transform.position;
                Renderer[] renderers = explosion.GetComponentsInChildren<Renderer>();

                foreach (Renderer rend in renderers)
                {
                    foreach (Material mat in rend.materials)
                    {
                        if (mat.HasProperty("_SpawnPosition"))
                            mat.SetVector("_SpawnPosition", spawnPos);
                        if (mat.HasProperty("_FadeDistance"))
                            mat.SetFloat("_FadeDistance", 5f);
                        if (mat.HasProperty("_EmissionFade"))
                            mat.SetFloat("_EmissionFade", 1f);
                    }
                }

                Destroy(explosion, m_VisualEffectLifetime);
            }

            Destroy(gameObject);
        }

        private float CalculateDamage(Vector3 targetPosition)
        {
            Vector3 explosionToTarget = targetPosition - transform.position;
            float explosionDistance = explosionToTarget.magnitude;
            float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;
            float damage = relativeDistance * m_MaxDamage;
            return Mathf.Max(0f, damage);
        }
    }
}
