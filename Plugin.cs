using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using VampireCommandFramework;
using CustomAbilities.Services;

namespace CustomAbilities;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("gg.deca.VampireCommandFramework")]
internal class Plugin : BasePlugin
{
  internal static Plugin Instance { get; private set; }

  Harmony _harmony;

  public static Harmony Harmony => Instance._harmony;
  public static bool IsServer { get; private set; }

  public static ManualLogSource LogInstance => Instance.Log;
  public static bool Initialized { get; internal set; }

  static ConfigEntry<bool> _blockJewelableAbilitiesOnItems;
  static ConfigEntry<bool> _devMode;

  public static bool BlockJewelableAbilitiesOnItems => _blockJewelableAbilitiesOnItems.Value;
  public static bool DevMode => _devMode.Value;

  public override void Load()
  {
    Instance = this;

    IsServer = Application.productName == "VRisingServer";

    if (!IsServer)
    {
      Core.Log.LogWarning("This plugin is intended to run on the server only. It will not function correctly on the client.");
      return;
    }
    _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

    InitConfig();

    CommandRegistry.RegisterAll();

    Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} loaded successfully!");
  }

  void InitConfig()
  {
    _blockJewelableAbilitiesOnItems = Config.Bind("General", "BlockJewelableAbilitiesOnItems", true, "Prevent jewelable abilities (spell school abilities) from being set on items/weapons.");
    _devMode = Config.Bind("General", "DevMode", false, "Enable development mode with additional logging/debugging features");
  }

  public override bool Unload()
  {
    _harmony.UnpatchSelf();

    if (IsServer)
    {
      PlayerDataService.FlushSaveToDisk();
      ItemDataService.FlushSaveToDisk();
    }

    return true;
  }
}
