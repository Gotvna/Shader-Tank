using UnityEngine;
using System.Collections;
using Tanks.Complete; // ← important si ton PowerUp est dans ce namespace

public class PowerUpSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] powerUpPrefabs;  // Liste de prefabs de power-ups
    [SerializeField] private Transform[] spawnPoints;      // Points de spawn possibles

    private GameObject currentPowerUp;

    private void Start()
    {
        SpawnRandomPowerUp();
    }

    private void SpawnRandomPowerUp()
    {
        if (powerUpPrefabs.Length == 0 || spawnPoints.Length == 0) return;

        int powerUpIndex = Random.Range(0, powerUpPrefabs.Length);
        int spawnIndex = Random.Range(0, spawnPoints.Length);

        currentPowerUp = Instantiate(powerUpPrefabs[powerUpIndex], spawnPoints[spawnIndex].position, Quaternion.identity);

        PowerUp powerUpScript = currentPowerUp.GetComponent<PowerUp>();
        if (powerUpScript != null)
        {
            powerUpScript.SetSpawner(this);
        }
    }

    public void OnPowerUpCollected()
    {
        if (currentPowerUp != null)
        {
            Destroy(currentPowerUp);
            currentPowerUp = null;
            StartCoroutine(RespawnAfterDelay(20f));
        }
    }

    private IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnRandomPowerUp();
    }
}
