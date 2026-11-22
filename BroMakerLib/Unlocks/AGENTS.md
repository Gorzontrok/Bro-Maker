# Unlocks

A bro's `unlockConfig` gates it:

```json
"unlockConfig": {
  "method": "RescueOrLevel",
  "unlockLevelPath": "Captain Ameribro.bfc",
  "unlockLevelName": "Behind Enemy Lines",
  "rescueCountRequired": 5
}
```

- `method`: `AlwaysUnlocked` (default); `RescueCount`: unlocks at the bro's staggered rescue target; `UnlockLevel`: only through its bundled `.bfc` campaign (see below); `RescueOrLevel`: either.
- `unlockLevelPath` is relative to the bro's folder (`.bfc` appended when missing); `unlockLevelName` is display text only.
- A rescue target is `max(TotalRescues, highest outstanding target) + rescueCountRequired`, so ten bros asking for 5 unlock at 5, 10, 15, … A `RescueOrLevel` bro unlocked by its level pulls every later target forward by the rescues it skipped.
- Progress persists in `BroMaker_UnlockProgress.json` in BroMaker's config folder. A bro's saved state is rebuilt when its `method` changes, or its `unlockLevelPath` for a level method, never when only `rescueCountRequired` changes.
- A level unlock counts only when `BroUnlockManager.LoadUnlockLevel` launched the level in the same session and the campaign then runs, outside the level editor, a "BroMaker - Play Custom Bro Cutscene" trigger (`CheckLevelUnlocks`'s only caller) naming any installed, still-locked bro; it unlocks the bro whose level was launched, and finishing the campaign without such a trigger unlocks nothing.
- A bro with no saved state counts as unlocked; an enabled but locked bro is force-disabled.
- A rescue on which a vanilla and a custom unlock would both land isn't counted toward custom unlocks. The in-game rescue counter shows whichever unlock comes first.
- `BroUnlockProgressData.MigrateData` is a no-op; bumping `Version` does nothing until a migration is written.
