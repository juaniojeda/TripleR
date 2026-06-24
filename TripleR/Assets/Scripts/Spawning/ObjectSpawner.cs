using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private TutorialPagesUI tutorialUI;

    [Header("Pool")]
    [SerializeField] private VRPoolManager poolManager;

    [Header("Score")]
    [SerializeField] private ScoreManager scoreManager;

    [Header("Items Base")]
    [SerializeField] private PoolItemData[] spawnableItems;

    [Header("Items Desbloqueables")]
    [SerializeField] private RecyclingLicenseData[] unlockedLicenses;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Settings")]
    [SerializeField, Min(0.1f)] private float spawnInterval = 2f;
    [SerializeField] private bool spawnOnStart = false;

    [SerializeField] private bool randomItem = true;
    [SerializeField] private bool randomPoint = true;

    private readonly List<PoolItemData> activeItems = new List<PoolItemData>(16);

    private Coroutine spawnRoutine;
    private int currentItemIndex;
    private int currentPointIndex;

    private void OnEnable()
    {
        RebuildActiveItems();

        if (spawnOnStart)
            StartSpawning();

        if (tutorialUI != null)
        {
            tutorialUI.OnTutorialFinished += TurnOn;
        }
    }

    private void OnDisable()
    {
        StopSpawning();

        if (tutorialUI != null)
        {
            tutorialUI.OnTutorialFinished -= TurnOn;
        }
    }

    public void TurnOn()
    {
        spawnOnStart = true;
        StartSpawning();
    }

    public void RebuildActiveItems()
    {
        activeItems.Clear();

        AddItems(spawnableItems);

        if (unlockedLicenses != null)
        {
            for (int i = 0; i < unlockedLicenses.Length; i++)
            {
                RecyclingLicenseData license = unlockedLicenses[i];

                if (license == null)
                    continue;

                if (!PlayerProfile.HasLicense(license.LicenseId))
                    continue;

                AddItems(license.UnlockedItems);
            }
        }

        currentItemIndex = 0;
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

        if (poolManager.TryGet(itemData.Id, position, rotation, out _))
            scoreManager?.RegisterWasteGenerated();
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

    private void AddItems(PoolItemData[] items)
    {
        if (items == null)
            return;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
                activeItems.Add(items[i]);
        }
    }

    private PoolItemData GetNextItem()
    {
        if (activeItems.Count == 0)
            return null;

        if (randomItem)
        {
            int index = Random.Range(0, activeItems.Count);
            return activeItems[index];
        }

        PoolItemData itemData = activeItems[currentItemIndex];

        currentItemIndex++;

        if (currentItemIndex >= activeItems.Count)
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