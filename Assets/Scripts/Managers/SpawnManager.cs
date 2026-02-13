using UnityEngine;
using System.Collections;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform spawnParent;
    [SerializeField] private float initialSpawnRate = 1.3f;
    [SerializeField] private float minSpawnRate = 0.3f;
    [SerializeField] private float spawnXRange = 8f;

    private bool isSpawning = false;
    private float currentSpawnRate;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartSpawning()
    {
        if (isSpawning) return;

        isSpawning = true;
        currentSpawnRate = initialSpawnRate;
        StartCoroutine(SpawnRoutine());
        StartCoroutine(SpecialWaveRoutine());
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            SpawnFireball();

            if (DifficultyManager.Instance != null)
            {
                currentSpawnRate = Mathf.Max(
                    minSpawnRate,
                    initialSpawnRate * DifficultyManager.Instance.GetSpawnRateMultiplier()
                );
            }

            yield return new WaitForSeconds(currentSpawnRate);
        }
    }

    void SpawnFireball()
    {
        float spawnX = Random.Range(-spawnXRange, spawnXRange);
        Vector3 spawnPos = new Vector3(spawnX, 7f, 0f);

        GameObject fireball = Instantiate(fireballPrefab, spawnPos, Quaternion.identity, spawnParent);
    }

    IEnumerator SpecialWaveRoutine()
    {
        while (isSpawning)
        {
            yield return new WaitForSeconds(Random.Range(15f, 30f));

            if (!isSpawning) break;

            yield return StartCoroutine(SpawnWave());
        }
    }

    IEnumerator SpawnWave()
    {
        float waveDuration = 8f;
        float waveTimer = 0f;
        float waveSpawnRate = 0.6f;

        while (waveTimer < waveDuration)
        {
            SpawnFireball();

            waveTimer += waveSpawnRate;
            yield return new WaitForSeconds(waveSpawnRate);
        }
    }
}