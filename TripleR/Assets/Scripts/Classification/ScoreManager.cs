using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class ScoreManager : MonoBehaviour
{
    [Header("UI - Puntuacion")]
    [SerializeField] private Text scoreText;
    [SerializeField] private bool clampToZero = true;

    [Header("UI - Temporizador")]
    [SerializeField] private Text timeText;
    [SerializeField] private float initialTime = 60f;
    [SerializeField] private float secondsPerPoint = 1f;

    [Header("Clasificacion")]
    [SerializeField] private int correctPoints = 10;
    [SerializeField] private int wrongPoints = 5;

    [Header("Rachas")]
    [SerializeField, Min(1)] private int comboStep = 3;
    [SerializeField, Min(0f)] private float comboMultiplierStep = 0.5f;
    [SerializeField, Min(1f)] private float maxComboMultiplier = 3f;

    [Header("Monedas")]
    [SerializeField] private int pointsPerCoin = 30;

    [Header("Eventos de Juego")]
    public UnityEvent OnWin;
    public UnityEvent OnLose;

    [Header("Animacion Positiva (Bump)")]
    [SerializeField] private float bumpScale = 1.4f;
    [SerializeField] private float bumpDuration = 0.25f;

    [Header("Animacion Negativa (Shake)")]
    [SerializeField] private float shakeMagnitude = 8f;
    [SerializeField] private float shakeDuration = 0.35f;

    private int currentScore;
    private float currentTime;
    private bool isTimerRunning;
    private int lastDisplayedSecond = -1;
    private bool coinsClaimed;
    private bool sessionFinished;
    private int pointCoinsEarnedThisSession;
    private int correctCount;
    private int errorCount;
    private int totalWasteGenerated;
    private int currentCombo;
    private float currentMultiplier = 1f;

    public int CurrentScore => currentScore;
    public float CurrentTime => currentTime;
    public int CorrectCount => correctCount;
    public int ErrorCount => errorCount;
    public int TotalWasteGenerated => totalWasteGenerated;
    public int CurrentCombo => currentCombo;
    public float CurrentMultiplier => currentMultiplier;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Coroutine activeAnimation;

    private void Awake()
    {
        if (scoreText != null)
        {
            originalScale = scoreText.transform.localScale;
            originalPosition = scoreText.transform.localPosition;
        }

        currentTime = initialTime;
        isTimerRunning = true;
        currentMultiplier = 1f;

        RefreshScoreUI();
        RefreshTimeUI();
    }

    private void Update()
    {
        if (!isTimerRunning)
            return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isTimerRunning = false;

            RefreshTimeUI();
            FinishSession();
            return;
        }

        int currentSecond = Mathf.CeilToInt(currentTime);

        if (currentSecond != lastDisplayedSecond)
        {
            lastDisplayedSecond = currentSecond;
            RefreshTimeUI();
        }
    }

    public void RegisterWasteGenerated()
    {
        RegisterWasteGenerated(1);
    }

    public void RegisterWasteGenerated(int amount)
    {
        if (amount <= 0)
            return;

        totalWasteGenerated += amount;
    }

    public int AddCorrectClassification()
    {
        correctCount++;
        currentCombo++;
        currentMultiplier = CalculateCurrentMultiplier();

        int points = Mathf.RoundToInt(correctPoints * currentMultiplier);
        AddScore(points);
        return points;
    }

    public int AddWrongClassification()
    {
        errorCount++;
        currentCombo = 0;
        currentMultiplier = 1f;

        int points = -Mathf.Abs(wrongPoints);
        AddScore(points);
        return points;
    }

    public void AddScore(int amount)
    {
        currentScore += amount;

        if (clampToZero && currentScore < 0)
            currentScore = 0;

        if (isTimerRunning)
        {
            currentTime += amount * secondsPerPoint;

            if (currentTime < 0f)
                currentTime = 0f;

            lastDisplayedSecond = Mathf.CeilToInt(currentTime);
            RefreshTimeUI();
        }

        PlayerProfile.TrySaveBestScore(currentScore);
        RefreshScoreUI();

        PlayScoreAnimation(amount);
    }

    public void SumarPuntos(int amount)
    {
        AddScore(amount);
    }

    public void RestarPuntos(int amount)
    {
        AddScore(-Mathf.Abs(amount));
    }

    public void RemoveScore(int amount)
    {
        AddScore(-Mathf.Abs(amount));
    }

    public int ClaimCoins()
    {
        if (coinsClaimed)
            return 0;

        coinsClaimed = true;

        if (pointsPerCoin <= 0)
            return 0;

        pointCoinsEarnedThisSession = Mathf.Max(0, currentScore / pointsPerCoin);
        PlayerProfile.AddCoins(pointCoinsEarnedThisSession);

        return pointCoinsEarnedThisSession;
    }

    private void FinishSession()
    {
        if (sessionFinished)
            return;

        sessionFinished = true;

        ClaimCoins();

        PerformanceResult result = PerformanceEvaluator.Evaluate(correctCount, errorCount, totalWasteGenerated);
        int totalCoinsEarned = pointCoinsEarnedThisSession + result.BaseCoins;

        PlayerProfile.AddCoins(result.BaseCoins);
        PerformanceSessionStorage.SaveLastSession(currentScore, correctCount, errorCount, totalWasteGenerated, result, totalCoinsEarned);
        PerformanceSessionStorage.UpdateTrashLevel(result.Stars);
        PlayerPrefs.Save();

        if (result.IsWin)
            OnWin?.Invoke();
        else
            OnLose?.Invoke();
    }

    private float CalculateCurrentMultiplier()
    {
        int safeComboStep = Mathf.Max(1, comboStep);
        float multiplier = 1f + Mathf.Floor(currentCombo / safeComboStep) * comboMultiplierStep;
        return Mathf.Clamp(multiplier, 1f, maxComboMultiplier);
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
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime - minutes * 60);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void PlayScoreAnimation(int amount)
    {
        if (scoreText == null)
            return;

        if (activeAnimation != null)
        {
            StopCoroutine(activeAnimation);
            scoreText.transform.localScale = originalScale;
            scoreText.transform.localPosition = originalPosition;
        }

        if (amount > 0)
            activeAnimation = StartCoroutine(BumpAnimation());
        else if (amount < 0)
            activeAnimation = StartCoroutine(ShakeAnimation());
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
            scoreText.transform.localScale = originalScale * scale;
            yield return null;
        }

        t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            float progress = t / half;
            float scale = Mathf.Lerp(bumpScale, 1f, Mathf.SmoothStep(0f, 1f, progress));
            scoreText.transform.localScale = originalScale * scale;
            yield return null;
        }

        scoreText.transform.localScale = originalScale;
        activeAnimation = null;
    }

    private IEnumerator ShakeAnimation()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float damping = 1f - elapsed / shakeDuration;
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude * damping;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude * damping;

            scoreText.transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
            yield return null;
        }

        scoreText.transform.localPosition = originalPosition;
        activeAnimation = null;
    }
}

public readonly struct PerformanceResult
{
    public PerformanceResult(float effectiveness, int stars, string label, int baseCoins)
    {
        Effectiveness = effectiveness;
        Stars = stars;
        Label = label;
        BaseCoins = baseCoins;
    }

    public float Effectiveness { get; }
    public int Stars { get; }
    public string Label { get; }
    public int BaseCoins { get; }
    public bool IsWin => Stars >= 3;
}

public static class PerformanceEvaluator
{
    public static PerformanceResult Evaluate(int correctCount, int errorCount, int totalWasteGenerated)
    {
        float effectiveness = 0f;

        if (totalWasteGenerated > 0)
            effectiveness = ((float)(correctCount - errorCount) / totalWasteGenerated) * 100f;

        effectiveness = Mathf.Clamp(effectiveness, 0f, 100f);

        if (effectiveness >= 90f)
            return new PerformanceResult(effectiveness, 5, "Rendimiento Excelente (Perfecto)", 50);

        if (effectiveness >= 75f)
            return new PerformanceResult(effectiveness, 4, "Rendimiento Destacado", 40);

        if (effectiveness >= 60f)
            return new PerformanceResult(effectiveness, 3, "Rendimiento Est\u00e1ndar", 30);

        if (effectiveness >= 40f)
            return new PerformanceResult(effectiveness, 2, "Rendimiento B\u00e1sico", 20);

        return new PerformanceResult(effectiveness, 1, "Rendimiento Insuficiente", 10);
    }
}

public static class PerformanceSessionStorage
{
    public const string LastScoreKey = "LastScore";
    public const string LastCorrectCountKey = "LastCorrectCount";
    public const string LastErrorCountKey = "LastErrorCount";
    public const string LastWasteGeneratedKey = "LastWasteGenerated";
    public const string LastEffectivenessKey = "LastEffectiveness";
    public const string LastStarsKey = "LastStars";
    public const string LastPerformanceLabelKey = "LastPerformanceLabel";
    public const string LastCoinsEarnedKey = "LastCoinsEarned";
    public const string LastWasWinKey = "LastWasWin";
    public const string TrashLevelKey = "TrashLevel";

    public static void SaveLastSession(int score, int correctCount, int errorCount, int totalWasteGenerated, PerformanceResult result, int coinsEarned)
    {
        PlayerPrefs.SetInt(LastScoreKey, score);
        PlayerPrefs.SetInt(LastCorrectCountKey, correctCount);
        PlayerPrefs.SetInt(LastErrorCountKey, errorCount);
        PlayerPrefs.SetInt(LastWasteGeneratedKey, totalWasteGenerated);
        PlayerPrefs.SetFloat(LastEffectivenessKey, result.Effectiveness);
        PlayerPrefs.SetInt(LastStarsKey, result.Stars);
        PlayerPrefs.SetString(LastPerformanceLabelKey, result.Label);
        PlayerPrefs.SetInt(LastCoinsEarnedKey, coinsEarned);
        PlayerPrefs.SetInt(LastWasWinKey, result.IsWin ? 1 : 0);
    }

    public static void UpdateTrashLevel(int stars)
    {
        int currentLevel = PlayerPrefs.GetInt(TrashLevelKey, 0);
        int delta = GetTrashDelta(stars);
        PlayerPrefs.SetInt(TrashLevelKey, Mathf.Max(0, currentLevel + delta));
    }

    private static int GetTrashDelta(int stars)
    {
        switch (stars)
        {
            case 1:
                return 3;
            case 2:
                return 2;
            case 3:
                return -1;
            case 4:
                return -2;
            case 5:
                return -3;
            default:
                return 0;
        }
    }
}