# Museum Getaway Attribution and AI Tool Register

This file records only verified project evidence. `TBD` means the original generation/session record must be supplied by the student; no license, terms language, prompt, URL, or date has been inferred.

## Tools and generative AI

| Tool | Verified use | Terms / attribution status |
|---|---|---|
| Unity 6.3 LTS / URP / Input System | Engine, rendering, input, primitives, colliders, UI, animation playback, builds | Unity-provided engine/tooling; consult the applicable Unity license for redistribution |
| OpenAI Codex | Planning, C# and Editor tooling, debugging assistance, audit support, and documentation | AI-assisted output directed and evaluated by the project author; exact account terms not recorded here |
| Tripo | Image-to-3D models, character Auto Rig, and generated character animations | Exact applicable account/public free-plan terms and source URLs are **TBD** and must be copied from the original Tripo sessions/account terms |
| Audio source/generation tool | Four WAV files supplied from the student's `Downloads/College` folder and integrated into gameplay | Original creation tool, prompts, source URL, and license/terms are **TBD**; they were not inferable from the files |
| Trailer generation/editing tool | Not yet used for this project | **TBD after trailer production** |

## Tripo-generated assets

### Required rubric assets

| Asset | Verified route | Format / import pipeline | Unity location | Visible and functional use | Generation prompt | Terms / public free-plan note |
|---|---|---|---|---|---|---|
| GoldenIdol | Tripo image-to-3D | 2K-textured `.glb`; imported by Unity as a GameObject; instantiated as a collider-free child by the builder | `Assets/Art/Models/Tripo/GoldenIdol/GoldenIdol.glb` | `GoldenIdol_VISUAL` on collectible rewards; the Unity trigger and `ArtifactCollectible` award Loot | **TBD — paste exact original prompt** | **TBD — paste exact applicable Tripo wording/URL** |
| LaserEmitter | Tripo image-to-3D | 2K-textured `.glb`; actual filename is `SecurityLaserEmitter.glb`; imported and duplicated as collider-free children | `Assets/Art/Models/Tripo/LaserEmitter/SecurityLaserEmitter.glb` | `LaserEmitter_VISUAL_Left/Right` move with every red damage beam; primitive trigger and `LaserHazard` handle damage | **TBD — paste exact original prompt** | **TBD — paste exact applicable Tripo wording/URL** |
| GetawayVan | Tripo image-to-3D | 2K-textured `.glb`; imported as a collider-free visual child | `Assets/Art/Models/Tripo/GetawayVan/GetawayVan.glb` | `GetawayVan_VISUAL` appears at the activated getaway; primitive `GetawayExit` trigger handles winning | **TBD — paste exact original prompt** | **TBD — paste exact applicable Tripo wording/URL** |

### Additional Tripo assets

| Asset | Verified route | Format / import pipeline | Unity location | Final use | Generation prompt | Terms / public free-plan note |
|---|---|---|---|---|---|---|
| ThiefCharacter | Tripo character generation → Auto Rig → animation generation | `.glb`; imported model plus extracted `.anim` clips and Animator Controller; root motion disabled | `Assets/Art/Models/Tripo/ThiefCharacter/ThiefCharacter.glb` | Collider-free `ThiefCharacter_VISUAL` under the existing Player; Idle, Sneaking Walk, and Cartoon Victory visuals | **TBD — paste exact character and animation prompts** | **TBD — paste exact applicable Tripo wording/URL** |
| RomanPillar | Intended Tripo image-to-3D route, but no model file is present | Expected `.glb`; builder path/fallback exists, but no imported Tripo asset was found | Expected `Assets/Art/Models/Tripo/RomanPillar/RomanPillar.glb` | **Not currently a Tripo asset in the game.** Primitive decorative pillars are used instead | **TBD — asset not supplied** | **TBD — asset not supplied** |

The imported Tripo models are visual children only. Gameplay collision is intentionally handled by Unity primitive colliders, triggers, CharacterController, `ArtifactCollectible`, `LaserHazard`, and `GetawayExit`.

## Project-created assets

| Asset | Source / creator | Use and modifications | License / terms |
|---|---|---|---|
| Unity primitive museum geometry and fallbacks | Project author with AI-assisted builder code | Floors, walls, pedestals, roof, fallback pillars, laser triggers, exit trigger, and decorative structures | Project-created arrangement using Unity primitives |
| `MuseumFloorTiles.png` | Generated locally by the project's Unity Editor builder | Deterministic 256×256 repeating warm-stone tile texture | Project-created asset |
| Menu and HUD graphics | Unity UI primitives and project-authored layout | Main Menu, Controls overlay, Loot/health/timer UI, and end screens | Project-created arrangement using Unity UI |

## Audio assets

| Asset | Supplied filename / source | Unity location | Import and final use | Generation prompt / tool / terms |
|---|---|---|---|---|
| Artifact pickup SFX | `pickup.wav`, copied from `Downloads/College` | `Assets/Audio/SFX/pickup.wav` | PCM / Decompress On Load; 2D one-shot after an accepted artifact collection | **TBD — original creation record not supplied** |
| Damage/alarm SFX | `alarm.wav`, copied from `Downloads/College` | `Assets/Audio/SFX/alarm.wav` | PCM / Decompress On Load; 2D one-shot only after damage passes the cooldown | **TBD — original creation record not supplied** |
| Victory SFX | `win.wav`, copied from `Downloads/College` | `Assets/Audio/SFX/win.wav` | PCM / Decompress On Load; 2D one-shot once when the win state begins | **TBD — original creation record not supplied** |
| Gameplay music | `music.wav`, copied from `Downloads/College` | `Assets/Audio/Music/music.wav` | Vorbis / Streaming; looping 2D gameplay music, stopped at win/loss | **TBD — original creation record not supplied** |

The original files remain in Downloads. No source URL, creator, AI tool, prompt, or redistribution terms were present in the available evidence, so those fields require student confirmation before publication.

## External assets and references

No third-party external art, audio, font, or code asset is currently documented beyond Unity-provided resources and the Tripo files listed above. If anything else is added before submission, record its creator, source URL, exact license/terms, modifications, and final use here.

Public attribution/repository URL: **TBD — add after the repository is published.**
