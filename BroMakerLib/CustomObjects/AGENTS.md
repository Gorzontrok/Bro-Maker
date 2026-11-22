# CustomObjects

- When an ability needs an owner member `IAbilityOwner` lacks, add it to the interface, implement it in the `RambroM` template and regenerate the wrappers, instead of `owner.GetFieldValue<T>("name")`, except where the root `AGENTS.md`'s reflection rule allows reflection on the owner.
- Never mutate the `List<T>` that `IAbilityOwner.Passives` returns.
