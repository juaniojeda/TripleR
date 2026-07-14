using UnityEngine;

[CreateAssetMenu(fileName = "PoolItemData_", menuName = "VR/Pooling/Pool Item Data")]
public sealed class PoolItemData : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string categoryId;

    [SerializeField] private GameObject prefab;

    [SerializeField, Min(1)] private int initialPoolSize = 5;
    [SerializeField, Min(1)] private int maxPoolSize = 5;
    [SerializeField] private bool canExpand;

    [SerializeField] private int correctPoints = 10;
    [SerializeField] private int wrongPoints = -5;

    public string Id => id;
    public string CategoryId => categoryId;
    public GameObject Prefab => prefab;
    public int InitialPoolSize => initialPoolSize;
    public int MaxPoolSize => maxPoolSize;
    public bool CanExpand => canExpand;
    public int CorrectPoints => correctPoints;
    public int WrongPoints => wrongPoints;

    private void OnValidate()
    {
        if (maxPoolSize < initialPoolSize)
            maxPoolSize = initialPoolSize;
    }
}
