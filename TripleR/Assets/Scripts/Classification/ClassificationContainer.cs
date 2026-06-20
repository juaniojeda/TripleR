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
        {
            points = isCorrect
                ? scoreManager.AddCorrectClassification()
                : scoreManager.AddWrongClassification();
        }

        PlayResultSound(isCorrect);

        if (isCorrect)
        {
            OnCorrectClassification?.Invoke(other.transform.position);
        }

        Debug.Log(isCorrect
            ? $"Correcto: {data.Id} en {acceptedCategoryId}. +{points}"
            : $"Incorrecto: {data.Id} no pertenece a {acceptedCategoryId}. {points}");

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

        Debug.Log(isCorrect ? "Sonido correcto Wwise" : "Sonido incorrecto Wwise");

        selectedEvent.Post(gameObject);
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
//using System;
//using UnityEngine;

//public sealed class ClassificationContainer : MonoBehaviour
//{
//    public event Action<Vector3> OnCorrectClassification;

//    [SerializeField] private string acceptedCategoryId;
//    [SerializeField] private LayerMask classifiableLayers;
//    [SerializeField] private ScoreManager scoreManager;
//    [SerializeField] private bool returnToPoolAfterClassify = true;

//    [Header("Wwise")]
//    [SerializeField] private AK.Wwise.Event correctEvent;
//    [SerializeField] private AK.Wwise.Event wrongEvent;

//    private void OnTriggerEnter(Collider other)
//    {
//        Rigidbody rb = other.attachedRigidbody;

//        if (rb == null)
//            return;

//        if (!IsLayerAccepted(rb.gameObject.layer))
//            return;

//        PoolableObject poolableObject = rb.GetComponent<PoolableObject>();

//        if (poolableObject == null)
//            poolableObject = rb.GetComponentInParent<PoolableObject>();

//        if (poolableObject == null)
//            return;

//        PoolItemData data = poolableObject.Data;

//        if (data == null)
//            return;

//        bool isCorrect = IsCorrectCategory(data);
//        int points = isCorrect ? data.CorrectPoints : data.WrongPoints;

//        if (scoreManager != null)
//            scoreManager.AddScore(points);

//        PlayResultSound(isCorrect);

//        if (isCorrect)
//        {
//            OnCorrectClassification?.Invoke(other.transform.position);
//        }

//        Debug.Log(isCorrect
//            ? $"Correcto: {data.Id} en {acceptedCategoryId}. +{points}"
//            : $"Incorrecto: {data.Id} no pertenece a {acceptedCategoryId}. {points}");

//        if (returnToPoolAfterClassify)
//            poolableObject.ReturnToPool();
//    }

//    private void PlayResultSound(bool isCorrect)
//    {
//        AK.Wwise.Event selectedEvent = isCorrect ? correctEvent : wrongEvent;

//        if (selectedEvent == null || !selectedEvent.IsValid())
//        {
//            Debug.LogWarning("Evento Wwise no asignado o inválido.");
//            return;
//        }

//        Debug.Log(isCorrect ? "Sonido correcto Wwise" : "Sonido incorrecto Wwise");

//        selectedEvent.Post(gameObject);
//    }

//    private bool IsCorrectCategory(PoolItemData data)
//    {
//        return string.Equals(data.CategoryId, acceptedCategoryId, StringComparison.OrdinalIgnoreCase);
//    }

//    private bool IsLayerAccepted(int layer)
//    {
//        return (classifiableLayers.value & (1 << layer)) != 0;
//    }
//}