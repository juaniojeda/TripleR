using UnityEngine;
using UnityEngine.UI;

public sealed class ScoreHudView
{
    private readonly Text scoreText;
    private readonly Text timeText;
    private readonly Canvas starsCanvas;
    private readonly Canvas comboCanvas;
    private Text starsText;
    private Text comboText;

    public ScoreHudView(Text scoreText, Text timeText, Canvas starsCanvas, Text starsText, Canvas comboCanvas, Text comboText)
    {
        this.scoreText = scoreText;
        this.timeText = timeText;
        this.starsCanvas = starsCanvas;
        this.starsText = starsText;
        this.comboCanvas = comboCanvas;
        this.comboText = comboText;

        if (this.starsText == null && this.starsCanvas != null)
            this.starsText = this.starsCanvas.GetComponentInChildren<Text>(true);

        if (this.comboText == null && this.comboCanvas != null)
            this.comboText = this.comboCanvas.GetComponentInChildren<Text>(true);
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
        if (starsText != null)
            starsText.text = ScoreStarsFormatter.BuildStarsText(stars);
    }

    public void RefreshCombo(int combo, float multiplier)
    {
        if (comboText != null)
            comboText.text = $"Combo: {combo}\nx{multiplier:0.#}";
    }
}
