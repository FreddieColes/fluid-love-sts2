using BaseLib.Utils.NodeFactories;
using FluidLoveBand.FluidLoveBandCode.Cards;
using FluidLoveBand.FluidLoveBandCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;

namespace FluidLoveBand.FluidLoveBandCode.Character;

/// <summary>
/// The Fluid Love Band: Alex (lead vocals, guitar), Toby (guitar, bass), Freddie (keys) and a
/// laptop running the drum track. A country rock folk electric disco band, playing their way up the Spire.
/// </summary>
public class Band : PlaceholderCharacterModel
{
    public const string CharacterId = "Band";

    /// <summary>Disco orange.</summary>
    public static readonly Color Color = new("f08a24");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 72;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeBand>(),
        ModelDb.Card<StrikeBand>(),
        ModelDb.Card<StrikeBand>(),
        ModelDb.Card<StrikeBand>(),
        ModelDb.Card<DefendBand>(),
        ModelDb.Card<DefendBand>(),
        ModelDb.Card<DefendBand>(),
        ModelDb.Card<DefendBand>(),
        ModelDb.Card<TuneUp>(),
        ModelDb.Card<Soundcheck>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<BackingTrack>()];

    public override CardPoolModel CardPool => ModelDb.CardPool<BandCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<BandRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<BandPotionPool>();

    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }

    public override string CustomIconTexturePath => "character_icon_band.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_band.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_band_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_band.png".CharacterUiPath();
}

public class BandCardPool : CustomCardPoolModel
{
    public override string Title => Band.CharacterId; // not a display name

    public override string BigEnergyIconPath => "big_energy.png".CharacterUiPath();
    public override string TextEnergyIconPath => "text_energy.png".CharacterUiPath();

    // Card-back tint (shader on an already coloured frame). Tune by eye in game.
    public override float H => 0.08f;
    public override float S => 1f;
    public override float V => 1f;

    public override Color DeckEntryCardColor => Band.Color;
    public override bool IsColorless => false;
}

public class BandRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => Band.Color;
    public override string BigEnergyIconPath => "big_energy.png".CharacterUiPath();
    public override string TextEnergyIconPath => "text_energy.png".CharacterUiPath();
}

public class BandPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => Band.Color;
    public override string BigEnergyIconPath => "big_energy.png".CharacterUiPath();
    public override string TextEnergyIconPath => "text_energy.png".CharacterUiPath();
}
