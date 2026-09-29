using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public enum State { IDLE, WINDUP, TACKLE, RETREAT, COOLDOWN }
    public State currentState = State.IDLE;
    
    public bool isTough = false;
    private int hp;
    private float speed;
    private int damage;

    private Transform player;
    private float stateTimer = 0;
    public string poolTag = "weak";

    private void OnEnable()
    {
        hp = isTough ? 3 : 1;
        speed = isTough ? 2.0f : 2.5f;
        damage = isTough ? 2 : 1;
        currentState = State.IDLE;
        player = GameObject.FindWithTag("Player")?.transform;
        
        // Ensure flocking manager exists before registering
        if (FlockManager.Instance != null)
            FlockManager.Instance.Register(transform);
    }

    private void OnDisable()
    {
        if (FlockManager.Instance != null) FlockManager.Instance.Unregister(transform);
    }

    private void Update()
    {
        if (player == null) return;

        stateTimer -= Time.deltaTime;

        switch (currentState)
        {
            case State.IDLE:
                // Steer towards player (Flocking simplified to direct seek for ponytail)
                transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
                if (Vector3.Distance(transform.position, player.position) < 3f)
                {
                    ChangeState(State.WINDUP, 0.3f);
                }
                break;
            case State.WINDUP:
                transform.LookAt(player);
                if (stateTimer <= 0) ChangeState(State.TACKLE, 0.2f);
                break;
            case State.TACKLE:
                transform.position += transform.forward * 7.5f * Time.deltaTime;
                if (stateTimer <= 0) ChangeState(State.RETREAT, 0.4f);
                // Tackle collision should check distance to player
                if (Vector3.Distance(transform.position, player.position) < 1.5f)
                {
                    player.SendMessage("TakeDamage", new object[] { damage, transform.position }, SendMessageOptions.DontRequireReceiver);
                }
                break;
            case State.RETREAT:
                transform.position -= transform.forward * 6.25f * Time.deltaTime;
                if (stateTimer <= 0) ChangeState(State.COOLDOWN, 1.5f);
                break;
            case State.COOLDOWN:
                if (stateTimer <= 0) ChangeState(State.IDLE, 0f);
                break;
        }
    }

    private void ChangeState(State newState, float timer)
    {
        currentState = newState;
        stateTimer = timer;
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        StartCoroutine(FlashWhite());
        if (hp <= 0)
        {
            DropManager.Instance.HandleDrops(transform.position);
            PoolManager.Instance.Despawn(poolTag, gameObject);
        }
    }

    private IEnumerator FlashWhite()
    {
        // Simple placeholder for squash/flash
        transform.localScale = new Vector3(1.2f, 0.8f, 1.2f);
        yield return new WaitForSeconds(0.1f);
        transform.localScale = Vector3.one;
    }
}
