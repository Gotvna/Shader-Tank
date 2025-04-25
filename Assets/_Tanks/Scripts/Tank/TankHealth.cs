using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using UnityEngine.VFX;

namespace Tanks.Complete
{
    public class TankHealth : MonoBehaviour
    {
        public float m_StartingHealth = 100f;               // The amount of health each tank starts with.
        public Slider m_Slider;                             // The slider to represent how much health the tank currently has.
        public Image m_FillImage;                           // The image component of the slider.
        public Color m_FullHealthColor = Color.green;    // The color the health bar will be when on full health.
        public Color m_ZeroHealthColor = Color.red;      // The color the health bar will be when on no health.
        public GameObject m_ExplosionPrefab;                // A prefab that will be instantiated in Awake, then used whenever the tank dies.
        [HideInInspector] public bool m_HasShield;          // Has the tank picked up a shield power up?

        private VisualEffect m_HealingEffect;
        private AudioSource m_ExplosionAudio;               // The audio source to play when the tank explodes.
        private float m_CurrentHealth;                      // How much health the tank currently has.
        private bool m_Dead;                                // Has the tank been reduced beyond zero health yet?
        private float m_ShieldValue;                        // Percentage of reduced damage when the tank has a shield.
        private bool m_IsInvincible;                        // Is the tank invincible in this moment?


        /// <summary>
        /// Added for shader project - this is to have an affect when getting hit
        /// </summary>
        [SerializeField] private FullScreenPassRendererFeature _aberrationFeature;
        [SerializeField] private float _maxIntensity = 0.02f;
        [SerializeField] private float _duration = 0.5f;

        private Material _aberrationMaterial;

        private void Awake ()
        {
            // Set the slider max value to the max health the tank can have
            m_Slider.maxValue = m_StartingHealth;
            m_HealingEffect = GetComponent<VisualEffect>();
        }

        private void OnDestroy()
        {
        }

        private void OnEnable()
        {
            // When the tank is enabled, reset the tank's health and whether or not it's dead.
            m_CurrentHealth = m_StartingHealth;
            m_Dead = false;
            m_HasShield = false;
            m_ShieldValue = 0;
            m_IsInvincible = false;

            _aberrationMaterial = _aberrationFeature.passMaterial;

            // Update the health slider's value and color.
            SetHealthUI();
        }


        public void TakeDamage (float amount)
        {
            // Check if the tank is not invincible
            if (!m_IsInvincible)
            {
                // Reduce current health by the amount of damage done.
                m_CurrentHealth -= amount * (1 - m_ShieldValue);

                // Change the UI elements appropriately.
                SetHealthUI ();

                /// <summary>
                /// Added for shader project - this is to have an affect when getting hit
                /// </summary>
                StartCoroutine(RunChromaticAberration());

                // If the current health is at or below zero and it has not yet been registered, call OnDeath.
                if (m_CurrentHealth <= 0f && !m_Dead)
                {
                    StopCoroutine(RunChromaticAberration());
                    _aberrationMaterial.SetFloat("_Intensity", 0f);
                    if (GetComponent<TankShooting>().m_ActiveEffect != null)
                    {
                        Destroy(GetComponent<TankShooting>().m_ActiveEffect);
                    }
                    OnDeath ();
                }
            }
        }


        public void IncreaseHealth(float amount)
        {
            // Check if adding the amount would keep the health within the maximum limit
            if (m_CurrentHealth + amount <= m_StartingHealth)
            {
                Debug.Log("Healing amount: " + amount);
                // If the new health value is within the limit, add the amount
                m_CurrentHealth += amount;

                if (m_HealingEffect != null)
                {
                    
                    Debug.Log("Healing effect instantiated");
                    m_HealingEffect.SendEvent("onHeal");
                }
            }
            else
            {
                // If the new health exceeds the starting health, set it at the maximum

                m_CurrentHealth = m_StartingHealth;

                m_HealingEffect.SendEvent("onHeal");
            }

            // Change the UI elements appropriately.
            SetHealthUI();
        }


        public void ToggleShield (float shieldAmount)
        {
            // Inverts the value of has shield.
            m_HasShield = !m_HasShield;

            // Stablish the amount of damage that will be reduced by the shield
            if (m_HasShield)
            {
                m_ShieldValue = shieldAmount;
            }
            else
            {
                m_ShieldValue = 0;
            }
        }

        public void ToggleInvincibility()
        {
            m_IsInvincible = !m_IsInvincible;
        }


        private void SetHealthUI ()
        {
            // Set the slider's value appropriately.
            m_Slider.value = m_CurrentHealth;

            // Interpolate the color of the bar between the choosen colours based on the current percentage of the starting health.
            m_FillImage.color = Color.Lerp (m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
        }


        private void OnDeath ()
        {
            // Set the flag so that this function is only called once.
            m_Dead = true;

            GameObject deathSplosion = Instantiate(m_ExplosionPrefab);
            deathSplosion.transform.SetPositionAndRotation(transform.position, Quaternion.identity);  

            // Turn the tank off.
            gameObject.SetActive (false);
        }

        private IEnumerator RunChromaticAberration()
        {
            float elapsed = 0f;

            while (elapsed < _duration)
            {
                float currentIntensity = Mathf.Lerp(_maxIntensity, 0f, elapsed / _duration);
                _aberrationMaterial.SetFloat("_Intensity", currentIntensity);

                _aberrationMaterial.SetVector("_Direction", new Vector2(1, 0));

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Reset intensity to 0
            _aberrationMaterial.SetFloat("_Intensity", 0f);
        }
    }
}