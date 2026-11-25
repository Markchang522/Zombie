using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieWaypointPatrol : MonoBehaviour
{
    public enum PatrolMode
    {
        Loop,      // 走到最後一點後回到起點循環
        PingPong   // 走到最後一點後反向折返
    }

    [Header("Waypoint Settings")]
    [Tooltip("這隻殭屍要走的固定路徑點")]
    public Transform[] waypoints;
    [Tooltip("路線循環方式")]
    public PatrolMode patrolMode = PatrolMode.Loop;
    [Tooltip("判定已抵達路徑點的距離門檻")]
    public float arrivalThreshold = 0.5f;
    [Tooltip("抵達每個路徑點後的停留秒數")]
    public float waitTimeAtPoint = 0.5f;

    private NavMeshAgent agent;
    private int currentIndex = 0;
    private int direction = 1;
    private float waitTimer = 0f;
    private bool isWaiting = false;

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

        if (agent != null && agent.enabled && agent.isOnNavMesh && waypoints != null && waypoints.Length > 0)
        {
            MoveToCurrentWaypoint();
        }
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0 || agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (!agent.pathPending && agent.remainingDistance <= arrivalThreshold)
        {
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
        else // PingPong
        {
            if (currentIndex == waypoints.Length - 1)
                direction = -1;
            else if (currentIndex == 0)
                direction = 1;

            currentIndex += direction;
        }
    }

    void MoveToCurrentWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        currentIndex = Mathf.Clamp(currentIndex, 0, waypoints.Length - 1);
        Transform target = waypoints[currentIndex];
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }
}