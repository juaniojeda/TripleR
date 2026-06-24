using UnityEngine;

public static class PerformanceSessionStorage
{
    public const string LastScoreKey = "LastScore";
    public const string LastCorrectCountKey = "LastCorrectCount";
    public const string LastErrorCountKey = "LastErrorCount";
    public const string LastWasteGeneratedKey = "LastWasteGenerated";
    public const string LastEffectivenessKey = "LastEffectiveness";
    public const string LastStarsKey = "LastStars";
    public const string LastPerformanceLabelKey = "LastPerformanceLabel";
    public const string LastCoinsEarnedKey = "LastCoinsEarned";
    public const string LastWasWinKey = "LastWasWin";
    public const string TrashLevelKey = "TrashLevel";

    public static void SaveLastSession(int score, int correctCount, int errorCount, int totalWasteGenerated, PerformanceResult result, int coinsEarned)
    {
        PlayerPrefs.SetInt(LastScoreKey, score);
        PlayerPrefs.SetInt(LastCorrectCountKey, correctCount);
        PlayerPrefs.SetInt(LastErrorCountKey, errorCount);
        PlayerPrefs.SetInt(LastWasteGeneratedKey, totalWasteGenerated);
        PlayerPrefs.SetFloat(LastEffectivenessKey, result.Effectiveness);
        PlayerPrefs.SetInt(LastStarsKey, result.Stars);
        PlayerPrefs.SetString(LastPerformanceLabelKey, result.Label);
        PlayerPrefs.SetInt(LastCoinsEarnedKey, coinsEarned);
        PlayerPrefs.SetInt(LastWasWinKey, result.IsWin ? 1 : 0);
    }

    public static void UpdateTrashLevel(int stars)
    {
        int currentLevel = PlayerPrefs.GetInt(TrashLevelKey, 0);
        int delta = GetTrashDelta(stars);
        PlayerPrefs.SetInt(TrashLevelKey, Mathf.Max(0, currentLevel + delta));
    }

    private static int GetTrashDelta(int stars)
    {
        switch (stars)
        {
            case 1:
                return 3;
            case 2:
                return 2;
            case 3:
                return -1;
            case 4:
                return -2;
            case 5:
                return -3;
            default:
                return 0;
        }
    }
}
