using UnityEngine;

public class Pickup : MonoBehaviour {
    public float life = 15f;
    public string pickupType = "Health"; // Health, Coin, Points, Power

    private float timer = 0f;
    private Collider triggerCollider;

    void Awake() {
        triggerCollider = GetComponent<Collider>();
    }

    void OnEnable() {
        timer = 0f;
        if (triggerCollider != null) {
            triggerCollider.enabled = true;
        }

        // Default lifetimes from slide-x audit:
        string type = pickupType != null ? pickupType.ToLowerInvariant() : "";
        if (type == "coin") {
            life = 20f;
        } else if (type == "health") {
            life = 15f;
        } else if (type == "points") {
            life = 10f;
        } else if (type == "power" || type == "powerup") {
            life = 10f;
        } else {
            life = 15f;
        }
    }

    void Update() {
        // Spin
        transform.Rotate(0f, 90f * Time.deltaTime, 0f);

        // Bobbing
        Vector3 pos = transform.position;
        pos.y += Mathf.Sin(Time.time * 5f) * Time.deltaTime * 0.5f;
        transform.position = pos;

        timer += Time.deltaTime;
        if (timer >= life) {
            RecycleSelf();
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other == null) return;

        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null) {
            ApplyPickupEffect(other.gameObject);
            RecycleSelf();
        }
    }

    private void ApplyPickupEffect(GameObject playerObj) {
        string type = pickupType != null ? pickupType.ToLowerInvariant() : "";

        if (type == "health") {
            PlayerHealth ph = playerObj.GetComponent<PlayerHealth>();
            if (ph == null && PlayerHealth.Instance != null) {
                ph = PlayerHealth.Instance;
            }
            if (ph != null) {
                ph.Heal(10);
            }
        } else if (type == "coin") {
            if (SaveManager.Instance != null) {
                SaveManager.Instance.AddCoins(1);
            } else {
                int c = PlayerPrefs.GetInt("Economy_coins", 0);
                PlayerPrefs.SetInt("Economy_coins", c + 1);
                PlayerPrefs.Save();
            }
        } else if (type == "points") {
            if (GameManager.Instance != null) {
                GameManager.Instance.points += 50;
            }
        } else if (type == "power" || type == "powerup") {
            PlayerController pc = playerObj.GetComponent<PlayerController>();
            if (pc == null && PlayerController.Instance != null) {
                pc = PlayerController.Instance;
            }
            if (pc != null) {
                pc.ActivatePowerMultiplier(15f, 2f);
            }
        }
    }

    private void RecycleSelf() {
        string tag = GetPoolTag();
        if (PoolManager.Instance != null) {
            PoolManager.Instance.Recycle(tag, gameObject);
        } else {
            gameObject.SetActive(false);
        }
    }

    private string GetPoolTag() {
        string type = pickupType != null ? pickupType.ToLowerInvariant() : "";
        if (type == "coin") return "coin";
        if (type == "health") return "health";
        if (type == "points") return "points";
        if (type == "power" || type == "powerup") return "powerup";
        return "health";
    }
}
