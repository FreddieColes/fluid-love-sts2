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
