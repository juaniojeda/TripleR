using UnityEngine;

public sealed class CapacityManager : MonoBehaviour
{
    [Header("Sistema de Capacidad")]
    [SerializeField, Min(1)] private int maxCapacity = 10;
    [SerializeField] private Transform fillIndicator;
    [SerializeField, Min(0f)] private float heightPerItem = 0.1f;

    [Header("Mecanismo de Palanca")]
    [SerializeField] private Transform leverPivot;
    [SerializeField, Min(0f)] private float emptyAngleThreshold = 40f;

    private int currentItems;
    private Vector3 initialIndicatorLocalPosition;

    public bool IsFull => currentItems >= maxCapacity;

    private void Start()
    {
        if (fillIndicator != null)
        {
            initialIndicatorLocalPosition = fillIndicator.localPosition;
            UpdateVisualIndicator();
        }
    }

    private void Update()
    {
        CheckLeverAngle();
    }

    public bool TryClassify(bool isCorrectCategory)
    {
        if (!isCorrectCategory || IsFull)
            return false;

        currentItems++;
        UpdateVisualIndicator();
        return true;
    }

    private void CheckLeverAngle()
    {
        if (leverPivot == null || currentItems == 0)
            return;

        float rawAngle = leverPivot.localEulerAngles.x;
        float normalizedAngle = rawAngle > 180f ? rawAngle - 360f : rawAngle;

        if (Mathf.Abs(normalizedAngle) >= emptyAngleThreshold)
            EmptyContainer();
    }

    private void EmptyContainer()
    {
        currentItems = 0;
        UpdateVisualIndicator();
    }

    private void UpdateVisualIndicator()
    {
        if (fillIndicator == null)
            return;

        bool show = currentItems >= 1;
        fillIndicator.gameObject.SetActive(show);

        if (show)
            fillIndicator.localPosition = initialIndicatorLocalPosition + Vector3.up * (currentItems * heightPerItem);
        else
            fillIndicator.localPosition = initialIndicatorLocalPosition;
    }
}
