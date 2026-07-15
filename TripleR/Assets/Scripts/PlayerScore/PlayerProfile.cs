using UnityEngine;

public static class PlayerProfile
{
    private const string BestScoreKey = "BestScore";
    private const string CoinsKey = "Coins";
    private const string LicensePrefix = "License_";
    private const string SkinPrefix = "Skin_";
    private const string SkinActivePrefix = "SkinActive_";

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

    public static bool HasSkin(string skinId)
    {
        if (string.IsNullOrWhiteSpace(skinId))
            return false;

        return PlayerPrefs.GetInt(GetSkinKey(skinId), 0) == 1;
    }

    public static bool TryBuySkin(string skinId, int price)
    {
        if (string.IsNullOrWhiteSpace(skinId))
            return false;

        if (HasSkin(skinId))
            return true;

        if (price < 0)
            price = 0;

        int currentCoins = Coins;

        if (currentCoins < price)
            return false;

        PlayerPrefs.SetInt(CoinsKey, currentCoins - price);
        PlayerPrefs.SetInt(GetSkinKey(skinId), 1);
        PlayerPrefs.SetInt(GetSkinActiveKey(skinId), 1);
        PlayerPrefs.Save();

        return true;
    }

    public static bool IsSkinActive(string skinId)
    {
        if (!HasSkin(skinId))
            return false;

        string activeKey = GetSkinActiveKey(skinId);

        // Las compras guardadas antes de incorporar SkinActive se consideran activas.
        if (!PlayerPrefs.HasKey(activeKey))
            return true;

        return PlayerPrefs.GetInt(activeKey, 0) == 1;
    }

    public static void SetSkinActive(string skinId, bool active)
    {
        if (!HasSkin(skinId))
            return;

        PlayerPrefs.SetInt(GetSkinActiveKey(skinId), active ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void ResetProfile()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    private static string GetLicenseKey(string licenseId)
    {
        return LicensePrefix + licenseId;
    }

    private static string GetSkinKey(string skinId)
    {
        return SkinPrefix + skinId;
    }

    private static string GetSkinActiveKey(string skinId)
    {
        return SkinActivePrefix + skinId;
    }
}
