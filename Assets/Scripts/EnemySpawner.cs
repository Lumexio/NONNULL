using UnityEngine;
using UnityEngine.UI;

public class EnemySpawner : MonoBehaviour {
    public static EnemySpawner Instance { get; private set; }

    public GameObject bossPrefab;
    public Transform player;

    public int currentRound = 1;
    public int killsThisRound = 0;
    public int targetKills = 10;

    private float spawnTimer = 0f;
    private const float SPAWN_INTERVAL = 1.0f;
    private const int MAX_ACTIVE_SPAWNS = 15;

    public Text roundText;
    public Image roundProgressBar;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
    }

    void Start() {
        if (player == null) {
            if (PlayerController.Instance != null) player = PlayerController.Instance.transform;
            else {
                GameObject p = GameObject.FindWithTag("Player");
                if (p != null) player = p.transform;
            }
        }
        UpdateUI();
    }

    void Update() {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        if (player == null) {
            if (PlayerController.Instance != null) player = PlayerController.Instance.transform;
            else {
                GameObject p = GameObject.FindWithTag("Player");
                if (p != null) player = p.transform;
            }
            if (player == null) return;
        }

        int activeCount = FlockingManager.Instance != null ? FlockingManager.Instance.enemies.Count : 0;
        if (activeCount < MAX_ACTIVE_SPAWNS) {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f) {
                SpawnEnemy();
                spawnTimer = SPAWN_INTERVAL;
            }
        }
    }

    void SpawnEnemy() {
        if (player == null) return;

        Vector2 randCircle = Random.insideUnitCircle.normalized * 25f;
        Vector3 spawnPos = player.position + new Vector3(randCircle.x, 0f, randCircle.y);

        // Clamp to level boundary
        spawnPos.x = Mathf.Clamp(spawnPos.x, -95f, 95f);
        spawnPos.z = Mathf.Clamp(spawnPos.z, -95f, 95f);
        spawnPos.y = 1f;

        bool isTough = Random.value < 0.2f;
        string tag = isTough ? "tough" : "weak";

        if (PoolManager.Instance != null) {
            PoolManager.Instance.Spawn(tag, spawnPos, Quaternion.identity);
        }
    }

    public void RegisterKill() {
        killsThisRound++;
        UpdateUI();

        if (killsThisRound >= targetKills) {
            currentRound++;
            killsThisRound = 0;
            targetKills = 10 + (currentRound - 1) * 5;
            UpdateUI();

            if (XPUI.Instance != null) {
                XPUI.Instance.AnnounceRound(currentRound);
            }

            // Boss spawn every 5 rounds (Round 5, 10, 15...)
            if (currentRound % 5 == 0) {
                SpawnBoss();
            }
        }
    }

    private void SpawnBoss() {
        Vector3 bossPos = (player != null ? player.position : Vector3.zero) + new Vector3(0f, 0f, 15f);
        bossPos.y = 1f;

        if (bossPrefab != null) {
            Instantiate(bossPrefab, bossPos, Quaternion.identity);
        }
    }

    public void ResetSpawner() {
        currentRound = 1;
        killsThisRound = 0;
        targetKills = 10;
        spawnTimer = 0f;
        UpdateUI();
    }

    public void UpdateUI() {
        if (roundText != null) {
            roundText.text = "Round " + currentRound;
        }
        if (roundProgressBar != null && targetKills > 0) {
            roundProgressBar.fillAmount = (float)killsThisRound / targetKills;
        }
    }
}
