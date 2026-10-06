namespace FluidLoveBand.FluidLoveBandCode.Music;

/// <summary>
/// The rules of the Setlist. Cards, relics and potions all add notes through here.
///
/// Every note goes into a 3-slot Setlist. When the third lands the band plays a Song:
///   3 of one role  -> Solo
///   1 of each      -> Full Band (gain Groove)
///   2 + 1          -> Jam (half the doubled role's Solo)
/// Groove adds 2 per stack to every damage and Block number in a Song.
/// Powers and relics that change Songs are read here, in one place.
/// </summary>
public static class Setlist
{
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

    public static int CountOf(Player? player, Role role) => Get(player)?.Notes.Count(n => n == role) ?? 0;

    public static int Groove(Player? player) => (int)(player?.Creature?.GetPower<GroovePower>()?.Amount ?? 0m);

    public static int SongsThisTurn(Player? player) => Get(player)?.SongsThisTurn ?? 0;

    public static int SongsThisCombat(Player? player) => Get(player)?.SongsThisCombat ?? 0;

    private static int PowerAmount<T>(Creature creature) where T : PowerModel => (int)(creature.GetPower<T>()?.Amount ?? 0m);

    private static bool HasRelic<T>(Player player) => player.Relics.Any(r => r is T);

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

    /// <summary>Changes every note currently in the Setlist (Key Change), or just the last one (Mixing Desk).</summary>
    public static void Rewrite(Player player, Role role, bool lastOnly)
    {
        var setlist = Get(player);
        if (setlist == null || setlist.Notes.Count == 0) return;
        if (lastOnly) setlist.Notes[^1] = role;
        else for (var i = 0; i < setlist.Notes.Count; i++) setlist.Notes[i] = role;
        setlist.LastNote = setlist.Notes[^1];
        setlist.Changed();
    }

    /// <summary>
    /// Plays a Song. Handles To My Darlin', Headliner (Solos twice), Setlist Paper (first Song twice),
    /// Man of the Cloth (Full Band also plays all three Solos) and Ol' Mac's Banjo (Jams do nothing).
    /// </summary>
    public static async Task PlaySong(PlayerChoiceContext ctx, Player player, SongKind kind, bool echo = false)
    {
        if (kind == SongKind.None) return;
        var creature = player.Creature;
        if (creature == null) return;
        var setlist = Get(player);

        if (setlist is { ForceFullBand: true } && !echo)
        {
            setlist.ForceFullBand = false;
            kind = SongKind.FullBand;
        }

        var times = 1;
        if (RoleInfo.IsSolo(kind) && creature.GetPower<HeadlinerPower>() != null) times++;
        if (!echo && setlist is { SongsThisCombat: 0 } && HasRelic<Relics.SetlistPaper>(player)) times++;
        if (!echo && setlist is { SongsThisTurn: 0 } && creature.GetPower<ReadyForBusinessPower>() != null) times++;

        for (var i = 0; i < times; i++) await PlayOnce(ctx, player, creature, kind);

        if (kind == SongKind.FullBand && !echo && creature.GetPower<ManOfTheClothPower>() != null)
        {
            foreach (var solo in new[] { SongKind.LeadSolo, SongKind.RhythmSolo, SongKind.KeysSolo })
                await PlaySong(ctx, player, solo, echo: true);
        }
    }

    private static async Task PlayOnce(PlayerChoiceContext ctx, Player player, Creature creature, SongKind kind)
    {
        var setlist = Get(player);
        var role = RoleInfo.RoleOf(kind);

        var bonus = Groove(player) * GroovePerStack;
        bonus += PowerAmount<CrescendoPower>(creature) * (setlist?.SongsThisTurn ?? 0);
        var leadBonus = PowerAmount<FrontOfStagePower>(creature) + (HasRelic<Relics.Plectrum>(player) ? 5 : 0);
        var rhythmBonus = PowerAmount<GiantsOfAlbionPower>(creature);
        var banjo = HasRelic<Relics.OlMacsBanjo>(player);

        setlist?.Pulse();
        setlist?.SongPlayed(kind);
        MainFile.Logger.Info($"Song: {kind} (bonus {bonus})");

        switch (kind)
        {
            case SongKind.LeadSolo:
                await DamageAll(ctx, creature, SoloDamage + bonus + leadBonus);
                break;
            case SongKind.RhythmSolo:
                await Block(creature, SoloBlock + bonus + rhythmBonus);
                break;
            case SongKind.KeysSolo:
                await CardPileCmd.Draw(ctx, SoloDraw, player);
                await PlayerCmd.GainEnergy(SoloEnergy, player);
                break;
            case SongKind.LeadJam when !banjo:
                await DamageAll(ctx, creature, JamDamage + bonus / 2 + leadBonus / 2);
                break;
            case SongKind.RhythmJam when !banjo:
                await Block(creature, JamBlock + bonus / 2 + rhythmBonus / 2);
                break;
            case SongKind.KeysJam when !banjo:
                await CardPileCmd.Draw(ctx, JamDraw, player);
                break;
            case SongKind.FullBand:
                await PowerCmd.Apply<GroovePower>(ctx, creature, 1m, creature, null);
                await DamageAll(ctx, creature, FullBandDamage + bonus);
                await Block(creature, FullBandBlock + bonus);
                break;
        }

        // ---- things that react to any Song
        if (role == Role.Keys && PowerAmount<KrishnakPower>(creature) is var krish and > 0)
        {
            foreach (var enemy in Enemies(creature))
            {
                await PowerCmd.Apply<WeakPower>(ctx, enemy, krish, creature, null);
                await PowerCmd.Apply<VulnerablePower>(ctx, enemy, krish, creature, null);
            }
        }

        if (PowerAmount<DiscoBallPower>(creature) is var disco and > 0)
            await DamageAll(ctx, creature, disco);

        if (RoleInfo.IsJam(kind) && PowerAmount<InThePocketPower>(creature) is var pocket and > 0)
        {
            await CardPileCmd.Draw(ctx, 1, player);
            await Block(creature, pocket);
        }

        if (kind == SongKind.FullBand && PowerAmount<HarmoniesPower>(creature) is var harm and > 0)
            await PlayerCmd.GainEnergy(harm, player);

        if (PowerAmount<FountainOfYouthPower>(creature) > 0)
        {
            var candidates = PileType.Hand.GetPile(player).Cards.Where(c => c.IsUpgradable).ToList();
            if (candidates.Count > 0)
            {
                var pick = candidates[player.RunState.Rng.CombatCardGeneration.NextInt(0, candidates.Count - 1)];
                CardCmd.Upgrade(pick, MegaCrit.Sts2.Core.Nodes.CommonUi.CardPreviewStyle.None);
            }
        }

        // The Old Banjo comes back whenever the band plays.
        foreach (var banjoCard in PileType.Discard.GetPile(player).Cards.Where(c => c is Cards.TheOldBanjo).ToList())
            await CardPileCmd.Add(banjoCard, PileType.Hand);

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
        await PlaySong(ctx, player, last, echo: true);
    }

    public static IReadOnlyList<Creature> Enemies(Creature creature) =>
        (creature.CombatState as CombatState)?.HittableEnemies.ToList() ?? new List<Creature>();

    public static async Task DamageAll(PlayerChoiceContext ctx, Creature creature, int amount)
    {
        var enemies = Enemies(creature);
        if (enemies.Count == 0 || amount <= 0) return;
        await CreatureCmd.Damage(ctx, enemies, amount, ValueProp.Unpowered, creature);
    }

    public static async Task Block(Creature creature, int amount)
    {
        if (amount <= 0) return;
        await CreatureCmd.GainBlock(creature, amount, ValueProp.Unpowered, null, false);
    }
}
