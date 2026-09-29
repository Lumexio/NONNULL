using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour {
    public static PlayerHealth Instance { get; private set; }

    public int maxHp = 100;
    public int currentHp;
    public Image lifeBar;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        }
    }

    void Start() {
        ResetHealth();
    }

    public void TakeDamage(int amount) {
        if (amount <= 0) return;

        currentHp -= amount;
        if (currentHp < 0) currentHp = 0;

        UpdateUI();

        if (AudioPool.Instance != null) {
            AudioPool.Instance.PlayPain();
        }

        if (currentHp <= 0) {
            if (GameManager.Instance != null) {
                GameManager.Instance.TriggerGameOver();
            }
        }
    }

    public void Heal(int amount) {
        if (amount <= 0) return;

        currentHp = Mathf.Min(maxHp, currentHp + amount);
        UpdateUI();
    }

    public void ResetHealth() {
        currentHp = maxHp;
        UpdateUI();
    }

    public void UpdateUI() {
        if (lifeBar != null && maxHp > 0) {
            lifeBar.fillAmount = (float)currentHp / maxHp;
        }
    }
}
