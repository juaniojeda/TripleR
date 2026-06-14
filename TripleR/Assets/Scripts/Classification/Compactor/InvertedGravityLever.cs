using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class InvertedGravityLever : MonoBehaviour
{
    [Header("Configuración de Fuerza")]
    [Tooltip("La fuerza que empujará la punta hacia arriba. 9.81 es igual a la gravedad normal.")]
    [SerializeField] private float upwardForce = 15f;

    [Tooltip("Arrastra aquí el objeto vacío que creaste en la punta de la palanca")]
    [SerializeField] private Transform leverTip;

    private Rigidbody _rb;
    private bool _isGrabbed = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        // Optimizamos el rigidbody para rotaciones precisas
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // Conecta esto al evento "When Select ()" del Pointable Unity Event Wrapper
    public void OnLeverGrabbed()
    {
        _isGrabbed = true;
    }

    // Conecta esto al evento "When Unselect ()" del Pointable Unity Event Wrapper
    public void OnLeverReleased()
    {
        _isGrabbed = false;

        // Opcional: Despertar al Rigidbody por si el motor de físicas lo puso a dormir
        if (_rb.IsSleeping())
            _rb.WakeUp();
    }

    private void FixedUpdate()
    {
        // Solo aplicamos la fuerza si el jugador NO la tiene agarrada y tenemos la referencia de la punta
        if (!_isGrabbed && leverTip != null)
        {
            // AddForceAtPosition empuja un punto específico del objeto.
            // Al empujar la punta hacia arriba de forma constante, la palanca se endereza sola.
            _rb.AddForceAtPosition(Vector3.up * upwardForce, leverTip.position, ForceMode.Force);
        }
    }
}