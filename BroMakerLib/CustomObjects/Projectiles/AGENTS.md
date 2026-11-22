# Custom projectiles

## Prefabs

- `CreatePrefab<T>()` returns the prefab cached for exactly `T`. On a miss it builds the GameObject (the component's `Awake` fires then, before `PrefabSetup()`), runs `PrefabSetup()` once, then disables and deactivates it under `DontDestroyOnLoad`. Each spawn clones the prefab and inherits its serialized field values.
- The base `Awake` resolves the sprite folder from `Assembly.GetCallingAssembly()`, so every subclass overrides `Awake` and calls `base.Awake()` from it; otherwise the bro's DLL folder isn't found.
- A bro creates each prefab in its own `Awake` or `Start`, keeps it in a field, and spawns it from the method that fires it (`FireWeapon`, `UseSpecial`).
- Spawn with `SpawnProjectileLocally` (`CustomProjectile`, `CustomSachelPack`) or `SpawnGrenadeLocally` (`CustomGrenade`).

## Sprite auto-loading

When `SpriteAutoLoad` is true and no sprite is set, `Awake` loads `<bro DLL folder>/<SpriteFolder>/<SpriteFileName>` (default `projectiles/<ClassName>.png`); sounds resolve against `<bro DLL folder>/<SoundFolder>` (default `sounds`).

- `AssemblyPath`, `SpriteDirectoryPath` and `SoundPath` are set only by that auto-load, so they stay empty on a prefab it skips; a spawned clone, its sprite already set, skips it and carries the prefab's values.
- `CustomGrenade` keeps the sprite in `StoredSprite`, not `Sprite`, and has no `SpriteWidth` / `SpriteHeight`; it writes the image size into the inherited `spriteWidth` / `spriteHeight` only while they hold the default 2.

## Pocketed specials

`CustomPockettedSpecial` (a plain class, not a MonoBehaviour) items are stored per player and used last-in, first-out by the special button. `AddPockettedSpecial` does nothing unless the character is a `BroBase`. The per-player lists are indexed by `playerNum`, so any call for -1 (prefabs, companions) throws.

## Piercing and bosses

- `Projectile.HitUnits()` destroys the projectile on any hit, so a piercing projectile overrides it. The vanilla piercing shape (one target per frame from `Map.GetFirstUnit(..., hitUnits)`, recorded in `hitUnits`) doesn't record heavy units, relying on stopping at once; a projectile that keeps flying would damage a heavy or boss unit every frame, so its `IsHeavy()` / `BroMakerUtilities.IsBoss()` branch adds the unit to `hitUnits` or terminates.
- `Map.HitUnits(..., penetrates: true, ..., alreadyHit)` also works when `alreadyHit` belongs to the projectile instance and is cleared only on a deliberate event.
- `Map.HitUnits` and `Map.GetFirstUnit` reach the `Unit`-based bosses `BroMakerUtilities.IsBoss` counts, such as `DolphLundrenSoldier` and `SatanMiniboss`, but not `AlienSandWorm`, which isn't in `Map.units` and takes damage through `DamageRelay` colliders, nor a `Boss` subclass, which isn't a `Unit` and takes damage through its pieces' colliders, on `Ground` and the other layers projectiles hit. Each crush of a ground-pierce budget (`groundLayersCrushLeft`, an overridden `Bounce` / `HitWalls`) on a boss collider deals damage unless its component caps it per frame, as `BossPiece` does and `BossBlockPiece`, `BossBlockWeapon` and `DamageRelay` don't (a `DamageRelay` only when opted in, and only for the unit it forwards to); the layer doesn't mark a boss collider, so clamp the budget when the hit collider has one of those last three components.
- A hit flash or brotality gain is no proof that damage landed.
