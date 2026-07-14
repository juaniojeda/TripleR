using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class MainMenuTrashController : MonoBehaviour
{
    [SerializeField] private GameObject[] trashObjects;
    [SerializeField] private bool autoFindTrashObjectsWhenEmpty = true;
    [SerializeField] private string autoFindNameContains = "trash";

    private void Awake()
    {
        if (ShouldAutoFindTrashObjects())
            AutoFindTrashObjects();
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (trashObjects == null)
            return;

        int storedTrashLevel = PlayerPrefs.GetInt(PerformanceSessionStorage.TrashLevelKey, 0);
        int trashLevel = Mathf.Clamp(storedTrashLevel, 0, trashObjects.Length);

        if (trashLevel != storedTrashLevel)
        {
            PlayerPrefs.SetInt(PerformanceSessionStorage.TrashLevelKey, trashLevel);
            PlayerPrefs.Save();
        }

        for (int i = 0; i < trashObjects.Length; i++)
        {
            if (trashObjects[i] != null)
                trashObjects[i].SetActive(i < trashLevel);
        }
    }

    public void SetTrashLevel(int trashLevel)
    {
        int maxLevel = trashObjects != null ? trashObjects.Length : 0;
        PlayerPrefs.SetInt(PerformanceSessionStorage.TrashLevelKey, Mathf.Clamp(trashLevel, 0, maxLevel));
        PlayerPrefs.Save();
        Refresh();
    }

    private bool ShouldAutoFindTrashObjects()
    {
        return autoFindTrashObjectsWhenEmpty
            && (trashObjects == null || trashObjects.Length == 0)
            && !string.IsNullOrWhiteSpace(autoFindNameContains);
    }

    private void AutoFindTrashObjects()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        List<GameObject> foundObjects = new List<GameObject>();

        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject candidate = allObjects[i];

            if (candidate == null)
                continue;

            if (candidate.scene != gameObject.scene)
                continue;

            if (candidate.hideFlags != HideFlags.None)
                continue;

            if (candidate.name.IndexOf(autoFindNameContains, StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            foundObjects.Add(candidate);
        }

        foundObjects.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
        trashObjects = foundObjects.ToArray();
    }
}

