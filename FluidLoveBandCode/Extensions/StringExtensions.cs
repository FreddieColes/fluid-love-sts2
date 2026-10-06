using Godot;

namespace FluidLoveBand.FluidLoveBandCode.Extensions;

/// <summary>Asset path helpers. Missing art falls back to a placeholder rather than crashing.</summary>
public static class StringExtensions
{
    private static string Res(params string[] parts) => Path.Join([MainFile.ResPath, "images", .. parts]);

    private static string OrFallback(string path, string fallback, string what)
    {
        if (ResourceLoader.Exists(path)) return path;
        MainFile.Logger.Info($"No {what} art at {path}, using placeholder");
        return fallback;
    }

    public static string ImagePath(this string path) => Res(path);

    public static string CharacterUiPath(this string path) => Res("charui", path);

    /// <summary>Card art by class name, else the role's placeholder, else the generic one.</summary>
    public static string CardImagePath(this string path, string roleFallback) =>
        OrFallback(Res("card_portraits", path),
            OrFallback(Res("card_portraits", roleFallback), Res("card_portraits", "card.png"), "role card"), "card");

    public static string BigCardImagePath(this string path, string roleFallback) =>
        OrFallback(Res("card_portraits", "big", path),
            OrFallback(Res("card_portraits", "big", roleFallback), Res("card_portraits", "big", "card.png"), "role card"), "big card");

    public static string PowerImagePath(this string path) =>
        OrFallback(Res("powers", path), Res("powers", "power.png"), "power");

    public static string BigPowerImagePath(this string path) =>
        OrFallback(Res("powers", "big", path), Res("powers", "big", "power.png"), "big power");

    public static string RelicImagePath(this string path) =>
        OrFallback(Res("relics", path), Res("relics", "relic.png"), "relic");

    public static string RelicOutlineImagePath(this string path) =>
        OrFallback(Res("relics", path), Res("relics", "relic_outline.png"), "relic outline");

    public static string BigRelicImagePath(this string path) =>
        OrFallback(Res("relics", "big", path), Res("relics", "big", "relic.png"), "big relic");

    public static string PotionImagePath(this string path) =>
        OrFallback(Res("potions", path), Res("potions", "potion.png"), "potion");

    public static string PotionOutlineImagePath(this string path) =>
        OrFallback(Res("potions", "outline", path), Res("potions", "outline", "potion.png"), "potion outline");
}
