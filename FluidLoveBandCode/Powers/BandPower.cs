namespace FluidLoveBand.FluidLoveBandCode.Powers;

/// <summary>Base for the band's powers. Icons load from FluidLoveBand/images/powers/&lt;snake_name&gt;.png.</summary>
public abstract class BandPower : CustomPowerModel
{
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();

    public abstract override PowerType Type { get; }
    public abstract override PowerStackType StackType { get; }
}

/// <summary>Groove: every stack adds 2 to the damage and Block numbers of your Songs. Lasts the combat.</summary>
public sealed class GroovePower : BandPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}
