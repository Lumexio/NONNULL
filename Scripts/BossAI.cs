using UnityEngine;

public class BossAI : MonoBehaviour
{
    public enum State { BURST, OVERHEAT, COOLDOWN }
    public State currentState = State.BURST;
    
    private Transform player;
    private float stateTimer = 3.0f;
    private float shotTimer = 0.5f;
    private int hp;

    private void OnEnable()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        int round = LevelManager.Instance != null ? LevelManager.Instance.currentRound : 1;
        hp = 200 + (round * 20);
        currentState = State.BURST;
        stateTimer = 3.0f;
    }

    private void Update()
    {
        if (player == null) return;
        stateTimer -= Time.deltaTime;

        switch (currentState)
        {
            case State.BURST:
                transform.LookAt(player);
                shotTimer -= Time.deltaTime;
                if (shotTimer <= 0)
                {
                    FireBurst();
                    shotTimer = 0.5f;
                }
                if (stateTimer <= 0)
                {
                    currentState = State.OVERHEAT;
                    stateTimer = 2.0f;
                    // Material turns red
                }
                break;
            case State.OVERHEAT:
                if (stateTimer <= 0)
                {
                    currentState = State.COOLDOWN;
                    stateTimer = 1.0f;
                    // Material turns gray
                }
                break;
            case State.COOLDOWN:
                if (stateTimer <= 0)
                {
                    currentState = State.BURST;
                    stateTimer = 3.0f;
                    // Material turns normal
                }
                break;
        }
    }

    private void FireBurst()
    {
        for (int i = -2; i <= 2; i++)
        {
            GameObject proj = PoolManager.Instance.Spawn("projectile", transform.position + transform.forward, Quaternion.Euler(0, i * 15f, 0) * transform.rotation);
        }
    }

    public void TakeDamage(int dmg)
    {
        if (currentState != State.OVERHEAT) return; // Vulnerable only in OVERHEAT
        
        hp -= dmg;
        if (hp <= 0)
        {
            SaveManager.Instance.Coins += 50;
            Destroy(gameObject);
        }
    }
}
