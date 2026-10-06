namespace FluidLoveBand.FluidLoveBandCode.Cards;

// The 16 rares. The first three are the headliners.

public sealed class OlMacDaddy() : BandPowerCard(2, CardRarity.Rare, Role.Lead, fluid: true)
{
    protected override int BaseAmount => 5;
    protected override int UpgradeAmount => 2;
    protected override Task ApplyPower(PlayerChoiceContext ctx, decimal amount) => Buff<OlMacPower>(ctx, amount);
}

public sealed class ManOfTheCloth() : BandPowerCard(2, CardRarity.Rare, Role.Keys)
{
    protected override Task ApplyPower(PlayerChoiceContext ctx, decimal amount) => Buff<ManOfTheClothPower>(ctx, 1m);
}

public sealed class CourtOfTheHolyKing() : BandPowerCard(3, CardRarity.Rare, Role.Rhythm)
{
    protected override IEnumerable<IHoverTip> MoreTips => [HoverTipFactory.Static(BandTips.Groove)];
    protected override Task ApplyPower(PlayerChoiceContext ctx, decimal amount) => Buff<HolyKingPower>(ctx, amount);
}

public sealed class Crescendo() : BandPowerCard(1, CardRarity.Rare, Role.Keys)
{
    protected override int BaseAmount => 4;
    protected override int UpgradeAmount => 2;
    protected override Task ApplyPower(PlayerChoiceContext ctx, decimal amount) => Buff<CrescendoPower>(ctx, amount);
}

public sealed class Headliner() : BandPowerCard(3, CardRarity.Rare, Role.Lead)
{
    protected override Task ApplyPower(PlayerChoiceContext ctx, decimal amount) => Buff<HeadlinerPower>(ctx, 1m);
}

public sealed class TwelveMinuteSolo() : BandCard(-2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, Role.Lead)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        for (var i = 0; i < EnergyCost.CapturedXValue; i++) await Hit(ctx, play, DynamicVars.Damage.BaseValue);
        await Music.Setlist.PlaySong(ctx, Owner, SongKind.LeadSolo, echo: true);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

public sealed class OnTopOfTheWorld() : BandCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, Role.Lead)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(24m, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> MoreTips => [HoverTipFactory.Static(BandTips.Groove)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        if (Music.Setlist.Groove(Owner) >= 3) await HitAll(ctx, DynamicVars.Damage.BaseValue);
        else await Hit(ctx, play, DynamicVars.Damage.BaseValue);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(6m);
}

public sealed class MirrorballMeltdown() : BandCard(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies, Role.Keys)
{
    protected override IEnumerable<CardKeyword> MoreKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        var songs = Music.Setlist.SongsThisCombat(Owner);
        if (songs > 0) await HitAll(ctx, DynamicVars.Damage.BaseValue * songs);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

public sealed class BarnDance() : BandCard(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies, Role.Rhythm)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10m, ValueProp.Move), new DynamicVar("PerEnemy", 4m)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        var count = Enemies.Count;
        await HitAll(ctx, DynamicVars.Damage.BaseValue);
        await Music.Setlist.Block(Owner.Creature, V("PerEnemy") * count);
    }
    protected override void OnUpgrade() { DynamicVars.Damage.UpgradeValueBy(3m); DynamicVars["PerEnemy"].UpgradeValueBy(1m); }
}

public sealed class PipeOrgan() : BandCard(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, Role.Keys)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(30m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play, DynamicVars.Damage.BaseValue);
        var refund = Math.Min(Music.Setlist.SongsThisTurn(Owner), 3);
        if (refund > 0) await Energy(refund);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(8m);
}

public sealed class TheOldBanjo() : BandCard(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, Role.Rhythm, fluid: true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move)];
    protected override Task Perform(PlayerChoiceContext ctx, CardPlay play) => Hit(ctx, play, DynamicVars.Damage.BaseValue);
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

public sealed class WallOfSound() : BandCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self, Role.Rhythm)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(10m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Block(play);
        // Fill whatever is in the Setlist with Rhythm (this card's own note is the last one).
        var setlist = await Music.Setlist.Ensure(ctx, Owner, this);
        if (setlist == null) return;
        Music.Setlist.Rewrite(Owner, Role.Rhythm, lastOnly: false);
        while (setlist.Notes.Count < SetlistPower.Slots - 1) await Music.Setlist.AddNote(ctx, Owner, Role.Rhythm, this);
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

public sealed class StandingOvation() : BandCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self, Role.Keys)
{
    protected override IEnumerable<CardKeyword> MoreKeywords => [CardKeyword.Exhaust];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Music.Setlist.Encore(ctx, Owner);
        await Music.Setlist.Encore(ctx, Owner);
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

public sealed class FluidLoveSong() : BandCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self, Role.Keys, fluid: true)
{
    protected override IEnumerable<CardKeyword> MoreKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(0)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        foreach (var card in CardPile.GetCards(Owner, PileType.Hand, PileType.Draw, PileType.Discard).OfType<BandCard>())
            card.FluidOverride = true;
        if (DynamicVars.Cards.IntValue > 0) await Draw(ctx, DynamicVars.Cards.IntValue);
    }
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(2m);
}

public sealed class LastOrders() : BandCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self, Role.Keys)
{
    protected override IEnumerable<CardKeyword> MoreKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> MoreTips => [HoverTipFactory.Static(BandTips.Groove)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        var groove = Music.Setlist.Groove(Owner);
        if (groove > 0) await Energy(groove);
    }
    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}

public sealed class KeyChange() : BandCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self, Role.Keys)
{
    protected override IEnumerable<CardKeyword> MoreKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        if (await RoleChooser.Choose(ctx, Owner) is { } role)
        {
            Music.Setlist.Rewrite(Owner, role, lastOnly: false);
            RoleChooser.RetuneHand(Owner, role);
            RetunedRole = role;
        }
        await Draw(ctx, DynamicVars.Cards.IntValue);
    }
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

// ---- Album cards. Rare, strong, and wearing the actual album covers.

/// <summary>Ready For Business: each turn gain 1 Energy, and your first Song each turn plays twice.</summary>
public sealed class ReadyForBusiness() : BandPowerCard(3, CardRarity.Rare, Role.Rhythm)
{
    protected override Task ApplyPower(PlayerChoiceContext ctx, decimal amount) => Buff<ReadyForBusinessPower>(ctx, 1m);
}

/// <summary>Backwater Crimes: hit ALL enemies three times, then gain Groove.</summary>
public sealed class BackwaterCrimes() : BandCard(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies, Role.Lead)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> MoreTips => [HoverTipFactory.Static(BandTips.Groove)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        for (var i = 0; i < 3; i++) await HitAll(ctx, DynamicVars.Damage.BaseValue);
        await Buff<GroovePower>(ctx, 1);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

/// <summary>Pleasure Island DLC: bonus content. Add 2 random Rare band cards to your hand, free this turn.</summary>
public sealed class PleasureIslandDlc() : BandCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self, Role.Keys)
{
    protected override IEnumerable<CardKeyword> MoreKeywords => [CardKeyword.Exhaust];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        var pool = Owner.Character.CardPool
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c is BandCard and not NoteToken and not PleasureIslandDlc && c.Rarity == CardRarity.Rare)
            .ToList();
        var cards = MegaCrit.Sts2.Core.Factories.CardFactory.GetDistinctForCombat(Owner, pool, 2, Owner.RunState.Rng.CombatCardGeneration).ToList();
        foreach (var card in cards)
        {
            card.SetToFreeThisTurn();
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
        }
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
