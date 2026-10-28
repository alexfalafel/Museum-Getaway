# Museum Getaway — Final Submission Handoff

Last updated: 2026-10-28

## Current working state

- Unity `6000.3.27f1`; URP `17.3.0`; Input System `1.20.1`.
- The project is playable from `Assets/Scenes/MainMenu.unity`.
- Main Menu includes **NEW GAME**, **CONTROLS**, and **QUIT**. The Controls panel emphasizes right-mouse camera orbit and Shift Lock.
- Runtime generation creates 5–15 connected rooms, one collectible artifact per room, moving lasers, a farthest-room getaway, scaled timer, and green Loot feedback.
- The student has manually validated WASD movement, artifact collection, Loot, alarm timer, laser damage, zero-health loss, timer loss, getaway activation, win, both end screens, and Restart.
- Camera is the final third-person manual orbit version: right mouse orbits in normal mode; Shift toggles Shift Lock; collision uses the existing camera-obstacle sphere cast.
- Required Tripo visuals are present: GoldenIdol (reward), SecurityLaserEmitter (damage), and GetawayVan (end). Animated ThiefCharacter is also present.
- RomanPillar Tripo model is not present; primitive pillar fallback is active.
- Gameplay collision remains primitive-based. Imported meshes are visual children only.

## Submission/build state

- Build Settings contain only `MainMenu` at scene index 0 and `MuseumGetaway` at scene index 1.
- `Tools > Build Museum Getaway Player` builds those two scenes for the active platform into `Builds/`.
- `Builds/`, `Build/`, `Library/`, `Temp/`, `Logs/`, `obj/`, and `UserSettings/` are ignored by `.gitignore`.
- The required root files and folders exist: `Assets/`, `Packages/`, `ProjectSettings/`, `.gitignore`, `README.md`, `PROMPT_LOG.md`, `MCP_HANDOFF.md`, and `ATTRIBUTION.md`.
- A macOS player was successfully produced at `Builds/macOS/Museum Getaway.app` (approximately 172 MB on disk). Standalone smoke testing confirmed the Main Menu opens and NEW GAME loads an 8-room gameplay run with HUD and Tripo visuals.
- This folder is not currently a Git repository (`.git` is absent), so no public GitHub URL or release exists yet.

## Completed rubric evidence

- Reward: `ArtifactCollectible`, tiered Loot, green HUD counter, GoldenIdol visuals.
- Damage: `MovingLaser`, `LaserHazard`, 3-health system, cooldown, SecurityLaserEmitter visuals.
- End: timer/health losses, all-artifacts getaway activation, GetawayVan visual, win/loss panels.
- Restart: both end screens reload gameplay without restarting Unity.
- Responsive controls: camera-relative WASD, right-mouse manual orbit, Shift Lock, menu control guide.
- Documentation: chronological prompt history, README, attribution/tool register, handoff, and 266-word final reflection.
- Source-control hygiene: standard Unity ignore rules and generated-build exclusion.

## Missing or partial submission requirements

1. **Gameplay audio — MISSING.** `Assets/Audio/` contains no sound files and no gameplay audio wiring was found. Required: pickup, damage/alarm, and win audio. Record actual prompts, sources, files, terms, import steps, and listening test.
2. **Trailer — MISSING.** No 30–60 second AI-made trailer, gameplay capture, trailer prompt history, exported file, or public link exists.
3. **Hosted build/download link — MISSING.** Upload the tested macOS build separately and add its URL to README.
4. **Public GitHub repository — MISSING.** Initialize/publish the repository, verify ignored folders are excluded, and add the public URL.
5. **Tripo generation records — PARTIAL.** Routes, formats, import paths, and uses are recorded. Exact GoldenIdol, LaserEmitter, GetawayVan, and ThiefCharacter generation prompts, generation dates/source URLs, and applicable Tripo/free-plan terms are still `TBD`.
6. **RomanPillar — PARTIAL/OPTIONAL ADDITIONAL ASSET.** Builder support exists, but the expected GLB does not. It must not be claimed as a shipped Tripo asset.
7. **Attribution/repository URL — MISSING.** Add the public link after publication.

## Exact next submission steps

1. Supply the original Tripo prompts and exact applicable terms/source URLs. Replace only the corresponding `TBD` fields in `PROMPT_LOG.md` and `ATTRIBUTION.md`.
2. Generate or source the required pickup, damage/alarm, and win sounds. Preserve the exact prompts, tool, files, URLs, and terms; import and wire them; then perform a listening/gameplay test.
3. Capture actual gameplay and create the required 30–60 second AI-made trailer with an opening hook and closing title/call to action. Document the actual trailer prompts and export details.
4. Perform the remaining full standalone regression: Controls panel, one complete win, both loss paths, and Restart. The build already passed Main Menu → gameplay startup.
5. Zip/upload `Builds/macOS/Museum Getaway.app` to a GitHub Release, itch.io, or the approved course host. Do not add the generated build folder to source control.
6. Initialize/publish the GitHub repository. Before the first push, confirm `Library/`, `Temp/`, `Logs/`, `obj/`, `Build/`, `Builds/`, and `UserSettings/` are absent from staged files.
7. Add the public repository, build/download, trailer, and attribution links to README/ATTRIBUTION.
8. Perform one final link check and submit the single public repository URL required by the assignment.

## Files created or modified in the final audit

- `Assets/Editor/BuildMuseumGetawayPlayer.cs` and `.meta` — active-platform packaged-player builder.
- `Assets/Editor/BuildMuseumGetaway.cs` — build-setting generation now retains only the two submission scenes.
- `Assets/Editor/BuildMuseumGetawayMainMenu.cs` — same exact two-scene build-setting rule.
- `ProjectSettings/EditorBuildSettings.asset` — MainMenu index 0; MuseumGetaway index 1; SampleScene removed from the build.
- `Assets/Scripts/Gameplay/MuseumProceduralGenerator.cs` — removed two redundant Editor-only light bake assignments that blocked standalone compilation; realtime light behavior is unchanged.
- `README.md` — final game, controls, rules, build, toolchain, Tripo, credits, references, and honest missing-item status.
- `PROMPT_LOG.md` — final audit, audio/trailer placeholders, error recovery, and 266-word reflection.
- `ATTRIBUTION.md` — verified AI/tool/asset register with explicit unknown terms/prompts.
- `MCP_HANDOFF.md` — this final-state handoff.
- `FINAL_RUBRIC_AUDIT.md` — persistent COMPLETE/PARTIAL/MISSING evidence table.

## Preservation rules

- Do not change working gameplay, camera, procedural museum, Tripo source files, animation, or UI for submission polish.
- Do not invent prompts, licenses, source links, generation dates, test results, build URLs, or trailer URLs.
- Do not add MeshColliders to imported Tripo visuals.
- Do not commit generated Unity caches or packaged player folders.
