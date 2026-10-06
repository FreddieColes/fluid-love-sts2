namespace FluidLoveBand.FluidLoveBandCode.Cards;

// The starting ten: 4 Strike (Lead), 4 Defend (Rhythm), 1 Tune Up (Keys), 1 Soundcheck (Fluid).

public sealed class StrikeBand() : BandCard(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy, Role.Lead)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override Task Perform(PlayerChoiceContext ctx, CardPlay play) => Hit(ctx, play, DynamicVars.Damage.BaseValue);

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

public sealed class DefendBand() : BandCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self, Role.Rhythm)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move)];
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];

    protected override Task Perform(PlayerChoiceContext ctx, CardPlay play) => Block(play);

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

public sealed class TuneUp() : BandCard(0, CardType.Skill, CardRarity.Basic, TargetType.Self, Role.Keys)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    protected override Task Perform(PlayerChoiceContext ctx, CardPlay play) => Draw(ctx, DynamicVars.Cards.IntValue);

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

public sealed class Soundcheck() : BandCard(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy, Role.Lead, fluid: true)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5m, ValueProp.Move), new BlockVar(4m, ValueProp.Move)];

    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play, DynamicVars.Damage.BaseValue);
        await Block(play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}
