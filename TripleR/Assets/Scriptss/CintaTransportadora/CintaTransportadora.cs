using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CintaTransportadora - Unity 6 / Meta XR SDK
/// Arquitectura: trigger-based conveyor + VelocityChange + MaterialPropertyBlock
/// Sin OnCollisionStay, sin material.mainTextureOffset.
/// </summary>
public class CintaTransportadora : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 1.0f;
    public Vector3 direccionLocal = Vector3.forward;

    [Header("Estabilidad")]
    [Min(0f)] public float amortiguacionLateral = 12f;

    [Header("Filtrado de Objetos")]
    [Tooltip("Tag requerido. Vacio = acepta todos.")]
    public string tagObjetivo = "Residuo";
    public LayerMask capasPermitidas;

    [Header("Animacion Visual (GPU-friendly)")]
    [Tooltip("Renderer del quad/mesh de la cinta para animar textura.")]
    public Renderer rendererCinta;
    [Tooltip("Nombre de la propiedad UV en el shader. Por defecto: _MainTex.")]
    public string propiedadUV = "_MainTex";

    private readonly HashSet<Rigidbody> objetosEnCinta = new HashSet<Rigidbody>();
    private readonly List<Rigidbody> buffer = new List<Rigidbody>();
    private readonly Dictionary<Rigidbody, RigidbodyConstraints> restriccionesOriginales =
        new Dictionary<Rigidbody, RigidbodyConstraints>();
    private readonly Dictionary<Rigidbody, RigidbodyInterpolation> interpolacionesOriginales =
        new Dictionary<Rigidbody, RigidbodyInterpolation>();
    private MaterialPropertyBlock mpb;
    private float offsetUV;

    private void Awake()
    {
        // Se crea una sola vez para evitar instancias de material.
        mpb = new MaterialPropertyBlock();
    }

    private void FixedUpdate()
    {
        MoverObjetos();
        AnimarTexturaUV();
    }

    private void MoverObjetos()
    {
        if (objetosEnCinta.Count == 0 || velocidad <= 0f)
            return;

        Vector3 direccionMundo = transform.TransformDirection(direccionLocal);
        if (direccionMundo.sqrMagnitude < Mathf.Epsilon)
            return;

        direccionMundo.Normalize();

        Vector3 normalCinta = transform.up;
        if (normalCinta.sqrMagnitude < Mathf.Epsilon)
            normalCinta = Vector3.up;
        else
            normalCinta.Normalize();

        buffer.Clear();
        buffer.AddRange(objetosEnCinta);

        bool hayNulos = false;
        foreach (Rigidbody rb in buffer)
        {
            if (rb == null)
            {
                hayNulos = true;
                continue;
            }

            if (!rb.gameObject.activeInHierarchy)
            {
                RestaurarRestricciones(rb, true);
                objetosEnCinta.Remove(rb);
                continue;
            }

            // Si el jugador lo toma, pasa a kinematic y la cinta deja de competir.
            if (rb.isKinematic)
            {
                RestaurarRestricciones(rb, false);
                continue;
            }

            AplicarRestriccionesCinta(rb);
            EstabilizarVelocidadEnCinta(rb, direccionMundo, normalCinta);
        }

        if (hayNulos)
            objetosEnCinta.RemoveWhere(rb => rb == null);
    }

    private void AnimarTexturaUV()
    {
        if (rendererCinta == null)
            return;

        offsetUV += velocidad * Time.fixedDeltaTime;
        if (offsetUV > 1f)
            offsetUV -= 1f;

        rendererCinta.GetPropertyBlock(mpb);
        mpb.SetVector(propiedadUV + "_ST", new Vector4(1f, 1f, offsetUV, 0f));
        rendererCinta.SetPropertyBlock(mpb);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & capasPermitidas) == 0)
            return;

        if (!string.IsNullOrEmpty(tagObjetivo) && !other.CompareTag(tagObjetivo))
            return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            objetosEnCinta.Add(rb);
            RegistrarEstadoOriginal(rb);
            PrepararObjetoParaCinta(rb);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            objetosEnCinta.Remove(rb);
            RestaurarRestricciones(rb, true);
        }
    }

    private void OnDisable()
    {
        foreach (Rigidbody rb in objetosEnCinta)
        {
            RestaurarRestricciones(rb, true);
        }

        objetosEnCinta.Clear();
    }

    private void RegistrarRestriccionesOriginales(Rigidbody rb)
    {
        if (rb == null || restriccionesOriginales.ContainsKey(rb))
            return;

        restriccionesOriginales.Add(rb, rb.constraints);
    }

    private void RegistrarEstadoOriginal(Rigidbody rb)
    {
        RegistrarRestriccionesOriginales(rb);

        if (rb == null || interpolacionesOriginales.ContainsKey(rb))
            return;

        interpolacionesOriginales.Add(rb, rb.interpolation);
    }

    private void PrepararObjetoParaCinta(Rigidbody rb)
    {
        if (rb == null)
            return;

        rb.angularVelocity = Vector3.zero;
    }

    private void AplicarRestriccionesCinta(Rigidbody rb)
    {
        RegistrarEstadoOriginal(rb);

        RigidbodyConstraints restriccionesDeseadas =
            restriccionesOriginales[rb]
            | RigidbodyConstraints.FreezeRotationX
            | RigidbodyConstraints.FreezeRotationZ;

        if (rb.constraints != restriccionesDeseadas)
            rb.constraints = restriccionesDeseadas;

        if (rb.interpolation != RigidbodyInterpolation.Interpolate)
            rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void EstabilizarVelocidadEnCinta(Rigidbody rb, Vector3 direccionMundo, Vector3 normalCinta)
    {
        Vector3 velocidadActual = rb.linearVelocity;
        float velocidadNormal = Vector3.Dot(velocidadActual, normalCinta);

        Vector3 velocidadPlano = Vector3.ProjectOnPlane(velocidadActual, normalCinta);
        float velocidadFrontal = Vector3.Dot(velocidadPlano, direccionMundo);
        Vector3 velocidadLateral = velocidadPlano - (direccionMundo * velocidadFrontal);

        float pasoLateral = amortiguacionLateral * Time.fixedDeltaTime;
        Vector3 nuevaVelocidadLateral = Vector3.MoveTowards(velocidadLateral, Vector3.zero, pasoLateral);

        rb.linearVelocity =
            (direccionMundo * Mathf.Max(velocidadFrontal, velocidad))
            + nuevaVelocidadLateral
            + (normalCinta * velocidadNormal);
    }

    private void RestaurarRestricciones(Rigidbody rb, bool olvidarOriginal)
    {
        if (rb == null)
            return;

        if (!restriccionesOriginales.TryGetValue(rb, out RigidbodyConstraints restriccionesOriginalesRb))
            return;

        if (rb.constraints != restriccionesOriginalesRb)
            rb.constraints = restriccionesOriginalesRb;

        if (interpolacionesOriginales.TryGetValue(rb, out RigidbodyInterpolation interpolacionOriginal) &&
            rb.interpolation != interpolacionOriginal)
        {
            rb.interpolation = interpolacionOriginal;
        }

        if (olvidarOriginal)
        {
            restriccionesOriginales.Remove(rb);
            interpolacionesOriginales.Remove(rb);
        }
    }
}
//using System.Collections.Generic;
//using UnityEngine;

///// <summary>
///// CintaTransportadora — Unity 6 / Meta XR SDK
///// Arquitectura: Trigger-based pooling + MovePosition + MaterialPropertyBlock
///// Sin OnCollisionStay, sin material.mainTextureOffset (evita clones en Android)
///// </summary>
//public class CintaTransportadora : MonoBehaviour
//{
//    [Header("Movimiento")]
//    public float velocidad = 1.0f;
//    public Vector3 direccionLocal = Vector3.forward;

//    [Header("Filtrado de Objetos")]
//    [Tooltip("Tag requerido. Vacío = acepta todos.")]
//    public string tagObjetivo = "Residuo";
//    public LayerMask capasPermitidas;

//    [Header("Animación Visual (GPU-friendly)")]
//    [Tooltip("Renderer del quad/mesh de la cinta para animar textura.")]
//    public Renderer rendererCinta;
//    [Tooltip("Nombre de la propiedad UV en el shader. Por defecto: _MainTex.")]
//    public string propiedadUV = "_MainTex";

//    // --- Estado interno ---
//    private HashSet<Rigidbody> objetosEnCinta = new HashSet<Rigidbody>();
//    private List<Rigidbody> _buffer = new List<Rigidbody>(); // buffer para iterar sin modificar el set
//    private MaterialPropertyBlock _mpb;
//    private float _offsetUV = 0f;

//    void Awake()
//    {
//        // Inicializar MaterialPropertyBlock UNA sola vez — nunca instancia materiales
//        _mpb = new MaterialPropertyBlock();
//    }

//    void FixedUpdate()
//    {
//        MoverObjetos();
//        AnimarTexturaUV();
//    }

//    private void MoverObjetos()
//    {
//        if (objetosEnCinta.Count == 0) return;

//        Vector3 movimiento = transform.TransformDirection(direccionLocal).normalized
//                             * velocidad * Time.fixedDeltaTime;

//        // Copiar a buffer para iterar de forma segura
//        _buffer.Clear();
//        _buffer.AddRange(objetosEnCinta);

//        bool hayNulos = false;
//        foreach (Rigidbody rb in _buffer)
//        {
//            if (rb == null) { hayNulos = true; continue; }

//            // Clave Meta XR: si el jugador agarra el objeto, isKinematic = true → no competir
//            if (!rb.isKinematic)
//            {
//                rb.MovePosition(rb.position + movimiento);
//            }
//        }

//        // Limpiar referencias muertas solo si es necesario
//        if (hayNulos)
//            objetosEnCinta.RemoveWhere(rb => rb == null);
//    }

//    private void AnimarTexturaUV()
//    {
//        if (rendererCinta == null) return;

//        // Acumular offset en dirección de movimiento
//        _offsetUV += velocidad * Time.fixedDeltaTime;
//        if (_offsetUV > 1f) _offsetUV -= 1f; // mantener en [0,1]

//        // MaterialPropertyBlock: CERO instanciación de materiales, seguro para batching
//        rendererCinta.GetPropertyBlock(_mpb);
//        _mpb.SetVector(propiedadUV + "_ST", new Vector4(1, 1, _offsetUV, 0));
//        rendererCinta.SetPropertyBlock(_mpb);
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        // 1. Filtro por Layer (bitwise — más rápido que CompareTag)
//        if (((1 << other.gameObject.layer) & capasPermitidas) == 0) return;

//        // 2. Filtro por Tag (solo si se especificó uno)
//        if (!string.IsNullOrEmpty(tagObjetivo) && !other.CompareTag(tagObjetivo)) return;

//        Rigidbody rb = other.attachedRigidbody;
//        if (rb != null)
//            objetosEnCinta.Add(rb);
//    }

//    private void OnTriggerExit(Collider other)
//    {
//        Rigidbody rb = other.attachedRigidbody;
//        if (rb != null)
//            objetosEnCinta.Remove(rb);
//    }

//    // Opcional: limpiar al desactivar (cuando el objeto vuelve al Pool)
//    private void OnDisable()
//    {
//        objetosEnCinta.Clear();
//    }
//}