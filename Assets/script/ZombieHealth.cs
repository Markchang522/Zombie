using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Collider))]
public class ZombieHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool isDead = false;

    private NavMeshAgent agent;
    private Collider zombieCollider;
    private ZombieWaypointPatrol waypointPatrol;

    void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;

        agent = GetComponent<NavMeshAgent>();
        zombieCollider = GetComponent<Collider>();
        waypointPatrol = GetComponent<ZombieWaypointPatrol>();
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead || damageAmount <= 0) return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // 之後可在此觸發死亡動畫

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        if (zombieCollider != null)
        {
            zombieCollider.enabled = false;
        }

        if (waypointPatrol != null)
        {
            waypointPatrol.enabled = false;
        }

        // 之後可在此增加擊殺分數（Score）

        this.enabled = false;
        Destroy(gameObject, 2f);
    }
}