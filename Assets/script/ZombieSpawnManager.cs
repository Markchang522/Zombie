using System.Collections;
using UnityEngine;

public class ZombieSpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class SpawnRoute
    {
        public string routeName = "Route";
        [Tooltip("這個路線的殭屍生成點")]
        public Transform spawnPoint;
        [Tooltip("這個生成點對應的固定路徑點（依順序排好）")]
        public Transform[] waypoints;
    }

    public enum SpawnOrderMode
    {
        RandomRoute, // 每次隨機挑 1 組（生成點 + 對應路徑）
        Sequential   // 依序輪流 0 -> 1 -> 2 -> 0
    }

    [Header("Zombie Prefab")]
    [Tooltip("殭屍 Prefab（需包含 NavMeshAgent 與 ZombieWaypointPatrol）")]
    public GameObject zombiePrefab;

    [Header("3 Spawn Points & 3 Paths")]
    [Tooltip("建議設定 3 組，每組包含 1 個生成點與 1 條對應路徑")]
    public SpawnRoute[] spawnRoutes;
    public SpawnOrderMode orderMode = SpawnOrderMode.RandomRoute;

    [Header("Auto Spawn Settings")]
    public bool autoSpawn = true;
    public float spawnInterval = 5f;
    public int maxActiveZombies = 12;

    private int nextRouteIndex = 0;

    void Start()
    {
        if (autoSpawn)
        {
            StartCoroutine(SpawnLoop());
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (FindObjectsByType<ZombieWaypointPatrol>(FindObjectsSortMode.None).Length < maxActiveZombies)
            {
                SpawnZombie();
            }
        }
    }

    public void SpawnZombie()
    {
        if (zombiePrefab == null || spawnRoutes == null || spawnRoutes.Length == 0) return;

        int chosenIndex = 0;
        if (orderMode == SpawnOrderMode.RandomRoute)
        {
            chosenIndex = Random.Range(0, spawnRoutes.Length);
        }
        else
        {
            chosenIndex = nextRouteIndex % spawnRoutes.Length;
            nextRouteIndex++;
        }

        SpawnRoute route = spawnRoutes[chosenIndex];
        if (route == null || route.spawnPoint == null) return;

        GameObject zombieObj = Instantiate(zombiePrefab, route.spawnPoint.position, route.spawnPoint.rotation);
        ZombieWaypointPatrol patrol = zombieObj.GetComponent<ZombieWaypointPatrol>();
        if (patrol != null)
        {
            patrol.SetPath(route.waypoints);
        }
    }
}