using System.Collections;
using UnityEngine;

public class BulletTimePowerUp : MonoBehaviour
{
    [Header("Power Up Settings")]
    public float bulletTimeDuration = 10f;
    public float respawnDelay = 10f;

    [Header("Components To Hide")]
    public Collider[] collidersToDisable;
    public Renderer[] renderersToDisable;

    [Header("Respawn Settings")]
    public bool returnToStartPosition = true;

    private bool isAvailable = true;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb;

    private void Awake()
    {
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

    public void GrabPowerUp()
    {
        Debug.Log("POWER UP AGARRADO - ACTIVANDO BULLET TIME");

        if (!isAvailable)
            return;

        StartCoroutine(PowerUpRoutine());
    }

    private IEnumerator PowerUpRoutine()
    {
        isAvailable = false;

        if (BulletTimeManager.Instance != null)
        {
            BulletTimeManager.Instance.ActivateBulletTime(bulletTimeDuration);
        }
        else
        {
            Debug.LogWarning("No hay BulletTimeManager en la escena.");
        }

        HidePowerUp();

        Debug.Log("POWER UP OCULTO - ESPERANDO RESPAWN");

        yield return new WaitForSecondsRealtime(respawnDelay);

        RespawnPowerUp();

        Debug.Log("POWER UP REAPARECIDO");

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