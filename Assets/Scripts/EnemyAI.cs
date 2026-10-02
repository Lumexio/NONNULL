using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class EnemyAI : MonoBehaviour
{
    [HideInInspector] public string poolTag = "";

    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float gravity = -20f;
    public bool isTough = false;

    public Vector3 flockingSteer = Vector3.zero;
    public bool isAttacker = false;

    private CharacterController cc;
    private Animator anim;
    private Transform player;

    private float attackTimer;
    private float verticalVelocity;
    private bool isDead;

    public enum State
    {
        IDLE,
        WALK,
        ATTACK,
        HIT,
        DEAD
    }

    public State state = State.IDLE;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (cc == null)
        {
            cc = gameObject.AddComponent<CharacterController>();
        }
        anim = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        isDead = false;
        state = State.WALK;
        attackTimer = 0f;
        verticalVelocity = 0f;
    }

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            player = p.transform;
        }
        state = State.WALK;
    }

    private void Update()
    {
        if (isDead || player == null)
        {
            return;
        }

        verticalVelocity += gravity * Time.deltaTime;
        float dist = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case State.WALK:
                MoveTowardPlayer(dist);
                break;

            case State.ATTACK:
                HandleAttack(dist);
                break;

            case State.HIT:
                HandleHitReaction(dist);
                break;

            case State.DEAD:
                HandleDeath(dist);
                break;

            case State.IDLE:
                break;
        }
    }

    private void MoveTowardPlayer(float dist)
    {
        if (dist <= attackRange)
        {
            state = State.ATTACK;
            if (anim != null)
            {
                anim.SetFloat("Speed", 0f);
            }
            return;
        }

        float speed = isTough ? runSpeed : walkSpeed;
        Vector3 dir = (player.position - transform.position).normalized;

        if (flockingSteer.sqrMagnitude > 0.01f)
        {
            dir = (dir + flockingSteer * 0.3f).normalized;
        }

        dir.y = 0f;

        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 8f
            );
        }

        Vector3 move = dir * speed;
        move.y = verticalVelocity;
        cc.Move(move * Time.deltaTime);

        if (anim != null)
        {
            anim.SetFloat("Speed", speed);
        }
    }

    private void HandleAttack(float dist)
    {
        if (dist > attackRange + 0.5f)
        {
            state = State.WALK;
            return;
        }

        if (anim != null)
        {
            anim.SetFloat("Speed", 0f);
        }

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;
            if (anim != null)
            {
                anim.SetTrigger("Attack");
            }
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.TakeDamage(attackDamage);
            }
        }
    }

    private void HandleHitReaction(float dist)
    {
        if (isDead)
        {
            return;
        }

        state = State.HIT;
        if (anim != null)
        {
            anim.SetTrigger("Hit");
        }
        StartCoroutine(HitReactionRoutine());
    }

    private IEnumerator HitReactionRoutine()
    {
        yield return new WaitForSeconds(0.4f);
        if (!isDead)
        {
            state = State.WALK;
        }
    }

    private void HandleDeath(float dist)
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        state = State.DEAD;
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }
        StartCoroutine(DisableAfterSeconds(3f));
    }

    private IEnumerator DisableAfterSeconds(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}