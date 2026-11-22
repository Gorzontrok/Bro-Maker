# Storages

## Mod format

BroMaker loads every `*.mod.json` manifest under `BroMaker_Storage/`, recursively; renaming one to another ending (`MyMod.mod.json.disabled`) disables its mod. Of two compatible manifests with the same `Name`, only the one with the higher `Version` loads (the first found on a tie), and of two bros with the same `name`, only the first registered; each skip logs a Warning.

```json
{
    "Name": "MyMod",
    "Author": "Author",
    "Version": "1.0.0",
    "HasHarmonyPatch": true,
    "CustomBros": ["MyBro.json"],
    "Assemblies": ["MyBro.dll"]
}
```

- `CustomBros`: bro JSON paths relative to the manifest's folder (a missing file is dropped with a Warning), or inline bro objects.
- `Assemblies`: the mod's DLLs (programmed bros, ability presets, parameters, Harmony patches). An empty entry or a missing file is skipped with a Warning.
- `HasHarmonyPatch`: without `true`, a DLL's `[HarmonyPatch]` classes are never applied, and nothing is logged. `CustomHero.HarmonyPatches(harmony)` gets BroMaker's own instance and doesn't need it.
- An unnamed mod takes its folder's name.

## Compatibility

A mod with a listed DLL that exists is judged by scanning its DLLs, without loading them, then by loading them. Any other mod, JSON-only or with every `Assemblies` entry empty or missing, is refused only when it loses every bro to an unresolvable `CharacterPreset`; a manifest's `BroMakerVersion` is ignored. A mod is refused:

- at the scan, when a DLL can't be scanned, or references a BroMakerLib or RocketLib type that isn't there, or a member that isn't there or isn't accessible (`InternalsVisibleTo` counts);
- in `PresetManager.Initialize()`, when a DLL fails `Assembly.LoadFile` / `GetTypes`, a mod type overrides a method its BroMakerLib or RocketLib base doesn't have, a missing-member, access or type-load exception (`BMLogger.IsIncompatibleAssembly`) is thrown while scanning a DLL's presets, or every bro it has is dropped for a `CharacterPreset` that doesn't resolve.

A mod refused before `PresetManager.Initialize()` never loads its DLLs; one refused later stays in `Mods` and is also in `IncompatibleMods`. Any other failure while scanning a DLL's presets isn't itself a refusal, but none of that DLL's presets register and its Harmony patches aren't applied, so every bro whose `CharacterPreset` is one of those presets is dropped as below, and a mod left with no bros is refused; a failure applying its Harmony patches is only reported. A bro whose `CharacterPreset` doesn't resolve is dropped with an Error of its own, and its mod's other bros load.

Refusal tokens, in `mod.RefusalTokens`:

| Token | Raised when |
|---|---|
| `missing:<Library>:<Target>` | A DLL references a BroMakerLib or RocketLib type that isn't there, or a member that isn't there or isn't accessible (`InternalsVisibleTo` counts) |
| `orphan:<Type::Method(sig)>` | A mod type overrides a method its BroMakerLib or RocketLib base doesn't have |

- "Load anyway" (`Settings.loadAnywayMods`), shown only with `Settings.developerMode` on, records the tokens a player accepted for a mod; from the next launch the mod loads with a Warning, and a new token asks again.
- Refusals with no token, which can't be overridden: a DLL the scanner can't read or check, a DLL failing `Assembly.LoadFile` / `GetTypes`, an incompatible-assembly failure while scanning presets, and a mod whose bros were all dropped for an unresolvable `CharacterPreset`.

## Refusal and registries

- `BroMakerStorage.MarkModIncompatible(mod, message, ex, report)` is the only refusal path: it records the mod incompatible, unregisters every bro it has (from the lookups, `BroSpawnManager` and `BroUnlockManager`), unpatches its Harmony instance and reports one Error, except for a mod whose bros were all dropped for an unresolvable `CharacterPreset`, each of which has its own.
- Lookups by hero type are empty until `PopulateTypesDictionary()` at the end of `PresetManager.Initialize()`.
- `PresetManager.parameters`, `heroesPreset` and `customObjectsPreset` return the live registries, so a write to one changes what BroMaker resolves; the special, melee and passive registries are reached only through its `Get*Preset` / `GetAll*Presets` methods. A missing ability or custom-object preset returns null.
