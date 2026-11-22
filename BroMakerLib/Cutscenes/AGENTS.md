# Cutscenes

A custom "X Joins the Battle!" intro has no scene of its own: `CustomCutsceneController.LoadHeroCutscene` loads vanilla's Rambro intro and a Harmony prefix on `CutsceneIntroRoot.OnLoadComplete` repaints it. It does nothing while a pause has frozen the level (a cutscene pause, or any pause offline) unless the pause menu itself is on top.

- `CustomBroInfo.Cutscene` holds one cutscene per variant (or one shared; `BroMakerLib/Infos/AGENTS.md` has the full length rule); read it with `BroMakerUtilities.GetVariantValue(info.Cutscene, variant)`, never by index.
- `path` is `[JsonIgnore]`, set after deserialization; a `CustomIntroCutscene` built in code needs it set, or every relative sprite and sound lookup fails.
- A bro's unlock plays its spawned variant's cutscene on first spawn only when the first cutscene has `playCutsceneOnFirstSpawn`.
- `Owner` (the bro's `CustomBroInfo`, `[JsonIgnore]`) is set wherever `path` is; the intro patches scope their reports to it, so a cutscene without one reports with no mod or bro.
- An empty `spritePath` shows the Rambro silhouette; a sprite that fails to load falls back to Rambro's intro sprite; a failed bark, fanfare or background is logged and skipped. A non-empty `anim` borrows that vanilla cutscene's animation clip only.
- The `backgroundPath` image is mapped whole onto the flag's banner, so any shape is stretched to the banner's 16:9, and only a band through its middle shows (nearly its full width and a little over half its height): put what matters in the image's middle.
- `spriteOffset` is divided by 256. `spriteAnimRateFramesWidth` is x frame rate, y frame count, z frame spacing width, written as `{ "FrameRate", "TotalFrames", "FrameWidth" }` or a raw `Vector3`.
- `playDefaultFanfare: false` suppresses the default fanfare by pausing any playing `AudioSource` named `camShake`.
- The `cutscene` JSON takes an array or a single object. A single object with a `variantSprites` array expands into one cutscene per sprite sharing every other field, the base `spritePath` first unless listed, duplicates dropped.
