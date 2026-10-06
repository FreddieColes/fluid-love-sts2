namespace FluidLoveBand.FluidLoveBandCode.Relics;

/// <summary>Base for the band's relics, with a once-per-combat opening hook (top of turn 1).</summary>
[Pool(typeof(BandRelicPool))]
public abstract class BandRelic : CustomRelicModel
{
    private string Slug => Id.Entry.RemovePrefix().ToLowerInvariant();
    public override string PackedIconPath => $"{Slug}.png".RelicImagePath();
    protected override string PackedIconOutlinePath => $"{Slug}_outline.png".RelicOutlineImagePath();
    protected override string BigIconPath => $"{Slug}.png".BigRelicImagePath();

    private bool _combatStarted;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext ctx, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState state)
    {
        if (side != Owner.Creature.Side) return;
        if (_combatStarted) { await OnOwnerTurnStart(ctx); return; }
        _combatStarted = true;
        await OnCombatOpening(ctx);
        await OnOwnerTurnStart(ctx);
    }

    protected virtual Task OnCombatOpening(PlayerChoiceContext ctx) => Task.CompletedTask;
    protected virtual Task OnOwnerTurnStart(PlayerChoiceContext ctx) => Task.CompletedTask;

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _combatStarted = false;
        return Task.CompletedTask;
    }
}

/// <summary>Starter. At the start of each combat, add a Rhythm note to your Setlist.</summary>
public sealed class BackingTrack : BandRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    public override RelicModel? GetUpgradeReplacement() => ModelDb.Relic<ClickTrack>();

    protected override async Task OnCombatOpening(PlayerChoiceContext ctx)
    {
        Flash();
        await Setlist.AddNote(ctx, Owner, Role.Rhythm);
    }
}

/// <summary>Starter upgrade. At the start of each turn, if the Setlist is empty, add a Rhythm note.</summary>
public sealed class ClickTrack : BandRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override async Task OnOwnerTurnStart(PlayerChoiceContext ctx)
    {
        if (Setlist.NoteCount(Owner) != 0) return;
        Flash();
        await Setlist.AddNote(ctx, Owner, Role.Rhythm);
    }
}
