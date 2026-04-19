using UnityEngine;

[CreateAssetMenu(fileName = "PoolItemData_", menuName = "VR/Pooling/Pool Item Data")]
public sealed class PoolItemData : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private GameObject prefab;
    [SerializeField, Min(1)] private int initialPoolSize = 5;
    [SerializeField, Min(1)] private int maxPoolSize = 5;
    [SerializeField] private bool canExpand;

    public string Id => id;
    public GameObject Prefab => prefab;
    public int InitialPoolSize => initialPoolSize;
    public int MaxPoolSize => maxPoolSize;
    public bool CanExpand => canExpand;
}
