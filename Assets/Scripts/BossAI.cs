using UnityEngine;

public enum BossState { BURST, OVERHEAT, COOLDOWN }

public class BossAI : MonoBehaviour {
    public float maxHp = 200f;
    public float hp = 200f;

    public float burstDuration = 3f;
    public float overheatDuration = 2f;
    public float cooldownDuration = 1f;

    public float burstInterval = 0.5f;
    public int projectilesPerBurst = 5;
    public int coinReward = 50;

    public Transform firePoint;

    private BossState state = BossState.BURST;
    private float stateTimer = 0f;
    private float burstTimer = 0f;

    private Transform player;
    private Renderer rend;
    private MaterialPropertyBlock propBlock;

    void Awake() {
        rend = GetComponentInChildren<Renderer>();
        propBlock = new MaterialPropertyBlock();
    }

    void Start() {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
        if (PlayerController.Instance != null) player = PlayerController.Instance.transform;

        hp = maxHp;
        EnterState(BossState.BURST);
    }

    void OnEnable() {
        hp = maxHp;
        state = BossState.BURST;
        stateTimer = 0f;
        burstTimer = 0f;
        UpdateVisuals();
    }

    void Update() {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        if (player == null) {
            if (PlayerController.Instance != null) {
                player = PlayerController.Instance.transform;
            } else {
                GameObject p = GameObject.FindWithTag("Player");
                if (p != null) player = p.transform;
            }
        }

        stateTimer += Time.deltaTime;

        switch (state) {
            case BossState.BURST:
                if (player != null) {
                    Vector3 lookDir = player.position - transform.position;
                    lookDir.y = 0f;
                    if (lookDir.sqrMagnitude > 0.01f) {
                        Quaternion targetRot = Quaternion.LookRotation(lookDir);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
                    }
                }

                burstTimer += Time.deltaTime;
                if (burstTimer >= burstInterval) {
                    burstTimer = 0f;
                    FireBurst();
                }

                if (stateTimer >= burstDuration) {
                    EnterState(BossState.OVERHEAT);
                }
                break;

            case BossState.OVERHEAT:
                if (stateTimer >= overheatDuration) {
                    EnterState(BossState.COOLDOWN);
                }
                break;

            case BossState.COOLDOWN:
                if (stateTimer >= cooldownDuration) {
                    EnterState(BossState.BURST);
                }
                break;
        }
    }

    private void EnterState(BossState newState) {
        state = newState;
        stateTimer = 0f;
        burstTimer = 0f;
        UpdateVisuals();
    }

    private void FireBurst() {
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 1.5f;
        Vector3 baseDir = transform.forward;
        if (player != null) {
            baseDir = (player.position - spawnPos).normalized;
            baseDir.y = 0f;
            baseDir.Normalize();
        }

        // Spawn 5 projectiles centered on player direction with angular spread
        float spreadAngle = 10f; // degrees between shots
        int count = projectilesPerBurst;
        float startOffset = -((count - 1) * spreadAngle) / 2f;

        for (int i = 0; i < count; i++) {
            float angle = startOffset + i * spreadAngle;
            Quaternion rot = Quaternion.Euler(0f, angle, 0f) * Quaternion.LookRotation(baseDir);

            if (PoolManager.Instance != null) {
                PoolManager.Instance.Spawn("boss_projectile", spawnPos, rot);
            }
        }
    }

    public void TakeMeleeHit(float damageMultiplier = 1f) {
        // Vulnerable ONLY during OVERHEAT state
        if (state != BossState.OVERHEAT) return;

        float dmg = 10f * damageMultiplier;
        hp -= dmg;

        if (hp <= 0f) {
            Die();
        }
    }

    private void Die() {
        if (SaveManager.Instance != null) {
            SaveManager.Instance.AddCoins(coinReward);
        }

        if (GameManager.Instance != null) {
            GameManager.Instance.AddKill();
        }

        gameObject.SetActive(false);
    }

    private void UpdateVisuals() {
        if (rend == null) return;

        Color targetColor = Color.white;
        if (state == BossState.OVERHEAT) {
            targetColor = new Color(1f, 0.2f, 0f); // Glowing red
        } else if (state == BossState.COOLDOWN) {
            targetColor = new Color(0.8f, 0.8f, 0.8f); // Cooling grey
        } else {
            float hpRatio = maxHp > 0f ? (hp / maxHp) : 1f;
            targetColor = Color.white * Mathf.Clamp01(hpRatio);
        }

        if (propBlock == null) propBlock = new MaterialPropertyBlock();
        rend.GetPropertyBlock(propBlock);
        propBlock.SetColor("_Color", targetColor);
        rend.SetPropertyBlock(propBlock);
    }

    public BossState CurrentState {
        get { return state; }
    }
}
