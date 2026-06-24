using UnityEngine;

public static class ScoreComboCalculator
{
    public static float Calculate(int currentCombo, int comboStep, float comboMultiplierStep, float maxComboMultiplier)
    {
        int safeComboStep = Mathf.Max(1, comboStep);
        float multiplier = 1f + Mathf.Floor((float)currentCombo / safeComboStep) * comboMultiplierStep;
        return Mathf.Clamp(multiplier, 1f, maxComboMultiplier);
    }
}
