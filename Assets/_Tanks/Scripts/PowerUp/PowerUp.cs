using UnityEngine;

namespace Tanks.Complete
{
    public class PowerUp : MonoBehaviour
    {
        public enum PowerUpType { Speed, DamageReduction, ShootingBonus, Healing, Invincibility, DamageMultiplier }

        [Tooltip("Select the kind of Power Up that you want.")]
        [SerializeField] private PowerUpType m_PowerUpType = PowerUpType.DamageReduction;

        [Tooltip("Time in seconds that this Power Up will be active.")]
        [SerializeField] private float m_DurationTime = 5f;

        [Header("Damage Reduction")]
        [Tooltip("Percentage of damage reduction [0 , 1].")]
        [SerializeField] private float m_DamageReduction = 0.5f;

        [Header("Speed Bonus")]
        [Tooltip("Extra speed value of the tank.")]
        [SerializeField] private float m_SpeedBonus = 5f;
        [Tooltip("Extra turn speed value of the tank.")]
        [SerializeField] private float m_TurnSpeedBonus = 0f;

        [Header("Shooting Bonus")]
        [Tooltip("Percentage of reduction in the cooldown shooting time (0 , 1].")]
        [SerializeField] private float m_CooldownReduction = 0.5f;

        [Header("Healing")]
        [Tooltip("Life that will recover the tank.")]
        [SerializeField] private float m_HealingAmount = 20f;

        [Header("Extra Damage")]
        [Tooltip("Amount by which the damage will be multiplied.")]
        [SerializeField] private float m_DamageMultiplier = 2f;

        private PowerUpSpawner m_Spawner;

        public void SetSpawner(PowerUpSpawner spawner)
        {
            m_Spawner = spawner;
        }

        private void Update()
        {
            // Rotates the power up game object
            transform.rotation = Quaternion.Euler(0, 50f * Time.time, 0);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Tank"))
            {
                PowerUpDetector m_PowerUpDetector = other.gameObject.GetComponent<PowerUpDetector>();

                if (!m_PowerUpDetector.m_HasActivePowerUp)
                {
                    switch (m_PowerUpType)
                    {
                        case PowerUpType.DamageReduction:
                            m_PowerUpDetector.PickUpShield(m_DamageReduction, m_DurationTime);
                            break;
                        case PowerUpType.Speed:
                            m_PowerUpDetector.PowerUpSpeed(m_SpeedBonus, m_TurnSpeedBonus, m_DurationTime);
                            break;
                        case PowerUpType.ShootingBonus:
                            m_PowerUpDetector.PowerUpShoootingRate(m_CooldownReduction, m_DurationTime);
                            break;
                        case PowerUpType.Healing:
                            m_PowerUpDetector.PowerUpHealing(m_HealingAmount);
                            break;
                        case PowerUpType.Invincibility:
                            m_PowerUpDetector.PowerUpInvincibility(m_DurationTime);
                            break;
                        case PowerUpType.DamageMultiplier:
                            m_PowerUpDetector.PowerUpSpecialShell(m_DamageMultiplier);
                            break;
                    }

                    m_Spawner?.OnPowerUpCollected();
                    Destroy(gameObject);
                }
            }
        }
    }
}
