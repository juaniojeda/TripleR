using System.Collections;
using UnityEngine;

public sealed class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private VRPoolManager poolManager;
    [SerializeField] private PoolItemData[] spawnableItems;
    [SerializeField] private Transform[] spawnPoints;

    [SerializeField, Min(0.1f)] private float spawnInterval = 2f;
    [SerializeField] private bool spawnOnStart = true;

    [SerializeField] private bool randomItem = true;
    [SerializeField] private bool randomPoint = true;

    private Coroutine spawnRoutine;
    private int currentItemIndex;
    private int currentPointIndex;

    private void OnEnable()
    {
        if (spawnOnStart)
            StartSpawning();
    }

    private void OnDisable()
    {
        StopSpawning();
    }

    public void StartSpawning()
    {
        if (spawnRoutine != null)
            return;

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (spawnRoutine == null)
            return;

        StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    public void Spawn()
    {
        if (poolManager == null)
            return;

        PoolItemData itemData = GetNextItem();

        if (itemData == null)
            return;

        Transform point = GetNextSpawnPoint();

        Vector3 position = point != null ? point.position : transform.position;
        Quaternion rotation = point != null ? point.rotation : transform.rotation;

        poolManager.TryGet(itemData.Id, position, rotation, out _);
    }

    private IEnumerator SpawnLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(spawnInterval);

        while (true)
        {
            Spawn();
            yield return wait;
        }
    }

    private PoolItemData GetNextItem()
    {
        if (spawnableItems == null || spawnableItems.Length == 0)
            return null;

        if (randomItem)
        {
            int index = Random.Range(0, spawnableItems.Length);
            return spawnableItems[index];
        }

        PoolItemData itemData = spawnableItems[currentItemIndex];

        currentItemIndex++;

        if (currentItemIndex >= spawnableItems.Length)
            currentItemIndex = 0;

        return itemData;
    }

    private Transform GetNextSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return transform;

        if (randomPoint)
        {
            int index = Random.Range(0, spawnPoints.Length);
            return spawnPoints[index];
        }

        Transform point = spawnPoints[currentPointIndex];

        currentPointIndex++;

        if (currentPointIndex >= spawnPoints.Length)
            currentPointIndex = 0;

        return point;
    }
}