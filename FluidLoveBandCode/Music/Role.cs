namespace FluidLoveBand.FluidLoveBandCode.Music;

/// <summary>The three band roles. Every Fluid Love Band card plays one of these as a note.</summary>
public enum Role
{
    Lead,   // Alex: vocals and guitar
    Rhythm, // Toby on guitar and bass, plus the drum track
    Keys,   // Freddie: keyboard and backing vocals
}

public enum SongKind
{
    None,
    LeadSolo,
    RhythmSolo,
    KeysSolo,
    LeadJam,
    RhythmJam,
    KeysJam,
    FullBand,
}

public static class RoleInfo
{
    public static string Name(Role role) => role switch
    {
        Role.Lead => "Lead",
        Role.Rhythm => "Rhythm",
        Role.Keys => "Keys",
        _ => "?",
    };

    /// <summary>Hex colours used by the Setlist widget.</summary>
    public static string Hex(Role role) => role switch
    {
        Role.Lead => "e8543f",   // stage red
        Role.Rhythm => "3f7fe8", // denim blue
        Role.Keys => "f2b632",   // disco gold
        _ => "888888",
    };

    public static string SongName(SongKind kind) => kind switch
    {
        SongKind.LeadSolo => "Lead Solo!",
        SongKind.RhythmSolo => "Rhythm Solo!",
        SongKind.KeysSolo => "Keys Solo!",
        SongKind.LeadJam => "Lead Jam",
        SongKind.RhythmJam => "Rhythm Jam",
        SongKind.KeysJam => "Keys Jam",
        SongKind.FullBand => "Full Band!",
        _ => "",
    };

    /// <summary>3 of a kind is a Solo, one of each is a Full Band, anything else is a Jam of the doubled role.</summary>
    public static SongKind Classify(IReadOnlyList<Role> notes)
    {
        if (notes.Count < 3) return SongKind.None;
        var distinct = notes.Distinct().Count();
        if (distinct == 3) return SongKind.FullBand;

        var top = notes.GroupBy(n => n).OrderByDescending(g => g.Count()).First().Key;
        if (distinct == 1)
            return top switch { Role.Lead => SongKind.LeadSolo, Role.Rhythm => SongKind.RhythmSolo, _ => SongKind.KeysSolo };
        return top switch { Role.Lead => SongKind.LeadJam, Role.Rhythm => SongKind.RhythmJam, _ => SongKind.KeysJam };
    }

    /// <summary>What a Song does, with the Groove bonus already added. Shown on screen when it plays.</summary>
    public static string SongEffect(SongKind kind, int bonus) => kind switch
    {
        SongKind.LeadSolo => $"{Setlist.SoloDamage + bonus} dmg to ALL",
        SongKind.RhythmSolo => $"+{Setlist.SoloBlock + bonus} Block",
        SongKind.KeysSolo => $"Draw {Setlist.SoloDraw}, +{Setlist.SoloEnergy} Energy",
        SongKind.LeadJam => $"{Setlist.JamDamage + bonus / 2} dmg to ALL",
        SongKind.RhythmJam => $"+{Setlist.JamBlock + bonus / 2} Block",
        SongKind.KeysJam => $"Draw {Setlist.JamDraw}",
        SongKind.FullBand => $"+1 Groove, {Setlist.FullBandDamage + bonus} dmg ALL, +{Setlist.FullBandBlock + bonus} Block",
        _ => "",
    };

    /// <summary>Short name for the preview line: "Solo", "Jam", "Full Band".</summary>
    public static string ShortName(SongKind kind) =>
        IsSolo(kind) ? "Solo" : IsJam(kind) ? "Jam" : kind == SongKind.FullBand ? "Band" : "";

    public static Role? RoleOf(SongKind kind) => kind switch
    {
        SongKind.LeadSolo or SongKind.LeadJam => Role.Lead,
        SongKind.RhythmSolo or SongKind.RhythmJam => Role.Rhythm,
        SongKind.KeysSolo or SongKind.KeysJam => Role.Keys,
        _ => null,
    };

    public static bool IsSolo(SongKind kind) => kind is SongKind.LeadSolo or SongKind.RhythmSolo or SongKind.KeysSolo;
    public static bool IsJam(SongKind kind) => kind is SongKind.LeadJam or SongKind.RhythmJam or SongKind.KeysJam;
}
