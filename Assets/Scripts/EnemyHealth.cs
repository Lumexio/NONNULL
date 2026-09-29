using UnityEngine;

public class EnemyHealth : MonoBehaviour {
    public int maxHp = 10;
    public int hp = 10;
    private EnemyAI ai;

    void Awake() {
        ai = GetComponent<EnemyAI>();
    }

    void OnEnable() {
        if (ai == null) ai = GetComponent<EnemyAI>();
        maxHp = (ai != null && ai.isTough) ? 30 : 10;
        hp = maxHp;
    }

    public void TakeDamage(int amt, Vector3 dir) {
        hp -= amt;

        if (ai != null) {
            ai.ApplyKnockback(dir);
            ai.StartFlashAndSquash();
        }

        if (hp <= 0) {
            Die();
        }
    }

    private void Die() {
        bool isTough = (ai != null && ai.isTough);

        if (DropManager.Instance != null) {
            DropManager.Instance.SpawnDrop(transform.position, isTough);
        }

        if (GameManager.Instance != null) {
            GameManager.Instance.AddKill();
        }

        string tag = isTough ? "tough" : "weak";
        if (PoolManager.Instance != null) {
            PoolManager.Instance.Recycle(tag, gameObject);
        } else {
            gameObject.SetActive(false);
        }
    }
}
