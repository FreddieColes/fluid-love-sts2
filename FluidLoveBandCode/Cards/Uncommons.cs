using MegaCrit.Sts2.Core.Factories;

namespace FluidLoveBand.FluidLoveBandCode.Cards;

// The 35 uncommons: 8 Powers, 11 Attacks, 16 Skills.

public abstract class BandPowerCard(int cost, CardRarity rarity, Role role, bool fluid = false)
    : BandCard(cost, CardType.Power, rarity, TargetType.Self, role, fluid)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Stack", (decimal)BaseAmount)];
    protected virtual int BaseAmount => 1;
    protected virtual int UpgradeAmount => 0;
    protected abstract Task ApplyPower(PlayerChoiceContext ctx, decimal amount);
    protected override Task Perform(PlayerChoiceContext ctx, CardPlay play) => ApplyPower(ctx, V("Stack"));
    protected override void OnUpgrade()
    {
        if (UpgradeAmount != 0) DynamicVars["Stack"].UpgradeValueBy(UpgradeAmount);
        else UpgradeOther();
    }
    protected virtual void UpgradeOther() => EnergyCost.UpgradeBy(-1);
}

// ---- Powers

public sealed class TheDrumTrack() : BandPowerCard(1, CardRarity.Uncommon, Role.Rhythm)
{
    protected override Task ApplyPower(PlayerChoiceContext ctx, decimal amount) => Buff<DrumTrackPower>(ctx, amount);
    protected override void UpgradeOther() => AddKeyword(CardKeyword.Innate);
}

public sealed class FrontOfStage() : BandPowerCard(1, CardRarity.Uncommon, Role.Lead)
{
    protected override int BaseAmount => 5;
    protected override int UpgradeAmount => 3;
    protected override Task ApplyPower(PlayerChoiceContext ctx, decimal amount) => Buff<FrontOfStagePower>(ctx, amount);
}

public sealed class GiantsOfAlbion() : BandPowerCard(1, CardRarity.Uncommon, Role.Rhythm)
{
    protected override int BaseAmount => 5;
    protected override int UpgradeAmount => 3;
    protected override Task ApplyPower(PlayerChoiceContext ctx, decimal amount) => Buff<GiantsOfAlbionPower>(ctx, amount);
}

public sealed class HitherToMeKrishnak() : BandPowerCard(1, CardRarity.Uncommon, Role.Keys)
{
    protected override Task ApplyPower(PlayerChoiceContext ctx, decimal amount) => Buff<KrishnakPower>(ctx, amount);
}

public sealed class DiscoBall() : BandPowerCard(2, CardRarity.Uncommon, Role.Keys)
{
    protected override int BaseAmount => 4;
    protected override int UpgradeAmount => 2;
    protected override Task ApplyPower(PlayerChoiceContext ctx, decimal amount) => Buff<DiscoBallPower>(ctx, amount);
}

public sealed class InThePocket() : BandPowerCard(1, CardRarity.Uncommon, Role.Rhythm)
{
    protected override int BaseAmount => 3;
    protected override int UpgradeAmount => 2;
    protected override Task ApplyPower(PlayerChoiceContext ctx, decimal amount) => Buff<InThePocketPower>(ctx, amount);
}

public sealed class Harmonies() : BandPowerCard(1, CardRarity.Uncommon, Role.Keys)
{
    protected override Task ApplyPower(PlayerChoiceContext ctx, decimal amount) => Buff<HarmoniesPower>(ctx, amount);
}

public sealed class FountainOfYouth() : BandPowerCard(1, CardRarity.Uncommon, Role.Keys)
{
    protected override Task ApplyPower(PlayerChoiceContext ctx, decimal amount) => Buff<FountainOfYouthPower>(ctx, 1m);
    protected override void UpgradeOther() => AddKeyword(CardKeyword.Innate);
}

// ---- Attacks

public sealed class Mumakil() : BandCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, Role.Lead)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(15m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        var notes = Music.Setlist.NoteCount(Owner);
        await Hit(ctx, play, DynamicVars.Damage.BaseValue);
        if (notes > 0) await Energy(notes);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(5m);
}

public sealed class TheForeman() : BandCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, Role.Rhythm)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];
    protected override Task Perform(PlayerChoiceContext ctx, CardPlay play) =>
        Hit(ctx, play, DynamicVars.Damage.BaseValue + 4 * Music.Setlist.CountOf(Owner, Role.Rhythm));
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

public sealed class OtziTheIceman() : BandCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, Role.Lead)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(14m, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> MoreTips => [HoverTipFactory.Static(BandTips.Groove)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play, DynamicVars.Damage.BaseValue);
        if (play.Target is { IsDead: true }) await Buff<GroovePower>(ctx, 2);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}

public sealed class GuitarDuel() : BandCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, Role.Lead)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play, DynamicVars.Damage.BaseValue);
        await Hit(ctx, play, DynamicVars.Damage.BaseValue);
        await Music.Setlist.AddNote(ctx, Owner, Role.Rhythm, this);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

public sealed class BillyYun() : BandCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, Role.Lead, fluid: true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];
    protected override Task Perform(PlayerChoiceContext ctx, CardPlay play) =>
        Hit(ctx, play, DynamicVars.Damage.BaseValue * (1 + Music.Setlist.SongsThisTurn(Owner)));
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

public sealed class Shred() : BandCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, Role.Lead)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3m, ValueProp.Move), new DynamicVar("Hits", 4m)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        for (var i = 0; i < V("Hits"); i++) await Hit(ctx, play, DynamicVars.Damage.BaseValue);
    }
    protected override void OnUpgrade() => DynamicVars["Hits"].UpgradeValueBy(1m);
}

public sealed class ChannelThreeSixNine() : BandCard(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, Role.Lead, fluid: true)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<CardKeyword> MoreKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3m, ValueProp.Move), new BlockVar(3m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play, DynamicVars.Damage.BaseValue);
        await Block(play);
        await Draw(ctx, 1);
    }
    protected override void OnUpgrade() { DynamicVars.Damage.UpgradeValueBy(2m); DynamicVars.Block.UpgradeValueBy(2m); }
}

public sealed class IntergalacticTravel() : BandCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies, Role.Keys)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await HitAll(ctx, DynamicVars.Damage.BaseValue);
        RoleChooser.RetuneHand(Owner, Role.Keys);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

public sealed class OrangeBlur() : BandCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, Role.Lead)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4m, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> MoreTips => [HoverTipFactory.Static(BandTips.Groove)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        var hits = 1 + Music.Setlist.Groove(Owner);
        for (var i = 0; i < hits; i++) await Hit(ctx, play, DynamicVars.Damage.BaseValue);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

public sealed class Blekens() : BandCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, Role.Rhythm)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10m, ValueProp.Move), new BlockVar(6m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play, DynamicVars.Damage.BaseValue);
        if (IsFinale) await Block(play);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

public sealed class Age() : BandCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, Role.Lead, fluid: true)
{
    protected override IEnumerable<CardKeyword> MoreKeywords => [CardKeyword.Retain];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];
    protected override Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        var turns = Math.Max((Music.Setlist.Get(Owner)?.TurnCount ?? 1) - 1, 0);
        return Hit(ctx, play, DynamicVars.Damage.BaseValue + 3 * turns);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}

// ---- Skills

public sealed class Encore() : BandCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, Role.Keys)
{
    protected override IEnumerable<CardKeyword> MoreKeywords => [CardKeyword.Exhaust];
    protected override Task Perform(PlayerChoiceContext ctx, CardPlay play) => Music.Setlist.Encore(ctx, Owner);
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

public sealed class SixHundredLeatherBags() : BandCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, Role.Rhythm)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(15m, ValueProp.Move)];
    protected override Task Perform(PlayerChoiceContext ctx, CardPlay play) => Block(play);
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(5m);
}

public sealed class ToMyDarlin() : BandCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, Role.Keys)
{
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        var setlist = await Music.Setlist.Ensure(ctx, Owner, this);
        if (setlist != null) setlist.ForceFullBand = true;
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

public sealed class SendYouALetterSoon() : BandCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, Role.Keys)
{
    protected override IEnumerable<CardKeyword> MoreKeywords => [CardKeyword.Exhaust];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        var discard = PileType.Discard.GetPile(Owner);
        if (discard.Cards.Count == 0) return;
        var chosen = (await CardSelectCmd.FromCombatPile(ctx, discard, Owner,
            new CardSelectorPrefs(new LocString("card_selection", "FLUIDLOVEBAND-TO_FETCH"), 1))).FirstOrDefault();
        if (chosen == null) return;
        await CardPileCmd.Add(chosen, PileType.Hand);
        chosen.SetToFreeThisTurn();
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

public sealed class HalflingsLeaf() : BandCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, Role.Keys)
{
    protected override IEnumerable<CardKeyword> MoreKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Groove", 2m)];
    protected override IEnumerable<IHoverTip> MoreTips => [HoverTipFactory.Static(BandTips.Groove), HoverTipFactory.FromPower<WeakPower>()];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Buff<GroovePower>(ctx, V("Groove"));
        await Buff<WeakPower>(ctx, 1);
    }
    protected override void OnUpgrade() => DynamicVars["Groove"].UpgradeValueBy(1m);
}

public sealed class Harmonise() : BandCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, Role.Keys)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await RoleChooser.Retune(ctx, Owner, this);
        await Draw(ctx, DynamicVars.Cards.IntValue);
    }
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

public sealed class Soundproofing() : BandCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, Role.Rhythm)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Block(play);
        if (IsFinale) await Block(play);
    }
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(2m);
}

public sealed class BackingVocals() : BandCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, Role.Keys)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        if (Music.Setlist.Get(Owner)?.LastNote is { } last) await Music.Setlist.AddNote(ctx, Owner, last, this);
        await Draw(ctx, DynamicVars.Cards.IntValue);
    }
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

public sealed class CountIn() : BandCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, Role.Rhythm)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(3m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Block(play);
        var setlist = await Music.Setlist.Ensure(ctx, Owner, this);
        if (setlist != null) setlist.ExtraNotesNextCard += 1;
    }
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(2m);
}

public sealed class Capo() : BandCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, Role.Lead)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(0)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        var prefs = new CardSelectorPrefs(new LocString("card_selection", "FLUIDLOVEBAND-CAPO"), 1);
        var picked = (await CardSelectCmd.FromHand(ctx, Owner, prefs,
            c => c is BandCard { CurrentRole: Role.Lead } && c != this, this)).FirstOrDefault();
        picked?.SetToFreeThisTurn();
        if (DynamicVars.Cards.IntValue > 0) await Draw(ctx, DynamicVars.Cards.IntValue);
    }
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

public sealed class MixingDesk() : BandCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, Role.Keys)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        if (Music.Setlist.NoteCount(Owner) > 0 && await RoleChooser.Choose(ctx, Owner) is { } role)
            Music.Setlist.Rewrite(Owner, role, lastOnly: true);
        await Draw(ctx, DynamicVars.Cards.IntValue);
    }
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

public sealed class SoftShoeShuffle() : BandCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, Role.Rhythm, fluid: true)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move)];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Block(play);
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1);
        var chosen = (await CardSelectCmd.FromHand(ctx, Owner, prefs, c => c != this, this)).ToList();
        if (chosen.Count > 0) await CardCmd.Discard(ctx, chosen);
        await Draw(ctx, 2);
    }
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(2m);
}

public sealed class WelcomeToPleasureIsland() : BandCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, Role.Keys)
{
    protected override IEnumerable<CardKeyword> MoreKeywords => [CardKeyword.Exhaust];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        var pool = Owner.Character.CardPool
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c is BandCard { PrintedFluid: false } and not NoteToken && c.Rarity != CardRarity.Basic)
            .ToList();
        foreach (var role in new[] { Role.Lead, Role.Rhythm, Role.Keys })
        {
            var options = pool.Where(c => ((BandCard)c).BaseRole == role);
            var card = CardFactory.GetDistinctForCombat(Owner, options, 1, Owner.RunState.Rng.CombatCardGeneration).FirstOrDefault();
            if (card == null) continue;
            card.SetToFreeThisTurn();
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
        }
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

public sealed class MakinGroovies() : BandCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, Role.Keys, fluid: true)
{
    protected override IEnumerable<CardKeyword> MoreKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Groove", 2m)];
    protected override IEnumerable<IHoverTip> MoreTips => [HoverTipFactory.Static(BandTips.Groove)];
    protected override Task Perform(PlayerChoiceContext ctx, CardPlay play) => Buff<GroovePower>(ctx, V("Groove"));
    protected override void OnUpgrade() => DynamicVars["Groove"].UpgradeValueBy(1m);
}

public sealed class MatterOfTime() : BandCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, Role.Keys)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
    protected override Task Perform(PlayerChoiceContext ctx, CardPlay play) => Buff<MatterOfTimePower>(ctx, DynamicVars.Cards.IntValue);
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

public sealed class AmpFeedback() : BandCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, Role.Lead)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<CardKeyword> MoreKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6m, ValueProp.Move), new DynamicVar("Thorns", 2m)];
    protected override IEnumerable<IHoverTip> MoreTips => [HoverTipFactory.FromPower<ThornsPower>()];
    protected override async Task Perform(PlayerChoiceContext ctx, CardPlay play)
    {
        await Block(play);
        await Buff<ThornsPower>(ctx, V("Thorns"));
    }
    protected override void OnUpgrade() { DynamicVars.Block.UpgradeValueBy(3m); DynamicVars["Thorns"].UpgradeValueBy(1m); }
}
