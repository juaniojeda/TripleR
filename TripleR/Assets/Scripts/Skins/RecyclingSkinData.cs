using UnityEngine;

[CreateAssetMenu(
    fileName = "New Recycling Skin",
    menuName = "Recycling Game/Recycling Skin"
)]
public sealed class RecyclingSkinData : ScriptableObject
{
    [SerializeField] private string skinId;
    [SerializeField, Min(0)] private int price;

    public string SkinId => skinId;
    public int Price => price;
}
