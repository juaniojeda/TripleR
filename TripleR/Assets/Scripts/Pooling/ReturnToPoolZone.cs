using UnityEngine;

public sealed class ReturnToPoolZone : MonoBehaviour
{
    [SerializeField] private LayerMask acceptedLayers = ~0;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsLayerAccepted(other.gameObject.layer))
            return;

        PoolableObject poolableObject = other.GetComponentInParent<PoolableObject>();

        if (poolableObject == null)
            return;

        poolableObject.ReturnToPool();
    }

    private bool IsLayerAccepted(int layer)
    {
        return (acceptedLayers.value & (1 << layer)) != 0;
    }
}
