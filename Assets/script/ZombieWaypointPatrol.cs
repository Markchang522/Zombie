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

    [Header("Waypoint Settings")]
    public Transform[] waypoints;
    public PatrolMode patrolMode = PatrolMode.StopAtEnd;
    public float arrivalThreshold = 0.5f;
    public float waitTimeAtPoint = 0.5f;

    private NavMeshAgent agent;
    private int currentIndex = 0;
    private int direction = 1;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private bool isFinished = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            MoveToCurrentWaypoint();
        }
    }

    public void SetPath(Transform[] newWaypoints)
    {
        waypoints = newWaypoints;
        currentIndex = 0;
        direction = 1;
        isWaiting = false;
        waitTimer = 0f;
        isFinished = false;

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
        if (isFinished || waypoints == null || waypoints.Length == 0 || agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (!agent.pathPending && agent.remainingDistance <= arrivalThreshold)
        {
            // 若是 StopAtEnd 且已經在最後一個點
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
        if (waypoints == null || waypoints.Length == 0 || isFinished) return;

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
        isFinished = true;
        isWaiting = false;
        agent.isStopped = true;
        agent.ResetPath();
    }
}