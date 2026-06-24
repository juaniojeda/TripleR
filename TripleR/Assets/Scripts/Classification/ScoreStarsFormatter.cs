using UnityEngine;

public static class ScoreStarsFormatter
{
    public const int MinStars = 1;
    public const int MaxStars = 5;

    public static int Clamp(int stars)
    {
        return Mathf.Clamp(stars, MinStars, MaxStars);
    }

    public static string BuildStarsText(int stars)
    {
        int clampedStars = Clamp(stars);
        string result = string.Empty;

        for (int i = 0; i < MaxStars; i++)
            result += i < clampedStars ? "\u2605" : "\u2606";

        return result;
    }
}
