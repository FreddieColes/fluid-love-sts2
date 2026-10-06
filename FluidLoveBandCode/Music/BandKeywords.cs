using BaseLib.Patches.Content;

namespace FluidLoveBand.FluidLoveBandCode.Music;

/// <summary>
/// Role keywords. BaseLib prints the keyword's title at the top of the card text ("Lead.") and shows
/// its description as a hover tip. Text lives in card_keywords.json as FLUIDLOVEBAND-LEAD etc.
/// </summary>
public static class BandKeywords
{
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword Lead;
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword Rhythm;
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword Keys;
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword Fluid;

    public static CardKeyword For(Role role, bool fluid) => fluid ? Fluid : role switch
    {
        Role.Lead => Lead,
        Role.Rhythm => Rhythm,
        _ => Keys,
    };
}

/// <summary>Explainer hover tips. Text lives in static_hover_tips.json as FLUIDLOVEBAND-SETLIST etc.</summary>
public static class BandTips
{
    [CustomEnum] public static StaticHoverTip Setlist;
    [CustomEnum] public static StaticHoverTip Groove;
}
