using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieWaypointPatrol : MonoBehaviour
{
    public enum PatrolMode
    {
        Loop,       // 走到最後一點後回到起點循環
        PingPong,   // 走到最後一點後反向折返
        StopAtEnd   // 走到最後一點後就停下來
    }

    public enum AIState
    {
        Patrol,     // 沿路徑移動（或停在終點）
        Chase,      // 追擊玩家
        Attack,     // 攻擊玩家
        Dead        // 死亡狀態
    }

    [Header("Target & Ranges")]
    public Transform player;
    public float chaseDistance = 10f;
    public float attackDistance = 2f;
    public float loseChaseMultiplier = 1.2f; // 離開追擊距離的緩衝倍率，避免邊界頻繁切換

    [Header("Attack Settings")]
    public int damage = 10;
    public float attackCooldown = 2f;
    public float attackDelay = 1.5f;
    private float lastAttackTime;
    private bool isAttacking = false;

    [Header("Waypoint Settings")]
    public Transform[] waypoints;
    public PatrolMode patrolMode = PatrolMode.StopAtEnd;
    public float arrivalThreshold = 0.5f;
    public float waitTimeAtPoint = 0.5f;

    [Header("Current State (Read Only / Debug)")]
    public AIState currentState = AIState.Patrol;

    private NavMeshAgent agent;
    private PlayerHealth playerHealth;
    private ZombieHealth zombieHealth;
    private int currentIndex = 0;
    private int direction = 1;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private bool isFinishedPatrol = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        zombieHealth = GetComponent<ZombieHealth>();
        lastAttackTime = -attackCooldown;
    }

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        if (waypoints != null && waypoints.Length > 0)
        {
            MoveToCurrentWaypoint();
        }
    }

    public void SetPath(Transform[] newWaypoints)
    {
        if (zombieHealth != null && zombieHealth.isDead)
            return;

        waypoints = newWaypoints;
        currentIndex = 0;
        direction = 1;
        isWaiting = false;
        waitTimer = 0f;
        isFinishedPatrol = false;
        isAttacking = false;
        currentState = AIState.Patrol;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            if (agent.isOnNavMesh && waypoints != null && waypoints.Length > 0)
            {
                MoveToCurrentWaypoint();
            }
        }
    }

    void Update()
    {
        if (zombieHealth != null && zombieHealth.isDead)
        {
            currentState = AIState.Dead;
            isAttacking = false;
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
            return;
        }

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (playerHealth != null && playerHealth.isDead)
        {
            agent.isStopped = true;
            agent.ResetPath();
            return;
        }

        UpdateStateTransitions();

        switch (currentState)
        {
            case AIState.Patrol:
                HandlePatrolState();
                break;

            case AIState.Chase:
                HandleChaseState();
                break;

            case AIState.Attack:
                HandleAttackState();
                break;

            case AIState.Dead:
                break;
        }
    }

    void UpdateStateTransitions()
    {
        if (player == null) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (currentState == AIState.Patrol)
        {
            if (distToPlayer <= chaseDistance)
            {
                currentState = AIState.Chase;
                isWaiting = false;
                agent.isStopped = false;
            }
        }
        else if (currentState == AIState.Chase)
        {
            if (distToPlayer <= attackDistance)
            {
                currentState = AIState.Attack;
            }
            else if (distToPlayer > chaseDistance * loseChaseMultiplier)
            {
                currentState = AIState.Patrol;
                isFinishedPatrol = false;
                agent.isStopped = false;
                MoveToCurrentWaypoint();
            }
        }
        else if (currentState == AIState.Attack)
        {
            if (!isAttacking && distToPlayer > attackDistance)
            {
                currentState = AIState.Chase;
                agent.isStopped = false;
            }
        }
    }

    void HandlePatrolState()
    {
        if (isFinishedPatrol || waypoints == null || waypoints.Length == 0)
            return;

        if (!agent.pathPending && agent.remainingDistance <= arrivalThreshold)
        {
            if (patrolMode == PatrolMode.StopAtEnd && currentIndex >= waypoints.Length - 1)
            {
                if (waitTimeAtPoint > 0f && !isWaiting)
                {
                    isWaiting = true;
                    waitTimer = 0f;
                }

                if (waitTimeAtPoint > 0f && waitTimer < waitTimeAtPoint)
                {
                    waitTimer += Time.deltaTime;
                    return;
                }

                StopAtFinalPoint();
                return;
            }

            if (waitTimeAtPoint > 0f)
            {
                if (!isWaiting)
                {
                    isWaiting = true;
                    waitTimer = 0f;
                }

                waitTimer += Time.deltaTime;
                if (waitTimer >= waitTimeAtPoint)
                {
                    isWaiting = false;
                    AdvanceWaypoint();
                    MoveToCurrentWaypoint();
                }
            }
            else
            {
                AdvanceWaypoint();
                MoveToCurrentWaypoint();
            }
        }
    }

    void HandleChaseState()
    {
        if (player == null) return;

        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    void HandleAttackState()
    {
        agent.isStopped = true;
        agent.ResetPath();

        if (player != null)
        {
            Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(lookPos);
        }

        if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
        {
            StartCoroutine(AttackWithDelay());
        }
    }

    private IEnumerator AttackWithDelay()
    {
        isAttacking = true;

        if (playerHealth == null && player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        if (playerHealth != null && !playerHealth.isDead)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"{gameObject.name} dealt {damage} damage to player! Current HP: {playerHealth.currentHealth}");
        }

        // 之後可在此加入流血畫面效果（blood screen effect）

        yield return new WaitForSeconds(attackDelay);

        isAttacking = false;
        lastAttackTime = Time.time;
    }

    void AdvanceWaypoint()
    {
        if (waypoints.Length <= 1) return;

        if (patrolMode == PatrolMode.Loop)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
        }
        else if (patrolMode == PatrolMode.PingPong)
        {
            if (currentIndex == waypoints.Length - 1)
                direction = -1;
            else if (currentIndex == 0)
                direction = 1;

            currentIndex += direction;
        }
        else if (patrolMode == PatrolMode.StopAtEnd)
        {
            if (currentIndex < waypoints.Length - 1)
            {
                currentIndex++;
            }
        }
    }

    void MoveToCurrentWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        currentIndex = Mathf.Clamp(currentIndex, 0, waypoints.Length - 1);
        Transform target = waypoints[currentIndex];
        if (target != null)
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
        }
    }

    void StopAtFinalPoint()
    {
        isFinishedPatrol = true;
        isWaiting = false;
        agent.isStopped = true;
        agent.ResetPath();
    }
}