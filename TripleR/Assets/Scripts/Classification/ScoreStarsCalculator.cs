using UnityEngine;

public static class ScoreStarsCalculator
{
    public static float CalculateFillAmount(int score, int maxScoreForStars)
    {
        int safeMaxScore = Mathf.Max(1, maxScoreForStars);
        int safeScore = Mathf.Max(0, score);

        return Mathf.Clamp01((float)safeScore / safeMaxScore);
    }

    public static float CalculateEffectivenessPercent(int score, int maxScoreForStars)
    {
        return CalculateFillAmount(score, maxScoreForStars) * 100f;
    }

    public static int CalculateStars(int score, int maxScoreForStars, int maxStars)
    {
        int safeMaxScore = Mathf.Max(1, maxScoreForStars);
        int safeMaxStars = Mathf.Max(1, maxStars);
        int safeScore = Mathf.Max(0, score);
        float pointsPerStar = (float)safeMaxScore / safeMaxStars;

        return Mathf.Clamp(Mathf.FloorToInt(safeScore / pointsPerStar), 0, safeMaxStars);
    }
}
