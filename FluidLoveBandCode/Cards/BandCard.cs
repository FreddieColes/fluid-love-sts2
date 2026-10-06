namespace FluidLoveBand.FluidLoveBandCode.Cards;

/// <summary>
/// Base for every Fluid Love Band card. Each card has a band role; after its effect resolves it
/// adds that role's note to the Setlist. Fluid cards change role each turn (Lead, Rhythm, Keys...).
///
/// Cards implement <see cref="Perform"/> instead of OnPlay so the note is never forgotten.
/// Opener / Finale are worked out before the effect runs, from the Setlist as it was.
/// </summary>
[Pool(typeof(BandCardPool))]
public abstract class BandCard(int cost, CardType type, CardRarity rarity, TargetType target, Role role, bool fluid = false)
    : CustomCardModel(cost, type, rarity, target)
{
    public Role BaseRole { get; } = role;
    public bool PrintedFluid { get; } = fluid;

    /// <summary>Set by the rare "Fluid Love": this card is Fluid for the rest of combat.</summary>
    public bool FluidOverride { get; set; }

    public bool IsFluid => PrintedFluid || FluidOverride;

    /// <summary>Set by Retune. Overrides the printed role for the rest of combat.</summary>
    public Role? RetunedRole { get; set; }

    /// <summary>True during Perform if the Setlist was empty when this card was played.</summary>
    protected bool WasOpener { get; private set; }

    /// <summary>True during Perform if this card's note will complete a Song.</summary>
    protected bool IsFinale { get; private set; }

    /// <summary>Role keyword first (printed on the card with a hover tip), then any extras.</summary>
    public override IEnumerable<CardKeyword> CanonicalKeywords => [BandKeywords.For(BaseRole, PrintedFluid), .. MoreKeywords];

    /// <summary>Extra keywords for a card, e.g. Exhaust or Retain.</summary>
    protected virtual IEnumerable<CardKeyword> MoreKeywords => [];

    /// <summary>The full Setlist rules live on the Setlist panel's hover; cards only add their own extras.</summary>
    protected override IEnumerable<IHoverTip> ExtraHoverTips => MoreTips;

    protected virtual IEnumerable<IHoverTip> MoreTips => [];

    /// <summary>How many notes this card adds. Some cards (and Count In) add more.</summary>
    protected virtual int NotesAdded => 1;

    public static Role FluidRoleForTurn(int turn, Role start) => (Role)(((int)start + Math.Max(turn - 1, 0)) % 3);

    public Role CurrentRole
    {
        get
        {
            if (RetunedRole is { } r) return r;
            if (!IsFluid) return BaseRole;
            var turn = Music.Setlist.Get(Owner)?.TurnCount ?? 1;
            return FluidRoleForTurn(turn, BaseRole);
        }
    }

    private string Slug => Id.Entry.RemovePrefix().ToLowerInvariant();
    private string RoleArt => $"role_{(PrintedFluid ? "fluid" : RoleInfo.Name(BaseRole).ToLowerInvariant())}.png";

    // Normal art 1000x760, small 250x190. Missing art falls back to the role placeholder.
    public override string CustomPortraitPath => $"{Slug}.png".BigCardImagePath(RoleArt);
    public override string PortraitPath => $"{Slug}.png".CardImagePath(RoleArt);
    public override string BetaPortraitPath => $"{Slug}.png".CardImagePath(RoleArt);

    protected sealed override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var notes = Music.Setlist.NoteCount(Owner);
        WasOpener = notes == 0;
        IsFinale = notes == SetlistPower.Slots - 1;

        // Count In's bonus belongs to the card played after it, so take it before this card's effect.
        var setlist = Music.Setlist.Get(Owner);
        var extra = setlist?.ExtraNotesNextCard ?? 0;
        if (setlist != null) setlist.ExtraNotesNextCard = 0;

        var role = CurrentRole;
        await Perform(ctx, play);

        if (IsFluid && Owner.Relics.Any(r => r is Relics.GafferTape))
            await Music.Setlist.Block(Owner.Creature, 3);

        await Music.Setlist.AddNotes(ctx, Owner, role, NotesAdded + extra, this);
    }

    protected abstract Task Perform(PlayerChoiceContext ctx, CardPlay play);

    // ---- small helpers so card bodies stay one-liners

    protected async Task Hit(PlayerChoiceContext ctx, CardPlay play, decimal amount, string fx = "vfx/vfx_attack_blunt", string sfx = "blunt_attack.mp3")
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(amount).FromCard(this).Targeting(play.Target).WithHitFx(fx, null, sfx).Execute(ctx);
    }

    protected Task Block(CardPlay play) => CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

    protected Task Draw(PlayerChoiceContext ctx, int count) => CardPileCmd.Draw(ctx, count, Owner);

    protected IReadOnlyList<Creature> Enemies => Music.Setlist.Enemies(Owner.Creature);

    protected async Task HitAll(PlayerChoiceContext ctx, decimal amount)
    {
        foreach (var enemy in Enemies.ToList())
            await DamageCmd.Attack(amount).FromCard(this).Targeting(enemy).Execute(ctx);
    }

    protected Task Debuff<T>(PlayerChoiceContext ctx, Creature target, decimal amount) where T : PowerModel =>
        PowerCmd.Apply<T>(ctx, target, amount, Owner.Creature, this);

    protected Task Buff<T>(PlayerChoiceContext ctx, decimal amount) where T : PowerModel =>
        PowerCmd.Apply<T>(ctx, Owner.Creature, amount, Owner.Creature, this);

    protected Task Energy(decimal amount) => PlayerCmd.GainEnergy(amount, Owner);

    protected IReadOnlyList<CardModel> HandExceptThis =>
        PileType.Hand.GetPile(Owner).Cards.Where(c => c != this).ToList();

    protected int V(string name) => DynamicVars[name].IntValue;
}
