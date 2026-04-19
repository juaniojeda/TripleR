using Meta.XR.MRUtilityKit.SceneDecorator;
using UnityEngine;

public sealed class PoolableObject : MonoBehaviour
{
    [SerializeField] private PoolItemData data;

    private VRPoolManager poolManager;
    private Rigidbody rb;

    public PoolItemData Data => data;
    public string Id => data != null ? data.Id : string.Empty;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(VRPoolManager manager, PoolItemData itemData)
    {
        poolManager = manager;
        data = itemData;
    }

    public void PrepareForSpawn(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        gameObject.SetActive(true);
    }

    public void ReturnToPool()
    {
        if (poolManager == null)
        {
            gameObject.SetActive(false);
            return;
        }

        poolManager.ReturnToPool(this);
    }

    public void Deactivate()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        gameObject.SetActive(false);
    }
}