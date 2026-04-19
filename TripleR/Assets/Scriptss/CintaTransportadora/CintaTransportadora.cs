using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CintaTransportadora — Unity 6 / Meta XR SDK
/// Arquitectura: Trigger-based pooling + MovePosition + MaterialPropertyBlock
/// Sin OnCollisionStay, sin material.mainTextureOffset (evita clones en Android)
/// </summary>
public class CintaTransportadora : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 1.0f;
    public Vector3 direccionLocal = Vector3.forward;

    [Header("Filtrado de Objetos")]
    [Tooltip("Tag requerido. Vacío = acepta todos.")]
    public string tagObjetivo = "Residuo";
    public LayerMask capasPermitidas;

    [Header("Animación Visual (GPU-friendly)")]
    [Tooltip("Renderer del quad/mesh de la cinta para animar textura.")]
    public Renderer rendererCinta;
    [Tooltip("Nombre de la propiedad UV en el shader. Por defecto: _MainTex.")]
    public string propiedadUV = "_MainTex";

    // --- Estado interno ---
    private HashSet<Rigidbody> objetosEnCinta = new HashSet<Rigidbody>();
    private List<Rigidbody> _buffer = new List<Rigidbody>(); // buffer para iterar sin modificar el set
    private MaterialPropertyBlock _mpb;
    private float _offsetUV = 0f;

    void Awake()
    {
        // Inicializar MaterialPropertyBlock UNA sola vez — nunca instancia materiales
        _mpb = new MaterialPropertyBlock();
    }

    void FixedUpdate()
    {
        MoverObjetos();
        AnimarTexturaUV();
    }

    private void MoverObjetos()
    {
        if (objetosEnCinta.Count == 0) return;

        Vector3 movimiento = transform.TransformDirection(direccionLocal).normalized
                             * velocidad * Time.fixedDeltaTime;

        // Copiar a buffer para iterar de forma segura
        _buffer.Clear();
        _buffer.AddRange(objetosEnCinta);

        bool hayNulos = false;
        foreach (Rigidbody rb in _buffer)
        {
            if (rb == null) { hayNulos = true; continue; }

            // Clave Meta XR: si el jugador agarra el objeto, isKinematic = true → no competir
            if (!rb.isKinematic)
            {
                rb.MovePosition(rb.position + movimiento);
            }
        }

        // Limpiar referencias muertas solo si es necesario
        if (hayNulos)
            objetosEnCinta.RemoveWhere(rb => rb == null);
    }

    private void AnimarTexturaUV()
    {
        if (rendererCinta == null) return;

        // Acumular offset en dirección de movimiento
        _offsetUV += velocidad * Time.fixedDeltaTime;
        if (_offsetUV > 1f) _offsetUV -= 1f; // mantener en [0,1]

        // MaterialPropertyBlock: CERO instanciación de materiales, seguro para batching
        rendererCinta.GetPropertyBlock(_mpb);
        _mpb.SetVector(propiedadUV + "_ST", new Vector4(1, 1, _offsetUV, 0));
        rendererCinta.SetPropertyBlock(_mpb);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Filtro por Layer (bitwise — más rápido que CompareTag)
        if (((1 << other.gameObject.layer) & capasPermitidas) == 0) return;

        // 2. Filtro por Tag (solo si se especificó uno)
        if (!string.IsNullOrEmpty(tagObjetivo) && !other.CompareTag(tagObjetivo)) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
            objetosEnCinta.Add(rb);
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
            objetosEnCinta.Remove(rb);
    }

    // Opcional: limpiar al desactivar (cuando el objeto vuelve al Pool)
    private void OnDisable()
    {
        objetosEnCinta.Clear();
    }
}