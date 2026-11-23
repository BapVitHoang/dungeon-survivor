using UnityEngine;

public static class Palette
{
    public static readonly Color BgVoid = Hex("#0D0F18");
    public static readonly Color PanelDark = Hex("#171B2B", 0.94f);
    public static readonly Color PanelBorder = Hex("#3B4261");
    public static readonly Color AccentGold = Hex("#F5C451");
    public static readonly Color HealthCrimson = Hex("#E04F5F");
    public static readonly Color XpCyan = Hex("#38D9A9");
    public static readonly Color DangerAmber = Hex("#FF9F1C");
    public static readonly Color TextPrimary = Hex("#F1F5F9");
    public static readonly Color TextMuted = Hex("#94A3B8");

    private static Color Hex(string hex, float alpha = 1f)
    {
        ColorUtility.TryParseHtmlString(hex, out var color);
        color.a = alpha;
        return color;
    }
}
