using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PlayerDetectionTrigger : MonoBehaviour
{
    [SerializeField] private SimplePatrolAI ai;
    [SerializeField] private string playerTag = "Player";

    private BoxCollider boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;

        if (ai == null)
            ai = GetComponentInParent<SimplePatrolAI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // En XR el collider puede estar en un hijo aunque la etiqueta Player viva en la raíz.
        Transform root = other.transform.root;

        if (other.CompareTag(playerTag) || root.CompareTag(playerTag))
            ai?.DetectPlayer();
    }
}