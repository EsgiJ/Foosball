# Foosball — Unity Project

3D foosball game. Single-player vs AI and local multiplayer (keyboard-split and gamepad).
This file is project memory for Claude Code. Keep it under 200 lines.

<!-- Fill in the bracketed values below before the first session. -->

- Unity version: `6000.3.9f1`
- Scripts root: `Assets/_Project/Scripts/`
- Main scene: `Assets/_Project/Scenes/SCN_Game.unity`

---

## Hard rules — never violate these

These break the project silently and are expensive to recover from.

- **Never edit or delete** `.unity`, `.prefab`, `.asset`, `.meta`, or `.controller`
  files. Unity owns their serialized format. If a change requires touching one,
  stop and tell me what to do in the Editor instead.
- **`.meta` files travel with their asset.** If you move or rename a `.cs` file,
  move or rename its `.meta` file in the same operation. A lost `.meta` means
  every reference to that script in every scene and prefab is severed.
- **Never touch** `Library/`, `Temp/`, `obj/`, `Logs/`, `Build/`.
- **You cannot compile or run this project.** You cannot see the Editor, the
  Inspector, or the Console. After any change, list exactly what I need to
  verify or wire up manually in Unity.
- **Never create ScriptableObject asset instances.** You write the C# class; I
  create the `.asset` in the Editor. Tell me the menu path to use.

---

## Architecture rules

- MonoBehaviours stay thin: Unity lifecycle, serialized references, and visual
  binding only. Game logic lives in plain C# classes.
- `Core/` must not contain `using UnityEngine`. It is unit-testable by design.
- **No singletons.** Pass dependencies explicitly via `[SerializeField]`
  references or an `Init(...)` method called by a composition root.
- **Never use** `FindObjectOfType`, `FindObjectsOfType`, `GameObject.Find`,
  `Resources.Load`, or `Camera.main` in per-frame code.
- Cross-system communication goes through the event bus, not direct references.
  Direct references are fine downward (a system owning its own components).
- Tunable values never live in code. They live in ScriptableObjects.
- No `public` fields. Use properties or `[SerializeField] private`.

## Performance rules

- No allocations in `Update`, `FixedUpdate`, or `LateUpdate`. No LINQ, no
  string concatenation, no `new` on reference types in hot paths.
- Cache component lookups in `Awake`. Never call `GetComponent` per frame.
- Physics work belongs in `FixedUpdate`. Input polling belongs in `Update`.
- Pool anything spawned repeatedly. Do not `Instantiate`/`Destroy` at runtime.

---

## Project layout

```
Assets/_Project/Scripts/
├── Data/           ScriptableObject definitions
├── Core/           Pure C# — match rules, scoring, state machine (no UnityEngine)
├── Gameplay/       MonoBehaviours — ball, rods, input, AI
├── Presentation/   UI, audio, VFX, camera
├── Infrastructure/ Event bus, object pool, save system
└── Editor/         Editor-only tools (must be in an `Editor` folder)
```

## Naming conventions

- Namespaces mirror folders: `Foosball.Core`, `Foosball.Gameplay`, etc.
- Private fields: `_camelCase`. Constants: `PascalCase`. Locals: `camelCase`.
- One type per file. Filename matches the type name exactly.
- ScriptableObject classes end in `Settings`, `Profile`, `Bank`, or `Config`.
- Interfaces prefixed with `I`. Abstract bases suffixed with `Base`.

---

## How to work with me

- **Start every non-trivial task in plan mode.** Propose the change, wait for
  approval, then implement.
- **One system per session.** After finishing one, I run `/clear` before the next.
- **Refactors must not change behavior.** Structure only. If you believe a
  behavior change is necessary, stop and ask first.
- **Small diffs.** If a change would touch more than ~5 files, split it and
  propose the split.
- End every code change with a short **"Verify in Unity"** checklist: what to
  recompile, which Inspector fields to rewire, what to play-test.

## Commit convention

Conventional Commits, English, imperative mood.

```
refactor(gameplay): extract rod tuning values into RodSettings SO
feat(infra): add type-safe event bus with auto-unsubscribe
fix(ai): clamp prediction lookahead to avoid overshoot
```

Do not commit unless I ask. Never use `git add -A` — stage explicit paths only,
so stray `Library/` or `.csproj` churn never enters a commit.

---

## Current state

The codebase is being refactored out of an early prototype structure. What
actually exists today, vs. the target layout above:

- **Single assembly.** No `.asmdef` files anywhere — everything compiles into
  the default `Assembly-CSharp` (`Foosball.slnx` has one project). The
  "`Core/` has no `UnityEngine`" rule is a convention only, not yet
  assembly-enforced.
- **Current folders:** `Audio/`, `Core/`, `Rod/`, `UI/`, plus loose top-level
  scripts (`BallController`, `CameraController`, `FootballPlayerController`,
  `Formation`, `TeamController`, `AimTrajectory`, `BallTrail`,
  `InteractiveMenu`, `RumbleManager`). No `Data/`, `Gameplay/`,
  `Presentation/`, `Infrastructure/`, or `Editor/` yet.
- **`Core/` mixes MonoBehaviours with `UnityEngine`** (`GameManager`,
  `GameStateManager`, `GameEvents`, `GameJuiceManager`, `VFXManager`) —
  contradicts the target rule; first thing the refactor fixes.
- **~7 manual singletons today:** `GameManager`, `GameStateManager`,
  `GameJuiceManager`, `AudioManager`, `RumbleManager`, `VFXManager`,
  `AimTrajectory`. "No singletons" is the target, not current reality.
  `GameJuiceManager` also does `GameObject.Find("Global Volume")` — one of the
  forbidden lookups being removed.
- **One SO already in real use ahead of the rest of the refactor:**
  `Rod/RodConfig.cs` (`Foosball/Rod Config`), assets under
  `Assets/_Project/ScriptableObjects/RodConfigs`.
- **Two event mechanisms coexist:** `GameStateManager`
  (`[DefaultExecutionOrder(-100)]`) drives a `GameState` FSM
  (MainMenu→Setup→Countdown→Playing→Goal/Paused→...→Settings) via an
  `OnStateChanged` event; `GameEvents` is a separate static pub/sub bus for
  gameplay signals (`OnBallImpact`, `OnShootEvent`, `OnGoalEvent`). These get
  unified into one event bus in the refactor.
- **No AI opponent.** Local 2-player only — two `InputActionAsset` instances
  cloned per team via the new Input System, control-scheme-masked to
  Keyboard-Left / Keyboard-Right / Gamepad.
- **No test/CI pipeline.** `com.unity.test-framework` is installed but no
  tests exist yet; there is no CLI build or test entry point. The only way to
  run anything is Unity's Editor Test Runner, once NUnit tests are added.

See `REFACTOR_PLAYBOOK.md` for the staged prompts driving this refactor.

When you touch a file, prefer leaving it closer to the target architecture than
you found it — but only within the scope of the current task.
