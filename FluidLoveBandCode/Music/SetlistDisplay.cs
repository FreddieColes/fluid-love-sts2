using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace FluidLoveBand.FluidLoveBandCode.Music;

/// <summary>
/// The Setlist on screen: three note slots above the Energy orb, coloured by role, plus a line
/// that flashes the name of each Song as it plays. Built from stock Godot nodes, driven by events.
/// It attaches itself to the combat UI the first time the local player's Setlist changes in a
/// combat, so it needs no hook into the UI build.
/// </summary>
public static class SetlistDisplay
{
    private const float Slot = 30f;
    private const float Gap = 8f;
    private const float Width = Slot * 3 + Gap * 2;
    private const float Height = 112f;

    private static readonly Color Empty = new("2b2233");
    private static readonly Color Frame = new("c9a0dc");

    private static Control? _root;
    private static readonly Panel?[] _slots = new Panel?[SetlistPower.Slots];
    private static readonly Label?[] _letters = new Label?[SetlistPower.Slots];
    private static Label? _song;
    private static Label? _fluid;
    private static Label? _effect;
    private static Label? _preview;
    private static Tween? _songTween;
    private static bool _initialised;

    public static void Init()
    {
        if (_initialised) return;
        _initialised = true;
        SetlistPower.AnyChanged += OnChanged;
        SetlistPower.AnySongPlayed += OnSong;
    }

    private static bool IsLocal(SetlistPower setlist)
    {
        var me = CombatManager.Instance?.DebugOnlyGetState() is { } state ? LocalContext.GetMe(state) : null;
        return me?.Creature != null && me.Creature == setlist.Owner;
    }

    private static bool EnsureBuilt()
    {
        if (_root != null && GodotObject.IsInstanceValid(_root) && _root.IsInsideTree()) return true;

        _root = null;
        if (Godot.Engine.GetMainLoop() is not SceneTree tree) return false;
        var ui = FindCombatUi(tree.Root);
        if (ui == null) return false;

        _root = Build();
        Attach(ui, _root);
        return true;
    }

    private static NCombatUi? FindCombatUi(Node node)
    {
        if (node is NCombatUi ui) return ui;
        foreach (var child in node.GetChildren())
            if (FindCombatUi(child) is { } found) return found;
        return null;
    }

    private static void OnChanged(SetlistPower setlist)
    {
        try
        {
            if (!IsLocal(setlist) || !EnsureBuilt() || _root == null) return;

            for (var i = 0; i < SetlistPower.Slots; i++)
            {
                Role? note = i < setlist.Notes.Count ? setlist.Notes[i] : null;
                if (_slots[i] is { } slot && GodotObject.IsInstanceValid(slot))
                    slot.AddThemeStyleboxOverride("panel", Box(note == null ? Empty : new Color(RoleInfo.Hex(note.Value)), Frame));
                if (_letters[i] is { } letter && GodotObject.IsInstanceValid(letter))
                    letter.Text = note == null ? "" : RoleInfo.Name(note.Value)[..1];
            }

            if (_fluid != null && GodotObject.IsInstanceValid(_fluid))
                _fluid.Text = $"Fluid cards play {RoleInfo.Name(Cards.BandCard.FluidRoleForTurn(setlist.TurnCount, Role.Lead))} this turn";

            if (_preview != null && GodotObject.IsInstanceValid(_preview))
                _preview.Text = Preview(setlist);
        }
        catch (Exception e)
        {
            MainFile.Logger.Info($"Setlist display update failed: {e.Message}");
        }
    }

    /// <summary>With 2 notes in, shows what each possible third note would play, e.g. "L: Solo  R: Jam  K: Full Band".</summary>
    private static string Preview(SetlistPower setlist)
    {
        if (setlist.Notes.Count != SetlistPower.Slots - 1) return setlist.Notes.Count == 0 ? "Play cards to add notes" : "";
        var parts = new List<string>();
        foreach (var role in new[] { Role.Lead, Role.Rhythm, Role.Keys })
        {
            var kind = RoleInfo.Classify([.. setlist.Notes, role]);
            parts.Add($"{RoleInfo.Name(role)[..1]}: {RoleInfo.ShortName(kind)}");
        }
        return "Next note: " + string.Join("   ", parts);
    }

    private static void OnSong(SetlistPower setlist, SongKind kind)
    {
        try
        {
            if (!IsLocal(setlist) || !EnsureBuilt()) return;
            if (_song == null || !GodotObject.IsInstanceValid(_song)) return;

            var bonus = Setlist.Groove(setlist.Owner?.Player) * Setlist.GroovePerStack;
            _song.Text = RoleInfo.SongName(kind);
            _song.Modulate = Colors.White;
            if (_effect != null && GodotObject.IsInstanceValid(_effect))
            {
                _effect.Text = RoleInfo.SongEffect(kind, bonus);
                _effect.Modulate = Colors.White;
            }
            if (_songTween != null && GodotObject.IsInstanceValid(_songTween)) _songTween.Kill();
            if (!_song.IsInsideTree()) return;
            _songTween = _song.CreateTween();
            _songTween.TweenInterval(2.0);
            _songTween.TweenProperty(_song, "modulate", new Color(1, 1, 1, 0), 0.8);
            if (_effect != null && GodotObject.IsInstanceValid(_effect))
                _songTween.Parallel().TweenProperty(_effect, "modulate", new Color(1, 1, 1, 0), 0.8);
        }
        catch (Exception e)
        {
            MainFile.Logger.Info($"Setlist song display failed: {e.Message}");
        }
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

    private static Control Build()
    {
        var root = new Control
        {
            Name = "FluidLoveSetlist",
            CustomMinimumSize = new Vector2(Width, Height),
            Size = new Vector2(Width, Height),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };

        _song = MakeLabel("Song", "", new Vector2(-80f, 0f), new Vector2(Width + 160f, 24f), 22, new Color("ffe08a"));
        root.AddChild(_song);
        _effect = MakeLabel("Effect", "", new Vector2(-120f, 24f), new Vector2(Width + 240f, 18f), 14, new Color("ffffff"));
        root.AddChild(_effect);

        for (var i = 0; i < SetlistPower.Slots; i++)
        {
            var pos = new Vector2(i * (Slot + Gap), 46f);
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

        _preview = MakeLabel("Preview", "", new Vector2(-120f, 80f), new Vector2(Width + 240f, 16f), 13, new Color("ffe08a"));
        root.AddChild(_preview);
        _fluid = MakeLabel("Fluid", "", new Vector2(-120f, 96f), new Vector2(Width + 240f, 16f), 12, new Color("c9a0dc"));
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
