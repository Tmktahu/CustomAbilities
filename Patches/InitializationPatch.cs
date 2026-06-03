using HarmonyLib;
using ProjectM;

namespace CustomAbilities.Patches;

[HarmonyPatch]
internal static class InitializationPatch
{
  [HarmonyPatch(typeof(SpawnTeamSystem_OnPersistenceLoad), nameof(SpawnTeamSystem_OnPersistenceLoad.OnUpdate))]
  [HarmonyPostfix]
  static void OnUpdatePostfix()
  {
    if (Core._initialized) return;

    try
    {
      Core.Initialize();
      Core.Log.LogInfo("CustomAbilities initialized successfully.");
    }
    catch (Exception ex)
    {
      Core.Log.LogError($"Initialization failed: {ex.Message}");
    }
  }
}
