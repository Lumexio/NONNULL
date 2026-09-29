using UnityEngine;
using System.Collections.Generic;

public class FlockingManager : MonoBehaviour {
    public static FlockingManager Instance { get; private set; }

    public Transform player;
    public List<EnemyAI> enemies = new List<EnemyAI>();

    public float attackerTimeout = 4.0f;
    public float attackerMaxRange = 12.5f;
    public float queuedSpacingRadius = 3.0f;
    public float activationRadius = 18.75f;

    private EnemyAI currentAttacker = null;
    private float attackerTimer = 0f;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
    }

    void Start() {
        if (player == null) {
            if (PlayerController.Instance != null) {
                player = PlayerController.Instance.transform;
            } else {
                GameObject p = GameObject.FindWithTag("Player");
                if (p != null) player = p.transform;
            }
        }
    }

    void Update() {
        if (player == null) {
            if (PlayerController.Instance != null) player = PlayerController.Instance.transform;
            return;
        }

        if (enemies.Count == 0) return;

        int frame = Time.frameCount;
        int bucket = frame % 3;

        // Attacker Token Management
        ManageAttackerToken();

        // 3-Frame Spatial Boids Update
        for (int i = 0; i < enemies.Count; i++) {
            EnemyAI enemy = enemies[i];
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;

            // Only update steering forces on the enemy's assigned frame bucket
            if (i % 3 == bucket) {
                UpdateEnemySteering(enemy);
            }
        }
    }

    private void ManageAttackerToken() {
        if (currentAttacker != null) {
            attackerTimer += Time.deltaTime;
            float distToPlayer = Vector3.Distance(currentAttacker.transform.position, player.position);

            // Timeout or range revocation
            if (attackerTimer >= attackerTimeout || distToPlayer > attackerMaxRange || !currentAttacker.gameObject.activeInHierarchy) {
                ClearAttacker();
            }
        }

        // If no attacker exists, assign token to closest queued candidate within activation radius
        if (currentAttacker == null) {
            EnemyAI bestCandidate = null;
            float closestDist = float.MaxValue;

            for (int i = 0; i < enemies.Count; i++) {
                EnemyAI enemy = enemies[i];
                if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;

                float dist = Vector3.Distance(enemy.transform.position, player.position);
                if (dist <= activationRadius && dist < closestDist) {
                    closestDist = dist;
                    bestCandidate = enemy;
                }
            }

            if (bestCandidate != null) {
                currentAttacker = bestCandidate;
                attackerTimer = 0f;
                currentAttacker.SetAttacker(true);
            }
        }
    }

    private void UpdateEnemySteering(EnemyAI enemy) {
        if (enemy.isAttacker) {
            // Attacker steers directly toward player
            Vector3 toPlayer = (player.position - enemy.transform.position);
            toPlayer.y = 0f;
            enemy.SetFlockingSteer(toPlayer.normalized);
            return;
        }

        Vector3 separation = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        int neighborCount = 0;
        float separationRadius = 1.5f;
        float cohesionRadius = 6.0f;

        Vector3 myPos = enemy.transform.position;

        for (int i = 0; i < enemies.Count; i++) {
            EnemyAI other = enemies[i];
            if (other == null || other == enemy || !other.gameObject.activeInHierarchy) continue;

            float dist = Vector3.Distance(myPos, other.transform.position);
            if (dist > 0.01f && dist < cohesionRadius) {
                cohesion += other.transform.position;
                neighborCount++;

                if (dist < separationRadius) {
                    separation += (myPos - other.transform.position).normalized / dist;
                }

                if (neighborCount >= 8) break;
            }
        }

        Vector3 steer = Vector3.zero;
        if (neighborCount > 0) {
            cohesion /= neighborCount;
            steer += (cohesion - myPos).normalized * 0.6f;
        }
        if (separation.sqrMagnitude > 0.01f) {
            steer += separation.normalized * 1.5f;
        }

        // Queued spacing ring around player (3.0 units)
        float distToPlayer = Vector3.Distance(myPos, player.position);
        Vector3 toPlayerDir = (player.position - myPos);
        toPlayerDir.y = 0f;

        if (distToPlayer < queuedSpacingRadius) {
            // Repulse away from player if within 3.0 spacing ring
            steer -= toPlayerDir.normalized * 2.0f;
        } else {
            // Creep gently toward player
            steer += toPlayerDir.normalized * 0.8f;
        }

        steer.y = 0f;
        enemy.SetFlockingSteer(steer.normalized);
    }

    public void ClearAttacker() {
        if (currentAttacker != null) {
            currentAttacker.SetAttacker(false);
            currentAttacker = null;
        }
        attackerTimer = 0f;
    }

    public void ReleaseAttacker(EnemyAI enemy) {
        if (currentAttacker == enemy) {
            ClearAttacker();
        }
    }

    public void RegisterEnemy(EnemyAI enemy) {
        if (enemy != null && !enemies.Contains(enemy)) {
            enemies.Add(enemy);
        }
    }

    public void UnregisterEnemy(EnemyAI enemy) {
        if (enemy != null) {
            if (currentAttacker == enemy) {
                ClearAttacker();
            }
            enemies.Remove(enemy);
        }
    }

    public void ClearEnemies() {
        ClearAttacker();
        enemies.Clear();
    }
}
