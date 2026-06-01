using UnityEngine;

public static class PlayerProfile
{
    private const string BestScoreKey = "BestScore";
    private const string CoinsKey = "Coins";

    public static int BestScore => PlayerPrefs.GetInt(BestScoreKey, 0);
    public static int Coins => PlayerPrefs.GetInt(CoinsKey, 0);

    public static bool TrySaveBestScore(int score)
    {
        if (score <= BestScore)
            return false;

        PlayerPrefs.SetInt(BestScoreKey, score);
        PlayerPrefs.Save();
        return true;
    }

    public static void AddCoins(int amount)
    {
        if (amount <= 0)
            return;

        PlayerPrefs.SetInt(CoinsKey, Coins + amount);
        PlayerPrefs.Save();
    }

    public static void ResetProfile()
    {
        PlayerPrefs.DeleteKey(BestScoreKey);
        PlayerPrefs.DeleteKey(CoinsKey);
        PlayerPrefs.Save();
    }
}

//using UnityEngine;

//public static class PlayerProfile
//{
//    private const string BestScoreKey = "BestScore";

//    public static int BestScore => PlayerPrefs.GetInt(BestScoreKey, 0);

//    public static bool TrySaveBestScore(int score)
//    {
//        if (score <= BestScore)
//            return false;

//        PlayerPrefs.SetInt(BestScoreKey, score);
//        PlayerPrefs.Save();
//        return true;
//    }

//    public static void ResetBestScore()
//    {
//        PlayerPrefs.DeleteKey(BestScoreKey);
//        PlayerPrefs.Save();
//    }
//}