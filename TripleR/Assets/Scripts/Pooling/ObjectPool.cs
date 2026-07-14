using System.Collections.Generic;
using UnityEngine;

public sealed class ObjectPool
{
    private readonly PoolItemData data;
    private readonly VRPoolManager manager;
    private readonly Transform parent;
    private readonly Queue<PoolableObject> availableObjects = new Queue<PoolableObject>();

    private int totalCreated;
    public string Id => data.Id;

    public ObjectPool(PoolItemData data, VRPoolManager manager, Transform parent)
    {
        this.data = data;
        this.manager = manager;
        this.parent = parent;

        CreateInitialObjects();
    }

    public bool TryGet(Vector3 position, Quaternion rotation, out PoolableObject poolableObject)
    {
        poolableObject = null;

        if (availableObjects.Count <= 0)
        {
            if (!CanCreateMore())
                return false;

            CreateObject();
        }

        poolableObject = availableObjects.Dequeue();
        poolableObject.PrepareForSpawn(position, rotation);
        return true;
    }

    public void Return(PoolableObject poolableObject)
    {
        poolableObject.Deactivate();
        poolableObject.transform.SetParent(parent);
        availableObjects.Enqueue(poolableObject);
    }

    private void CreateInitialObjects()
    {
        for (int i = 0; i < data.InitialPoolSize; i++)
        {
            CreateObject();
        }
    }

    private bool CanCreateMore()
    {
        return data.CanExpand && totalCreated < data.MaxPoolSize;
    }

    private void CreateObject()
    {
        GameObject instance = Object.Instantiate(data.Prefab, parent);
        instance.SetActive(false);

        PoolableObject poolableObject = instance.GetComponent<PoolableObject>();

        if (poolableObject == null)
        {
            poolableObject = instance.AddComponent<PoolableObject>();
        }

        poolableObject.Initialize(manager, data);
        availableObjects.Enqueue(poolableObject);

        totalCreated++;
    }
}
