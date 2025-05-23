using System.Collections;
using UnityEngine;

namespace Tanks.Complete
{
    public class PowerUpDetector : MonoBehaviour
    {
        public bool m_HasActivePowerUp = false;

        private TankShooting m_TankShooting;
        private TankMovement m_TankMovement;
        private TankHealth m_TankHealth;
        private PowerUpHUD m_PowerUpHUD;

        [SerializeField] private GameObject m_ShieldVisualEffect;

        private void Awake()
        {
            m_TankShooting = GetComponent<TankShooting>();
            m_TankMovement = GetComponent<TankMovement>();
            m_TankHealth = GetComponent<TankHealth>();
            m_PowerUpHUD = GetComponentInChildren<PowerUpHUD>();

            if (m_ShieldVisualEffect != null)
                m_ShieldVisualEffect.SetActive(false);

        }

        public void PowerUpSpeed(float speedBoost, float turnSpeedBoost, float duration)
        {
            StartCoroutine(IncreaseSpeed(speedBoost, turnSpeedBoost, duration));
        }

        private IEnumerator IncreaseSpeed(float speedBoost, float TurnSpeedBoost, float duration)
        {
            m_HasActivePowerUp = true;
            m_PowerUpHUD.SetActivePowerUp(PowerUp.PowerUpType.Speed, duration);
            m_TankMovement.m_Speed += speedBoost;
            m_TankMovement.m_TurnSpeed += TurnSpeedBoost;
            yield return new WaitForSeconds(duration);
            m_TankMovement.m_Speed -= speedBoost;
            m_TankMovement.m_TurnSpeed -= TurnSpeedBoost;
            m_HasActivePowerUp = false;
        }

        public void PowerUpShoootingRate(float cooldownReduction, float duration)
        {
            StartCoroutine(IncreaseShootingRate(cooldownReduction, duration));
        }

        private IEnumerator IncreaseShootingRate(float cooldownReduction, float duration)
        {
            if (cooldownReduction > 0)
            {
                m_HasActivePowerUp = true;
                m_PowerUpHUD.SetActivePowerUp(PowerUp.PowerUpType.ShootingBonus, duration);
                m_TankShooting.m_ShotCooldown *= cooldownReduction;
                yield return new WaitForSeconds(duration);
                m_TankShooting.m_ShotCooldown /= cooldownReduction;
                m_HasActivePowerUp = false;
            }
        }

        public void PickUpShield(float shieldAmount, float duration)
        {
            if (!m_TankHealth.m_HasShield)
            {
                StartCoroutine(ActivateShield(shieldAmount, duration));
            }
        }

        private IEnumerator ActivateShield(float shieldAmount, float duration)
        {
            m_HasActivePowerUp = true;
            m_PowerUpHUD.SetActivePowerUp(PowerUp.PowerUpType.DamageReduction, duration);
            m_TankHealth.ToggleShield(shieldAmount);

            if (m_ShieldVisualEffect != null)
            {
                m_ShieldVisualEffect.SetActive(true);
                var ps = m_ShieldVisualEffect.GetComponent<ParticleSystem>();
                if (ps != null) ps.Play();
            }

            yield return new WaitForSeconds(duration);

            m_TankHealth.ToggleShield(shieldAmount);
            m_HasActivePowerUp = false;

            if (m_ShieldVisualEffect != null)
            {
                var ps = m_ShieldVisualEffect.GetComponent<ParticleSystem>();
                if (ps != null) ps.Stop();
                m_ShieldVisualEffect.SetActive(false);
            }
        }

        public void PowerUpHealing(float healAmount)
        {
            m_TankHealth.IncreaseHealth(healAmount);
            m_PowerUpHUD.SetActivePowerUp(PowerUp.PowerUpType.Healing, 1.0f);
        }

        public void PowerUpInvincibility(float duration)
        {
            StartCoroutine(ActivateInvincibility(duration));
        }

        private IEnumerator ActivateInvincibility(float duration)
        {
            m_HasActivePowerUp = true;
            m_PowerUpHUD.SetActivePowerUp(PowerUp.PowerUpType.Invincibility, duration);
            m_TankHealth.ToggleInvincibility();
            yield return new WaitForSeconds(duration);
            m_HasActivePowerUp = false;
            m_TankHealth.ToggleInvincibility();
        }

        public void PowerUpSpecialShell(float damageMultiplier)
        {
            m_HasActivePowerUp = true;
            m_PowerUpHUD.SetActivePowerUp(PowerUp.PowerUpType.DamageMultiplier, 0f);
            m_TankShooting.EquipSpecialShell(damageMultiplier);
        }
    }
}