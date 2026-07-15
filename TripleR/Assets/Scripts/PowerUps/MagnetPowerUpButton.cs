using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class MagnetPowerUpButton : MonoBehaviour
{
    [Header("Magnet Settings")]
    [SerializeField] private MagnetPowerUpController magnetPowerUp;

    [Header("Respawn Settings")]
    public float respawnDelay = 10f;
    public bool returnToStartPosition = true;

    [Header("Components To Hide")]
    public Collider[] collidersToDisable;
    public Renderer[] renderersToDisable;

    private bool isAvailable = true;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb;

    private void Awake()
    {
        if (magnetPowerUp == null)
            magnetPowerUp = MagnetPowerUpController.Instance;

        startPosition = transform.position;
        startRotation = transform.rotation;

        rb = GetComponent<Rigidbody>();

        if (collidersToDisable == null || collidersToDisable.Length == 0)
        {
            collidersToDisable = GetComponentsInChildren<Collider>(true);
        }

        if (renderersToDisable == null || renderersToDisable.Length == 0)
        {
            renderersToDisable = GetComponentsInChildren<Renderer>(true);
        }
    }

    private void Reset()
    {
        collidersToDisable = GetComponentsInChildren<Collider>(true);
        renderersToDisable = GetComponentsInChildren<Renderer>(true);
    }

    public void Press()
    {
        if (!isAvailable)
            return;

        StartCoroutine(PowerUpRoutine());
    }

    private IEnumerator PowerUpRoutine()
    {
        isAvailable = false;

        MagnetPowerUpController target = magnetPowerUp != null
            ? magnetPowerUp
            : MagnetPowerUpController.Instance;

        if (target != null)
            target.Activate();

        HidePowerUp();

        // El respawn no debe alargarse si bullet time está activo.
        yield return new WaitForSecondsRealtime(respawnDelay);

        RespawnPowerUp();

        isAvailable = true;
    }

    private void HidePowerUp()
    {
        foreach (Renderer rend in renderersToDisable)
        {
            if (rend != null)
                rend.enabled = false;
        }

        foreach (Collider col in collidersToDisable)
        {
            if (col != null)
                col.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    private void RespawnPowerUp()
    {
        if (returnToStartPosition)
        {
            transform.position = startPosition;
            transform.rotation = startRotation;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }

        foreach (Renderer rend in renderersToDisable)
        {
            if (rend != null)
                rend.enabled = true;
        }

        foreach (Collider col in collidersToDisable)
        {
            if (col != null)
                col.enabled = true;
        }
    }
}