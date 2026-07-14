using System;
using UnityEngine;

public sealed class ClassificationContainer : MonoBehaviour
{
    public event Action<Vector3> OnCorrectClassification;

    [SerializeField] private string acceptedCategoryId;
    [SerializeField] private LayerMask classifiableLayers;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private bool returnToPoolAfterClassify = true;

    [Header("Wwise")]
    [SerializeField] private AK.Wwise.Event correctEvent;
    [SerializeField] private AK.Wwise.Event wrongEvent;

    private CapacityManager capacityManager;

    private void Awake()
    {
        capacityManager = GetComponentInChildren<CapacityManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;
        if (!IsLayerAccepted(rb.gameObject.layer)) return;

        PoolableObject poolableObject = rb.GetComponent<PoolableObject>();
        if (poolableObject == null) poolableObject = rb.GetComponentInParent<PoolableObject>();
        if (poolableObject == null) return;

        PoolItemData data = poolableObject.Data;
        if (data == null) return;

        bool isCorrectCategory = IsCorrectCategory(data);

        bool isSuccessful;
        if (capacityManager != null)
        {
            isSuccessful = capacityManager.TryClassify(isCorrectCategory);
        }
        else
        {
            isSuccessful = isCorrectCategory;
        }

        if (scoreManager != null)
            scoreManager.AddClassificationResult(isSuccessful, data.CorrectPoints, data.WrongPoints);

        PlayResultSound(isSuccessful);

        if (isSuccessful)
            OnCorrectClassification?.Invoke(other.transform.position);

        if (returnToPoolAfterClassify)
            poolableObject.ReturnToPool();
    }

    private void PlayResultSound(bool isCorrect)
    {
        AK.Wwise.Event selectedEvent = isCorrect ? correctEvent : wrongEvent;
        if (selectedEvent == null || !selectedEvent.IsValid())
        {
            Debug.LogWarning("Evento Wwise no asignado o inválido.");
            return;
        }
        selectedEvent.Post(gameObject);
    }

    private bool IsCorrectCategory(PoolItemData data) =>
        string.Equals(data.CategoryId, acceptedCategoryId, StringComparison.OrdinalIgnoreCase);

    private bool IsLayerAccepted(int layer) =>
        (classifiableLayers.value & (1 << layer)) != 0;
}
