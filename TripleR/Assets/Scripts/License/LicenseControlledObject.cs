using UnityEngine;

public sealed class LicenseControlledObject : MonoBehaviour
{
    [SerializeField] private RecyclingLicenseData license;
    [SerializeField] private GameObject targetObject;

    private void Awake()
    {
        Apply();
    }

    public void Apply()
    {
        if (license == null)
        {
            Debug.LogError($"{name}: No tiene licencia asignada.");
            return;
        }

        if (targetObject == null)
        {
            Debug.LogError($"{name}: No tiene targetObject asignado.");
            return;
        }

        bool unlocked = PlayerProfile.HasLicense(license.LicenseId);

        Debug.Log($"{name}: licencia {license.LicenseId} comprada = {unlocked}");

        targetObject.SetActive(unlocked);
    }
}