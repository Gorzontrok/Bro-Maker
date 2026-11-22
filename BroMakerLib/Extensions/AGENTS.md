# Extensions

- `hero.Log` / `hero.LogDebug` write only to the UMM log, prefixed with the bro name (`ModName/BroName` in a multi-bro mod); `hero.LogWarning` / `hero.LogError` go through `BMLogger` with the bro's mod and name filled in; pass `ex` when one is in scope.
- `SetupCustomHero(false)` copies a fixed set of fields off the vanilla hero component and destroys it, then reads the parameters. A `*M` wrapper instead calls `SetupCustomHero(true)`, which destroys the component and reads the parameters.
