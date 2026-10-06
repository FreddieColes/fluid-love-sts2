using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace FluidLoveBand.FluidLoveBandCode.Music;

/// <summary>
/// The Setlist panel above the Energy orb.
///   SETLIST ?          header (hover the panel for the full rules)
///   [o] [o] [o]        the three note slots, role icons
///   [L] SOLO [R] JAM   with 2 notes in: what each possible third note plays
///   FLUID [icon]       which role Fluid cards play this turn
/// When a Song plays, the header flashes its name and effect.
/// Attaches itself to the combat UI on the first Setlist change of each combat.
/// </summary>
public static class SetlistDisplay
{
    private const float Width = 236f;
    private const float Height = 156f;
    private const float Slot = 46f;
    private const float SlotGap = 12f;
    private const string Ui = "res://FluidLoveBand/images/ui/";

    private static readonly Color Gold = new("ffe08a");
    private static readonly Color Muted = new("b9a8c8");

    private static Control? _root;
    private static Label? _title;
    private static Label? _effect;
    private static readonly TextureRect?[] _slots = new TextureRect?[SetlistPower.Slots];
    private static readonly TextureRect?[] _previewIcons = new TextureRect?[3];
    private static readonly Label?[] _previewText = new Label?[3];
    private static Control? _previewRow;
    private static Label? _hint;
    private static TextureRect? _fluidIcon;
    private static Tween? _tween;
    private static bool _initialised;

    public static void Init()
    {
        if (_initialised) return;
        _initialised = true;
        SetlistPower.AnyChanged += OnChanged;
        SetlistPower.AnySongPlayed += OnSong;
    }

    private static Texture2D? Icon(Role? role) => ResourceLoader.Load<Texture2D>(Ui + role switch
    {
        Role.Lead => "note_lead.png",
        Role.Rhythm => "note_rhythm.png",
        Role.Keys => "note_keys.png",
        _ => "note_empty.png",
    });

    private static Color SongColour(SongKind kind) =>
        RoleInfo.IsSolo(kind) ? Gold : kind == SongKind.FullBand ? new Color("7fe0a0") : new Color("d8d0e0");

    // ------------------------------------------------------------------ state

    private static bool IsLocal(SetlistPower setlist)
    {
        var me = CombatManager.Instance?.DebugOnlyGetState() is { } state ? LocalContext.GetMe(state) : null;
        return me?.Creature != null && me.Creature == setlist.Owner;
    }

    private static void OnChanged(SetlistPower setlist)
    {
        try
        {
            if (!IsLocal(setlist) || !EnsureBuilt()) return;

            for (var i = 0; i < SetlistPower.Slots; i++)
            {
                Role? note = i < setlist.Notes.Count ? setlist.Notes[i] : null;
                if (_slots[i] is { } slot && GodotObject.IsInstanceValid(slot)) slot.Texture = Icon(note);
            }

            var showPreview = setlist.Notes.Count == SetlistPower.Slots - 1;
            if (_previewRow != null && GodotObject.IsInstanceValid(_previewRow)) _previewRow.Visible = showPreview;
            if (_hint != null && GodotObject.IsInstanceValid(_hint)) _hint.Visible = !showPreview;
            if (showPreview)
            {
                var roles = new[] { Role.Lead, Role.Rhythm, Role.Keys };
                for (var i = 0; i < 3; i++)
                {
                    var kind = RoleInfo.Classify([.. setlist.Notes, roles[i]]);
                    if (_previewIcons[i] is { } ic && GodotObject.IsInstanceValid(ic)) ic.Texture = Icon(roles[i]);
                    if (_previewText[i] is { } tx && GodotObject.IsInstanceValid(tx))
                    {
                        tx.Text = RoleInfo.ShortName(kind).ToUpperInvariant();
                        tx.AddThemeColorOverride("font_color", SongColour(kind));
                    }
                }
            }

            if (_fluidIcon != null && GodotObject.IsInstanceValid(_fluidIcon))
                _fluidIcon.Texture = Icon(Cards.BandCard.FluidRoleForTurn(setlist.TurnCount, Role.Lead));
        }
        catch (Exception e)
        {
            MainFile.Logger.Info($"Setlist display update failed: {e.Message}");
        }
    }

    private static void OnSong(SetlistPower setlist, SongKind kind)
    {
        try
        {
            if (!IsLocal(setlist) || !EnsureBuilt() || _title == null || _effect == null) return;
            var bonus = Setlist.Groove(setlist.Owner?.Player) * Setlist.GroovePerStack;

            _title.Text = RoleInfo.SongName(kind).ToUpperInvariant();
            _title.AddThemeColorOverride("font_color", SongColour(kind));
            _title.AddThemeFontSizeOverride("font_size", 20);
            _effect.Text = RoleInfo.SongEffect(kind, bonus);
            _effect.Visible = true;

            if (_tween != null && GodotObject.IsInstanceValid(_tween)) _tween.Kill();
            if (!_title.IsInsideTree()) return;
            _tween = _title.CreateTween();
            _tween.TweenInterval(2.2);
            _tween.TweenCallback(Callable.From(ResetHeader));
        }
        catch (Exception e)
        {
            MainFile.Logger.Info($"Setlist song display failed: {e.Message}");
        }
    }

    private static void ResetHeader()
    {
        if (_title != null && GodotObject.IsInstanceValid(_title))
        {
            _title.Text = "SETLIST  (?)";
            _title.AddThemeColorOverride("font_color", Muted);
            _title.AddThemeFontSizeOverride("font_size", 15);
        }
        if (_effect != null && GodotObject.IsInstanceValid(_effect)) _effect.Visible = false;
    }

    // ------------------------------------------------------------------ hover

    private static void OnHoverStart()
    {
        if (_root is not { } root || !GodotObject.IsInstanceValid(root)) return;
        IEnumerable<IHoverTip> tips =
        [
            HoverTipFactory.Static(BandTips.Setlist),
            HoverTipFactory.Static(BandTips.Songs),
            HoverTipFactory.Static(BandTips.Groove),
            HoverTipFactory.Static(BandTips.Words),
        ];
        NHoverTipSet.CreateAndShow(root, tips, HoverTip.GetHoverTipAlignment(root))?.SetFollowOwner();
    }

    private static void OnHoverEnd()
    {
        if (_root is { } root && GodotObject.IsInstanceValid(root)) NHoverTipSet.Remove(root);
    }

    // ------------------------------------------------------------------ nodes

    private static bool EnsureBuilt()
    {
        if (_root != null && GodotObject.IsInstanceValid(_root) && _root.IsInsideTree()) return true;
        _root = null;
        if (Godot.Engine.GetMainLoop() is not SceneTree tree) return false;
        if (FindCombatUi(tree.Root) is not { } ui) return false;
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

    private static Control? FindEnergy(Node node)
    {
        foreach (var child in node.GetChildren())
        {
            if (child is Control c && c.Name.ToString().Contains("energy", StringComparison.OrdinalIgnoreCase)) return c;
            if (FindEnergy(child) is { } found) return found;
        }
        return null;
    }

    private static void Attach(NCombatUi ui, Control root)
    {
        if (FindEnergy(ui) is { } energy)
        {
            energy.AddChild(root);
            root.Position = new Vector2((energy.Size.X - Width) / 2f, -(Height + 14f));
        }
        else
        {
            ui.AddChild(root);
            root.Position = new Vector2(24f, 560f);
        }
        root.TreeEntered += () => ClampOnScreen(root);
        ClampOnScreen(root);
    }

    /// <summary>Keeps the whole panel inside the screen, whatever the resolution.</summary>
    private static void ClampOnScreen(Control root)
    {
        if (!GodotObject.IsInstanceValid(root) || !root.IsInsideTree()) return;
        var screen = root.GetViewportRect().Size;
        var gp = root.GlobalPosition;
        var dx = 0f; var dy = 0f;
        if (gp.X < 8f) dx = 8f - gp.X;
        if (gp.X + Width > screen.X - 8f) dx = screen.X - 8f - Width - gp.X;
        if (gp.Y < 8f) dy = 8f - gp.Y;
        root.Position += new Vector2(dx, dy);
    }

    private static Control Build()
    {
        var root = new Control
        {
            Name = "FluidLoveSetlist",
            Size = new Vector2(Width, Height),
            CustomMinimumSize = new Vector2(Width, Height),
            MouseFilter = Control.MouseFilterEnum.Stop,
        };
        root.MouseEntered += OnHoverStart;
        root.MouseExited += OnHoverEnd;

        var panel = new Panel { Size = new Vector2(Width, Height), MouseFilter = Control.MouseFilterEnum.Ignore };
        var style = new StyleBoxFlat
        {
            BgColor = new Color(0.08f, 0.05f, 0.11f, 0.82f),
            BorderColor = new Color("c9a0dc"),
            CornerRadiusTopLeft = 14, CornerRadiusTopRight = 14, CornerRadiusBottomLeft = 14, CornerRadiusBottomRight = 14,
        };
        style.SetBorderWidthAll(2);
        panel.AddThemeStyleboxOverride("panel", style);
        root.AddChild(panel);

        _title = MakeLabel("SETLIST  (?)", new Vector2(0, 6), new Vector2(Width, 22), 15, Muted);
        root.AddChild(_title);
        _effect = MakeLabel("", new Vector2(0, 28), new Vector2(Width, 18), 13, Colors.White);
        _effect.Visible = false;
        root.AddChild(_effect);

        var slotsLeft = (Width - (Slot * 3 + SlotGap * 2)) / 2f;
        for (var i = 0; i < SetlistPower.Slots; i++)
        {
            var slot = MakeIcon(new Vector2(slotsLeft + i * (Slot + SlotGap), 48), Slot);
            slot.Texture = Icon(null);
            root.AddChild(slot);
            _slots[i] = slot;
        }

        _previewRow = new Control { Position = new Vector2(0, 104), Size = new Vector2(Width, 24), MouseFilter = Control.MouseFilterEnum.Ignore, Visible = false };
        for (var i = 0; i < 3; i++)
        {
            var x = 8 + i * 76f;
            var ic = MakeIcon(new Vector2(x, 2), 20);
            _previewRow.AddChild(ic);
            _previewIcons[i] = ic;
            var tx = MakeLabel("", new Vector2(x + 22, 0), new Vector2(52, 24), 13, Colors.White);
            tx.HorizontalAlignment = HorizontalAlignment.Left;
            _previewRow.AddChild(tx);
            _previewText[i] = tx;
        }
        root.AddChild(_previewRow);

        _hint = MakeLabel("3 notes = a Song", new Vector2(0, 104), new Vector2(Width, 24), 13, Muted);
        root.AddChild(_hint);

        root.AddChild(MakeLabel("FLUID =", new Vector2(56, 130), new Vector2(80, 20), 13, new Color("c9a0dc")));
        _fluidIcon = MakeIcon(new Vector2(140, 130), 20);
        root.AddChild(_fluidIcon);

        return root;
    }

    private static TextureRect MakeIcon(Vector2 pos, float size) => new()
    {
        Position = pos,
        Size = new Vector2(size, size),
        CustomMinimumSize = new Vector2(size, size),
        ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
        StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
        MouseFilter = Control.MouseFilterEnum.Ignore,
    };

    private static Label MakeLabel(string text, Vector2 pos, Vector2 size, int fontSize, Color colour)
    {
        var label = new Label
        {
            Text = text,
            Position = pos,
            Size = size,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            ClipText = true,
        };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AddThemeColorOverride("font_color", colour);
        label.AddThemeColorOverride("font_outline_color", Colors.Black);
        label.AddThemeConstantOverride("outline_size", 4);
        return label;
    }
}
