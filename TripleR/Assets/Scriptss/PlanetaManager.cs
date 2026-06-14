using System.Collections;
using UnityEngine;

/// <summary>
/// PlanetaManager — Unity 6 / Meta XR
/// - Rotación realista con inclinación axial
/// - Color reactivo al rendimiento: errores = gris, aciertos = color original
/// </summary>
public sealed class PlanetaManager : MonoBehaviour
{
    [Header("Rotación")]
    [SerializeField] private float velocidadRotacion = 10f;
    [Tooltip("Inclinación axial en grados (Tierra real = 23.5°)")]
    [SerializeField] private float inclinacionAxial = 23.5f;

    [Header("Salud del Planeta")]
    [Tooltip("Cuántos errores seguidos para llegar al gris total")]
    [SerializeField] private int erroresParaGrisTotal = 5;
    [Tooltip("Velocidad de transición de color (0 = instantáneo)")]
    [SerializeField] private float velocidadTransicion = 2f;

    [Header("Referencias")]
    [SerializeField] private Renderer rendererPlaneta;

    // ── Estado interno ──────────────────────────────────────────────
    private int _erroresSeguidos = 0;
    private float _saludNormalizada = 1f;   // 1 = colorido, 0 = gris total
    private float _saludObjetivo = 1f;
    private MaterialPropertyBlock _mpb;
    private static readonly int ColorID = Shader.PropertyToID("_BaseColor");

    // Colores de la textura base
    private Color _colorOriginal = Color.white;                      // tinte neutro = textura normal
    private Color _colorGris = new Color(0.3f, 0.3f, 0.3f, 1f); // gris oscuro contaminado

    private Coroutine _transicionActiva;

    // ── Inicialización ──────────────────────────────────────────────
    private void Awake()
    {
        // Aplicar inclinación axial al eje de rotación del planeta
        transform.rotation = Quaternion.Euler(0f, 0f, inclinacionAxial);

        _mpb = new MaterialPropertyBlock();

        if (rendererPlaneta == null)
            rendererPlaneta = GetComponent<Renderer>();

        // Guardar color original si el material ya tiene uno
        if (rendererPlaneta != null && rendererPlaneta.sharedMaterial.HasProperty(ColorID))
            _colorOriginal = rendererPlaneta.sharedMaterial.GetColor(ColorID);

        AplicarColor(_colorOriginal);
    }

    // ── Rotación ────────────────────────────────────────────────────
    private void Update()
    {
        // Rotar sobre el eje Y local (que ya está inclinado por Awake)
        transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime, Space.Self);

        // Interpolar suavemente hacia el color objetivo
        if (!Mathf.Approximately(_saludNormalizada, _saludObjetivo))
        {
            _saludNormalizada = Mathf.MoveTowards(
                _saludNormalizada,
                _saludObjetivo,
                velocidadTransicion * Time.deltaTime
            );
            AplicarColorPorSalud();
        }
    }

    // ── API pública (llamar desde ScoreManager / ContenedorClasificador) ──

    /// <summary>Llamar cuando el jugador comete un error de clasificación.</summary>
    public void RegistrarError()
    {
        _erroresSeguidos++;
        _erroresSeguidos = Mathf.Clamp(_erroresSeguidos, 0, erroresParaGrisTotal);

        ActualizarSaludObjetivo();
    }

    /// <summary>Llamar cuando el jugador acierta una clasificación.</summary>
    public void RegistrarAcierto()
    {
        // Un acierto reduce los errores acumulados (recuperación gradual)
        _erroresSeguidos = Mathf.Max(0, _erroresSeguidos - 1);

        ActualizarSaludObjetivo();
    }

    /// <summary>
    /// Llamar directamente con un delta de score (positivo = acierto, negativo = error).
    /// Compatible con el flujo de AddScore() de ScoreManager.
    /// </summary>
    public void NotificarCambioScore(int delta)
    {
        if (delta > 0) RegistrarAcierto();
        else if (delta < 0) RegistrarError();
    }

    // ── Lógica interna ──────────────────────────────────────────────
    private void ActualizarSaludObjetivo()
    {
        // Salud va de 1 (sin errores) a 0 (máximo de errores seguidos)
        _saludObjetivo = 1f - ((float)_erroresSeguidos / erroresParaGrisTotal);
    }

    private void AplicarColorPorSalud()
    {
        Color colorActual = Color.Lerp(_colorGris, _colorOriginal, _saludNormalizada);
        AplicarColor(colorActual);
    }

    private void AplicarColor(Color color)
    {
        if (rendererPlaneta == null) return;

        // MaterialPropertyBlock: sin instanciar materiales, seguro para batching
        rendererPlaneta.GetPropertyBlock(_mpb);
        _mpb.SetColor(ColorID, color);
        rendererPlaneta.SetPropertyBlock(_mpb);
    }

    // ── Gizmos de debug (solo en Editor) ───────────────────────────
}