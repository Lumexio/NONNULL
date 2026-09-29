using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    
    public int HP = 10;
    private bool isInvincible = false;
    
    private float gravity = 13.5f;
    private float walkSpeed = 3.0f;
    private float runSpeed = 7.0f;
    private float jumpVelocity = 8.0f;
    private float doubleJumpVelocity = 12.0f;
    private int jumpCount = 0;
    
    private float yVelocity = 0;
    
    private int punchCombo = 0;
    private int kickCombo = 0;
    private float lastAttackTime = 0;
    private float comboTimeout = 0.5f;

    private int powerupCharges = 0;
    private bool isPoweredUp = false;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (HP <= 0) return;

        HandleMovement();
        HandleCombat();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(h, 0, v).normalized;
        float currentSpeed = InputManager.Instance.Run ? runSpeed : walkSpeed;

        if (controller.isGrounded)
        {
            yVelocity = -gravity * Time.deltaTime;
            jumpCount = 0;
        }
        else
        {
            yVelocity -= gravity * Time.deltaTime;
        }

        if (InputManager.Instance.Jump && jumpCount <= 1)
        {
            yVelocity = (jumpCount == 0) ? jumpVelocity : doubleJumpVelocity;
            jumpCount++;
        }

        Vector3 velocity = move * currentSpeed;
        velocity.y = yVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleCombat()
    {
        if (Time.time - lastAttackTime > comboTimeout)
        {
            punchCombo = 0;
            kickCombo = 0;
        }

        if (InputManager.Instance.Punch)
        {
            punchCombo++;
            lastAttackTime = Time.time;
            if (punchCombo == 5)
            {
                StartCoroutine(Dash(30f, 0.15f));
                punchCombo = 0;
            }
            DoDamage();
        }
        else if (InputManager.Instance.Elbow)
        {
            punchCombo = 3; // Skips to elbow
            lastAttackTime = Time.time;
            DoDamage();
        }

        if (InputManager.Instance.Kick)
        {
            kickCombo++;
            lastAttackTime = Time.time;
            if (kickCombo == 4)
            {
                StartCoroutine(Dash(15f, 0.1f));
                kickCombo = 0;
            }
            DoDamage();
        }
    }

    private void DoDamage()
    {
        // Simple forward distance check for enemies
        foreach (Transform enemy in FlockManager.Instance.activeEnemies)
        {
            Vector3 dir = enemy.position - transform.position;
            if (dir.sqrMagnitude < 2.5f * 2.5f && Vector3.Dot(transform.forward, dir.normalized) > 0.3f)
            {
                enemy.SendMessage("TakeDamage", isPoweredUp ? 2 : 1, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    private IEnumerator Dash(float speed, float duration)
    {
        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            controller.Move(transform.forward * speed * Time.deltaTime);
            yield return null;
        }
    }

    public void TakeDamage(int dmg, Vector3 sourcePos)
    {
        if (isInvincible || HP <= 0) return;
        HP -= dmg;
        if (HP <= 0)
        {
            LevelManager.Instance.GameOver();
            return;
        }

        StartCoroutine(InvincibilityFrames());
        StartCoroutine(ApplyKnockback(sourcePos));
    }

    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        // Flash red logic would go here (e.g. material tint)
        yield return new WaitForSeconds(1f);
        isInvincible = false;
    }

    private IEnumerator ApplyKnockback(Vector3 sourcePos)
    {
        Vector3 dir = (transform.position - sourcePos).normalized;
        dir.y = 0;
        float startTime = Time.time;
        while (Time.time < startTime + 0.15f)
        {
            controller.Move(dir * 10f * Time.deltaTime);
            yield return null;
        }
    }
}
