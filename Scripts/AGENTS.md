# Scripts

`python3 Scripts/generate_vanilla_bros.py` writes the vanilla bro wrappers from the `RambroM` template and `vanilla-bros.json`. After changing either, regenerate, then build.

## vanilla-bros.json

- `bros` stays sorted case-insensitively by `className` (`<BaseBro>M`). `heroType` can differ from `baseClass`; a bro with no `HeroType` of its own borrows another's prefab (`BroshipTrooper` takes `Rambro`'s) and sets the fields it needs in `nullFixes`.
- `nullFixes` emit into `PrefabSetup()`, run on the cached prefab after it copies the vanilla component's serialized fields.
- `awakeFixes` emit into `Awake()` after `StandardBeforeAwake()` and before `base.Awake()`, to restore the vanilla value of a flag the default `BetterAnimation` parameter forces on; they run only when the bro's JSON leaves `BetterAnimation` out.
- `postSpriteFixes` emit at the end of `Awake()`'s `try`, after `StandardAfterAwake()`: the only point in `Awake()` where the custom sheet is on the renderer. They emit in config order, so a fix that depends on another is listed after it.
- `cacheResyncs` emit into `Start()`, each entry by its required `stage`: `beforeStart` after `StandardBeforeStart()` and before `base.Start()`, `afterStart` after `StandardAfterStart()`. Use them to re-copy a value the bro's own `Awake` or `Start` cached before a `beforeStart` / `afterStart` override landed.
- `meleeStartGuards` emit into `StartMelee()`, before the melee ability starts a melee-button melee it takes.
- `members` emit into the class body, for fields and overrides the other lists need.
- `afterCheckInput` emit into `CheckInput()`, after `base.CheckInput()` and before the abilities' `HandleAfterCheckInput`.

## Fix types

`awakeFixes` take only `prefabCopy`; `postSpriteFixes` only `setValue` and `replaceIfVanilla`; every other list only `raw`. A wrong type in any list, or a `cacheResyncs` entry without a valid `stage`, stops the generator with an error.

- `prefabCopy` (`fields`): copies each field from the vanilla prefab, except one the bro's `beforeAwake` sets.
- `setValue` (`field`, `value`) assigns unguarded; use it only to keep a field coupled to another the bro's own `Awake` assigns.
- `raw` (`code`): one line per string, emitted indented, inside the template's namespace with only its usings; write any other type the way `make cleanup` leaves it (`Loggers.BMLogger`, `System.Linq.Enumerable`). No prefab variable is in scope.

Write every `raw` line, every `setValue` `field` and `value`, and every `replaceIfVanilla` `value` as `make cleanup` leaves it; `git diff --stat -- BroMakerLib/Vanilla/Bros/` must stay empty after `make cleanup`.

## Material caches

A vanilla field the bro's own `Awake` caches off the renderer holds the vanilla sheet, because the custom sheet is installed later, and vanilla code writing it back repaints the bro as the vanilla one. `replaceIfVanilla` in `postSpriteFixes` sets each of its `fields` to `value` (`gunMaterial`, `CurrentFirstAvatar`, or `material`, the body renderer's live material) unless the author set it, tested by `guard`:

- `notOverridden`, for a field the bro's `Awake` overwrites from the renderer: skipped when `afterAwake` sets the field. Also for a serialized field re-read in `Awake` (`BroveHeart.defaultGunMaterial`).
- `prefabField`, for a field the bro's `Awake` leaves alone: replaced only while it still equals the prefab's value.

`requires` (`prefabField` only) takes only `avatar`, with `value` `CurrentFirstAvatar`, and also gates the replacement on this variant having a custom avatar; any other `requires`, or another `value` with it, stops the generator with an error.

Which form a field takes:

- Cached from the renderer in the bro's `Awake`: `notOverridden`. Fields cached in `Start`, and `TestVanDammeAnim.defaultMaterial` / `gunMaterial`, need nothing.
- A serialized `public Material` restoring the bro's normal look (`Brominator.humanBrominator`): `prefabField`.
- A HUD avatar material holding the bro's normal face (`Brominator.brominatorHumanAvatar`, `ChevBrolios.avatarTired`, and `BoondockBro.avatar1`, the lead brother's normal in-level face): `prefabField` plus `"requires": "avatar"`, value `CurrentFirstAvatar`.
- Leave alone: a short-lived alternate look swapped in and restored (an armless swing frame, an open mouth per shot), a mode-length look (`metalBrominator`, `stealthMaterial`), a differently framed sheet (`BroveHeart.disarmedGunMaterial`), a field vanilla assigns as the new default, a companion's skin, and any other HUD avatar material.

## Adding or removing a bro

- Add: check whether the game's `HeroType` enum has it (without one, borrow another as above), add its `bros` entry in order, regenerate, add the file to `BroMakerLib/BroMakerLib.csproj`'s `<Compile Include="Vanilla\Bros\..." />` list, build, and compare it in game with the vanilla bro.
- Remove: drop the entry, delete the wrapper and its `<Compile Include>` line, build, and search for other references (other wrappers' hook overrides, abilities' `SourceBroType`).
