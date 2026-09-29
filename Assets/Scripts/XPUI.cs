using UnityEngine;
using UnityEngine.UI;

public class XPUI : MonoBehaviour {
    public static XPUI Instance { get; private set; }

    // Master 960x544 screen space constants
    public static readonly Vector2 SCREEN_RESOLUTION = new Vector2(960, 544);
    public static readonly Vector2 HP_PANEL_POS = new Vector2(20, 20);
    public static readonly Vector2 HP_PANEL_SIZE = new Vector2(300, 100);
    public static readonly Vector2 ROUND_PANEL_POS = new Vector2(640, 20);
    public static readonly Vector2 ROUND_PANEL_SIZE = new Vector2(300, 140);
    public static readonly Vector2 ROUND_BANNER_POS = new Vector2(0, 200);
    public static readonly Vector2 ROUND_BANNER_SIZE = new Vector2(960, 100);

    // HP Panel Components
    public RectTransform hpPanel;
    public Text hpLabel;
    public Image hpBar;
    public Text damageMultiplierLabel;
    public Image damageMultiplierBar;

    // Round Panel Components
    public RectTransform roundPanel;
    public Text roundLabel;
    public Image roundProgressBar;
    public Text pointsLabel;
    public Text coinsLabel;

    // Round Banner Component
    public RectTransform roundBanner;
    public Text roundBannerText;

    private float bannerTimer = 0f;
    private const float BANNER_DURATION = 2.0f;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
    }

    void Start() {
        ApplyLayoutCoordinates();
        UpdateAll();
        AnnounceRound(1);
    }

    void Update() {
        UpdateAll();

        // Banner auto-clear timer
        if (bannerTimer > 0f) {
            bannerTimer -= Time.deltaTime;
            if (bannerTimer <= 0f) {
                bannerTimer = 0f;
                if (roundBannerText != null) {
                    roundBannerText.text = "";
                }
                if (roundBanner != null) {
                    roundBanner.gameObject.SetActive(false);
                }
            }
        }
    }

    public void ApplyLayoutCoordinates() {
        // Enforce exact 960x544 coordinates on UI RectTransforms if assigned
        if (hpPanel != null) {
            hpPanel.anchorMin = new Vector2(0f, 1f);
            hpPanel.anchorMax = new Vector2(0f, 1f);
            hpPanel.pivot = new Vector2(0f, 1f);
            hpPanel.anchoredPosition = new Vector2(HP_PANEL_POS.x, -HP_PANEL_POS.y);
            hpPanel.sizeDelta = HP_PANEL_SIZE;
        }

        if (roundPanel != null) {
            roundPanel.anchorMin = new Vector2(0f, 1f);
            roundPanel.anchorMax = new Vector2(0f, 1f);
            roundPanel.pivot = new Vector2(0f, 1f);
            roundPanel.anchoredPosition = new Vector2(ROUND_PANEL_POS.x, -ROUND_PANEL_POS.y);
            roundPanel.sizeDelta = ROUND_PANEL_SIZE;
        }

        if (roundBanner != null) {
            roundBanner.anchorMin = new Vector2(0.5f, 0.5f);
            roundBanner.anchorMax = new Vector2(0.5f, 0.5f);
            roundBanner.pivot = new Vector2(0.5f, 0.5f);
            roundBanner.anchoredPosition = new Vector2(ROUND_BANNER_POS.x, ROUND_BANNER_POS.y - (SCREEN_RESOLUTION.y / 2f));
            roundBanner.sizeDelta = ROUND_BANNER_SIZE;
        }
    }

    public void UpdateAll() {
        // 1. Update Player HP & Damage Multiplier
        if (PlayerHealth.Instance != null && hpBar != null) {
            float hpRatio = PlayerHealth.Instance.maxHp > 0
                ? (float)PlayerHealth.Instance.currentHp / PlayerHealth.Instance.maxHp
                : 0f;
            hpBar.fillAmount = Mathf.Clamp01(hpRatio);
        }

        if (PlayerController.Instance != null && damageMultiplierBar != null) {
            float powerRatio = PlayerController.Instance.powerupTimer / PlayerController.POWERUP_DURATION;
            damageMultiplierBar.fillAmount = Mathf.Clamp01(powerRatio);
        }

        // 2. Update Round, Points, Coins
        if (GameManager.Instance != null) {
            if (roundLabel != null) {
                roundLabel.text = "Round " + GameManager.Instance.round;
            }
            if (pointsLabel != null) {
                pointsLabel.text = "Points: " + GameManager.Instance.points;
            }

            int targetKills = 10;
            if (EnemySpawner.Instance != null) {
                targetKills = EnemySpawner.Instance.targetKills;
            }
            if (roundProgressBar != null && targetKills > 0) {
                float progressRatio = (float)GameManager.Instance.killsThisRound / targetKills;
                roundProgressBar.fillAmount = Mathf.Clamp01(progressRatio);
            }
        }

        if (coinsLabel != null) {
            int currentCoins = SaveManager.Instance != null ? SaveManager.Instance.coins : PlayerPrefs.GetInt("Economy_coins", 0);
            coinsLabel.text = "Coins: " + currentCoins;
        }
    }

    public void AnnounceRound(int roundNumber) {
        if (roundBanner != null) {
            roundBanner.gameObject.SetActive(true);
        }
        if (roundBannerText != null) {
            roundBannerText.text = "ROUND " + roundNumber + " - FIGHT!";
        }
        bannerTimer = BANNER_DURATION;
    }
}
