# Infos

`CustomBroInfo` is a bro's JSON config:

```json
{
  "name": "My Bro",
  "CharacterPreset": "Rambro",
  "parameters": { "Sprite": "bro.png", "Avatar": "avatar.png" },
  "beforeAwake": { "fieldName": "value" },
  "afterAwake":  { "fieldName": "value" },
  "beforeStart": { "fieldName": "value" },
  "afterStart":  { "fieldName": "value" }
}
```

- In `special`, `melee` and `passives`, `preset` is required, `allowConflict` applies to passives only and takes `true` or `false` unquoted (any other value but `null` skips the passive), and every other key sets a field on the ability.
- `path` is never read from JSON; it is set to the bro JSON's folder (the mod folder for an inline bro). `Mod`, an internal field set beside it by BroMakerLib's storage code, attributes the bro's reports to its mod; an info a mod creates itself has no `Mod`, so its reports resolve as any report without a mod does.
- A mod's info for an object that isn't a bro derives from `CustomBroforceObjectInfo`, `CustomBroInfo`'s base, which carries `parameters` and the field dictionaries with their callbacks.

## Field dictionaries

- `beforeAwake`, `afterAwake`, `beforeStart` and `afterStart` set bro fields around `Awake` / `Start`, bypassing `parameters`. Keys may be dotted paths (`"projectile.damage"`); an unresolvable key logs a warning and is skipped.
- A dotted key first swaps each shared prefab component along its path, up to the first struct, for a per-bro copy, here and in ability overrides, so writes never reach the shared vanilla asset (`ICustomProjectile` prefabs aren't copied).
- Values convert by field type, here and in ability overrides: sprites, materials and textures load by path; `AudioClip` / `AudioClip[]` load sounds; enums parse case-insensitively from a string or cast from an int; `Vector2`, `Vector3` and `Color` take a number array (three numbers for a `Color` set alpha 1) or an object of named components (`x`/`y`/`z`, `r`/`g`/`b`/`a`), an object's omitted component keeping its value; `Projectile` / `Grenade` resolve by vanilla name.
- A png on a `SpriteSM` field, here and in ability overrides, retextures that sprite in place when it is an instantiated object, the bro's own included, re-pointing every `Material` field of the bro (or the ability) that held its old material; a sprite on a shared prefab is swapped for a per-bro copy carrying the image, and one already on such a copy gets a new material carrying it, with no field re-pointed.
- Per-bro copies stay off `InstantiationController.PrefabList`, which networked spawns address by index: an appended copy would give every custom bro prefab `LoadHero` appends after it a different index than on a client without that copy. A networked spawn of a copy of a listed prefab spawns the copy locally and sends that prefab's index, so other clients instantiate the vanilla prefab, without the bro's changes.

## Parameters

- Each `parameters` key names a `[Parameter]` method exactly; a typo or type mismatch logs an error and skips the entry.
- `Sprite`, `GunSprite`, `GunSpriteOffset`, `SpecialIcons`, `SpecialIconOffset`, `SpecialIconSpacing`, `Avatar` and the `cutscene` list each take one value (shared) or exactly `VariantCount` values, the largest count among them. An empty list reads as unset, except that a `cutscene` list given as `[]` or `null` means no cutscene, and the unlock-cutscene trigger then logs a Warning and doesn't finish the campaign (an omitted one holds one default cutscene); a list of two or more entries that isn't `VariantCount` long stops the bro with an Error: a `CustomHero` bro fails to spawn, a `*M` wrapper bro spawns disabled.
- A list-filling parameter does nothing once its list is filled, so the first call wins.
- `BetterAnimation` is `true` when omitted. `Halo`, `BetterAnimation` and `JetPackSprite` act only on `true`; `false` resets nothing.
- `SpecialIcons` takes `"icon.png"` (shared), `["a.png", "b.png"]` (one set, an icon per special ammo slot) or `[["v1a.png", "v1b.png"], ["v2a.png"]]` (a set per variant).
- `GunSpriteOffset` and `SpecialIconOffset` take `{ "x": 2, "y": 0 }` (shared) or an array of such objects (an offset per variant), never the number array a `Vector2` field takes in the field dictionaries: an array item that isn't an object is skipped, so `[2, 0]` reads as an empty list and leaves the offset at zero with no report.

## Order

Parameters are read through `SetupCustomHero`, once per prefab in a `CustomHero`'s `PrefabSetup` and on every spawn at the start of a `*M` wrapper's `Awake`, which then applies `beforeAwake`, runs its awake fixes, then creates its abilities (an ability's `Initialize(owner)` already sees the `beforeAwake` values), then calls `base.Awake()`. The order is `beforeAwake` → abilities → `base.Awake` → sprites → `afterAwake` → `beforeStart` → `base.Start` → `afterStart`; a `CustomHero` has the same order without the abilities step.
