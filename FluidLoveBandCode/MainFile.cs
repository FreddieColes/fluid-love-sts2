using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace FluidLoveBand.FluidLoveBandCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "FluidLoveBand";
    public const string ResPath = $"res://{ModId}";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();
        Harmony harmony = new(ModId);
        harmony.PatchAll(assembly);
        Music.SetlistDisplay.Init();
        Logger.Info("Fluid Love Band loaded. Sound check one two.");
    }
}
