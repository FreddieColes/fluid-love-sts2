
namespace FluidLoveBand.FluidLoveBandCode.Music;

/// <summary>
/// The rules of the Setlist. Cards, relics and potions all add notes through here.
///
/// Every note goes into a 3-slot Setlist. When the third lands the band plays a Song:
///   3 of one role  -> Solo
///   1 of each      -> Full Band (gain Groove)
///   2 + 1          -> Jam (half the doubled role's Solo)
/// Groove adds 2 per stack to every damage and Block number in a Song.
/// </summary>
public static class Setlist
{
    // Base numbers. Card text and the design doc both quote these.
    public const int SoloDamage = 10;
    public const int SoloBlock = 10;
    public const int SoloDraw = 2;
    public const int SoloEnergy = 1;
    public const int JamDamage = 5;
    public const int JamBlock = 5;
    public const int JamDraw = 1;
    public const int FullBandDamage = 4;
    public const int FullBandBlock = 4;
    public const int GroovePerStack = 2;

    public static SetlistPower? Get(Player? player)
    {
        var setlist = player?.Creature?.GetPower<SetlistPower>();
        setlist?.SyncCombat();
        return setlist;
    }

    public static async Task<SetlistPower?> Ensure(PlayerChoiceContext ctx, Player player, CardModel? source)
    {
        var creature = player.Creature;
        if (creature == null) return null;

        var existing = creature.GetPower<SetlistPower>();
        if (existing != null)
        {
            existing.SyncCombat();
            return existing;
        }

        var applied = await PowerCmd.Apply<SetlistPower>(ctx, creature, 1m, creature, source);
        applied?.SyncCombat();
        applied?.Changed();
        return applied;
    }

    public static int NoteCount(Player? player) => Get(player)?.Notes.Count ?? 0;

    public static int Groove(Player? player) => (int)(player?.Creature?.GetPower<GroovePower>()?.Amount ?? 0m);

    /// <summary>Adds one note. If the Setlist fills, the Song plays and the Setlist clears.</summary>
    public static async Task AddNote(PlayerChoiceContext ctx, Player player, Role role, CardModel? source = null)
    {
        var setlist = await Ensure(ctx, player, source);
        if (setlist == null) return;

        setlist.Notes.Add(role);
        setlist.LastNote = role;
        setlist.Changed();

        if (setlist.Notes.Count < SetlistPower.Slots) return;

        var kind = RoleInfo.Classify(setlist.Notes);
        setlist.Notes.Clear();
        setlist.Changed();
        await PlaySong(ctx, player, kind);
    }

    public static async Task AddNotes(PlayerChoiceContext ctx, Player player, Role role, int count, CardModel? source = null)
    {
        for (var i = 0; i < count; i++) await AddNote(ctx, player, role, source);
    }

    /// <summary>Plays a Song's effect. Used by full Setlists and by Encore.</summary>
    public static async Task PlaySong(PlayerChoiceContext ctx, Player player, SongKind kind)
    {
        if (kind == SongKind.None) return;
        var creature = player.Creature;
        if (creature == null) return;

        var setlist = Get(player);
        var bonus = Groove(player) * GroovePerStack;

        setlist?.Pulse();
        setlist?.SongPlayed(kind);
        MainFile.Logger.Info($"Song: {kind} (Groove bonus {bonus})");

        switch (kind)
        {
            case SongKind.LeadSolo:
                await DamageAll(ctx, creature, SoloDamage + bonus);
                break;
            case SongKind.RhythmSolo:
                await Block(creature, SoloBlock + bonus);
                break;
            case SongKind.KeysSolo:
                await CardPileCmd.Draw(ctx, SoloDraw, player);
                await PlayerCmd.GainEnergy(SoloEnergy, player);
                break;
            case SongKind.LeadJam:
                await DamageAll(ctx, creature, JamDamage + bonus / 2);
                break;
            case SongKind.RhythmJam:
                await Block(creature, JamBlock + bonus / 2);
                break;
            case SongKind.KeysJam:
                await CardPileCmd.Draw(ctx, JamDraw, player);
                break;
            case SongKind.FullBand:
                await PowerCmd.Apply<GroovePower>(ctx, creature, 1m, creature, null);
                await DamageAll(ctx, creature, FullBandDamage + bonus);
                await Block(creature, FullBandBlock + bonus);
                break;
        }

        if (setlist != null)
        {
            setlist.LastSong = kind;
            setlist.SongsThisTurn++;
            setlist.SongsThisCombat++;
            setlist.Changed();
        }
    }

    /// <summary>Encore: play your last Song again without touching the Setlist.</summary>
    public static async Task Encore(PlayerChoiceContext ctx, Player player)
    {
        var last = Get(player)?.LastSong ?? SongKind.None;
        await PlaySong(ctx, player, last);
    }

    public static IReadOnlyList<Creature> Enemies(Creature creature) =>
        (creature.CombatState as CombatState)?.HittableEnemies.ToList() ?? new List<Creature>();

    private static async Task DamageAll(PlayerChoiceContext ctx, Creature creature, int amount)
    {
        var enemies = Enemies(creature);
        if (enemies.Count == 0 || amount <= 0) return;
        await CreatureCmd.Damage(ctx, enemies, amount, ValueProp.Unpowered, creature);
    }

    private static async Task Block(Creature creature, int amount)
    {
        if (amount <= 0) return;
        await CreatureCmd.GainBlock(creature, amount, ValueProp.Unpowered, null, false);
    }
}
