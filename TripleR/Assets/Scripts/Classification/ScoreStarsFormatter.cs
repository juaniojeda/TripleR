using UnityEngine;

public static class ScoreStarsFormatter
{
    public const int MinStars = 0;
    public const int MaxStars = 5;

    public static int Clamp(int stars)
    {
        return Clamp(stars, MaxStars);
    }

    public static int Clamp(int stars, int maxStars)
    {
        int safeMaxStars = Mathf.Max(MinStars, maxStars);
        return Mathf.Clamp(stars, MinStars, safeMaxStars);
    }

    public static string BuildStarsText(int stars)
    {
        return BuildStarsText(stars, MaxStars);
    }

    public static string BuildStarsText(int stars, int maxStars)
    {
        int safeMaxStars = Mathf.Max(MinStars, maxStars);
        int clampedStars = Clamp(stars, safeMaxStars);
        string result = string.Empty;

        for (int i = 0; i < safeMaxStars; i++)
            result += i < clampedStars ? "\u2605" : "\u2606";

        return result;
    }
}
