using System.Text.Json;
using CustomAbilities.Models;
using Unity.Entities;
using System.Collections;
using UnityEngine;
using CustomAbilities.Resources;
using Stunlock.Core;
using ProjectM;
using ProjectM.Network;

namespace CustomAbilities.Services;

public static class PlayerDataService
{
  // Path for saving player data
  private static readonly string SaveDirectory = Path.Combine(Directory.GetCurrentDirectory(), "BepInEx", "config", "CustomAbilities");
  private static readonly string SavePath = Path.Combine(SaveDirectory, "customabilities_player_data.json");
  // Main data store - list of all player data
  private static List<PlayerData> _playerDataList = new List<PlayerData>();
  // Dictionary cache for O(1) lookups by GuidHash
  private static Dictionary<int, PlayerData> _playerDataCache = new Dictionary<int, PlayerData>();
  // Dirty flag and periodic save
  private static bool _dataDirty = false;
  private static bool _periodicSaveCoroutineRunning = false;
  private const float PERIODIC_SAVE_INTERVAL = 30f;

  public static void Initialize()
  {
    Directory.CreateDirectory(SaveDirectory);
    LoadData();
  }

  public static PlayerData GetPlayerData(Entity characterEntity)
  {
    Entity actualCharacterEntity = characterEntity;
    if (Core.EntityManager.TryGetComponentData<PrefabGUID>(characterEntity, out var prefabGuid) && !prefabGuid.Equals(PrefabGUIDs.CHAR_VampireMale))
    {
      // they are controlling a mob, so we want to get the actual entity off the user which we snag through controlledby
      if (Core.EntityManager.TryGetComponentData<ControlledBy>(characterEntity, out var controlledBy))
      {
        Entity userEntity = controlledBy.Controller;
        User user = userEntity.GetUser();
        actualCharacterEntity = user.LocalCharacter._Entity;
      }
    }

    string characterName = actualCharacterEntity.GetUser().CharacterName.ToString();
    if (string.IsNullOrEmpty(characterName)) characterName = "Unknown Player";

    int guidHash = GetGuidHash(actualCharacterEntity);

    // Fast path: O(1) cache lookup
    if (guidHash != 0 && _playerDataCache.TryGetValue(guidHash, out var cached))
      return cached;

    // Slow path: find or create player data
    return GetOrCreatePlayerData(guidHash, characterName, actualCharacterEntity);
  }

  private static PlayerData GetOrCreatePlayerData(int guidHash, string characterName, Entity characterEntity)
  {
    ulong steamId = 0;
    if (characterEntity.TryGetComponent(out User steamUser))
      steamId = steamUser.PlatformId;
    else if (characterEntity.TryGetComponent(out PlayerCharacter steamPC))
      steamId = steamPC.UserEntity.Read<User>().PlatformId;

    if (guidHash != 0)
    {
      var guidData = _playerDataList.Find(pd => pd.GuidHash == guidHash);
      if (guidData != null)
      {
        guidData.CharacterName = characterName;
        _playerDataCache[guidHash] = guidData;
        return guidData;
      }
    }

    var newData = new PlayerData
    {
      SteamId = steamId,
      CharacterName = characterName,
      GuidHash = guidHash != 0 ? guidHash : System.Guid.NewGuid().GetHashCode(),
      PrefabGuidHash = PrefabGUIDsExtensions.GetPrefabGUID(characterEntity).GuidHash
    };
    if (guidHash == 0)
    {
      var newGuid = new ProjectM.SequenceGUID(newData.GuidHash);
      Core.EntityManager.AddComponentData(characterEntity, newGuid);
      guidHash = newData.GuidHash;
    }
    _playerDataList.Add(newData);
    _playerDataCache[guidHash] = newData;
    SaveData();
    return newData;
  }

  public static void SaveData()
  {
    MarkDirty();
  }

  public static void MarkDirty()
  {
    _dataDirty = true;
    if (!_periodicSaveCoroutineRunning)
    {
      Core.StartCoroutine(PeriodicSaveCoroutine());
      _periodicSaveCoroutineRunning = true;
    }
  }

  private static IEnumerator PeriodicSaveCoroutine()
  {
    while (true)
    {
      yield return new WaitForSeconds(PERIODIC_SAVE_INTERVAL);
      if (_dataDirty)
      {
        FlushSaveToDisk();
        _dataDirty = false;
      }
    }
  }

  public static void FlushSaveToDisk()
  {
    try
    {
      // Save as a single JSON array
      string json = JsonSerializer.Serialize(_playerDataList);
      File.WriteAllText(SavePath, json);
    }
    catch (Exception ex)
    {
      Core.Log.LogError($"Failed to save player data: {ex.Message}");
    }
  }

  private static void LoadData()
  {
    if (File.Exists(SavePath))
    {
      try
      {
        string json = File.ReadAllText(SavePath);
        _playerDataList = JsonSerializer.Deserialize<List<PlayerData>>(json)
          ?? new List<PlayerData>();

        // Rebuild cache
        _playerDataCache.Clear();
        foreach (var playerData in _playerDataList)
        {
          if (playerData.GuidHash != 0)
            _playerDataCache[playerData.GuidHash] = playerData;
        }
      }
      catch (Exception ex)
      {
        Core.Log.LogError($"Failed to load player data: {ex.Message}");
        _playerDataList = new List<PlayerData>();
        _playerDataCache.Clear();
      }
    }
  }

  private static int GetGuidHash(Entity characterEntity)
  {
    int guidHash = 0;
    if (Core.EntityManager.HasComponent<ProjectM.SequenceGUID>(characterEntity))
    {
      var seqGuid = Core.EntityManager.GetComponentData<ProjectM.SequenceGUID>(characterEntity);
      guidHash = seqGuid.GuidHash;
    }
    return guidHash;
  }
}
