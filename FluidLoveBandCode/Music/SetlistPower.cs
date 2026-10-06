
namespace FluidLoveBand.FluidLoveBandCode.Music;

/// <summary>
/// The Setlist itself: up to 3 notes. Lives as a power on the player so it is per-creature and
/// shows on the power bar. Nothing mutates it directly; everything goes through <see cref="Setlist"/>.
/// Power models can be reused between combats, so state is reset on every new combat.
/// </summary>
public sealed class SetlistPower : BandPower
{
    public const int Slots = 3;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public readonly List<Role> Notes = new();

    /// <summary>Owner turns started this combat. Fluid cards cycle on this.</summary>
    public int TurnCount { get; set; }
    public SongKind LastSong { get; set; } = SongKind.None;
    public int SongsThisTurn { get; set; }
    public int SongsThisCombat { get; set; }
    public Role? LastNote { get; set; }

    /// <summary>Raised whenever any Setlist changes. The widget listens to this.</summary>
    public static event Action<SetlistPower>? AnyChanged;

    /// <summary>Raised when a Song is played, with its kind. The widget shows the name.</summary>
    public static event Action<SetlistPower, SongKind>? AnySongPlayed;

    private ICombatState? _combat;

    public void SyncCombat()
    {
        var combat = Owner?.CombatState;
        if (combat == null || ReferenceEquals(combat, _combat)) return;
        _combat = combat;
        ClearForNewCombat();
    }

    public void ClearForNewCombat()
    {
        Notes.Clear();
        TurnCount = 0;
        LastSong = SongKind.None;
        SongsThisTurn = 0;
        SongsThisCombat = 0;
        LastNote = null;
        Changed();
    }

    public void Changed() => AnyChanged?.Invoke(this);

    public void Pulse() => Flash();

    public void SongPlayed(SongKind kind) => AnySongPlayed?.Invoke(this, kind);

    public override Task BeforeSideTurnStart(PlayerChoiceContext ctx, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState state)
    {
        SyncCombat();
        if (side == Owner.Side)
        {
            TurnCount++;
            SongsThisTurn = 0;
            Changed();
        }
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _combat = null;
        ClearForNewCombat();
        return Task.CompletedTask;
    }
}
