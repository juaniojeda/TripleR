using TMPro;
using UnityEngine;

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

    private void OnEnable()
    {
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
        int score = PlayerPrefs.GetInt(PerformanceSessionStorage.LastScoreKey, 0);
        int correct = PlayerPrefs.GetInt(PerformanceSessionStorage.LastCorrectCountKey, 0);
        int errors = PlayerPrefs.GetInt(PerformanceSessionStorage.LastErrorCountKey, 0);
        float effectiveness = PlayerPrefs.GetFloat(PerformanceSessionStorage.LastEffectivenessKey, 0f);
        int stars = PlayerPrefs.GetInt(PerformanceSessionStorage.LastStarsKey, 1);
        int coins = PlayerPrefs.GetInt(PerformanceSessionStorage.LastCoinsEarnedKey, 0);
        string label = PlayerPrefs.GetString(PerformanceSessionStorage.LastPerformanceLabelKey, "Rendimiento Insuficiente");
        bool wasWin = PlayerPrefs.GetInt(PerformanceSessionStorage.LastWasWinKey, 0) == 1;

        SetText(resultTitleText, wasWin ? "Victoria" : "Derrota");
        SetText(scoreText, score.ToString());
        SetText(correctText, correct.ToString());
        SetText(errorText, errors.ToString());
        SetText(effectivenessText, $"{effectiveness:0.#}%");
        SetText(starsText, Mathf.Clamp(stars, 1, 5).ToString());
        SetText(coinsText, coins.ToString());
        SetText(performanceLabelText, label);
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

    private static string BuildStarsText(int stars)
    {
        int clampedStars = Mathf.Clamp(stars, 1, 5);
        string result = string.Empty;

        for (int i = 0; i < 5; i++)
            result += i < clampedStars ? "\u2605" : "\u2606";

        return result;
    }
}