using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public sealed class ScoreHudView
{
    private readonly Text scoreText;
    private readonly Text timeText;
    private readonly Image starsBackground;
    private readonly Image starsFill;
    private readonly Canvas comboCanvas;
    private Text comboText;

    public ScoreHudView(Text scoreText, Text timeText, Image starsBackground, Image starsFill, Canvas comboCanvas, Text comboText)
    {
        this.scoreText = scoreText;
        this.timeText = timeText;
        this.starsBackground = starsBackground;
        this.starsFill = starsFill;
        this.comboCanvas = comboCanvas;
        this.comboText = comboText;

        if (this.comboText == null && this.comboCanvas != null)
            this.comboText = this.comboCanvas.GetComponentInChildren<Text>(true);

        ConfigureStarsFill();
    }

    public void RefreshScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Puntos: {score}";
    }

    public void RefreshTime(float currentTime)
    {
        if (timeText == null)
            return;

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime - minutes * 60);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void RefreshStars(int stars)
    {
        int clampedStars = ScoreStarsFormatter.Clamp(stars);
        float fillAmount = (float)clampedStars / ScoreStarsFormatter.MaxStars;
        RefreshStarsFill(fillAmount);
    }

    public void RefreshStarsFill(float fillAmount)
    {
        if (starsBackground != null)
            starsBackground.fillAmount = 1f;

        if (starsFill != null)
            starsFill.fillAmount = Mathf.Clamp01(fillAmount);
    }

    public void RefreshCombo(int combo, float multiplier)
    {
        _ = combo;

        if (comboText != null)
            comboText.text = FormatMultiplier(multiplier);
    }

    private static string FormatMultiplier(float multiplier)
    {
        float safeMultiplier = Mathf.Max(1f, multiplier);
        return safeMultiplier.ToString("0.#", CultureInfo.InvariantCulture);
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
