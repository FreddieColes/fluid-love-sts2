using BaseLib.Patches.UI;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace FluidLoveBand.FluidLoveBandCode.Music;

/// <summary>Registers the Setlist widget with BaseLib's combat-UI hook. Carries no state itself.</summary>
public sealed class SetlistResource() : BasicCustomResource("FluidLoveBand.Setlist")
{
    public override ICustomResourceVisualsHandler ResourceVisualsHandler() => new SetlistDisplay();
}

/// <summary>
/// The Setlist on screen: three note slots above the Energy orb, coloured by role, plus a line
/// that flashes the name of each Song as it plays. Built from stock Godot nodes, driven by events.
/// </summary>
public sealed class SetlistDisplay : ICustomResourceVisualsHandler
{
    private const float Slot = 30f;
    private const float Gap = 8f;
    private const float Width = Slot * 3 + Gap * 2;
    private const float Height = 74f;

    private static readonly Color Empty = new("2b2233");
    private static readonly Color Frame = new("c9a0dc");

    private Creature? _creature;
    private Control? _root;
    private readonly Panel?[] _slots = new Panel?[SetlistPower.Slots];
    private readonly Label?[] _letters = new Label?[SetlistPower.Slots];
    private Label? _song;
    private Label? _fluid;
    private Tween? _songTween;

    public void AddDisplay(NCombatUi nCombatUi, PlayerCombatState playerCombatState)
    {
        var me = CombatManager.Instance?.DebugOnlyGetState() is { } state ? LocalContext.GetMe(state) : null;
        if (me == null || me.PlayerCombatState != playerCombatState || me.Creature == null) return;

        Reset();
        _creature = me.Creature;
        var root = _root = Build();
        Attach(nCombatUi, root);

        // Only shown once this player actually has a Setlist, so other characters see nothing.
        root.Visible = false;

        SetlistPower.AnyChanged += OnChanged;
        SetlistPower.AnySongPlayed += OnSong;
        root.TreeExited += () => { if (_root == root) Reset(); };

        if (_creature.GetPower<SetlistPower>() is { } existing) OnChanged(existing);
    }

    private void Reset()
    {
        SetlistPower.AnyChanged -= OnChanged;
        SetlistPower.AnySongPlayed -= OnSong;
        _creature = null;
        _root = null;
        _song = null;
        _fluid = null;
        _songTween = null;
        Array.Clear(_slots);
        Array.Clear(_letters);
    }

    private void OnChanged(SetlistPower setlist)
    {
        if (setlist.Owner != _creature) return;
        if (_root == null || !GodotObject.IsInstanceValid(_root)) return;
        _root.Visible = true;

        for (var i = 0; i < SetlistPower.Slots; i++)
        {
            Role? note = i < setlist.Notes.Count ? setlist.Notes[i] : null;
            if (_slots[i] is { } slot && GodotObject.IsInstanceValid(slot))
                slot.AddThemeStyleboxOverride("panel", Box(note == null ? Empty : new Color(RoleInfo.Hex(note.Value)), Frame));
            if (_letters[i] is { } letter && GodotObject.IsInstanceValid(letter))
                letter.Text = note == null ? "" : RoleInfo.Name(note.Value)[..1];
        }

        if (_fluid != null && GodotObject.IsInstanceValid(_fluid))
            _fluid.Text = $"Fluid: {RoleInfo.Name(Cards.BandCard.FluidRoleForTurn(setlist.TurnCount, Role.Lead))}";
    }

    private void OnSong(SetlistPower setlist, SongKind kind)
    {
        if (setlist.Owner != _creature) return;
        if (_song == null || !GodotObject.IsInstanceValid(_song)) return;

        _song.Text = RoleInfo.SongName(kind);
        _song.Modulate = Colors.White;
        if (_songTween != null && GodotObject.IsInstanceValid(_songTween)) _songTween.Kill();
        if (!_song.IsInsideTree()) return;
        _songTween = _song.CreateTween();
        _songTween.TweenInterval(1.2);
        _songTween.TweenProperty(_song, "modulate", new Color(1, 1, 1, 0), 0.6);
    }

    private static void Attach(NCombatUi nCombatUi, Control root)
    {
        if (FindEnergy(nCombatUi) is { } energy)
        {
            energy.AddChild(root);
            root.Position = energy.Size.X > 0
                ? new Vector2((energy.Size.X - Width) / 2f, -(Height + 10f))
                : new Vector2(-Width / 2f, -(Height + 10f));
            return;
        }

        nCombatUi.AddChild(root);
        root.SetAnchorsPreset(Control.LayoutPreset.BottomLeft);
        root.Position = new Vector2(40f, -220f);
    }

    private static Control? FindEnergy(Node node)
    {
        foreach (var child in node.GetChildren())
        {
            if (child is Control control && control.Name.ToString().Contains("energy", StringComparison.OrdinalIgnoreCase))
                return control;
            if (FindEnergy(child) is { } found) return found;
        }
        return null;
    }

    private Control Build()
    {
        var root = new Control
        {
            Name = "FluidLoveSetlist",
            CustomMinimumSize = new Vector2(Width, Height),
            Size = new Vector2(Width, Height),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };

        _song = MakeLabel("Song", "", new Vector2(-40f, 0f), new Vector2(Width + 80f, 20f), 18, new Color("ffe08a"));
        root.AddChild(_song);

        for (var i = 0; i < SetlistPower.Slots; i++)
        {
            var pos = new Vector2(i * (Slot + Gap), 22f);
            var slot = new Panel
            {
                Name = $"Note{i}",
                Position = pos,
                Size = new Vector2(Slot, Slot),
                CustomMinimumSize = new Vector2(Slot, Slot),
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };
            slot.AddThemeStyleboxOverride("panel", Box(Empty, Frame));
            root.AddChild(slot);
            _slots[i] = slot;

            var letter = MakeLabel($"Letter{i}", "", pos, new Vector2(Slot, Slot), 16, Colors.White);
            root.AddChild(letter);
            _letters[i] = letter;
        }

        _fluid = MakeLabel("Fluid", "", new Vector2(-30f, 56f), new Vector2(Width + 60f, 18f), 13, new Color("c9a0dc"));
        root.AddChild(_fluid);
        return root;
    }

    private static Label MakeLabel(string name, string text, Vector2 pos, Vector2 size, int fontSize, Color colour)
    {
        var label = new Label
        {
            Name = name,
            Text = text,
            Position = pos,
            Size = size,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AddThemeColorOverride("font_color", colour);
        label.AddThemeColorOverride("font_outline_color", Colors.Black);
        label.AddThemeConstantOverride("outline_size", 4);
        return label;
    }

    private static StyleBoxFlat Box(Color fill, Color border)
    {
        var style = new StyleBoxFlat
        {
            BgColor = fill,
            BorderColor = border,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8,
        };
        style.SetBorderWidthAll(2);
        return style;
    }
}
