# Vanilla bro wrappers

## RambroM and the generated wrappers

- `RambroM` is the template and the only wrapper edited by hand. Every other `*M` wrapper is generated from it by `python3 Scripts/generate_vanilla_bros.py`, keeps the generator's header line, and is committed with the template or config change that drove it.
- The generator rewrites `RambroM`'s class declaration, its `[HeroPreset]` line and the code between the `// GENERATOR:*` marker pairs by regex with no match check. Never remove a marker or reformat those two lines: a broken declaration match writes every wrapper declaring `RambroM`, a broken `[HeroPreset]` match registers every wrapper as the Rambro preset, and a broken marker pair silently drops every bro's code for that block.
- Write the template as `make cleanup` leaves it (braced bodies, explicit types instead of `var`, `this.` only before an extension-method call or a member a same-named local shadows), with only the usings its own code needs, so the generated wrappers come out unchanged by `make cleanup`.
- `PrefabSetup` copies every field Unity serializes (public or `[SerializeField]`, references included) off the vanilla component on the cached prefab, so a wrapper needs no per-field copy unless that prefab is another bro's: then only the fields of the classes both share are copied, and `vanilla-bros.json`'s `nullFixes` set what it needs beyond them.
- A per-bro fix to the wrapper's own dispatch, input handling or cached values goes in a `vanilla-bros.json` list, never in hand-written wrapper code.

## Dispatch

- Copy the shape of the closest existing dispatcher in `RambroM`.
- A dispatcher for a method that is `virtual` or `override` anywhere in its vanilla base chain is an `override` with the vanilla method's access, never `virtual`: a new `virtual` hides the vanilla method, so vanilla callers never reach the hooks.
- A dispatcher for a `bool` hook honors its result, by returning early or folding it into the method's result.
- Every loop runs the special-slot ability, then the melee-slot ability, then the passives in JSON order, so for a vetoing hook the earlier ability wins. `HandleUpdate` is the exception: every ability's runs, and any `false` skips the base.
- Runtime `AddPassive` (which honors `IsRedundant`), `RemovePassive<T>` and `ClearPassives` skip the factory's type dedup and conflict checks.
