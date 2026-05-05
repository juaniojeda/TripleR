using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class ScoreManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text scoreText;
    [SerializeField] private bool clampToZero = true;

    [Header("Animación Positiva (Bump)")]
    [SerializeField] private float bumpScale = 1.4f;
    [SerializeField] private float bumpDuration = 0.25f;

    [Header("Animación Negativa (Shake)")]
    [SerializeField] private float shakeMagnitude = 8f;
    [SerializeField] private float shakeDuration = 0.35f;

    [Header("Planeta")]
    [SerializeField] private PlanetaManager planeta;

    private int currentScore;

    public int CurrentScore => currentScore;

    private Vector3 _originalScale;
    private Vector3 _originalPosition;
    private Coroutine _activeAnimation;

    private void Awake()
    {
        if (scoreText != null)
        {
            _originalScale = scoreText.transform.localScale;
            _originalPosition = scoreText.transform.localPosition;
        }

        RefreshUI();
    }

    public void AddScore(int amount)
    {
        currentScore += amount;

        if (clampToZero && currentScore < 0)
            currentScore = 0;

        PlayerProfile.TrySaveBestScore(currentScore);

        RefreshUI();

        if (planeta != null)
            planeta.NotificarCambioScore(amount);

        if (scoreText == null)
            return;

        if (_activeAnimation != null)
        {
            StopCoroutine(_activeAnimation);
            scoreText.transform.localScale = _originalScale;
            scoreText.transform.localPosition = _originalPosition;
        }

        if (amount > 0)
            _activeAnimation = StartCoroutine(BumpAnimation());
        else if (amount < 0)
            _activeAnimation = StartCoroutine(ShakeAnimation());
    }

    private void RefreshUI()
    {
        if (scoreText != null)
            scoreText.text = $"Puntos: {currentScore}";
    }

    private IEnumerator BumpAnimation()
    {
        float half = bumpDuration * 0.5f;
        float t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            float progress = t / half;
            float scale = Mathf.Lerp(1f, bumpScale, Mathf.SmoothStep(0f, 1f, progress));
            scoreText.transform.localScale = _originalScale * scale;
            yield return null;
        }

        t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            float progress = t / half;
            float scale = Mathf.Lerp(bumpScale, 1f, Mathf.SmoothStep(0f, 1f, progress));
            scoreText.transform.localScale = _originalScale * scale;
            yield return null;
        }

        scoreText.transform.localScale = _originalScale;
        _activeAnimation = null;
    }

    private IEnumerator ShakeAnimation()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float damping = 1f - (elapsed / shakeDuration);
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude * damping;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude * damping;

            scoreText.transform.localPosition = _originalPosition + new Vector3(offsetX, offsetY, 0f);
            yield return null;
        }

        scoreText.transform.localPosition = _originalPosition;
        _activeAnimation = null;
    }
}

//using System.Collections;
//using UnityEngine;
//using UnityEngine.UI;

//public sealed class ScoreManager : MonoBehaviour
//{
//    [Header("UI")]
//    [SerializeField] private Text scoreText;
//    [SerializeField] private bool clampToZero = true;

//    [Header("Animación Positiva (Bump)")]
//    [SerializeField] private float bumpScale = 1.4f;
//    [SerializeField] private float bumpDuration = 0.25f;

//    [Header("Animación Negativa (Shake)")]
//    [SerializeField] private float shakeMagnitude = 8f;
//    [SerializeField] private float shakeDuration = 0.35f;

//    [Header("Planeta")]
//    [SerializeField] private PlanetaManager planeta;

//    private int currentScore;
//    public int CurrentScore => currentScore;

//    private Vector3 _originalScale;
//    private Vector3 _originalPosition;
//    private Coroutine _activeAnimation;

//    private void Awake()
//    {
//        if (scoreText != null)
//        {
//            _originalScale = scoreText.transform.localScale;
//            _originalPosition = scoreText.transform.localPosition;
//        }
//        RefreshUI();
//    }

//    public void AddScore(int amount)
//    {
//        currentScore += amount;
//        if (clampToZero && currentScore < 0)
//            currentScore = 0;

//        RefreshUI();

//        // Notificar al planeta
//        if (planeta != null)
//            planeta.NotificarCambioScore(amount);

//        // Cancelar animación previa si hay una corriendo
//        if (_activeAnimation != null)
//        {
//            StopCoroutine(_activeAnimation);
//            // Resetear a estado limpio antes de la nueva animación
//            if (scoreText != null)
//            {
//                scoreText.transform.localScale = _originalScale;
//                scoreText.transform.localPosition = _originalPosition;
//            }
//        }

//        if (amount > 0)
//            _activeAnimation = StartCoroutine(BumpAnimation());
//        else if (amount < 0)
//            _activeAnimation = StartCoroutine(ShakeAnimation());
//    }

//    private void RefreshUI()
//    {
//        if (scoreText != null)
//            scoreText.text = $"Puntos: {currentScore}";
//    }

//    // ── Bump: crece rápido y vuelve suavemente ──────────────────────────────
//    private IEnumerator BumpAnimation()
//    {
//        float half = bumpDuration * 0.5f;
//        float t = 0f;

//        // Fase 1: escalar hacia arriba
//        while (t < half)
//        {
//            t += Time.deltaTime;
//            float progress = t / half;
//            float scale = Mathf.Lerp(1f, bumpScale, Mathf.SmoothStep(0f, 1f, progress));
//            scoreText.transform.localScale = _originalScale * scale;
//            yield return null;
//        }

//        t = 0f;

//        // Fase 2: volver al tamaño original
//        while (t < half)
//        {
//            t += Time.deltaTime;
//            float progress = t / half;
//            float scale = Mathf.Lerp(bumpScale, 1f, Mathf.SmoothStep(0f, 1f, progress));
//            scoreText.transform.localScale = _originalScale * scale;
//            yield return null;
//        }

//        scoreText.transform.localScale = _originalScale;
//        _activeAnimation = null;
//    }

//    // ── Shake: vibración aleatoria en X/Y que se amortigua ─────────────────
//    private IEnumerator ShakeAnimation()
//    {
//        float elapsed = 0f;

//        while (elapsed < shakeDuration)
//        {
//            elapsed += Time.deltaTime;

//            // La magnitud se reduce linealmente hasta 0 al final
//            float damping = 1f - (elapsed / shakeDuration);
//            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude * damping;
//            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude * damping;

//            scoreText.transform.localPosition = _originalPosition + new Vector3(offsetX, offsetY, 0f);
//            yield return null;
//        }

//        scoreText.transform.localPosition = _originalPosition;
//        _activeAnimation = null;
//    }
//}