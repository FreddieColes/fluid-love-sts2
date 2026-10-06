namespace FluidLoveBand.FluidLoveBandCode.Potions;

[Pool(typeof(BandPotionPool))]
public abstract class BandPotion : CustomPotionModel
{
    private string Slug => Id.Entry.RemovePrefix().ToLowerInvariant();
    public override string? CustomPackedImagePath => $"{Slug}.png".PotionImagePath();
    public override string? CustomPackedOutlinePath => $"{Slug}.png".PotionOutlineImagePath();

    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;
}

/// <summary>Gain 2 Groove.</summary>
public sealed class PintOfCider : BandPotion
{
    public override PotionRarity Rarity => PotionRarity.Common;

    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        var me = Owner.Creature;
        await PowerCmd.Apply<GroovePower>(ctx, me, 2m, me, null);
    }
}

/// <summary>Encore: play your last Song again.</summary>
public sealed class ThroatSpray : BandPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;
    protected override Task OnUse(PlayerChoiceContext ctx, Creature? target) => Setlist.Encore(ctx, Owner);
}

/// <summary>Choose a role: every card in your hand plays it this combat. Gain 10 Block.</summary>
public sealed class SmokeMachine : BandPotion
{
    public override PotionRarity Rarity => PotionRarity.Rare;
    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        if (await RoleChooser.Choose(ctx, Owner) is { } role) RoleChooser.RetuneHand(Owner, role);
        await Setlist.Block(Owner.Creature, 10);
    }
}
