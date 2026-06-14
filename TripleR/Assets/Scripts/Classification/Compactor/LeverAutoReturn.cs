using System.Collections;
using UnityEngine;

public class LeverAutoReturn : MonoBehaviour
{
    [Header("Configuración de Retorno")]
    [Tooltip("Tiempo en segundos a esperar tras soltar la palanca")]
    [SerializeField] private float delayInSeconds = 1f;
    [Tooltip("Velocidad del movimiento de regreso (mayor = más rápido)")]
    [SerializeField] private float returnSpeed = 5f;

    [Header("Destino Exacto (Local)")]
    [Tooltip("Posición exacta a la que debe volver respecto al pivote padre")]
    [SerializeField] private Vector3 targetLocalPosition = new Vector3(0f, 0.5f, 0f);
    [Tooltip("Rotación exacta a la que debe volver")]
    [SerializeField] private Vector3 targetLocalRotation = Vector3.zero; // 0,0,0

    // Umbrales para evitar cálculos infinitos
    private const float PositionThreshold = 0.001f;
    private const float RotationThreshold = 0.1f;

    private bool _isGrabbed = false;
    private Coroutine _returnCoroutine;
    private WaitForSeconds _waitForDelay;

    private void Awake()
    {
        _waitForDelay = new WaitForSeconds(delayInSeconds);
    }

    // Se conecta en el inspector al "When Select" del Pointable Wrapper
    public void OnLeverGrabbed()
    {
        _isGrabbed = true;

        if (_returnCoroutine != null)
        {
            StopCoroutine(_returnCoroutine);
            _returnCoroutine = null;
        }
    }

    // Se conecta en el inspector al "When Unselect" del Pointable Wrapper
    public void OnLeverReleased()
    {
        _isGrabbed = false;
        _returnCoroutine = StartCoroutine(ReturnToZeroRoutine());
    }

    private IEnumerator ReturnToZeroRoutine()
    {
        yield return _waitForDelay;

        // Transformamos el Vector3 a Quaternion para que Unity pueda calcular la rotación suave
        Quaternion targetRot = Quaternion.Euler(targetLocalRotation);

        while (!_isGrabbed)
        {
            // 1. Interpolamos la Posición hacia (0, 0.5, 0)
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                targetLocalPosition,
                Time.deltaTime * returnSpeed
            );

            // 2. Interpolamos la Rotación hacia (0, 0, 0)
            transform.localRotation = Quaternion.Lerp(
                transform.localRotation,
                targetRot,
                Time.deltaTime * returnSpeed
            );

            // 3. Verificamos si estamos lo suficientemente cerca del objetivo
            float dist = Vector3.Distance(transform.localPosition, targetLocalPosition);
            float angle = Quaternion.Angle(transform.localRotation, targetRot);

            if (dist < PositionThreshold && angle < RotationThreshold)
            {
                // Forzamos los valores exactos para cerrar el ciclo perfectamente
                transform.localPosition = targetLocalPosition;
                transform.localRotation = targetRot;
                break;
            }

            yield return null;
        }

        _returnCoroutine = null;
    }
}