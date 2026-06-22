using Meta.XR.MRUtilityKit.SceneDecorator;
using UnityEngine;

public sealed class PoolableObject : MonoBehaviour
{
    [SerializeField] private PoolItemData data;

    private VRPoolManager poolManager;
    private Rigidbody rb;
    private WasteReleaseMagnetListener magnetListener;

    public PoolItemData Data => data;
    public string Id => data != null ? data.Id : string.Empty;
    public Rigidbody Rigidbody => rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        EnsureMagnetListener();
    }

    public void Initialize(VRPoolManager manager, PoolItemData itemData)
    {
        poolManager = manager;
        data = itemData;
        EnsureMagnetListener();
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
        magnetListener?.ResetForSpawn();
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
        magnetListener?.ResetForPool();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = true;
        }

        gameObject.SetActive(false);
    }

    private void EnsureMagnetListener()
    {
        if (magnetListener == null)
            magnetListener = GetComponent<WasteReleaseMagnetListener>();

        if (magnetListener == null)
            magnetListener = gameObject.AddComponent<WasteReleaseMagnetListener>();

        magnetListener.Initialize(this);
    }
}
