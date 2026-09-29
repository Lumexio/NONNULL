using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public int currentRound = 1;
    private int enemiesToSpawn;
    private int activeEnemiesCount;

    public GameObject bossPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        Transform player = GameObject.FindWithTag("Player").transform;

        while (true)
        {
            enemiesToSpawn = 10 + (currentRound * 5);
            activeEnemiesCount = 0;

            if (currentRound % 5 == 1 && currentRound > 1) // e.g., 6, 11, 16
            {
                if (UIManager.Instance != null) UIManager.Instance.ShowBossWarning();
                Instantiate(bossPrefab, GetSpawnPoint(player.position), Quaternion.identity);
                // Wait for boss to die
                yield return new WaitUntil(() => GameObject.FindObjectOfType<BossAI>() == null);
            }

            while (enemiesToSpawn > 0 || FlockManager.Instance.activeEnemies.Count > 0)
            {
                if (enemiesToSpawn > 0 && FlockManager.Instance.activeEnemies.Count < 3)
                {
                    SpawnEnemy(player.position);
                    enemiesToSpawn--;
                }
                yield return new WaitForSeconds(1f);
            }

            currentRound++;
        }
    }

    private Vector3 GetSpawnPoint(Vector3 playerPos)
    {
        Vector2 randCircle = Random.insideUnitCircle.normalized * Random.Range(15f, 30f);
        Vector3 spawnPos = playerPos + new Vector3(randCircle.x, 100f, randCircle.y);

        if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 150f))
        {
            return hit.point;
        }
        return spawnPos;
    }

    private void SpawnEnemy(Vector3 playerPos)
    {
        Vector3 spawnPos = GetSpawnPoint(playerPos);
        float roll = Random.value;
        string poolTag = (roll < (currentRound * 0.1f)) ? "tough" : "weak";
        PoolManager.Instance.Spawn(poolTag, spawnPos, Quaternion.identity);
    }

    public void GameOver()
    {
        StopAllCoroutines();
        Time.timeScale = 0; // Stop enemies & player
        if (UIManager.Instance != null) UIManager.Instance.ShowGameOver();
    }
}
