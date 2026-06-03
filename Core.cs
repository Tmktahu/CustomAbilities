using BepInEx.Logging;
using ProjectM;
using ProjectM.Scripting;
using Unity.Entities;
using System.Collections;
using CustomAbilities.Services;
using UnityEngine;
using ProjectM.Physics;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace CustomAbilities;

internal static class Core
{
  public static ManualLogSource Log => Plugin.LogInstance;

  public static bool _initialized = false;
  public static World World { get; } = GetServerWorld() ?? throw new Exception("Server world not found. Ensure this plugin is running on a dedicated server.");
  public static EntityManager EntityManager => World.EntityManager;

  static DebugEventsSystem _debugEventsSystem;
  public static DebugEventsSystem DebugEventsSystem => _debugEventsSystem ??= World.GetExistingSystemManaged<DebugEventsSystem>();

  static ServerScriptMapper _serverScriptMapper;
  public static ServerScriptMapper ServerScriptMapper => _serverScriptMapper ??= World.GetExistingSystemManaged<ServerScriptMapper>();

  public static ServerGameManager ServerGameManager => ServerScriptMapper.GetServerGameManager();

  static MonoBehaviour _monoBehaviour;

  public static PlayerService PlayerService { get; private set; }

  public static void Initialize()
  {
    PlayerDataService.Initialize();
    ItemDataService.Initialize();

    PlayerService = new PlayerService();

    _initialized = true;
  }

  static World GetServerWorld()
  {
    return World.s_AllWorlds.ToArray().FirstOrDefault(world => world.Name == "Server");
  }

  public static void StartCoroutine(IEnumerator routine)
  {
    if (_monoBehaviour == null)
    {
      _monoBehaviour = new GameObject(MyPluginInfo.PLUGIN_NAME).AddComponent<IgnorePhysicsDebugSystem>();
      UnityEngine.Object.DontDestroyOnLoad(_monoBehaviour.gameObject);
    }

    _monoBehaviour.StartCoroutine(routine.WrapToIl2Cpp());
  }
}
