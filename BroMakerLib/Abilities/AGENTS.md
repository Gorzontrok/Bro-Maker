# Abilities

## Naming a new method

- **Lifecycle** (`Initialize`, `Update`, `Cleanup`): the ability's own callbacks, run for every attached ability. Plain name.
- **Game-event hooks** (`Handle*` / `HandleAfter*` on `AbilityBase`): dispatched to every attached ability. A `bool` `Handle*` is a pre-hook whose `false` vetoes; a `void` hook (every `HandleAfter*`, and `HandleLateUpdate`) runs after the base. Exception: `HandleSetupThrownMookVelocity` runs after the base, and its `false` only stops later abilities adjusting the result.
- **Single-slot methods** (`PressSpecial`, `StartMelee`, `CancelMelee` and the like on `SpecialAbility` / `MeleeAbility`): dispatched only to the special- or melee-slot occupant, never to passives. Plain name.

## Adding a hook

1. Declare it `public virtual` in `AbilityBase`; `AbilityBase` and the `RambroM` template list hooks in the same order.
2. Add the dispatcher at the matching spot in `RambroM`, shaped like the closest existing one.
3. Regenerate: `python3 Scripts/generate_vanilla_bros.py`.
4. Override the hook in the ability that needs it.
5. Build.

## AbilityFactory

- Order: construct, apply the JSON overrides, then `Initialize(owner)`.
- Overrides resolve by field name through Traverse, so `[JsonIgnore]` doesn't stop JSON setting a field. A nested object on a struct field stays whole; any other nested object flattens to `parent.child` paths.
- Passives, in order: dropped if `IsRedundant` after `Initialize`; dropped if a second of the same concrete type; dropped if it conflicts either way with a preset the owner has natively (that preset's `IsOwnerRedundant(owner)` on a fresh, uninitialized instance); then checked both ways against those already accepted, the earlier-listed winning. `allowConflict: true` bypasses both conflict checks. A passive these checks drop gets `Cleanup`; one whose construction, overrides or `Initialize` throws is left out with an Error and no `Cleanup`.

## Melee abilities

- Holding jump re-applies `jumpForce` while `jumpTime > 0`, and `jumpTime` freezes during a melee. A melee ending with `jumpTime` positive and jump held gets a fresh jump impulse; one that never reaches the cancel leaves `doingMelee` set and kills the hold-boost for good.
- `MeleeAbility.StartMelee` zeroes `hero.JumpTime` only when `customStart` is false, as the game's `BroBase.StartKnifeMelee` does; the game's custom-melee start leaves it, so a `customStart` melee zeroes it only where its source bro does.
- Every terminal frame of the melee animation reaches `hero.CancelMelee()`, except where the source bro ends its melee without `CancelMelee` (vanilla `TheBrofessional`'s teleport-out with no target clears `doingMelee` and `invulnerable` and sets `wasHighFive`): a preset copying such an ending makes the source's writes itself, as neither the base `CancelMelee`'s resets nor the restore hook below runs.
- `MeleeAbility.CancelMelee` is the restore hook. It runs after the base `CancelMelee`, which already resets `maxFallSpeed` to `hero.OriginalMaxFallSpeed`; restore there every `speed`, `canWallClimb` or gravity write the melee makes.

## Special state

- `SpecialStatePatches` routes the `usingSpecial` reads and writes of each `TestVanDammeAnim` and `BroBase` method in its `targets` table through `SpecialAbility`, so that code sees an attached special's state as well as the bro's own; an unlisted method sees and sets only the bro's own. A newly found such method that reads or writes `usingSpecial` needs its own `targets` entry, with its counts of reads, stores and copies into `wasUsingSpecial`.
