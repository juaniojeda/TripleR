using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class ScoreManager : MonoBehaviour
{
    [SerializeField] private TutorialPagesUI tutorialUI;

    [Header("UI - Puntuacion y Estrellas")]
    [SerializeField] private Text scoreText;
    [SerializeField] private bool clampToZero = true;
    [SerializeField] private Canvas starsCanvas;
    [SerializeField] private Text starsText;

    [Header("UI - Combos")]
    [SerializeField] private Canvas comboCanvas;
    [SerializeField] private Text comboText;

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
    private bool coinsClaimed;
    private bool sessionFinished;
    private int pointCoinsEarnedThisSession;
    private int correctCount;
    private int errorCount;
    private int totalWasteGenerated;
    private int currentCombo;
    private float currentMultiplier = 1f;

    private ScoreTimer scoreTimer;
    private ScoreHudView hudView;
    private ScoreTextAnimator scoreTextAnimator;
    private bool initialized;

    public int CurrentScore => currentScore;
    public float CurrentTime => scoreTimer?.CurrentTime ?? initialTime;
    public int CorrectCount => correctCount;
    public int ErrorCount => errorCount;
    public int TotalWasteGenerated => totalWasteGenerated;
    public int CurrentCombo => currentCombo;
    public float CurrentMultiplier => currentMultiplier;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void Update()
    {
        EnsureInitialized();

        if (scoreTimer == null || !scoreTimer.IsRunning)
            return;

        bool shouldRefreshTime = scoreTimer.Tick(Time.deltaTime, out bool finished);

        if (shouldRefreshTime)
            hudView.RefreshTime(scoreTimer.CurrentTime);

        if (finished)
            FinishSession();
    }

    public void RegisterWasteGenerated()
    {
        RegisterWasteGenerated(1);
    }

    public void RegisterWasteGenerated(int amount)
    {
        EnsureInitialized();

        if (amount <= 0)
            return;

        totalWasteGenerated += amount;
        RefreshStarsPreview();
    }

    public int AddCorrectClassification()
    {
        return AddCorrectClassification(correctPoints);
    }

    public int AddCorrectClassification(int basePoints)
    {
        EnsureInitialized();

        correctCount++;
        currentCombo++;
        currentMultiplier = ScoreComboCalculator.Calculate(currentCombo, comboStep, comboMultiplierStep, maxComboMultiplier);

        int points = Mathf.RoundToInt(basePoints * currentMultiplier);
        AddScore(points);
        RefreshStarsPreview();
        return points;
    }

    public int AddWrongClassification()
    {
        return AddWrongClassification(wrongPoints);
    }

    public int AddWrongClassification(int penaltyPoints)
    {
        EnsureInitialized();

        errorCount++;
        currentCombo = 0;
        currentMultiplier = 1f;

        int points = -Mathf.Abs(penaltyPoints);
        AddScore(points);
        RefreshStarsPreview();
        return points;
    }

    public int AddClassificationResult(bool isSuccessful, int correctPointsAmount, int wrongPointsAmount)
    {
        return isSuccessful
            ? AddCorrectClassification(correctPointsAmount)
            : AddWrongClassification(wrongPointsAmount);
    }

    public void AddScore(int amount)
    {
        EnsureInitialized();

        currentScore += amount;

        if (clampToZero && currentScore < 0)
            currentScore = 0;

        if (scoreTimer != null && scoreTimer.IsRunning)
        {
            scoreTimer.AddSeconds(amount * secondsPerPoint);
            hudView.RefreshTime(scoreTimer.CurrentTime);
        }

        PlayerProfile.TrySaveBestScore(currentScore);
        hudView.RefreshScore(currentScore);
        hudView.RefreshCombo(currentCombo, currentMultiplier);
        scoreTextAnimator.Play(amount, bumpScale, bumpDuration, shakeMagnitude, shakeDuration);
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
        EnsureInitialized();

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
        EnsureInitialized();

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

        hudView.RefreshStars(result.Stars);

        if (result.IsWin)
            OnWin?.Invoke();
        else
            OnLose?.Invoke();
    }

    private void EnsureInitialized()
    {
        if (initialized)
            return;

        scoreTimer = new ScoreTimer(initialTime);
        hudView = new ScoreHudView(scoreText, timeText, starsCanvas, starsText, comboCanvas, comboText);
        scoreTextAnimator = new ScoreTextAnimator(this, scoreText);
        currentMultiplier = 1f;
        initialized = true;

        hudView.RefreshScore(currentScore);
        hudView.RefreshTime(scoreTimer.CurrentTime);
        hudView.RefreshCombo(currentCombo, currentMultiplier);
        RefreshStarsPreview();
    }

    private void RefreshStarsPreview()
    {
        if (hudView == null)
            return;

        PerformanceResult result = PerformanceEvaluator.Evaluate(correctCount, errorCount, totalWasteGenerated);
        hudView.RefreshStars(result.Stars);
    }

    private void OnEnable()
    {
        if (tutorialUI != null)
            tutorialUI.OnTutorialFinished += IniciarReloj;
    }

    private void OnDisable()
    {
        if (tutorialUI != null)
            tutorialUI.OnTutorialFinished -= IniciarReloj;
    }

    private void IniciarReloj()
    {
        if (scoreTimer != null)
            scoreTimer.TurnOn();
    }
}
