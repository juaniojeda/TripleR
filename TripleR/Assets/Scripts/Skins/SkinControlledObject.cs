using System.Collections.Generic;
using UnityEngine;

public sealed class SkinControlledObject : MonoBehaviour
{
    [System.Serializable]
    private sealed class SkinEntry
    {
        public RecyclingSkinData skin;
        public GameObject objectToActivate;
        public GameObject objectToDeactivate;
    }

    [SerializeField] private SkinEntry[] skinEntries;

    [SerializeField, HideInInspector] private RecyclingSkinData skin;
    [SerializeField, HideInInspector] private GameObject objectToActivate;
    [SerializeField, HideInInspector] private GameObject objectToDeactivate;

    private void Awake()
    {
        Apply();
    }

    public void Apply()
    {
        Dictionary<GameObject, bool> deactivateTargets = new Dictionary<GameObject, bool>();

        ApplyEntry(skin, objectToActivate, objectToDeactivate, deactivateTargets);

        if (skinEntries == null)
        {
            ApplyDeactivateTargets(deactivateTargets);
            return;
        }

        for (int i = 0; i < skinEntries.Length; i++)
        {
            SkinEntry entry = skinEntries[i];

            if (entry == null)
                continue;

            ApplyEntry(entry.skin, entry.objectToActivate, entry.objectToDeactivate, deactivateTargets);
        }

        ApplyDeactivateTargets(deactivateTargets);
    }

    private void ApplyEntry(
        RecyclingSkinData skinData,
        GameObject activateTarget,
        GameObject deactivateTarget,
        Dictionary<GameObject, bool> deactivateTargets)
    {
        if (skinData == null)
            return;

        bool active = PlayerProfile.IsSkinActive(skinData.SkinId);

        Debug.Log($"{name}: skin {skinData.SkinId} activa = {active}");

        if (activateTarget != null)
            activateTarget.SetActive(active);
        else
            Debug.LogError($"{name}: No tiene objectToActivate asignado.");

        if (deactivateTarget != null)
            deactivateTargets[deactivateTarget] = IsTargetMarked(deactivateTargets, deactivateTarget) || active;
        else
            Debug.LogError($"{name}: No tiene objectToDeactivate asignado.");
    }

    private static bool IsTargetMarked(Dictionary<GameObject, bool> deactivateTargets, GameObject target)
    {
        return deactivateTargets.TryGetValue(target, out bool marked) && marked;
    }

    private static void ApplyDeactivateTargets(Dictionary<GameObject, bool> deactivateTargets)
    {
        foreach (KeyValuePair<GameObject, bool> entry in deactivateTargets)
        {
            if (entry.Key != null)
                entry.Key.SetActive(!entry.Value);
        }
    }
}
