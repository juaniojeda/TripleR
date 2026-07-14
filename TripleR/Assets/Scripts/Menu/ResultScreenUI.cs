using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ResultScreenUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject initialPanel;
    [SerializeField] private GameObject resultPanel;

    [Header("Texts")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text correctText;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private TMP_Text effectivenessText;
    [SerializeField] private TMP_Text starsText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text performanceLabelText;
    [SerializeField] private TMP_Text resultTitleText;

    [Header("Stars")]
    [SerializeField] private Image starsBackground;
    [SerializeField] private Image starsFill;

    private void Awake()
    {
        ConfigureStarsFill();
    }

    private void OnEnable()
    {
        ConfigureStarsFill();
        Refresh();
        ShowInitialPanel();
    }

    public void ShowInitialPanel()
    {
        SetPanel(initialPanel, true);
        SetPanel(resultPanel, false);
    }

    public void ShowResultPanel()
    {
        Refresh();
        SetPanel(initialPanel, false);
        SetPanel(resultPanel, true);
    }

    public void Refresh()
    {
        // La partida ya cambió de escena; PlayerPrefs conserva el resultado de ScoreManager.
        int score = PlayerPrefs.GetInt(PerformanceSessionStorage.LastScoreKey, 0);
        int correct = PlayerPrefs.GetInt(PerformanceSessionStorage.LastCorrectCountKey, 0);
        int errors = PlayerPrefs.GetInt(PerformanceSessionStorage.LastErrorCountKey, 0);
        float effectiveness = PlayerPrefs.GetFloat(PerformanceSessionStorage.LastEffectivenessKey, 0f);
        int stars = PlayerPrefs.GetInt(PerformanceSessionStorage.LastStarsKey, 0);
        int maxScoreForStars = PlayerPrefs.GetInt(PerformanceSessionStorage.LastMaxScoreForStarsKey, 1000);
        int maxStars = PlayerPrefs.GetInt(PerformanceSessionStorage.LastMaxStarsKey, ScoreStarsFormatter.MaxStars);
        int coins = PlayerPrefs.GetInt(PerformanceSessionStorage.LastCoinsEarnedKey, 0);
        string label = PlayerPrefs.GetString(PerformanceSessionStorage.LastPerformanceLabelKey, "Rendimiento Insuficiente");
        bool wasWin = PlayerPrefs.GetInt(PerformanceSessionStorage.LastWasWinKey, 0) == 1;
        float starsFillAmount = ScoreStarsCalculator.CalculateFillAmount(score, maxScoreForStars);

        SetText(resultTitleText, wasWin ? "Victoria" : "Derrota");
        SetText(scoreText, score.ToString());
        SetText(correctText, correct.ToString());
        SetText(errorText, errors.ToString());
        SetText(effectivenessText, $"{effectiveness:0.#}%");
        SetText(starsText, ScoreStarsFormatter.BuildStarsText(stars, maxStars));
        SetText(coinsText, coins.ToString());
        SetText(performanceLabelText, label);
        RefreshStarsFill(starsFillAmount);
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null)
            target.text = value;
    }

    private static void SetPanel(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }

    private void RefreshStarsFill(float fillAmount)
    {
        if (starsBackground != null)
            starsBackground.fillAmount = 1f;

        if (starsFill != null)
            starsFill.fillAmount = Mathf.Clamp01(fillAmount);
    }

    private void ConfigureStarsFill()
    {
        if (starsFill == null)
            return;

        starsFill.type = Image.Type.Filled;
        starsFill.fillMethod = Image.FillMethod.Horizontal;
        starsFill.fillOrigin = (int)Image.OriginHorizontal.Left;
    }
}
