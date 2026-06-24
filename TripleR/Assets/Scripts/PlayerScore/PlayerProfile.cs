using UnityEngine;

public static class PlayerProfile
{
    private const string BestScoreKey = "BestScore";
    private const string CoinsKey = "Coins";
    private const string LicensePrefix = "License_";

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

    public static bool TrySpendCoins(int amount)
    {
        if (amount < 0)
            return false;

        if (Coins < amount)
            return false;

        PlayerPrefs.SetInt(CoinsKey, Coins - amount);
        PlayerPrefs.Save();
        return true;
    }

    public static bool HasLicense(string licenseId)
    {
        if (string.IsNullOrWhiteSpace(licenseId))
            return false;

        return PlayerPrefs.GetInt(GetLicenseKey(licenseId), 0) == 1;
    }

    public static bool TryBuyLicense(string licenseId, int price)
    {
        if (string.IsNullOrWhiteSpace(licenseId))
            return false;

        if (HasLicense(licenseId))
            return true;

        if (price < 0)
            price = 0;

        int currentCoins = Coins;

        if (currentCoins < price)
            return false;

        PlayerPrefs.SetInt(CoinsKey, currentCoins - price);
        PlayerPrefs.SetInt(GetLicenseKey(licenseId), 1);
        PlayerPrefs.Save();

        return true;
    }

    public static void ResetProfile()
    {
        PlayerPrefs.DeleteKey(BestScoreKey);
        PlayerPrefs.DeleteKey(CoinsKey);
        PlayerPrefs.Save();
    }

    private static string GetLicenseKey(string licenseId)
    {
        return LicensePrefix + licenseId;
    }
}