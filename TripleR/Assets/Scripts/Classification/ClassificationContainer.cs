using System;
using UnityEngine;

public sealed class ClassificationContainer : MonoBehaviour
{
    [SerializeField] private string acceptedCategoryId;
    [SerializeField] private LayerMask classifiableLayers;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private bool returnToPoolAfterClassify = true;

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;

        if (rb == null)
            return;

        if (!IsLayerAccepted(rb.gameObject.layer))
            return;

        PoolableObject poolableObject = rb.GetComponent<PoolableObject>();

        if (poolableObject == null)
            poolableObject = rb.GetComponentInParent<PoolableObject>();

        if (poolableObject == null)
            return;

        PoolItemData data = poolableObject.Data;

        if (data == null)
            return;

        bool isCorrect = IsCorrectCategory(data);
        int points = isCorrect ? data.CorrectPoints : data.WrongPoints;

        if (scoreManager != null)
            scoreManager.AddScore(points);

        Debug.Log(isCorrect
            ? $"Correcto: {data.Id} en {acceptedCategoryId}. +{points}"
            : $"Incorrecto: {data.Id} no pertenece a {acceptedCategoryId}. {points}");

        if (returnToPoolAfterClassify)
            poolableObject.ReturnToPool();
    }

    private bool IsCorrectCategory(PoolItemData data)
    {
        return string.Equals(data.CategoryId, acceptedCategoryId, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsLayerAccepted(int layer)
    {
        return (classifiableLayers.value & (1 << layer)) != 0;
    }
}