# Loaders

- Spawn a custom bro only through `LoadHero.WithCustomBroInfo(playerNum, info, type)`: it sets the static `LoadHero.currentInfo` that the bro's `Awake` reads. Never instantiate a custom hero yourself.
- The first spawn of a bro name clones the vanilla prefab for its `HeroType` into a cached `"BM_" + info.name` prefab and runs `PrefabSetup()` on it. The cache is keyed by `info.name`, so a renamed bro gets a new prefab and the old one stays loaded for the session.
- `LoadBroforceObjects.GetProjectileFromName` / `GetGrenadeFromName` take a vanilla name, or a resource path when the name contains `:`. An unknown name, a null prefab or an exception falls back to the `"Default"` entry with a Warning.
