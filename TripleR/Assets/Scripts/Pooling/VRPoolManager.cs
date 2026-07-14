using System.Collections.Generic;
using UnityEngine;

public sealed class VRPoolManager : MonoBehaviour
{
    [SerializeField] private PoolItemData[] poolItems;
    [SerializeField] private Transform poolRoot;

    private readonly Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();

    private void Awake()
    {
        if (poolRoot == null)
            poolRoot = transform;

        CreatePools();
    }

    public bool TryGet(string id, Vector3 position, Quaternion rotation, out PoolableObject poolableObject)
    {
        poolableObject = null;

        if (string.IsNullOrWhiteSpace(id))
            return false;

        if (!pools.TryGetValue(id, out ObjectPool pool))
            return false;

        return pool.TryGet(position, rotation, out poolableObject);
    }

    public void ReturnToPool(PoolableObject poolableObject)
    {
        if (poolableObject == null)
            return;

        if (!pools.TryGetValue(poolableObject.Id, out ObjectPool pool))
        {
            poolableObject.Deactivate();
            return;
        }

        pool.Return(poolableObject);
    }

    private void CreatePools()
    {
        pools.Clear();

        if (poolItems == null)
            return;

        for (int i = 0; i < poolItems.Length; i++)
        {
            PoolItemData itemData = poolItems[i];

            if (itemData == null)
                continue;

            if (string.IsNullOrWhiteSpace(itemData.Id))
                continue;

            if (itemData.Prefab == null)
                continue;

            if (pools.ContainsKey(itemData.Id))
                continue;

            ObjectPool pool = new ObjectPool(itemData, this, poolRoot);
            pools.Add(itemData.Id, pool);
        }
    }
}
