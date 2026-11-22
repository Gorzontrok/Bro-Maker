# Attributes

- Never move `CustomObjectPresetAttribute` or `HeroPresetAttribute` out of the `BroMakerLib` namespace: compiled custom bros reference them there.
- Preset names are unique per registry; on a duplicate the first registration wins.
- `[ConflictsWithPreset]`, which takes one or more preset names and may be repeated with different `Scope`s, is enforced only when a bro JSON's `passives` list is built, never by runtime `AddPassive`. `Scope`: `All` (default) checks among listed passives and against the owner's native presets both ways; `ListedOnly` only among listed passives; `NativeOnly` only drops the declaring passive when the owner natively has a named preset.
- `[Parameter]` methods must be static (an instance method is silently skipped), are scanned only in the first type named exactly `Parameters` per assembly, take `(target, jsonValue)`, and are keyed by method name. A call is rejected when either runtime type isn't the declared type or a subclass, so an interface-typed parameter never matches.
