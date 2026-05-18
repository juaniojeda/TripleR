using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public sealed class ScoreManager : MonoBehaviour
{
    [Header("UI - Puntuación")]
    [SerializeField] private Text scoreText;
    [SerializeField] private bool clampToZero = true;

    [Header("UI - Temporizador")]
    [SerializeField] private Text timeText;
    [SerializeField] private float initialTime = 60f;
    [Tooltip("Cuántos segundos se suman/restan por cada punto obtenido/perdido.")]
    [SerializeField] private float secondsPerPoint = 1f;

    [Header("Eventos de Juego")]
    public UnityEvent OnTimeOut;

    [Header("Animación Positiva (Bump)")]
    [SerializeField] private float bumpScale = 1.4f;
    [SerializeField] private float bumpDuration = 0.25f;

    [Header("Animación Negativa (Shake)")]
    [SerializeField] private float shakeMagnitude = 8f;
    [SerializeField] private float shakeDuration = 0.35f;

    private int currentScore;
    private float currentTime;
    private bool isTimerRunning;
    private int lastDisplayedSecond = -1;

    public int CurrentScore => currentScore;
    public float CurrentTime => currentTime;

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

        currentTime = initialTime;
        isTimerRunning = true;

        RefreshScoreUI();
        RefreshTimeUI();
    }

    private void Update()
    {
        if (!isTimerRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isTimerRunning = false;
            RefreshTimeUI();
            OnTimeOut?.Invoke();
            return;
        }

        int currentSecond = Mathf.CeilToInt(currentTime);
        if (currentSecond != lastDisplayedSecond)
        {
            lastDisplayedSecond = currentSecond;
            RefreshTimeUI();
        }
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        if (clampToZero && currentScore < 0)
            currentScore = 0;

        if (isTimerRunning)
        {
            float timeModification = amount * secondsPerPoint;
            currentTime += timeModification;

            if (currentTime < 0f) currentTime = 0f;

            lastDisplayedSecond = Mathf.CeilToInt(currentTime);
            RefreshTimeUI();
        }

        PlayerProfile.TrySaveBestScore(currentScore);

        RefreshScoreUI();

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

    private void RefreshScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Puntos: {currentScore}";
    }

    private void RefreshTimeUI()
    {
        if (timeText != null)
        {
            // Formateo limpio a MM:SS
            int minutes = Mathf.FloorToInt(currentTime / 60F);
            int seconds = Mathf.FloorToInt(currentTime - minutes * 60);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
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