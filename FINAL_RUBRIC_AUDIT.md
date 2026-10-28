# Museum Getaway — Final Rubric Audit

Audit date: 2026-10-28. Status is based on files and Unity/build evidence in this workspace; unknown external work is not assumed.

| Requirement | Status | Evidence / file | Remaining action |
|---|---|---|---|
| Reward: artifact collection and Loot | COMPLETE | `ArtifactCollectible.cs`, `MuseumGameManager.cs`, `GameHUD.cs`, GoldenIdol GLB; manually validated | None |
| Damage: moving laser hazards | COMPLETE | `MovingLaser.cs`, `LaserHazard.cs`, `PlayerHealth.cs`, SecurityLaserEmitter GLB; manually validated | None |
| End: win and both loss states | COMPLETE | `GetawayExit.cs`, `MuseumGameManager.cs`, `EndScreenUI.cs`; manually validated | None |
| Restart without restarting Unity | COMPLETE | End-screen restart flow; manually validated from win and loss | None |
| Responsive controls | COMPLETE | `PlayerMovement.cs`, `SimpleFollowCamera.cs`, Input Actions, Main Menu Controls panel | Final subjective build check is recommended, but no known defect remains |
| Three functional Tripo assets | COMPLETE | GoldenIdol = reward, SecurityLaserEmitter = damage, GetawayVan = end; imported GLBs and builder integration | None for the minimum three assets |
| Tripo generation/import documentation | PARTIAL | `ATTRIBUTION.md`, `PROMPT_LOG.md` | Paste exact original generation prompts, dates/source URLs, and applicable Tripo/free-plan terms |
| Additional ThiefCharacter | COMPLETE | `ThiefCharacter.glb`, extracted animations, Animator Controller, `PlayerAnimationDriver.cs` | Add exact original generation/animation prompts and terms to finish its documentation |
| Additional RomanPillar | MISSING | Builder fallback slot exists, but `RomanPillar.glb` does not | Optional for the minimum-three rubric; do not claim it as shipped unless imported |
| Gameplay audio | MISSING | `Assets/Audio/` contains no sound files; no AudioSource/AudioClip gameplay implementation found | Add pickup, damage/alarm, and win audio; test and document prompts/sources/terms |
| 30–60 second AI-made trailer with gameplay | MISSING | No video file, trailer project, capture, prompts, or link found | Capture gameplay, create/export trailer, document prompts, publish and link it |
| Unity source project | COMPLETE | `Assets/`, `Packages/`, `ProjectSettings/` | Publish without generated cache/build folders |
| Unity `.gitignore` | COMPLETE | `.gitignore` includes Library, Temp, Logs, obj, Build, Builds, UserSettings, IDE and OS files | Recheck staged files after Git initialization |
| README / controls / play / build instructions | COMPLETE | `README.md` | Replace public URL placeholders after hosting |
| Prompt log chronology and recovery history | COMPLETE | `PROMPT_LOG.md` | Replace genuine Tripo/audio/trailer placeholders only after those events occur |
| Final 200–300 word reflection | COMPLETE | `PROMPT_LOG.md`, 266 words | None |
| MCP handoff | COMPLETE | `MCP_HANDOFF.md` | Keep updated if audio/trailer/hosting work is completed |
| Attribution and AI-tool inventory | PARTIAL | `ATTRIBUTION.md` | Add exact Tripo terms, audio terms, trailer-tool terms, and public attribution URL |
| References/inspirations | COMPLETE | README References section states the recorded design inspiration and that no specific external reference asset is recorded | Add any omitted real references before publication if applicable |
| Build scene order | COMPLETE | `EditorBuildSettings.asset`: MainMenu index 0, MuseumGetaway index 1; no SampleScene in build | None |
| Packaged playable build | COMPLETE | `Builds/macOS/Museum Getaway.app`; Unity reported 179,567,251 bytes; standalone Main Menu/gameplay smoke test passed | Complete full-run standalone regression, then zip for hosting |
| Hosted build/download link | MISSING | README public build URL is `TBD` | Upload the ignored packaged build separately and paste its URL |
| Public GitHub repository | MISSING | No `.git` directory or public repository URL exists in this workspace | Initialize, review ignored files, commit, publish, and add URL |
| Trailer link/file in repository record | MISSING | README trailer link is `TBD` | Add after trailer export/publication |

## Submission blockers

Do not submit as fully rubric-complete until gameplay audio, the trailer, original Tripo prompt/terms records, the hosted build link, and the public GitHub repository are complete.
