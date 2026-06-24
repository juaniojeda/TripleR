using System;
using System.Collections.Generic;
using UnityEngine;

public class CintaTransportadora : MonoBehaviour
{
    public TutorialPagesUI tutorialUI;

    public float velocidad = 1.0f;
    public Vector3 direccionLocal = Vector3.forward;
    [Min(0f)] public float amortiguacionLateral = 12f;
    public string tagObjetivo = "Residuo";
    public LayerMask capasPermitidas;

    public bool isOn = false;

    private readonly HashSet<Rigidbody> objetosEnCinta = new HashSet<Rigidbody>();
    private readonly List<Rigidbody> buffer = new List<Rigidbody>();
    private readonly Dictionary<Rigidbody, RigidbodyConstraints> restriccionesOriginales = new Dictionary<Rigidbody, RigidbodyConstraints>();
    private readonly Dictionary<Rigidbody, RigidbodyInterpolation> interpolacionesOriginales = new Dictionary<Rigidbody, RigidbodyInterpolation>();

    public void TurnOn() => isOn = true;
    public void TurnOff() => isOn = false;
    public void Toggle() => isOn = !isOn;

    private void FixedUpdate()
    {
        MoverObjetos();
    }

    private void MoverObjetos()
    {
        if (!isOn || objetosEnCinta.Count == 0 || velocidad <= 0f)
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

    private void OnEnable()
    {
        if (tutorialUI != null)
        {
            tutorialUI.OnTutorialFinished += TurnOn;
        }
    }

    private void OnDisable()
    {
        if (tutorialUI != null)
        {
            tutorialUI.OnTutorialFinished -= TurnOn;
        }

        foreach (Rigidbody rb in objetosEnCinta)
        {
            RestaurarRestricciones(rb, true);
        }

        objetosEnCinta.Clear();
    }
}