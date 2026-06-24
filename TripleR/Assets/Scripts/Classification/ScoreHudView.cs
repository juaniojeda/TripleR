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

        if (starsBackground != null)
            starsBackground.fillAmount = 1f;

        if (starsFill != null)
            starsFill.fillAmount = fillAmount;
    }

    public void RefreshCombo(int combo, float multiplier)
    {
        if (comboText != null)
            comboText.text = $"Combo: {combo}\nx{multiplier:0.#}";
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
