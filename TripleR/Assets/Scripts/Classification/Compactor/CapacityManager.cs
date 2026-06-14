using UnityEngine;

public sealed class CapacityManager : MonoBehaviour
{
    [Header("Sistema de Capacidad")]
    [SerializeField] private int maxCapacity = 10;
    [SerializeField] private Transform fillIndicator;
    [SerializeField] private float heightPerItem = 0.1f;

    [Header("Mecanismo de Palanca")]
    [SerializeField] private Transform leverPivot;
    [SerializeField] private float emptyAngleThreshold = 40f;

    private int _currentItems = 0;
    private Vector3 _initialIndicatorLocalPos;
    private Quaternion _initialLeverRotation;

    public bool IsFull => _currentItems >= maxCapacity;

    private void Start()
    {
        if (fillIndicator != null)
        {
            _initialIndicatorLocalPos = fillIndicator.localPosition;
            UpdateVisualIndicator();
        }

        if (leverPivot != null)
            _initialLeverRotation = leverPivot.localRotation;
    }

    private void Update()
    {
        CheckLeverAngle();
    }

    public bool TryClassify(bool isCorrectCategory)
    {
        if (!isCorrectCategory) return false;
        if (IsFull) return false;

        _currentItems++;
        UpdateVisualIndicator();
        return true;
    }

    private void CheckLeverAngle()
    {
        if (leverPivot == null || _currentItems == 0) return;

        float rawAngle = leverPivot.localEulerAngles.x;
        float normalizedAngle = rawAngle > 180f ? rawAngle - 360f : rawAngle;

        if (Mathf.Abs(normalizedAngle) >= emptyAngleThreshold)
            EmptyContainer();
    }

    private void EmptyContainer()
    {
        _currentItems = 0;
        UpdateVisualIndicator();
        Debug.Log($"[CapacityManager] Vaciado por palanca.");
    }

    private void UpdateVisualIndicator()
    {
        if (fillIndicator == null) return;

        bool show = _currentItems >= 1;
        fillIndicator.gameObject.SetActive(show);

        if (show)
            fillIndicator.localPosition = _initialIndicatorLocalPos + Vector3.up * (_currentItems * heightPerItem);
        else
            fillIndicator.localPosition = _initialIndicatorLocalPos;
    }
}