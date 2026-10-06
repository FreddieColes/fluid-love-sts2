namespace FluidLoveBand.FluidLoveBandCode.Music;

/// <summary>
/// Retune and friends. Picking a role uses the game's own choose-a-card screen with three
/// unplayable Note cards (Lead, Rhythm, Keys) standing in for the options.
/// </summary>
public static class RoleChooser
{
    public static async Task<Role?> Choose(PlayerChoiceContext ctx, Player player)
    {
        var combat = player.Creature?.CombatState;
        if (combat == null) return null;

        var options = new List<CardModel>
        {
            combat.CreateCard(ModelDb.Card<Cards.NoteLead>(), player),
            combat.CreateCard(ModelDb.Card<Cards.NoteRhythm>(), player),
            combat.CreateCard(ModelDb.Card<Cards.NoteKeys>(), player),
        };

        var chosen = await CardSelectCmd.FromChooseACardScreen(ctx, options, player, canSkip: true);
        return chosen switch
        {
            Cards.NoteLead => Role.Lead,
            Cards.NoteRhythm => Role.Rhythm,
            Cards.NoteKeys => Role.Keys,
            _ => null,
        };
    }

    /// <summary>Retune: pick a band card in your hand, then pick the role it plays for the rest of combat.</summary>
    public static async Task Retune(PlayerChoiceContext ctx, Player player, CardModel? source, int count = 1)
    {
        for (var i = 0; i < count; i++)
        {
            var prefs = new CardSelectorPrefs(new LocString("card_selection", "FLUIDLOVEBAND-RETUNE"), 1);
            var picked = (await CardSelectCmd.FromHand(ctx, player, prefs, c => c is Cards.BandCard && c != source, source)).FirstOrDefault();
            if (picked is not Cards.BandCard card) return;

            var role = await Choose(ctx, player);
            if (role == null) return;
            card.RetunedRole = role;
            MainFile.Logger.Info($"Retuned {card.Id.Entry} to {role}");
        }
    }

    public static void RetuneHand(Player player, Role role)
    {
        foreach (var card in PileType.Hand.GetPile(player).Cards.OfType<Cards.BandCard>())
            card.RetunedRole = role;
    }
}
