using System.Text.Json;
using ProjectM;
using CustomAbilities.Models;
using CustomAbilities.Resources;
using Stunlock.Core;
using Unity.Entities;
using System.Collections;
using UnityEngine;

namespace CustomAbilities.Services;

public
struct ItemData(string prefabGUIDName = "", int itemPrefabGUID = 0, int sequenceGuidHash = 0, int[] abilityGUIDs = null)
{
  public string PrefabGUIDName { get; set; } = prefabGUIDName;
  public int PrefabGUID { get; set; } = itemPrefabGUID;
  public int SequenceGuidHash { get; set; } = sequenceGuidHash;
  public int[] AbilityGUIDs { get; set; } = abilityGUIDs;
}

public static class ItemDataService
{
  // Path for saving item data
  private static readonly string SaveDirectory = Path.Combine(Directory.GetCurrentDirectory(), "BepInEx", "config", "CustomAbilities");
  private static readonly string SavePath = Path.Combine(SaveDirectory, "customabilities_item_data.json");
  private static List<ItemData> _itemDataList = new List<ItemData>();
  private static Dictionary<int, ItemData> _itemDataCache = new Dictionary<int, ItemData>();
  // Dirty flag and periodic save
  private static bool _dataDirty = false;
  private static bool _periodicSaveCoroutineRunning = false;
  private const float PERIODIC_SAVE_INTERVAL = 30f;

  public static void Initialize()
  {
    // Create directory if it doesn't exist
    Directory.CreateDirectory(SaveDirectory);

    // Load existing data
    LoadData();
  }

  public static bool HasItemData(Entity itemEntity)
  {
    int guidHash = 0;
    if (Core.EntityManager.HasComponent<SequenceGUID>(itemEntity))
    {
      var seqGuid = Core.EntityManager.GetComponentData<SequenceGUID>(itemEntity);
      guidHash = seqGuid.GuidHash;
    }

    // Try to find by GUID hash
    if (guidHash != 0)
    {
      var hasData = _itemDataCache.ContainsKey(guidHash);
      return hasData;
    }

    return false;
  }

  public static ItemData GetItemData(Entity itemEntity)
  {
    int guidHash = 0;
    if (Core.EntityManager.HasComponent<SequenceGUID>(itemEntity))
    {
      var seqGuid = Core.EntityManager.GetComponentData<SequenceGUID>(itemEntity);
      guidHash = seqGuid.GuidHash;
    }

    // Try to find by GUID hash
    if (guidHash != 0)
    {
      var guidData = _itemDataCache.ContainsKey(guidHash) ? _itemDataCache[guidHash] : default;
      if (guidData.PrefabGUIDName != "" && guidData.SequenceGuidHash != 0)
        return guidData;
    }

    // If neither found, create new item data
    PrefabGUID itemPrefabGUID = Core.EntityManager.GetComponentData<PrefabGUID>(itemEntity);
    string prefabName = PrefabGUIDsExtensions.GetPrefabGUIDName(itemPrefabGUID);

    guidHash = ItemService.GetItemUniqueId(itemEntity);

    var newData = new ItemData
    {
      PrefabGUIDName = prefabName,
      PrefabGUID = itemPrefabGUID._Value,
      SequenceGuidHash = guidHash,
      AbilityGUIDs = [0, 0, 0, 0, 0, 0, 0, 0]
    };

    _itemDataList.Add(newData);
    _itemDataCache[guidHash] = newData;
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
      string json = JsonSerializer.Serialize(_itemDataList);
      File.WriteAllText(SavePath, json);
    }
    catch (Exception ex)
    {
      Core.Log.LogError($"Failed to save item data: {ex.Message}");
    }
  }

  private static void LoadData()
  {
    if (File.Exists(SavePath))
    {
      try
      {
        string json = File.ReadAllText(SavePath);
        _itemDataList = JsonSerializer.Deserialize<List<ItemData>>(json)
          ?? new List<ItemData>();

        // Populate cache
        _itemDataCache = new Dictionary<int, ItemData>();
        foreach (var itemData in _itemDataList)
        {
          _itemDataCache[itemData.SequenceGuidHash] = itemData;
        }
      }
      catch (Exception ex)
      {
        Core.Log.LogError($"Failed to load item data: {ex.Message}");
        _itemDataList = new List<ItemData>();
      }
    }
  }
}
