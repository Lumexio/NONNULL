using UnityEngine;

public class EnemyFlocking : MonoBehaviour {
    public float maxSpeed = 40f;
    public float separationWeight = 1.5f;
    public float cohesionWeight = 0.6f;
    public float separationRadius = 24f;
    public float cohesionRadius = 96f;
    public float maxForce = 0.5f;

    private Collider col;
    private EnemyAI ai;

    void Awake() {
        col = GetComponent<Collider>();
        ai = GetComponent<EnemyAI>();
    }

    void Start() {
        if (ai == null) ai = GetComponent<EnemyAI>();
        if (FlockingManager.Instance != null && ai != null) {
            FlockingManager.Instance.RegisterEnemy(ai);
        }
    }

    void OnDestroy() {
        if (FlockingManager.Instance != null && ai != null) {
            FlockingManager.Instance.UnregisterEnemy(ai);
        }
    }

    public void ToggleCollider(bool isActive) {
        if (col != null && col.enabled != isActive) {
            col.enabled = isActive;
        }
    }
}
