using UnityEngine;

public class DropManager : MonoBehaviour {
    public static DropManager Instance { get; private set; }

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
    }

    public void SpawnDrop(Vector3 pos, bool isTough) {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) {
            return;
        }

        float roll = Random.value;
        float mult = isTough ? 2f : 1f;

        string dropTag = null;
        if (roll < 0.04f * mult) {
            dropTag = "powerup";
        } else if (roll < (0.04f + 0.08f) * mult) {
            dropTag = "health";
        } else if (roll < (0.04f + 0.08f + 0.15f) * mult) {
            dropTag = "coin";
        } else if (roll < (0.04f + 0.08f + 0.15f + 0.25f) * mult) {
            dropTag = "points";
        }

        if (dropTag != null && PoolManager.Instance != null) {
            PoolManager.Instance.Spawn(dropTag, pos, Quaternion.identity);
        }
    }
}
