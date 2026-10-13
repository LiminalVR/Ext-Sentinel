# Sentinel: offline Unity 6.7 compatibility

Status: **all 306 current Assets C# files pass after two guarded compatibility repairs**. Actual Unity import, gameplay and URP visual acceptance remain pending. Branch `experiment/sentinel-urp`; enabled scene `Assets/Scenes/TurretGame.unity`; on-disk Unity remains `6000.4.3f1`. No Unity editor was launched, imported, reloaded, restarted or closed. No rendering/scene/timing asset, shared SDK or global file was changed; no commit/push.

| Current scope | Sources | Errors | Warnings |
| --- | ---: | ---: | ---: |
| App000000000311, Editor runtime | 109 | 0 | 8 |
| App000000000311.Editor | 197 | 0 | 9 |
| App000000000311, Android managed runtime | 109 | 0 | 8 |

The revised shared compiler enforces each target's project dependency order and prohibits cached Assets-owned fallback. Initial runtime had six CS0619 errors per target, so the Editor scope was correctly blocked instead of compiling against cached main. After repair, Editor uses the fresh successful current-source main DLL. Readback authenticates that dependency and **945 unique current compiler/tool inputs**, covering every source, reference, analyzer, compiler/host, asmdef and cached response file. Android options contain `UNITY_ANDROID` without `UNITY_EDITOR`, with installed Android IL2CPP managed engine references.

These checks use installed Unity `6000.7.0b2` compiler/engine/source-generator files, cached packages/options and the undeployed `Unity67FinalAliases` SDK candidate. Record final deployed SDK hashes and repeat affected behavior after actual import. Managed compatibility is not a shader/IL2CPP/player build or runtime/rendering acceptance.

Before editing, **2,451 exact current inputs** were copied and read back against SHA256, including all source/configuration, current tracked dirty files, enabled scene and **all 2,110 current Assets metas**. Initial failed reports/log, dependency blocking report, Git HEAD/status/diff and exact index were retained. Source/meta after-fix copies are separate. Original dirty state was ten modified entries and one untracked entry; only the two documented source modifications were added. Git HEAD/index and all other status entries remain unchanged.

Repairs:

- `CFX_SpawnSystem.cs`: full `EntityId` keys flow through both private pool dictionaries, three lookup locals and the private cursor method on 6.7. Earlier Unity retains `Int32` and original lookups. Cursor indices, preload/unload, activation, ordering, pool growth and all public/protected/serialized declarations remain intact. Exact original/revised hashes match the authenticated Fireside template; no template meta was copied.
- `WaterReflection.cs`: only three temporary render-texture/camera naming calls use `GetEntityId` on 6.7, with older branches retained. Exact originals match Cannon's Roar, and reversing both guarded naming blocks recovers the full original bytes. Camera/reflection math, allocation, dictionary behavior, material settings and timing are unchanged. This does not port Built-in reflection callbacks to URP.

Every preserved original and current input matches, except those two source repairs. Both repairs reverse byte-for-byte to their originals and retain public/protected/serialized declarations. A project-local verifier initially misread Git's quoted whitespace-containing Water path; that harness failure is retained and corrected by decoding the path. No original file or Git configuration was changed to satisfy it.

Existing archive preflight agrees with the original manifest: **3,953 files / 1,997,453,791 source bytes**, SHA256 `f43d80297b6046c899cdf8d292a48dcfeb6f966fbd26468a6774e1d94b957185`. This check verifies prior authenticated provenance, without claiming a fresh archive rehash.

Evidence:

- [Current source/input and exact repair verification](../../Builds/URPMigration/OfflineCompileUnity67/verification.json)
- [Current scope/dependency summary](../../Builds/URPMigration/OfflineCompileUnity67/summary.json)
- [Original preservation and failed stage](../../Builds/URPMigration/OfflineCompileUnity67/BeforeCompatibilityFix/manifest.json)
- [Guarded repair provenance and reversal](../../Builds/URPMigration/OfflineCompileUnity67/unity67-identity-repairs.json)
- [Authenticated progression and nested prefab candidates](../../Builds/URPMigration/OfflineCompileUnity67/source-progression-plan.json)
- [Reproducible verification](VerifyOfflineChecks.py)

## Required ordinary progression and effect review

The first ordinary cannon grab starts waves and removes the tutorial. Idle Play does not prove that start path. Exercise actual controller grip/trigger and OpenXR hand tracking/pinch auto fire, with release/re-grab, hand models, recoil, audio and both haptics. Save actual player/cannon/hand poses for comparison.

Scene configuration has three timed 60-second waves. Source estimates from initial grab are wave 1 at 6–66 seconds (MeteorEnemy, single shot), wave 2 at 78–138 (MeteorEnemy/BigMeteorEnemy, scatter) and wave 3 at 154–214 (MeteorEnemy/DroneEnemy, full auto). The preceding intermissions use six seconds plus the incoming wave's extra delay. These estimates assume ordinary enabled progress and are not runtime observations. Shield break stops/disables the spawner, plays/reset effects and later reenables it, so actual clocks can shift. Preserve authored settings and health; do not seek, skip gates or hack health to claim normal coverage.

Measure actual game/wave/music progress, capture around 60/120/180 game seconds, then continue through all final-wave content and the actual SDK ending. EndAllWaves waits seven seconds before twelve seconds of ending dialogue, then activates the initially inactive Timeline under the active ExperienceApp root. Its serialized fade signal is 13.9833 seconds into the ending Timeline and calls a two-second FadeToBlack; EndExperience is at 16.1167. The estimated SDK exit is therefore 249.1167 seconds from grab before pauses/resets. Actual hierarchy activity, event delivery, full fireworks/fade and SDK completion require runtime proof.

Player cannonballs really travel: existing code activates them, waits the next FixedUpdate and applies impulse. Capture several distinct player and enemy instances visibly at spawn, travel, impact/hit and pooled reset/disappearance; a turret-only screenshot is insufficient. Include ordinary meteor/drone/UFO/projectile/powerup collisions where reachable, muzzle particles, trails, shield flashing/pulsing/break/reset, hit-material swaps, explosions, score popups and powerup scale/fade. EnemyHealth skips portal on its first enable and adds portal effects on later enables, so repeated pooled enemy reuse matters.

The source plan inventories **37 serialized prefab dependency candidates**, their nested renderer/particle/trail/script counts and direct material/shader references, plus source dynamic spawn/material-write sites. This is an over-approximation of serialized dependencies, not actual reachability or coverage. Inspect effective hierarchy, prefab overrides, imported model materials, runtime replacements and secondary spawners; exercise every reachable nested family through its ordinary event path. Capture end fireworks as well as combat. The repaired CFX/Water GUIDs have no local serialized references in the searched scene/prefab/asset/Timeline/controller files; that does not prove runtime-unreachable and does not establish active rendering coverage.

Match wave/music/particle ages, animated hand/cannon/enemy poses, camera and live material state in before/after diagnostics. Preserve source shader property types/HDR flags, texture defaults, scalars, ambient/reflection, blend/depth and runtime property writes. Review forward and useful surrounding/side/up/back/down views, then verify final appearance in a fresh URP-only session.

Fresh Unity 6.7 import/package compilation, full ordinary game and chosen ending, all reachable variants and several actual effect instances/lifecycles, source-derived URP conversion/rendered review, Android IL2CPP/shader player build and physical Quest 3 controller/hand tracking/stereo/performance remain pending.
