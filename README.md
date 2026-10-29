# Museum Getaway

Museum Getaway is a third-person arcade heist game made in Unity 6.3 LTS with URP. Each run generates a connected museum of 5–15 rooms. Steal every artifact, dodge moving security lasers, and reach the getaway van before the alarm expires.

## Controls

- `W`, `A`, `S`, `D` — camera-relative movement
- Hold **right mouse** and move the mouse — orbit and pitch the camera
- `Shift` — toggle Shift Lock; press Shift again to unlock
- **CONTROLS** on the Main Menu — open the large in-game control guide
- **BACK** or `Escape` — close the control guide
- **RESTART** on a win/loss screen — begin another run without restarting Unity

There are no jump, attack, zoom, or inventory controls.

## Objective and rules

1. Select **NEW GAME**.
2. Explore the procedurally generated museum and collect every artifact.
3. Each pickup increases the green **LOOT** total. The first pickup starts the alarm.
4. Avoid moving red lasers. The thief has 3 health and a short damage cooldown.
5. After the final artifact, the getaway van appears and the exit activates in the farthest room.
6. Enter the active getaway exit to win.

You lose if health reaches `0` or the alarm reaches `0`. The alarm scales from 60 seconds for 5 rooms to 120 seconds for 15 rooms. Win and loss screens both provide an in-session Restart button.

## Project and build instructions

Required editor version: Unity `6000.3.27f1` (Unity 6.3 LTS). The project uses URP `17.3.0` and Input System `1.20.1`.

1. Clone/download the repository and open its root folder in Unity Hub.
2. Allow Unity to import packages and assets.
3. Open `Assets/Scenes/MainMenu.unity` and press Play for an Editor test.
4. If scene regeneration is required, use `Tools > Build Museum Getaway` and `Tools > Build Museum Getaway Main Menu`.
5. For a standalone player, choose `Tools > Build Museum Getaway Player`. The active Unity build target is used and output is written under the ignored `Builds/` folder.

Build Settings contain only:

1. `Assets/Scenes/MainMenu.unity` — scene 0
2. `Assets/Scenes/MuseumGetaway.unity` — scene 1

Local packaged build: `Builds/macOS/Museum Getaway.app` (successfully built and smoke-tested on macOS; intentionally ignored by Git).

Public build/download URL: **TBD — upload the tested build to a GitHub Release, itch.io, or approved course host and paste the public URL here.**

Generated `Build/` and `Builds/` folders are intentionally excluded from source control.

## Toolchain and AI inventory

- Unity `6000.3.27f1`, Universal Render Pipeline `17.3.0`, Input System `1.20.1`
- C# and Unity Editor scene-builder tooling
- Tripo image-to-3D, Auto Rig, and animation generation
- OpenAI Codex for planning, C# implementation, debugging assistance, validation support, and documentation
- Audio source/generation tool: **TBD — four supplied WAV files were imported from the student's Downloads folder, but their original creation tool and terms were not recorded**
- Trailer generation/editing tool: **TBD — trailer has not yet been created**

## Tripo assets

The three rubric-critical models are present and used as collider-free visuals:

- **GoldenIdol** — visible reward model on collectible artifacts
- **LaserEmitter** — visible damage-category prop at both ends of moving laser beams
- **GetawayVan** — visible end-category prop revealed when the exit activates

The animated **ThiefCharacter** is also present. The optional **RomanPillar** Tripo file is not present, so the builder uses its documented primitive fallback. Unity primitive colliders/triggers remain authoritative for collection, laser damage, player control, and the win condition. See [ATTRIBUTION.md](ATTRIBUTION.md) for the verified pipeline and outstanding prompt/terms fields.

## Audio status

Gameplay audio is integrated: `pickup.wav` plays after a successful artifact collection, `alarm.wav` plays only when damage passes the health cooldown, `win.wav` plays once on victory, and `music.wav` loops during gameplay. The builder creates separate 2D Music and SFX AudioSources. A final human listening pass is still required to confirm volume balance and the files' original creation tool/terms; see [ATTRIBUTION.md](ATTRIBUTION.md).

## Trailer status

The required 30–60 second AI-made trailer is currently **missing**. It must contain an opening hook, actual Museum Getaway gameplay footage, generated material mixed with gameplay, and a closing title/call to action.

Trailer link/file: **TBD**

## References and project records

- Design inspiration: arcade heist and museum-escape games; no specific external reference asset has been recorded.
- Development history and final reflection: [PROMPT_LOG.md](PROMPT_LOG.md)
- Current project handoff and submission checklist: [MCP_HANDOFF.md](MCP_HANDOFF.md)
- Asset, AI-tool, and terms register: [ATTRIBUTION.md](ATTRIBUTION.md)
- Evidence-based rubric status: [FINAL_RUBRIC_AUDIT.md](FINAL_RUBRIC_AUDIT.md)

## Credits

- Game direction, manual Unity testing, visual judgment, and submission decisions: project author/student
- AI-assisted programming, scene-builder iteration, debugging support, and documentation: OpenAI Codex under student direction
- Bespoke generated 3D models, rig, and animations: Tripo; exact prompts and applicable terms remain to be supplied where marked `TBD`
