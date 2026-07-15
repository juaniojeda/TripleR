using UnityEngine;

public static class PerformanceSessionStorage
{
    public const string LastScoreKey = "LastScore";
    public const string LastCorrectCountKey = "LastCorrectCount";
    public const string LastErrorCountKey = "LastErrorCount";
    public const string LastWasteGeneratedKey = "LastWasteGenerated";
    public const string LastEffectivenessKey = "LastEffectiveness";
    public const string LastStarsKey = "LastStars";
    public const string LastMaxScoreForStarsKey = "LastMaxScoreForStars";
    public const string LastMaxStarsKey = "LastMaxStars";
    public const string LastPerformanceLabelKey = "LastPerformanceLabel";
    public const string LastCoinsEarnedKey = "LastCoinsEarned";
    public const string LastWasWinKey = "LastWasWin";
    public const string TrashLevelKey = "TrashLevel";

    // PlayerPrefs funciona como puente entre Game y la escena de resultados.
    public static void SaveLastSession(int score, int correctCount, int errorCount, int totalWasteGenerated, PerformanceResult result, int coinsEarned)
    {
        SaveLastSession(score, correctCount, errorCount, totalWasteGenerated, result, coinsEarned, 1000, ScoreStarsFormatter.MaxStars);
    }

    public static void SaveLastSession(int score, int correctCount, int errorCount, int totalWasteGenerated, PerformanceResult result, int coinsEarned, int maxScoreForStars, int maxStars)
    {
        PlayerPrefs.SetInt(LastScoreKey, score);
        PlayerPrefs.SetInt(LastCorrectCountKey, correctCount);
        PlayerPrefs.SetInt(LastErrorCountKey, errorCount);
        PlayerPrefs.SetInt(LastWasteGeneratedKey, totalWasteGenerated);
        PlayerPrefs.SetFloat(LastEffectivenessKey, result.Effectiveness);
        PlayerPrefs.SetInt(LastStarsKey, ScoreStarsFormatter.Clamp(result.Stars, maxStars));
        PlayerPrefs.SetInt(LastMaxScoreForStarsKey, Mathf.Max(1, maxScoreForStars));
        PlayerPrefs.SetInt(LastMaxStarsKey, Mathf.Max(1, maxStars));
        PlayerPrefs.SetString(LastPerformanceLabelKey, result.Label);
        PlayerPrefs.SetInt(LastCoinsEarnedKey, coinsEarned);
        PlayerPrefs.SetInt(LastWasWinKey, result.IsWin ? 1 : 0);
    }

    public static void UpdateTrashLevel(int stars)
    {
        int currentLevel = PlayerPrefs.GetInt(TrashLevelKey, 0);
        int delta = GetTrashDelta(ScoreStarsFormatter.Clamp(stars));
        PlayerPrefs.SetInt(TrashLevelKey, Mathf.Max(0, currentLevel + delta));
    }

    private static int GetTrashDelta(int stars)
    {
        // Un mal resultado acumula basura en el menú; desde tres estrellas empieza a limpiarla.
        switch (stars)
        {
            case 0:
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
