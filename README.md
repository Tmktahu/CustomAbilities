![](banner.png)

[![AGPL-3.0 License](https://img.shields.io/static/v1?label=Licence&message=AGPL-3.0&color=green)](https://opensource.org/licenses/AGPL-3.0) [![GitHub Release](https://img.shields.io/static/v1?label=Version&message=1.0.1&color=blue)]() [![Patreon](https://img.shields.io/badge/Patreon-FFFFFF)](https://patreon.com/FrykesFiddlings)

This is the repository for CustomAbilities, coded by Fryke (fryke) on Discord.

CustomAbilities is a V Rising dedicated server mod that allows server admins to assign custom abilities to players and items. Override any ability slot on any weapon category, apply abilities directly to specific items, and provide a public API for other mods to integrate with.

## Features

- **Player Ability Assignment** - Set any ability on any slot (Attack, Q, E, Dash, Spell 1, Spell 2, Ultimate) for any weapon category.
- **Item Ability Assignment** - Apply abilities directly to specific weapons/items. Item abilities override category-level abilities.
- **Quick Set Commands** - Players can set their currently equipped R/C spell to Q/E with a single command.
- **Spell Pool System** - External mods register spell providers to define which spells a player has access to. Player spell assignment is gated by the pool; admin assignment bypasses it.
- **Form Ability Overrides** - API for other mods to supply custom ability arrays for shapeshift forms.
- **Special Case Handlers** - API for other mods to modify ability slots at runtime based on tags, buffs, or state.
- **Jewelable Ability Blocking** - Prevents jewelable abilities (spell school abilities) from being assigned to items, protecting against a known engine bug that can delete jewels and corrupt inventory data.
- **Ability Merge Priority** - Item abilities > category abilities > default, with a clear merge chain.
- **Persistent Data** - All player and item ability data saved to local JSON files and survives server restarts.

## Commands

All commands use the `.abilities` prefix (shorthand: `.ab`).

### Player Commands

| Command | Shorthand | Description | Admin Only |
|---|---|---|---|
| `.abilities set primary` | `.ab set q` | Set your currently selected R spell to Q | No |
| `.abilities set secondary` | `.ab set e` | Set your currently selected C spell to E | No |
| `.abilities clear primary` | `.ab clear q` | Clear your Q ability slot | No |
| `.abilities clear secondary` | `.ab clear e` | Clear your E ability slot | No |
| `.abilities spell list [player]` | `.ab spl` | List available spells from your pool (optionally view another player's pool as admin) | No* |
| `.abilities spell assign <slot> <guid>` | `.ab spa` | Assign a spell from your pool to a slot | No |
| `.abilities spell unassign <slot>` | `.ab spu` | Clear a spell from a slot | No |

\* Viewing another player's spell pool requires admin.

### Admin Commands

| Command | Shorthand | Description |
|---|---|---|
| `.abilities set <slot> <guid> [player] [category]` | `.ab as` | Set an ability on a slot for a player/weapon category |
| `.abilities clear <slot> [player] [category]` | `.ab ac` | Clear an ability slot for a player/weapon category |
| `.abilities clearall [player] [category]` | `.ab aca` | Clear all ability slots for a player/weapon category |
| `.abilities cast <guid>` | `.ab cast` | Cast an ability by prefab GUID |
| `.abilities item set <slot> <guid>` | `.ab ias` | Set an ability on the held item |
| `.abilities item clear <slot>` | `.ab iac` | Clear an ability slot on the held item |
| `.abilities item clearall` | `.ab iaca` | Clear all abilities on the held item |

Slot IDs: `0` = Attack, `1` = Q, `2` = E, `3` = Dash, `4` = Spell 1, `5` = Spell 2, `6` = Ultimate

Weapon categories: `All`, `Unarmed`, `Axe`, `Claws`, `Crossbow`, `Daggers`, `FishingPole`, `GreatSword`, `Longbow`, `Mace`, `Pistols`, `Reaper`, `Slashers`, `Spear`, `Sword`, `TwinBlades`, `Whip`. Passing `All` as the weapon category applies the change across all categories.

## Configuration

Config is stored in `BepInEx/config/io.vrising.CustomAbilities.cfg`.

| Key | Default | Description |
|---|---|---|
| `BlockJewelableAbilitiesOnItems` | `true` | Prevent jewelable abilities (spell school abilities) from being set on items/weapons |
| `DevMode` | `false` | Enable development mode with additional logging/debugging features |

## Data Storage

| File | Path |
|---|---|
| BepInEx config | `BepInEx/config/io.vrising.CustomAbilities.cfg` |
| Player data | `BepInEx/config/CustomAbilities/customabilities_player_data.json` |
| Item data | `BepInEx/config/CustomAbilities/customabilities_item_data.json` |

Data persists through server restarts and is saved automatically every 30 seconds.

## Installation

1. Install [BepInEx](https://thunderstore.io/c/v-rising/p/BepInEx/BepInExPack_V_Rising/) and [VampireCommandFramework](https://github.com/decaprime/VampireCommandFramework) on your V Rising dedicated server.
2. Place `CustomAbilities.dll` in your `BepInEx/plugins/` directory.
3. Start the server, the mod will generate its config file automatically.
4. Configure settings in `BepInEx/config/io.vrising.CustomAbilities.cfg`.

## API for Mod Developers

CustomAbilities provides a public API for other mods. Reference `CustomAbilities.dll` and add `[BepInDependency("io.vrising.CustomAbilities")]` to your Plugin class.

### Registering Form Ability Overrides

Supply custom ability arrays for shapeshift forms. First non-null result wins.

```csharp
CustomAbilities.AbilityAPI.RegisterFormAbilityOverride((playerEntity, formBuffGuid) =>
{
    if (formBuffGuid.Equals(MyFormBuff))
        return new int[8] { 0, 0, 0, 0, myAbility, 0, 0, 0 };
    return null;
});
```

### Registering Special Case Handlers

Modify ability slots at runtime (e.g., based on tags or buffs).

```csharp
CustomAbilities.AbilityAPI.RegisterSpecialCaseHandler((playerEntity, weaponEquipBuff, slots) =>
{
    if (MyTagSystem.HasTag(playerEntity, "empowered"))
        slots[4] = myEmpoweredAbility;
    return slots;
});
```

### Refreshing Abilities

Force an ability refresh after external changes.

```csharp
CustomAbilities.AbilityAPI.RefreshEquipBuff(playerEntity);
```

### Registering Spell Pool Providers

Contribute spells to a player's available spell pool. The spell pool gates which spells players can self-assign via `.abilities spell assign`. Multiple providers are aggregated and deduplicated.

```csharp
CustomAbilities.AbilityAPI.RegisterSpellPoolProvider(playerEntity =>
{
    return new List<int> { mySpellGuid1, mySpellGuid2 };
});
```

Querying the pool:

```csharp
// Get all available spells for a player
List<int> spells = CustomAbilities.AbilityAPI.GetAvailableSpells(playerEntity);

// Check if a specific spell is available
bool hasSpell = CustomAbilities.AbilityAPI.HasAvailableSpell(playerEntity, spellPrefabGuid);
```

## Compatibility with Bloodcraft

[Bloodcraft](https://github.com/mfoltz/Bloodcraft) is another V Rising server mod that provides ability slot customization. Both mods patch the same `ReplaceAbilityOnSlotSystem` to apply ability changes.

**How they interact:**
- Both mods run as Harmony prefixes on the same system, so both execute on every weapon equip buff update.
- CustomAbilities applies abilities with a **priority of 1** (or 9 for form overrides), while Bloodcraft applies with priority 0. Higher priority values take effect, so CustomAbilities assignments override Bloodcraft's when they target the same slot.
- Bloodcraft supports assigning abilities to the unarmed/fishing pole Q and E slots, plus a class spell (dash slot). CustomAbilities supports all 8 ability slots across all weapon categories and individual items.
- The mods do not break each other. If both are installed, Bloodcraft's unarmed slot assignments will still apply to any slots that CustomAbilities hasn't configured.

**Recommendation:** If you use both mods, assign abilities through CustomAbilities for full control. Bloodcraft's unarmed slot assignments will be automatically superseded where CustomAbilities has a slot configured.

## Known Engine Issues and Safeguards

CustomAbilities has safeguards around jewelable abilities (abilities that can have jewels socketed into them) and abilities with recast mechanics. These protections exist because of a known engine-level bug in V Rising:

When a jewelable ability is assigned directly to an item, weapon swapping can cause the jewel on that ability to be deleted and can create a duplicate inventory slot entry for the weapon. This can potentially lead to data corruption. As a precaution, the `BlockJewelableAbilitiesOnItems` config option (enabled by default) prevents jewelable abilities from being assigned to items entirely.

Additionally, if a player assigns a recastable ability to their unarmed Q or E slot and weapon swaps in the middle of a recast, the jewel on that ability can disappear. This is why the mod warns players when they assign a recastable ability to those slots. These are engine-level issues that the mod cannot fix, so the safeguards are in place to protect player data.

## Attribution

This project is licensed under AGPL-3.0.

Portions of code and design patterns in this project were inspired by or adapted from the following projects:

  - KindredCommands <https://github.com/odjit/KindredCommands>
    Licensed under AGPL-3.0
  - Bloodcraft <https://github.com/mfoltz/Bloodcraft>
    Licensed under CC BY-NC 4.0

This is an independent project with its own purpose and functionality. It is not a fork, modification, or derivative of any of the above projects. Some utility code and patterns were referenced during development.
