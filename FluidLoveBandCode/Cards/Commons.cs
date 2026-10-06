namespace FluidLoveBand.FluidLoveBandCode.Cards;

// The 20 commons: 7 Lead, 6 Rhythm, 5 Keys, 2 Fluid.

public sealed class Hamfisted() : BandCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, Role.Lead)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play, DynamicVars.Damage.BaseValue);
        if (IsFinale) await Hit(ctx, play, DynamicVars.Damage.BaseValue);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

public sealed class RunninJaq() : BandCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, Role.Lead)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play, DynamicVars.Damage.BaseValue);
        await Hit(ctx, play, DynamicVars.Damage.BaseValue);
        if (WasOpener) await Draw(ctx, 1);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1m);
}

public sealed class PowerChord() : BandCard(2, CardType.Attack, CardRarity.Common, TargetType.AllEnemies, Role.Lead)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move)];
    protected override Task Perform(PlayerChoiceContext ctx, CardPlay play) => HitAll(ctx, DynamicVars.Damage.BaseValue);
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

public sealed class StageDive() : BandCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, Role.Lead)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move), new DynamicVar("Vuln", 1m)];
    protected override IEnumerable<IHoverTip> MoreTips => [HoverTipFactory.FromPower<VulnerablePower>()];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play, DynamicVars.Damage.BaseValue);
        await Debuff<VulnerablePower>(ctx, play.Target!, V("Vuln"));
    }
    protected override void OnUpgrade() { DynamicVars.Damage.UpgradeValueBy(2m); DynamicVars["Vuln"].UpgradeValueBy(1m); }
}

public sealed class Frontman() : BandCard(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, Role.Lead)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        var hadLead = Music.Setlist.CountOf(Owner, Role.Lead) > 0;
        await Hit(ctx, play, DynamicVars.Damage.BaseValue);
        if (hadLead) await Draw(ctx, 1);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

public sealed class LineDance() : BandCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, Role.Lead)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play, DynamicVars.Damage.BaseValue);
        await RoleChooser.Retune(ctx, Owner, this);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

public sealed class CrowdSurf() : BandCard(1, CardType.Skill, CardRarity.Common, TargetType.Self, Role.Lead)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7m, ValueProp.Move)];
    protected override int NotesAdded => 2;
    protected override Task Perform(PlayerChoiceContext ctx, CardPlay play) => Block(play);
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

public sealed class Bassline() : BandCard(1, CardType.Skill, CardRarity.Common, TargetType.Self, Role.Rhythm)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8m, ValueProp.Move)];
    protected override Task Perform(PlayerChoiceContext ctx, CardPlay play) => Block(play);
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

public sealed class StandingOnTheEustonLine() : BandCard(1, CardType.Skill, CardRarity.Common, TargetType.Self, Role.Rhythm)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6m, ValueProp.Move), new DynamicVar("NextBlock", 4m)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Block(play);
        await Buff<NextTurnBlockPower>(ctx, V("NextBlock"));
    }
    protected override void OnUpgrade() { DynamicVars.Block.UpgradeValueBy(2m); DynamicVars["NextBlock"].UpgradeValueBy(1m); }
}

public sealed class FourOnTheFloor() : BandCard(1, CardType.Skill, CardRarity.Common, TargetType.Self, Role.Rhythm)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(4m, ValueProp.Move)];
    protected override int NotesAdded => 2;
    protected override Task Perform(PlayerChoiceContext ctx, CardPlay play) => Block(play);
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

public sealed class HoedownStomp() : BandCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, Role.Rhythm)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move), new BlockVar(5m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play, DynamicVars.Damage.BaseValue);
        await Block(play);
    }
    protected override void OnUpgrade() { DynamicVars.Damage.UpgradeValueBy(2m); DynamicVars.Block.UpgradeValueBy(2m); }
}

public sealed class Roadie() : BandCard(1, CardType.Skill, CardRarity.Common, TargetType.Self, Role.Rhythm)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Block(play);
        await Draw(ctx, 1);
    }
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

public sealed class HecklerHandling() : BandCard(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, Role.Rhythm)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8m, ValueProp.Move), new DynamicVar("Weak", 1m)];
    protected override IEnumerable<IHoverTip> MoreTips => [HoverTipFactory.FromPower<WeakPower>()];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Block(play);
        await Debuff<WeakPower>(ctx, play.Target!, V("Weak"));
    }
    protected override void OnUpgrade() { DynamicVars.Block.UpgradeValueBy(3m); DynamicVars["Weak"].UpgradeValueBy(1m); }
}

public sealed class Arpeggio() : BandCard(1, CardType.Skill, CardRarity.Common, TargetType.Self, Role.Keys)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];
    protected override Task Perform(PlayerChoiceContext ctx, CardPlay play) => Draw(ctx, DynamicVars.Cards.IntValue);
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

public sealed class WatchingBirds() : BandCard(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, Role.Keys)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Weak", 2m)];
    protected override IEnumerable<IHoverTip> MoreTips => [HoverTipFactory.FromPower<WeakPower>()];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Debuff<WeakPower>(ctx, play.Target!, V("Weak"));
        await Draw(ctx, 1);
    }
    protected override void OnUpgrade() => DynamicVars["Weak"].UpgradeValueBy(1m);
}

public sealed class LettersHome() : BandCard(0, CardType.Skill, CardRarity.Common, TargetType.Self, Role.Keys)
{
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        var top = PileType.Draw.GetPile(Owner).Cards.Take(3).ToList();
        if (top.Count == 0) return;
        var chosen = await CardSelectCmd.FromChooseACardScreen(ctx, top, Owner, canSkip: true);
        if (chosen != null) await CardPileCmd.Add(chosen, PileType.Hand);
    }
    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}

public sealed class SynthPad() : BandCard(1, CardType.Skill, CardRarity.Common, TargetType.Self, Role.Keys)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Block(play);
        if (WasOpener) await Energy(1);
    }
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

public sealed class MarketScene() : BandCard(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies, Role.Keys)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Weak", 1m), new DynamicVar("Vuln", 1m)];
    protected override IEnumerable<IHoverTip> MoreTips => [HoverTipFactory.FromPower<WeakPower>(), HoverTipFactory.FromPower<VulnerablePower>()];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        foreach (var enemy in Enemies.ToList())
        {
            await Debuff<WeakPower>(ctx, enemy, V("Weak"));
            await Debuff<VulnerablePower>(ctx, enemy, V("Vuln"));
        }
    }
    protected override void OnUpgrade() { DynamicVars["Weak"].UpgradeValueBy(1m); DynamicVars["Vuln"].UpgradeValueBy(1m); }
}

public sealed class Salsa() : BandCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, Role.Lead, fluid: true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        for (var i = 0; i < 3; i++) await Hit(ctx, play, DynamicVars.Damage.BaseValue);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1m);
}

public sealed class TempoCheck() : BandCard(1, CardType.Skill, CardRarity.Common, TargetType.Self, Role.Rhythm, fluid: true)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Block(play);
        await Draw(ctx, 1);
    }
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}
