# Custom hero base classes

`[HeroPreset("Name", HeroType.X)]` registers a `CustomHero` (gun) or `SwordHero` (melee) bro; the `HeroType` picks the vanilla prefab cloned as its base (default `Rambro`).

## Lifecycle

```
PrefabSetup()   once per prefab, from LoadHero.CreateOriginal
  BeforePrefabSetup()
  sound holders cloned from the vanilla prefab
  AssignDirectoryPaths(LoadHero.currentInfo.path)
  LoadSettings()
  Character = this; Info = LoadHero.currentInfo
  SetupCustomHero(false)
  AfterPrefabSetup()

Awake()         per instance
  Character = this; Info = LoadHero.currentInfo
  EnableSyncing()
  CurrentVariant = GetVariant(); RegisterVariant() if deduplicateVariants; cache the variant's properties

Start()
  meleeType = currentMeleeType = Custom if useCustomMelee
```

- `PrefabSetup` holds no instance state. Prefabs are cached by `CustomBroInfo.name`, so two JSON bros sharing a C# type each get their own prefab.
- `HarmonyPatches`, `RegisterCustomTriggers` and `PreloadAssets` run once at startup on a throwaway component on an inactive holder GameObject, never on a spawned bro.
- Set `SoundHolderHeroType` and `SoundHolderVoiceType` in `BeforePrefabSetup`. `SoundHolderHeroType.None` uses the preset's `HeroType`; `SoundHolderVoiceType.None` takes the voice of `SoundHolderHeroType` (or the preset's `HeroType`); other values pick a vanilla bro's voice (`Female` is Ellen Ripbro's).

## Update

- Every `Update()` override calls `base.Update()` first, then `if (acceptedDeath) return;`. Skipping `base.Update()` freezes a dead bro mid-air.
- Register extra materials for the invulnerability tint with `RegisterTintMaterial(Material)`; `ResetTintColors()` resets them with the main and gun sprites, and runs when invulnerability ends if `trackInvulnerabilityTint` is set.

## Custom melee

`useCustomMelee` routes the melee through `StartCustomMelee`, `AnimateCustomMelee` and `RunCustomMeleeMovement`. Holding jump re-applies `jumpForce` while `jumpTime > 0`; `jumpTime` freezes during a melee, and `StartCustomMelee` doesn't zero it. A melee ending with `jumpTime` positive and jump held gets a fresh jump impulse; one that never reaches `CancelMelee` leaves `doingMelee` set and kills the hold-boost for good.

`CancelMelee` is the restore hook (`ResetMeleeValues` fires mid-combo and restores nothing). The base always restores `maxFallSpeed` to `originalMaxFallSpeed`, and clears the melee flags and re-derives `actionState` only while `doingMelee` is set. A `useCustomMelee` bro:

- reaches `CancelMelee()` on every terminal frame of `AnimateCustomMelee`;
- calls `base.CancelMelee()` from its override;
- zeroes `jumpTime` on entry or in `CancelMelee`;
- restores in `CancelMelee` every `speed`, `canWallClimb` or gravity write the melee makes.

## Variants

A variant property in `CustomBroInfo` is a list of one item (shared) or `VariantCount` items; `BroMakerLib/Infos/AGENTS.md` has the full length rule. With `deduplicateVariants`, `GetVariant()` avoids variants other instances of the same bro type use, falling back to random when all are taken.

## Settings

On a `CustomHero` type, `[SaveableSetting]` (or `[SaveableSetting("json_key")]`) persists a static field or property per bro type to `Config/BroMaker/Bros/<TypeName>/<TypeName>_settings.json` in the Unity Mod Manager folder.

## Abilities

A JSON bro naming `CustomHero`, `SwordHero` or a subclass as `CharacterPreset` gets a warning naming its ignored non-empty `special` / `melee` / `passives` blocks. A programmed bro that needs abilities subclasses a vanilla `*M` wrapper instead of `CustomHero`, with a `[HeroPreset]` of its own (the attribute isn't inherited) under a new preset name and the wrapper's `HeroType`; it gets none of `CustomHero`'s `[SaveableSetting]` persistence, `HarmonyPatches`, `RegisterCustomTriggers`, `PreloadAssets` or `UIOptions`.
