# Bro-Maker

BroMakerLib is the Broforce framework for custom playable bros: sprites, variants, abilities, intro cutscenes and unlocks. It requires [RocketLib](https://github.com/Gorzontrok/RocketLib).

A BroMakerLib feature is added, kept or removed on whether it is useful for writing mods: judge it by what a mod could do with it, never by which mods exist today or what they use.

What a BroMakerLib change breaks is a different question: settle it by searching the repos that build on BroMakerLib ([BroforceMods](https://github.com/alexneargarder/BroforceMods), [CustomBros](https://github.com/Gorzontrok/CustomBros), [VojkanBroforceMods](https://github.com/VojkanSE87/BroforceMods), and the `Bro Template` and `JSON Bro Template` in [Broforce-Templates](https://github.com/alexneargarder/Broforce-Templates)), never by supposing a mod that isn't among them. Also run `make compat-check`, which names the published Thunderstore mods the build breaks, including breaks a source search misses: an added optional parameter, a changed return type or a field turned property can compile from source but leave a DLL built before it referencing a member that no longer exists.

- A programmed bro derives from `CustomHero` (gun) or `SwordHero` (melee) and is registered with `[HeroPreset]`.
- A bro JSON's `CharacterPreset` names a hero preset. Only the vanilla presets' `*M` wrappers and classes derived from them host abilities (the JSON's `special` / `melee` / `passives`); `CustomHero` and `SwordHero` ignore those blocks.
- Use Harmony's Traverse (RocketLib's `GetFieldValue<T>` / `SetFieldValue` / `CallMethod`, or `Traverse.Create(...)`), not `GetField` / `GetMethod` + `Invoke`. Reflection on an ability's owner, instead of an `IAbilityOwner` member, is right only for a member used once at an event (a single read, write or call), private fields of one bro-specific class (behind the ability's cast check), methods specific to one bro, and public fields on a class `IAbilityOwner` doesn't cover; a raw accessor instead of Traverse only for IL emit, bulk member loops, stored `MethodInfo` tables, `AccessTools.Method` with an explicit type array when an argument may be null (Traverse types a null as `object` and can miss the overload), a field a subclass hides with `new` (Traverse on the instance finds the subclass's), and a per-frame path, which resolves a delegate once at setup and keeps it instead of looking the member up each frame (`AccessTools.FieldRefAccess` / `StaticFieldRefAccess`, `AccessTools.MethodDelegate`).

## Logging

- Never call `UnityEngine.Debug.Log`: Unity Mod Manager (UMM) doesn't capture its output, which goes to Unity's player log.
- Log through `BMLogger` in BroMakerLib and in bros; inside a bro, prefer its `ICustomHero` extensions (`Log`, `LogDebug`, `LogWarning`, `LogError`) to static `BMLogger` calls, since they fill in its mod and name. Errors and warnings a player must see go through `BMLogger.Error`/`Warning` or a bro's `LogError`/`LogWarning` extensions, which feed the Issues panel and notifications.
- For file logging, add the feature to RocketLib's logger; never write ad-hoc file output.

## Logs

- **UMM log** (UMM's and every mod logger's output): `UMM/Core/Log.txt` in the r2modman profile the game ran with, under `%AppData%\r2modmanPlus-local\Broforce\profiles\<profile>\` on Windows and `~/.config/r2modmanPlus-local/Broforce/profiles/<profile>/` on Linux.
- **Unity player log** (the engine's log from process start, overwritten each launch): read it when the UMM log is missing, for engine errors and for native crashes. Windows `%USERPROFILE%\AppData\LocalLow\Free Lives\Broforce\output_log.txt`; native Linux `~/.config/unity3d/Free Lives/Broforce/Player.log`; Proton `<Steam library>/steamapps/compatdata/274190/pfx/drive_c/users/steamuser/AppData/LocalLow/Free Lives/Broforce/output_log.txt`.

## In-game testing

- Before reporting a game run, account for every crash, error, exception and Issues panel entry its log holds: trace it to its cause in source, or show it in a vanilla or default-branch run of the same steps.
- Never crash the game on purpose, or drive it with a load normal play doesn't produce.
- A change a player meets in game is done once the scenario scripts [unity-inspector-mcp's `scripts/README.md`](https://github.com/alexneargarder/unity-inspector-mcp/blob/master/scripts/README.md) names for it pass on the final build, with every new UMM log line fixed or explained.

## Code style

These rules cover every C# file in the repo. No comment, XML docs included, carries design rationale, quality claims, anything obvious from the API shape, ordering or coupling notes, restatements of the code, history or status (`// added for bug #123`, `// was previously X`), TODO / HACK / FIXME markers, commented-out code, or references to files that don't ship with the code.

### Inline comments

- Add a `//` comment only for a genuinely complex algorithm, an engine or runtime gotcha the code can't show, or a non-obvious deliberate deviation from vanilla: at most two lines, at the point of use, never wrapped to a column width.
- Code reimplementing vanilla behavior gets no comments: no narration of what vanilla does, sequence or branch descriptions, default-value explanations, or state-machine or phase labels.
- No section dividers over declarations (`// --- X ---`). Branch labels inside a long method body only where they materially speed up reading. Keep existing label comments (`// Set main sprite`, `// Set gun sprite`).

### XML doc comments

- Only on public, protected or internal members, where a bullet below calls for one; never on files or private members.
- Tags: no `<see cref>` or `<c>` (name classes and members in backticks); `<remarks>` only for what the `<summary>` can't absorb in one or two sentences; `<example>` only when the code teaches what the signature can't (attribute composition, unusual constructor shapes, non-obvious generic use); `<param>` / `<returns>` only when name and type leave the role unclear.
- Concrete ability class: one sentence, source bro and ability type (`Nebro's airdash.`). Where a common inherited field (`animationRow`, `frameRate`, `triggerFrame` and the like) has a narrower effect on the class, on a bro of its source bro's class, or beside another ability, the summary also names it and what it drives there; a field the class declares documents its effect itself.
- Shared ability base class: one sentence on the shared behavior plus a roster of bare class names: `Used by:` for the bros a concrete base serves as is, then `Extended by:` for the classes extending it; an abstract base has only `Extended by:`. A concrete base's sentence also names its fields as a concrete ability class does.
- Ability-system base virtuals, abstract methods and properties: the implementer's contract (what to return, when it's called, defaults), never "override in subclasses".
- No doc on public fields mirroring a vanilla field, methods and hook overrides reimplementing vanilla, private fields, `[JsonIgnore]` state, or the generated dispatch overrides in the `*M` wrappers. A hook override doing something the source bro doesn't gets one.
- New public tunables extending vanilla: a short doc on the extension, even when the name is self-explanatory. Layout or index fields controlled by one `enable*` flag may rely on the flag's doc.
- Attribute classes: what it declares and how the system responds, from the caller's side, with an `<example>` when composition on a realistic class is the point.
- Other public API (factory methods, interfaces such as `IAbilityOwner`, info-class properties, helpers): one sentence on what the caller gets or what the field represents, no implementation detail, keeping caller contracts: mutation policy on returned collections, call order, thread safety (``Do not mutate the returned list directly; use the bro's `AddPassive`/`RemovePassive`/`ClearPassives` helpers.``).
- Enums: a short `<summary>` on the type; per-value docs only where the name leaves the behavior unclear (`IssueSeverity { Error, Warning }` gets none).
- No "Defaults to X": the initializer shows it.
- Never name framework types a caller doesn't use directly (`AbilityFactory`, `PresetManager`) in user-facing attribute or class docs; describe the behavior from the caller's side.
- Of two sibling APIs, one carries the full summary and the other says "variant of X", optionally with an `<example>` of a materially different usage shape.
- No parenthetical examples (`(e.g. ...)`, `(..., etc.)`, `(such as ...)`), except in error messages shown to mod authors and in a base-virtual `<summary>` where naming a stable subclass override clarifies the contract, preferring a pointer to a stable subclass list at the top of the file.
- Tone: ability public fields, tunables, base virtuals and framework internals (factory, generator, wrappers, private helpers) are third person and terse ("Damage dealt per tick during a horizontal airdash."); the older bro-author API (`CustomHero`, `CustomProjectile`, `CustomGrenade`, `CustomSachelPack`) is second person, tutorial-style ("Set this to control how far your bro dashes.").

### Harmony patches

- A `*Patches` file holds only Harmony patch classes, and a patch class holds only the methods Harmony calls: `Prefix`, `Postfix`, `Transpiler`, `Finalizer`, `Prepare`, `TargetMethod` / `TargetMethods`, `Cleanup`, a method passed to `harmony.Patch`, and the method making those `harmony.Patch` calls.
- Every other type, field and method a patch uses (state, registries, helpers, code a transpiler's IL calls) goes in the existing class that owns the feature it serves, in that class's file.

### Cleanup

- Run `make cleanup-changed` (ReSharper cleanup of changed C# lines) before every commit, never during feature work; `BASE=main` adds the branch's committed changes.

## Commits

- A commit is one subject line with no body: an imperative verb first, naming at most its two largest changes, with no trailing period and no version number except in a `Release vX.Y.Z` commit.

## Changelogs

- `Release/Changelog.md`. Headers are `## vX.Y.Z`, newest first; `(unreleased)` on the topmost marks a version not yet packaged, and adding one bumps `Info.VERSION` to the same value in the same change. Keep that header at the version being worked on: `bt package` packages the highest of it, `manifest.json`'s `version_number` and `Info.json`'s `Version`, then writes that version into both, so `BroMakerLib/_ModContent/Info.json`'s `Version` may lag `Info.VERSION` until release packaging.
- Entries are `-` bullets, each a change a player would notice in one short, non-technical clause of at most 20 words, never a list of symptoms, names or implementation detail. A branch adds at most one bug-fix line: one clause saying what its fixes change for a player.
- Write a branch's lines once its changes are complete, covering the whole branch.
- Versioning: `v1.0.X` for minor features, non-breaking changes and small fixes; `v1.X.0` for breaking changes and significant features; `vX.0.0` for major rewrites or major new features.

## Docs

- Repo docs (`AGENTS.md` files, READMEs, changelogs) state the current contract only: no other project's defects, transitional caveats or fixes in progress.
- A change to BroMakerLib's public API or behavior updates the [Bro-Maker wiki](https://github.com/Gorzontrok/Bro-Maker/wiki) page documenting it, in the same change.
- Working bros to learn from: Furibrosa, Mission Impossibro, Captain Ameribro and the other bros in [BroforceMods](https://github.com/alexneargarder/BroforceMods).

## Releases

- Players install mods from Thunderstore through r2modman, which pulls in Unity Mod Manager as a dependency of every mod package: never point install instructions at a Nexus Mods UMM page or a manual UMM install.
- `bt package` builds the Thunderstore zip.
- Mod READMEs and Thunderstore pages link the end-user setup guide: https://steamcommunity.com/sharedfiles/filedetails/?id=2434812447

## Build

Install BroMakerLib only through its build (`make build`), never by copying a DLL by hand.

After changing RocketLib, rebuild it with `make -C ../RocketLib build` from this repo's root, with the clones side by side: BroMakerLib compiles against its installed build.

`Scripts/BroforceModBuild.targets`, `Scripts/Makefile.common`, `Scripts/BroforceMods.DotSettings`, `.editorconfig` and `LocalBroforcePath.example.props` are Broforce-Templates' build files, edited there and synced here, never edited in this repo; their facts are in [Broforce-Templates' Scripts/AGENTS.md](https://github.com/alexneargarder/Broforce-Templates/blob/master/Scripts/AGENTS.md).

## Index

- `BroMakerLib/AGENTS.md`: startup order, resource loader reporting and readable textures.
- `BroMakerLib/Abilities/AGENTS.md`: naming ability methods, adding a hook, the ability factory, the melee jump and restore contract, the special state patches.
- `BroMakerLib/Attributes/AGENTS.md`: preset attributes, conflict declarations, `[Parameter]`.
- `BroMakerLib/CustomObjects/AGENTS.md`: promoting members to `IAbilityOwner`.
- `BroMakerLib/CustomObjects/Bros/AGENTS.md`: `CustomHero` lifecycle, update and custom-melee contracts, variants, settings, ability hosting.
- `BroMakerLib/CustomObjects/Projectiles/AGENTS.md`: projectile prefabs, sprite auto-loading, pocketed specials, piercing and bosses.
- `BroMakerLib/Cutscenes/AGENTS.md`: custom intro cutscenes and their JSON.
- `BroMakerLib/Extensions/AGENTS.md`: hero logging extensions, `SetupCustomHero`.
- `BroMakerLib/Infos/AGENTS.md`: bro JSON, field dictionaries, parameters.
- `BroMakerLib/Loaders/AGENTS.md`: spawning, prefab caching, vanilla projectile and grenade names.
- `BroMakerLib/Loggers/AGENTS.md`: `BMLogger` severities, scopes and mod/bro attribution, reporting one failure once, incompatible-mod errors, the issue collector.
- `BroMakerLib/Storages/AGENTS.md`: mod format, compatibility and refusal, "Load anyway", registries.
- `BroMakerLib/Unlocks/AGENTS.md`: unlock methods, rescue targets, persisted progress.
- `BroMakerLib/Vanilla/Bros/AGENTS.md`: the `RambroM` template and generated wrappers, dispatch and passive priority.
- `Scripts/AGENTS.md`: the wrapper generator and `vanilla-bros.json`.
