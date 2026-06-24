public readonly struct PerformanceResult
{
    public PerformanceResult(float effectiveness, int stars, string label, int baseCoins)
    {
        Effectiveness = effectiveness;
        Stars = stars;
        Label = label;
        BaseCoins = baseCoins;
    }

    public float Effectiveness { get; }
    public int Stars { get; }
    public string Label { get; }
    public int BaseCoins { get; }
    public bool IsWin => Stars >= 3;
}
