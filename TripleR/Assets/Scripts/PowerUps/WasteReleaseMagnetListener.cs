using Oculus.Interaction;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class WasteReleaseMagnetListener : MonoBehaviour
{
    private PoolableObject poolableObject;
    private IPointable pointable;
    private bool subscribed;

    private void Awake()
    {
        CacheReferences();
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        ResetForPool();
        Unsubscribe();
    }

    public void Initialize(PoolableObject owner)
    {
        poolableObject = owner;
        CachePointable();
        Subscribe();
    }

    public void ResetForSpawn()
    {
        Subscribe();
        MagnetPowerUpController.Instance?.CancelMagnet(poolableObject);
    }

    public void ResetForPool()
    {
        MagnetPowerUpController.Instance?.CancelMagnet(poolableObject);
    }

    private void CacheReferences()
    {
        if (poolableObject == null)
            poolableObject = GetComponent<PoolableObject>();

        CachePointable();
    }

    private void CachePointable()
    {
        if (pointable != null)
            return;

        pointable = GetComponent<IPointable>();

        if (pointable == null)
            pointable = GetComponentInChildren<IPointable>(true);
    }

    private void Subscribe()
    {
        if (subscribed)
            return;

        if (pointable == null)
            CachePointable();

        if (pointable == null)
            return;

        pointable.WhenPointerEventRaised += HandlePointerEventRaised;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed || pointable == null)
            return;

        pointable.WhenPointerEventRaised -= HandlePointerEventRaised;
        subscribed = false;
    }

    private void HandlePointerEventRaised(PointerEvent pointerEvent)
    {
        if (pointerEvent.Type != PointerEventType.Unselect)
            return;

        MagnetPowerUpController.Instance?.TryRequestMagnet(poolableObject);
    }
}
