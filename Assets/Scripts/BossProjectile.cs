using UnityEngine;

public class BossProjectile : MonoBehaviour {
    public float speed = 10f;
    public float lifetime = 5f;
    public int damage = 15;
    public string poolTag = "boss_projectile";

    private float age = 0f;

    void OnEnable() {
        age = 0f;
    }

    void Update() {
        age += Time.deltaTime;
        if (age >= lifetime) {
            RecycleSelf();
            return;
        }

        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other) {
        if (other == null) return;

        if (other.CompareTag("Player") || other.GetComponent<PlayerHealth>() != null) {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null) {
                ph.TakeDamage(damage);
            }
            RecycleSelf();
        }
    }

    private void RecycleSelf() {
        if (PoolManager.Instance != null) {
            PoolManager.Instance.Recycle(poolTag, gameObject);
        } else {
            gameObject.SetActive(false);
        }
    }
}
