namespace CustomAbilities.Models;

public class PlayerData
{
  public ulong SteamId { get; set; }
  public string CharacterName { get; set; }
  public int GuidHash { get; set; }
  public int PrefabGuidHash { get; set; } = 0;
  public Dictionary<int, int[]> AbilitySlotDefinitions { get; set; } = new Dictionary<int, int[]>();
}
