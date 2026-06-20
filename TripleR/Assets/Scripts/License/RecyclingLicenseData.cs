using UnityEngine;

[CreateAssetMenu(
    fileName = "New Recycling License",
    menuName = "Recycling Game/Recycling License"
)]
public sealed class RecyclingLicenseData : ScriptableObject
{
    [SerializeField] private string licenseId;
    [SerializeField, Min(0)] private int price;
    [SerializeField] private PoolItemData[] unlockedItems;

    public string LicenseId => licenseId;
    public int Price => price;
    public PoolItemData[] UnlockedItems => unlockedItems;
}