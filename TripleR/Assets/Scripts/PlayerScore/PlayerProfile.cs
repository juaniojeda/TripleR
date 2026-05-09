using UnityEngine;

public static class PlayerProfile
{
    private const string BestScoreKey = "BestScore";

    public static int BestScore => PlayerPrefs.GetInt(BestScoreKey, 0);

    public static bool TrySaveBestScore(int score)
    {
        if (score <= BestScore)
            return false;

        PlayerPrefs.SetInt(BestScoreKey, score);
        PlayerPrefs.Save();
        return true;
    }

    public static void ResetBestScore()
    {
        PlayerPrefs.DeleteKey(BestScoreKey);
        PlayerPrefs.Save();
    }
}