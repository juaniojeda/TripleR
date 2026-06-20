using UnityEngine;

public sealed class LicenseControlledObject : MonoBehaviour
{
    [SerializeField] private string licenseId;
    [SerializeField] private GameObject targetObject;

    private void Awake()
    {
        Apply();
    }

    private void OnEnable()
    {
        Apply();
    }

    public void Apply()
    {
        if (targetObject == null)
            return;

        bool unlocked = PlayerProfile.HasLicense(licenseId);
        targetObject.SetActive(unlocked);
    }
}