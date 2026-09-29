using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class EnemyAI : MonoBehaviour {
    [HideInInspector] public string poolTag = "";
    public float speed = 2.5f;
    public float tackleWindupTime = 0.3f;
    public float tackleDashSpeed = 7.5f;
    public float tackleDuration = 0.2f;
    public float tackleRetreatSpeed = 6.25f;
    public float retreatDuration = 0.4f;
    public float tackleCooldown = 1.5f;
    public int tackleDamage = 10;
    public bool isTough = false;
    public bool isAttacker = false;

    private CharacterController cc;
    private Renderer rend;
    private Transform player;

    public enum State { IDLE, WINDUP, TACKLE, RETREAT, COOLDOWN }
    public State state = State.IDLE;
    private float stateTimer = 0f;
    private Vector3 tackleDir = Vector3.zero;
    private Vector3 velocity = Vector3.zero;
    private Vector3 flockingSteer = Vector3.zero;
    private Vector3 knockbackVel = Vector3.zero;
    private float knockbackTimer = 0f;

    void Awake() {
        cc = GetComponent<CharacterController>();
        if (cc == null) cc = gameObject.AddComponent<CharacterController>();
        rend = GetComponentInChildren<Renderer>();
    }

    void OnEnable() {
        state = State.IDLE;
        stateTimer = 0f;
        velocity = Vector3.zero;
        flockingSteer = Vector3.zero;
        knockbackTimer = 0f;
        knockbackVel = Vector3.zero;
        isAttacker = false;

        if (isTough) {
            speed = 2.0f;
            tackleDamage = 20;
            if (rend != null) rend.material.color = Color.green;
        } else {
            speed = 2.5f;
            tackleDamage = 10;
            if (rend != null) rend.material.color = Color.white;
        }

        if (FlockingManager.Instance != null) {
            FlockingManager.Instance.RegisterEnemy(this);
        }
    }

    void OnDisable() {
        if (FlockingManager.Instance != null) {
            FlockingManager.Instance.UnregisterEnemy(this);
        }
    }

    void Start() {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
        if (PlayerController.Instance != null) player = PlayerController.Instance.transform;
    }

    void Update() {
        if (player == null) {
            if (PlayerController.Instance != null) {
                player = PlayerController.Instance.transform;
            } else {
                GameObject p = GameObject.FindWithTag("Player");
                if (p != null) player = p.transform;
            }
            if (player == null) return;
        }

        // Knockback handling
        if (knockbackTimer > 0f) {
            knockbackTimer -= Time.deltaTime;
            velocity = knockbackVel;
            knockbackVel = Vector3.Lerp(knockbackVel, Vector3.zero, Time.deltaTime * 4f);
            velocity.y = -0.1f;
            cc.Move(velocity * Time.deltaTime);
            return;
        }

        stateTimer += Time.deltaTime;

        switch (state) {
            case State.IDLE:
                if (isAttacker) {
                    float dist = Vector3.Distance(transform.position, player.position);
                    if (dist <= 3.0f) {
                        EnterState(State.WINDUP);
                        break;
                    }
                }

                // Move according to flocking steering
                velocity = flockingSteer * speed;
                if (flockingSteer.sqrMagnitude > 0.01f) {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flockingSteer), Time.deltaTime * 8f);
                }
                break;

            case State.WINDUP:
                velocity = Vector3.zero;
                Vector3 lookDir = (player.position - transform.position);
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.01f) {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir.normalized), Time.deltaTime * 10f);
                }
                if (stateTimer >= tackleWindupTime) {
                    tackleDir = lookDir.normalized;
                    EnterState(State.TACKLE);
                }
                break;

            case State.TACKLE:
                velocity = tackleDir * tackleDashSpeed;
                if (stateTimer >= tackleDuration) {
                    EnterState(State.RETREAT);
                }
                break;

            case State.RETREAT:
                velocity = -tackleDir * tackleRetreatSpeed;
                if (stateTimer >= retreatDuration) {
                    EnterState(State.COOLDOWN);
                }
                break;

            case State.COOLDOWN:
                velocity = Vector3.zero;
                if (stateTimer >= tackleCooldown) {
                    if (isAttacker && FlockingManager.Instance != null) {
                        FlockingManager.Instance.ReleaseAttacker(this);
                    }
                    EnterState(State.IDLE);
                }
                break;
        }

        velocity.y = -0.1f;
        cc.Move(velocity * Time.deltaTime);
    }

    public void EnterState(State s) {
        state = s;
        stateTimer = 0f;
    }

    public void SetAttacker(bool attacker) {
        isAttacker = attacker;
        if (!isAttacker && (state == State.WINDUP || state == State.TACKLE)) {
            EnterState(State.RETREAT);
        }
    }

    public void SetFlockingSteer(Vector3 steerDir) {
        flockingSteer = steerDir;
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        if (state == State.TACKLE && hit.gameObject.CompareTag("Player")) {
            PlayerHealth ph = hit.gameObject.GetComponent<PlayerHealth>();
            if (ph == null && PlayerHealth.Instance != null) ph = PlayerHealth.Instance;
            if (ph != null) {
                ph.TakeDamage(tackleDamage);
            }

            PlayerController pc = hit.gameObject.GetComponent<PlayerController>();
            if (pc == null && PlayerController.Instance != null) pc = PlayerController.Instance;
            if (pc != null) {
                pc.ApplyKnockback(tackleDir);
            }

            EnterState(State.RETREAT);
        }
    }

    public void ApplyKnockback(Vector3 dir) {
        dir.y = 0f;
        knockbackVel = dir.normalized * (200f / 16f);
        knockbackTimer = 0.25f;
    }

    public void StartFlashAndSquash() {
        StartCoroutine(FlashAndSquashRoutine());
    }

    private IEnumerator FlashAndSquashRoutine() {
        if (rend == null) yield break;

        Vector3 origScale = transform.localScale;
        Color origColor = isTough ? Color.green : Color.white;

        rend.material.color = Color.white;
        transform.localScale = new Vector3(1.2f, 0.8f, 1.2f);

        yield return new WaitForSeconds(0.15f);

        if (rend != null) rend.material.color = origColor;
        transform.localScale = origScale;
    }
}
