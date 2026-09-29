using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {
    public static PlayerController Instance { get; private set; }

    public float walkSpeed = 3f;
    public float runSpeed = 7f;
    public float jumpVelocity = 8f;
    public float doubleJumpVelocity = 12f;
    public float gravity = 13.5f;
    public float knockbackForce = 100f;

    public float damageMultiplier = 1f;
    public float powerupTimer = 0f;
    public const float POWERUP_DURATION = 15f;

    private CharacterController cc;
    private Animator anim;
    private Vector3 velocity = Vector3.zero;

    private bool isAttacking = false;
    private float dashSpeed = 0f;
    private float dashEndTime = 0f;
    private float knockbackTimer = 0f;
    private Vector3 knockbackVel = Vector3.zero;

    private int punchCount = 0;
    private int kickCount = 0;
    private int jumpCount = 0;
    private float comboResetTime = 0f;

    private Coroutine currentActionRoutine;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
    }

    void Start() {
        cc = GetComponent<CharacterController>();
        if (cc == null) cc = gameObject.AddComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update() {
        if (Time.timeScale == 0) return;

        // Power multiplier timer countdown
        if (powerupTimer > 0f) {
            powerupTimer -= Time.deltaTime;
            if (powerupTimer <= 0f) {
                powerupTimer = 0f;
                damageMultiplier = 1f;
            }
        }

        // Combo timeout reset
        if (Time.time > comboResetTime && !isAttacking) {
            punchCount = 0;
            kickCount = 0;
        }

        // Knockback handling
        if (knockbackTimer > 0f) {
            knockbackTimer -= Time.deltaTime;
            float tempY = velocity.y - gravity * Time.deltaTime;
            velocity = knockbackVel;
            velocity.y = tempY;
            cc.Move(velocity * Time.deltaTime);
            return;
        }

        // Ground and gravity handling
        if (cc.isGrounded) {
            if (velocity.y < 0f) velocity.y = -0.1f;
            jumpCount = 0;
        } else {
            velocity.y -= gravity * Time.deltaTime;
        }

        // Combat & Jump Inputs
        if (!isAttacking) {
            bool elbowMod = IsElbowModPressed();
            bool punchPressed = IsPunchPressed();
            bool kickPressed = IsKickPressed();
            bool elbowPressed = IsElbowPressed();
            bool jumpPressed = IsJumpPressed();

            if (elbowPressed || (elbowMod && punchPressed)) {
                HandleElbow();
            } else if (punchPressed) {
                HandlePunch();
            } else if (kickPressed) {
                HandleKick();
            } else if (jumpPressed) {
                HandleJump();
            }
        }

        // Locomotion or Dash translation
        if (!isAttacking) {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector3 camFwd = Vector3.forward;
            Vector3 camRight = Vector3.right;
            if (Camera.main != null) {
                camFwd = Camera.main.transform.forward; camFwd.y = 0f; camFwd.Normalize();
                camRight = Camera.main.transform.right; camRight.y = 0f; camRight.Normalize();
            }

            Vector3 move = (camFwd * v + camRight * h);
            if (move.sqrMagnitude > 1f) move.Normalize();

            bool isRunning = IsRunPressed();
            float speed = isRunning ? runSpeed : walkSpeed;

            velocity.x = move.x * speed;
            velocity.z = move.z * speed;

            if (move.sqrMagnitude > 0.01f) {
                transform.rotation = Quaternion.LookRotation(move);
                if (anim != null) anim.SetFloat("Speed", isRunning ? 1f : 0.5f);
            } else {
                if (anim != null) anim.SetFloat("Speed", 0f);
            }
        } else {
            Vector3 dashDir = transform.forward;
            if (Time.time < dashEndTime) {
                velocity.x = dashDir.x * dashSpeed;
                velocity.z = dashDir.z * dashSpeed;
            } else {
                velocity.x = 0f;
                velocity.z = 0f;
            }
        }

        cc.Move(velocity * Time.deltaTime);
    }

    private void HandleJump() {
        if (cc.isGrounded || jumpCount == 0) {
            velocity.y = jumpVelocity;
            jumpCount = 1;
            if (anim != null) anim.SetTrigger("Jump");
        } else if (jumpCount == 1) {
            // Mid-air double jump impulse (12.0 units/s)
            velocity.y = doubleJumpVelocity;
            jumpCount = 2;
            if (anim != null) anim.SetTrigger("Jump");
        }
    }

    private void HandlePunch() {
        punchCount++;
        kickCount = 0;
        comboResetTime = Time.time + 1.5f;

        if (punchCount <= 2) {
            PlayAction("punch", 0f, 0f, 1.2f, false);
        } else if (punchCount <= 4) {
            PlayAction("punch-elbow", 0f, 0f, 1.0f, false);
        } else {
            // Finisher: dash 30.0 for punch-hard over 0.15s, resets punchCount
            PlayAction("punch-hard", 30f, 0.15f, 2.0f, false);
            punchCount = 0;
        }
    }

    private void HandleKick() {
        kickCount++;
        punchCount = 0;
        comboResetTime = Time.time + 1.5f;

        if (kickCount <= 3) {
            PlayAction("kick", 0f, 0f, 1.0f, false);
        } else {
            // Finisher: dash 15.0 for tornado kick over 0.10s, 360-degree sweep, resets kickCount
            PlayAction("kick-tornado", 15f, 0.10f, 1.0f, true);
            kickCount = 0;
        }
    }

    private void HandleElbow() {
        // Dedicated elbow strike or modifier: resets both combo chains
        punchCount = 0;
        kickCount = 0;
        comboResetTime = Time.time + 1.5f;
        PlayAction("punch-elbow", 0f, 0f, 1.0f, false);
    }

    public void PlayAction(string animName, float dSpeed, float dTime, float animSpeed, bool is360Sweep) {
        if (currentActionRoutine != null) {
            StopCoroutine(currentActionRoutine);
        }
        currentActionRoutine = StartCoroutine(PlayActionRoutine(animName, dSpeed, dTime, animSpeed, is360Sweep));
    }

    public IEnumerator PlayActionRoutine(string animName, float dSpeed, float dTime, float animSpeed, bool is360Sweep) {
        isAttacking = true;
        dashSpeed = dSpeed;
        dashEndTime = Time.time + dTime;

        if (anim != null) {
            anim.speed = animSpeed;
            anim.Play(animName, 0, 0f);
        }

        // IEnumerator coroutine hit delay: 0.1s scaled by anim speed
        float windup = 0.1f / Mathf.Max(animSpeed, 0.01f);
        yield return new WaitForSeconds(windup);

        CheckMeleeHit(is360Sweep);

        if (dSpeed > 0f && dTime > 0f) {
            yield return new WaitForSeconds(dTime);
            dashSpeed = 0f;
        }

        float animLength = 0.35f;
        float remaining = Mathf.Max((animLength / Mathf.Max(animSpeed, 0.01f)) - windup - dTime, 0.05f);
        yield return new WaitForSeconds(remaining);

        if (anim != null) anim.speed = 1.0f;
        dashSpeed = 0f;
        isAttacking = false;
        currentActionRoutine = null;
    }

    private void CheckMeleeHit(bool is360Sweep) {
        Collider[] hits = Physics.OverlapSphere(transform.position, 2.5f);
        Vector3 playerPos = transform.position;
        Vector3 forward = transform.forward;
        bool hitAny = false;

        for (int i = 0; i < hits.Length; i++) {
            Collider hit = hits[i];
            if (hit == null || hit.gameObject == gameObject) continue;

            EnemyHealth eh = hit.GetComponent<EnemyHealth>();
            BossAI boss = hit.GetComponent<BossAI>();

            if (eh == null && boss == null) continue;

            // Planar elevation projection: toEnemy.y = 0 before distance and dot checks
            Vector3 enemyPos = hit.transform.position;
            Vector3 toEnemy = enemyPos - playerPos;
            toEnemy.y = 0f;

            if (toEnemy.sqrMagnitude > (2.5f * 2.5f)) continue;

            bool inArc = is360Sweep || (Vector3.Dot(forward, toEnemy.normalized) > 0.3f);
            if (!inArc) continue;

            hitAny = true;
            int damage = Mathf.RoundToInt(10f * damageMultiplier);

            if (eh != null) {
                eh.TakeDamage(damage, forward);
            }
            if (boss != null) {
                boss.TakeMeleeHit(damageMultiplier);
            }
        }

        if (hitAny && AudioPool.Instance != null) {
            AudioPool.Instance.PlayHit();
        }
    }

    public void ActivatePowerMultiplier(float duration = 15f, float multiplier = 2f) {
        powerupTimer = duration;
        damageMultiplier = multiplier;
    }

    public void ApplyKnockback(Vector3 dir) {
        dir.y = 0f;
        knockbackVel = dir.normalized * (knockbackForce / 16f);
        knockbackTimer = 0.15f;
    }

    public void ResetCombatState() {
        if (currentActionRoutine != null) {
            StopCoroutine(currentActionRoutine);
            currentActionRoutine = null;
        }

        punchCount = 0;
        kickCount = 0;
        jumpCount = 0;
        isAttacking = false;
        dashSpeed = 0f;
        dashEndTime = 0f;
        knockbackTimer = 0f;
        knockbackVel = Vector3.zero;
        velocity = Vector3.zero;
        damageMultiplier = 1f;
        powerupTimer = 0f;

        if (anim != null) {
            anim.speed = 1.0f;
            anim.SetFloat("Speed", 0f);
        }
    }

    // Legacy input helpers
    private bool IsPunchPressed() {
        return Input.GetButtonDown("Fire3") || Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.JoystickButton2);
    }

    private bool IsKickPressed() {
        return Input.GetButtonDown("Fire2") || Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.JoystickButton1);
    }

    private bool IsElbowPressed() {
        return Input.GetButtonDown("Elbow") || Input.GetKeyDown(KeyCode.C);
    }

    private bool IsElbowModPressed() {
        return Input.GetButton("ElbowMod") || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.JoystickButton5);
    }

    private bool IsJumpPressed() {
        return Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0);
    }

    private bool IsRunPressed() {
        return Input.GetButton("Run") || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.JoystickButton4);
    }
}
