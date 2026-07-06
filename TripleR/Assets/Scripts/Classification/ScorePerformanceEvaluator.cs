using UnityEngine;

public static class ScorePerformanceEvaluator
{
    public static PerformanceResult Evaluate(int score, int maxScoreForStars, int maxStars)
    {
        int stars = ScoreStarsCalculator.CalculateStars(score, maxScoreForStars, maxStars);
        float effectiveness = ScoreStarsCalculator.CalculateEffectivenessPercent(score, maxScoreForStars);
        string label = BuildLabel(stars, maxStars);
        int baseCoins = CalculateBaseCoins(stars, maxStars);

        return new PerformanceResult(effectiveness, stars, label, baseCoins);
    }

    private static string BuildLabel(int stars, int maxStars)
    {
        float progress = GetStarProgress(stars, maxStars);

        if (progress >= 1f)
            return "Rendimiento Excelente (Perfecto)";

        if (progress >= 0.8f)
            return "Rendimiento Destacado";

        if (progress >= 0.6f)
            return "Rendimiento Estandar";

        if (progress >= 0.4f)
            return "Rendimiento Basico";

        return "Rendimiento Insuficiente";
    }

    private static int CalculateBaseCoins(int stars, int maxStars)
    {
        float progress = GetStarProgress(stars, maxStars);
        int equivalentFiveStarRating = Mathf.RoundToInt(progress * ScoreStarsFormatter.MaxStars);

        return Mathf.Max(10, equivalentFiveStarRating * 10);
    }

    private static float GetStarProgress(int stars, int maxStars)
    {
        int safeMaxStars = Mathf.Max(1, maxStars);
        int clampedStars = ScoreStarsFormatter.Clamp(stars, safeMaxStars);

        return Mathf.Clamp01((float)clampedStars / safeMaxStars);
    }
}
