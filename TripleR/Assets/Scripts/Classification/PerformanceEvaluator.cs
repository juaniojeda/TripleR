using UnityEngine;

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
