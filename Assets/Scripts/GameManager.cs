using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public int points = 0;
    public int coins = 0;
    public int round = 1;
    public int killsThisRound = 0;
    public bool isGameOver = false;

    public Text pointsText;
    public Text coinsText;
    public Image damageBar;

    public CanvasGroup gameOverGroup;
    public Button retryBtn;
    public Button mainMenuBtn;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
    }

    void Start() {
        if (retryBtn != null) {
            retryBtn.onClick.AddListener(ResetLevel);
        }
        UpdateUI();
    }

    void Update() {
        UpdateUI();
    }

    public void AddKill() {
        if (isGameOver) return;

        killsThisRound++;
        points += 5;

        if (EnemySpawner.Instance != null) {
            EnemySpawner.Instance.RegisterKill();
            round = EnemySpawner.Instance.currentRound;
            killsThisRound = EnemySpawner.Instance.killsThisRound;
        }

        UpdateUI();
    }

    public void TriggerGameOver() {
        isGameOver = true;
        Time.timeScale = 0f;

        if (gameOverGroup != null) {
            gameOverGroup.alpha = 1f;
            gameOverGroup.interactable = true;
            gameOverGroup.blocksRaycasts = true;
        } else {
            // If no game over panel exists, reset in-place directly
            ResetLevel();
        }
    }

    public void ResetLevel() {
        // Set isGameOver flag to prevent death and drop cascades during object recycling
        isGameOver = true;

        if (gameOverGroup != null) {
            gameOverGroup.alpha = 0f;
            gameOverGroup.interactable = false;
            gameOverGroup.blocksRaycasts = false;
        }

        // 1. Recycle all pooled entities via PoolManager
        if (PoolManager.Instance != null) {
            PoolManager.Instance.RecycleAll();
        }

        if (FlockingManager.Instance != null) {
            FlockingManager.Instance.ClearEnemies();
        }

        // 2. Reset PlayerController & PlayerHealth
        if (PlayerController.Instance != null) {
            PlayerController.Instance.transform.position = new Vector3(0f, 1f, 0f);
            PlayerController.Instance.transform.rotation = Quaternion.identity;
            PlayerController.Instance.ResetCombatState();
        }

        if (PlayerHealth.Instance != null) {
            PlayerHealth.Instance.ResetHealth();
        }

        // 3. Reset Economy / Wave Counters
        points = 0;
        round = 1;
        killsThisRound = 0;

        if (EnemySpawner.Instance != null) {
            EnemySpawner.Instance.ResetSpawner();
        }

        // 4. Reset UI
        if (XPUI.Instance != null) {
            XPUI.Instance.UpdateAll();
            XPUI.Instance.AnnounceRound(1);
        }

        UpdateUI();

        // 5. Restore gameplay state
        isGameOver = false;
        Time.timeScale = 1f;
    }

    public void UpdateUI() {
        if (pointsText != null) {
            pointsText.text = "Points: " + points;
        }
        if (coinsText != null) {
            int currentCoins = SaveManager.Instance != null ? SaveManager.Instance.coins : PlayerPrefs.GetInt("Economy_coins", 0);
            coinsText.text = "Coins: " + currentCoins;
        }
        if (XPUI.Instance != null) {
            XPUI.Instance.UpdateAll();
        }
    }
}
