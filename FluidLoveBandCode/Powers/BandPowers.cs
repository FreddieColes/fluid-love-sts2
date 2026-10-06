namespace FluidLoveBand.FluidLoveBandCode.Powers;

// Most of these are read directly by Setlist.PlaySong. The ones with turn hooks act themselves.

public abstract class BandBuff : BandPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected bool IsMyTurnStart(CombatSide side) => side == Owner.Side && !Owner.IsDead;
}

/// <summary>Your Lead Songs deal X more damage.</summary>
public sealed class FrontOfStagePower : BandBuff { }

/// <summary>Your Rhythm Songs grant X more Block.</summary>
public sealed class GiantsOfAlbionPower : BandBuff { }

/// <summary>Your Keys Songs apply X Weak and X Vulnerable to ALL enemies.</summary>
public sealed class KrishnakPower : BandBuff { }

/// <summary>Whenever you play a Song, deal X damage to ALL enemies.</summary>
public sealed class DiscoBallPower : BandBuff { }

/// <summary>Whenever you play a Jam, draw 1 card and gain X Block.</summary>
public sealed class InThePocketPower : BandBuff { }

/// <summary>Whenever you play a Full Band, gain X Energy.</summary>
public sealed class HarmoniesPower : BandBuff { }

/// <summary>Whenever you play a Song, Upgrade a random card in your hand.</summary>
public sealed class FountainOfYouthPower : BandBuff
{
    public override PowerStackType StackType => PowerStackType.Single;
}

/// <summary>Each Song this turn deals and grants X more than the one before.</summary>
public sealed class CrescendoPower : BandBuff { }

/// <summary>Your Solos are played twice.</summary>
public sealed class HeadlinerPower : BandBuff
{
    public override PowerStackType StackType => PowerStackType.Single;
}

/// <summary>Whenever you play a Full Band, also play a Lead, Rhythm and Keys Solo.</summary>
public sealed class ManOfTheClothPower : BandBuff
{
    public override PowerStackType StackType => PowerStackType.Single;
}

/// <summary>At the start of your turn, add X Rhythm notes.</summary>
public sealed class DrumTrackPower : BandBuff
{
    public override async Task BeforeSideTurnStart(PlayerChoiceContext ctx, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState state)
    {
        if (!IsMyTurnStart(side) || Owner.Player is not { } player) return;
        Flash();
        await Setlist.AddNotes(ctx, player, Role.Rhythm, (int)Amount);
    }
}

/// <summary>At the start of your turn, gain X Groove.</summary>
public sealed class HolyKingPower : BandBuff
{
    public override async Task BeforeSideTurnStart(PlayerChoiceContext ctx, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState state)
    {
        if (!IsMyTurnStart(side)) return;
        Flash();
        await PowerCmd.Apply<GroovePower>(ctx, Owner, Amount, Owner, null);
    }
}

/// <summary>Ol' Mac joins the band: at the start of your turn, deal X damage to ALL enemies and add a random note.</summary>
public sealed class OlMacPower : BandBuff
{
    public override async Task BeforeSideTurnStart(PlayerChoiceContext ctx, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState state)
    {
        if (!IsMyTurnStart(side) || Owner.Player is not { } player) return;
        Flash();
        await Setlist.DamageAll(ctx, Owner, (int)Amount);
        var role = (Role)player.RunState.Rng.CombatCardGeneration.NextInt(0, 2);
        await Setlist.AddNote(ctx, player, role);
    }
}

/// <summary>Next turn, gain X Block. Then removed.</summary>
public sealed class NextTurnBlockPower : BandBuff
{
    public override async Task BeforeSideTurnStart(PlayerChoiceContext ctx, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState state)
    {
        if (!IsMyTurnStart(side)) return;
        Flash();
        await Setlist.Block(Owner, (int)Amount);
        await PowerCmd.Remove(this);
    }
}

/// <summary>Next turn, gain 1 Energy and draw X cards. Then removed.</summary>
public sealed class MatterOfTimePower : BandBuff
{
    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner.Player) return;
        Flash();
        await PlayerCmd.GainEnergy(1, player);
        await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), (int)Amount, player);
        await PowerCmd.Remove(this);
    }
}

/// <summary>Ready For Business: gain 1 Energy each turn; your first Song each turn is played twice (read by Setlist).</summary>
public sealed class ReadyForBusinessPower : BandBuff
{
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner.Player) return;
        Flash();
        await PlayerCmd.GainEnergy(1, player);
    }
}
