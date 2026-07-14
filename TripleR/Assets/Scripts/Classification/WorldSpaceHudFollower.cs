using UnityEngine;

[DisallowMultipleComponent]
public sealed class WorldSpaceHudFollower : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField, Min(0.2f)] private float distance = 1.25f;
    [SerializeField] private float horizontalOffset = 0f;
    [SerializeField] private float verticalOffset = 0.16f;
    [SerializeField] private Vector3 hudScale = new Vector3(0.001f, 0.001f, 0.001f);
    [SerializeField] private bool followPosition = true;
    [SerializeField] private bool copyTargetRotation = true;
    [SerializeField] private bool lookAtTargetHorizontally;

    private Camera cachedMainCamera;
    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        ApplyUiLayerRecursively();
        transform.localScale = hudScale;
    }

    private void LateUpdate()
    {
        Transform followTarget = ResolveTarget();
        if (followTarget == null)
            return;

        if (followPosition)
        {
            Vector3 offset =
                (followTarget.forward * distance) +
                (followTarget.right * horizontalOffset) +
                (followTarget.up * verticalOffset);

            transform.position = followTarget.position + offset;
        }

        if (copyTargetRotation)
            transform.rotation = followTarget.rotation;
        else if (lookAtTargetHorizontally)
            RotateTowardsTargetHorizontally(followTarget);

        transform.localScale = hudScale;

        if (canvas != null && canvas.worldCamera != cachedMainCamera)
            canvas.worldCamera = cachedMainCamera;
    }

    private Transform ResolveTarget()
    {
        if (target != null)
            return target;

        if (cachedMainCamera == null || !cachedMainCamera.isActiveAndEnabled)
            cachedMainCamera = Camera.main;

        return cachedMainCamera != null ? cachedMainCamera.transform : null;
    }

    private void ApplyUiLayerRecursively()
    {
        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer < 0)
            return;

        SetLayerRecursively(transform, uiLayer);
    }

    private static void SetLayerRecursively(Transform current, int layer)
    {
        current.gameObject.layer = layer;

        for (int i = 0; i < current.childCount; i++)
            SetLayerRecursively(current.GetChild(i), layer);
    }

    private void RotateTowardsTargetHorizontally(Transform followTarget)
    {
        Vector3 toHud = transform.position - followTarget.position;
        toHud.y = 0f;

        if (toHud.sqrMagnitude < 0.0001f)
            return;

        transform.rotation = Quaternion.LookRotation(toHud.normalized, Vector3.up);
    }
}
