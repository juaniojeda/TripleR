using UnityEngine;

public sealed class SkinControlledObject : MonoBehaviour
{
    [SerializeField] private RecyclingSkinData skin;
    [SerializeField] private GameObject objectToActivate;
    [SerializeField] private GameObject objectToDeactivate;

    private void Awake()
    {
        Apply();
    }

    public void Apply()
    {
        if (skin == null)
        {
            Debug.LogError($"{name}: No tiene skin asignada.");
            return;
        }

        bool unlocked = PlayerProfile.HasSkin(skin.SkinId);

        Debug.Log($"{name}: skin {skin.SkinId} comprada = {unlocked}");

        if (objectToActivate != null)
            objectToActivate.SetActive(unlocked);
        else
            Debug.LogError($"{name}: No tiene objectToActivate asignado.");

        if (objectToDeactivate != null)
            objectToDeactivate.SetActive(!unlocked);
        else
            Debug.LogError($"{name}: No tiene objectToDeactivate asignado.");
    }
}
