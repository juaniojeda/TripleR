using System.Collections;
using UnityEngine;

public class LeverAutoReturn : MonoBehaviour
{
    [Header("Referencias (Estilo Meta)")]
    [Tooltip("El objeto que se va a mover. Si está vacío, moverá el objeto actual.")]
    [SerializeField] private Transform _targetTransform;

    [Header("Configuración de Retorno")]
    [SerializeField] private float returnSpeed = 8f;
    [SerializeField] private float delaySeconds = 1f;

    [Header("Destino Local Exacto")]
    [Tooltip("La posición local a la que debe volver respecto a su padre (el Pivot)")]
    [SerializeField] private Vector3 targetLocalPosition = new Vector3(0f, 0.5f, 0f);
    [Tooltip("La rotación local a la que debe volver respecto a su padre")]
    [SerializeField] private Vector3 targetLocalEuler = Vector3.zero;

    private bool _isGrabbed = false;
    private Coroutine _returnRoutine;
    private WaitForSeconds _delayInstruction;

    private void Awake()
    {
        // Caché del tiempo para evitar garbage collection (Zero allocation)
        _delayInstruction = new WaitForSeconds(delaySeconds);

        // Si no asignamos un Target, usa el propio objeto (Igual que en Grabbable.cs)
        if (_targetTransform == null)
        {
            _targetTransform = transform;
        }
    }

    // -> Conectar al evento "When Select" de Meta
    public void BeginInteraction()
    {
        _isGrabbed = true;
        if (_returnRoutine != null)
        {
            StopCoroutine(_returnRoutine);
            _returnRoutine = null;
        }
    }

    // -> Conectar al evento "When Unselect" de Meta
    public void EndInteraction()
    {
        _isGrabbed = false;
        _returnRoutine = StartCoroutine(TransformRoutine());
    }

    // Esta corrutina actúa como el "UpdateTransform()" del ITransformer de Meta
    private IEnumerator TransformRoutine()
    {
        yield return _delayInstruction;

        Quaternion targetRotation = Quaternion.Euler(targetLocalEuler);

        while (!_isGrabbed)
        {
            // 1. Interpolamos la posición local hacia 0, 0.5, 0
            _targetTransform.localPosition = Vector3.Lerp(
                _targetTransform.localPosition,
                targetLocalPosition,
                Time.deltaTime * returnSpeed
            );

            // 2. Interpolamos la rotación local hacia 0, 0, 0
            _targetTransform.localRotation = Quaternion.Lerp(
                _targetTransform.localRotation,
                targetRotation,
                Time.deltaTime * returnSpeed
            );

            // 3. Condición de parada para apagar el cálculo (Ahorro de CPU)
            float posDiff = Vector3.Distance(_targetTransform.localPosition, targetLocalPosition);
            float rotDiff = Quaternion.Angle(_targetTransform.localRotation, targetRotation);

            if (posDiff < 0.001f && rotDiff < 0.1f)
            {
                _targetTransform.localPosition = targetLocalPosition;
                _targetTransform.localRotation = targetRotation;
                break;
            }

            yield return null; // Espera al siguiente frame
        }

        _returnRoutine = null;
    }
}