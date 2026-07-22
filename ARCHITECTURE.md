# Architecture Notes — Assembly Split & Event Bus

This document explains the refactor that took the project from one monolithic
assembly and ~7 singletons to a layered `.asmdef` structure wired through a
composition root and a generic event bus. It's a companion to `CLAUDE.md`,
which states the rules; this explains *why* those rules exist and how to keep
the architecture from eroding as the project grows.

## Before → After

**Before:** everything (`GameManager`, `GameStateManager`, `AudioManager`,
`RumbleManager`, `VFXManager`, `GameJuiceManager`, `AimTrajectory`, and every
gameplay script) compiled into one implicit `Assembly-CSharp`. Each manager was
a static singleton (`Manager.Instance`), reachable from anywhere, with no
compiler-enforced boundary between "game rules," "gameplay behaviour," and
"audio/VFX/rumble feedback."

**After:**

```
Data ← Core ← Gameplay ← Presentation ← App
                 ^
        Infrastructure (EventBus) — consumed by Gameplay, Presentation, App
Editor — isolated, referenced by nothing
Core.Tests — consumes Core + Data
```

- `Data/` — ScriptableObject configs (`MatchSettings`, `BallConfig`,
  `RodConfig`, `AudioConfig`, `JuiceConfig`). No logic.
- `Core/` — plain C# domain rules (`MatchState`). `noEngineReferences: true`:
  the compiler physically rejects a `using UnityEngine` here, not just a code
  review comment.
- `Gameplay/` — MonoBehaviours that *do things* (`RodController`,
  `BallController`, `GameManager`, `TeamController`, `GameStateManager`).
- `Presentation/` — MonoBehaviours that make things *look/sound* like
  something happened (`AudioManager`, `VFXManager`, `RumbleManager`,
  `GameJuiceManager`, `MatchFeedbackController`, UI controllers).
- `Infrastructure/` — the `EventBus`, dependency-free, doesn't know Foosball
  exists.
- `App/` — `GameBootstrap`, the single composition root. It's the only class
  allowed to reference every layer, because wiring everything together is
  its entire job.
- No more singletons: every manager is a `[SerializeField]` reference or an
  `Init(...)` argument, assigned once by `GameBootstrap`.

## The asmdef graph, and why the arrows point this way

The reference chain is `Data ← Core ← Gameplay ← Presentation ← App`, plus
`Infrastructure` as a leaf consumed by `Gameplay`, `Presentation`, and `App`.
Two separate forces produced this shape:

**1. A hard Unity constraint.** Named assembly definitions compile *before*
the implicit `Assembly-CSharp`. That means a named asmdef can never reference
`Assembly-CSharp` by name — not "shouldn't," *can't*. The only fix is to leave
nothing in the implicit assembly: every folder, including third-party source
(`DOTween` Modules, `QuickOutline`), needed its own asmdef.

**2. A stability ordering, which is why the arrows point the way they do and
not some other way.** `Data` and `Core` change rarely and don't know Unity or
features exist — they're the most stable thing in the project. `Presentation`
changes constantly (new SFX, new juice, new camera feedback) and is the least
stable. The rule is: **volatile code depends on stable code, never the
reverse.** If `Core` referenced `Presentation`, every new sound effect would
force the domain-rules assembly to recompile, and `MatchState` would become
untestable without a Unity player. `Foosball.Core` also carries
`noEngineReferences: true` — a second, compiler-enforced guarantee that this
boundary can't quietly erode.

`App` sits above all of it for a narrow reason: `GameBootstrap` is the one
class in the whole project that legitimately needs to reference both
`Gameplay` and `Presentation` concrete types, because assembling the object
graph is its entire responsibility. Nothing else has that excuse.

## Why the EventBus, not direct references

Before this pass, `RodController`, `BallController`, and `GameManager`
(`Gameplay`) called `AudioManager`/`VFXManager`/`RumbleManager`/
`GameJuiceManager` (`Presentation`) directly, while several Presentation files
also referenced `Gameplay` types (`GameStateManager`, event structs). That's a
genuine cycle — `Gameplay → Presentation → Gameplay` — which Unity's asmdef
system cannot resolve at all, in either direction.

The fix: `Gameplay` no longer names a single `Presentation` type. Instead it
publishes plain, Presentation-agnostic structs through the `EventBus` —
`ShootEvent`, `GoalEvent`, `BlockEvent`, `RodStunnedEvent`, `WallBounceEvent`,
etc. (`Gameplay/GameEvents.cs`). `Presentation/MatchFeedbackController.cs` is
the one place that subscribes to all of them and turns each into calls on the
managers it owns:

```csharp
private void HandleBlock(BlockEvent evt)
{
    m_VFXManager?.PlayBlock(evt.Position, evt.VfxScale);
    m_AudioManager?.PlayDefenseCatch();
    m_RumbleManager?.RumbleBlock(evt.Gamepad);
    ...
}
```

`Gameplay` now depends on `Infrastructure` (the bus) instead of on
`Presentation` concrete types — and `Infrastructure` knows nothing about
either side. That breaks the cycle without any code needing to guess what the
other layer will do with the information.

## "Isn't MatchFeedbackController backwards? It holds manager references."

This looked like a layering violation but it's actually two different axes,
easy to conflate:

- **Cross-layer axis** (what the asmdef graph enforces): `Presentation →
  Gameplay → Core`. This is the axis that was violated before the fix — a
  `Gameplay` class naming a `Presentation` type. That's fixed: `Gameplay`
  no longer references `Presentation` at all.
- **Within-layer axis** (what `MatchFeedbackController` is actually doing):
  `MatchFeedbackController`, `AudioManager`, `VFXManager`, `RumbleManager`,
  and `GameJuiceManager` are all *peers inside `Presentation`* — there is no
  cross-layer arrow here to get backwards. Within one layer, it is completely
  normal — expected, even — for an **orchestrator to hold references to the
  leaf services it coordinates**. A conductor references the musicians; the
  musicians never reference the conductor.

The direction is verifiably correct because the leaf services have zero
knowledge of `MatchFeedbackController`. Open `AudioManager.cs` or
`VFXManager.cs`: every public method is a stateless command
(`PlayGoal()`, `ShakeCamera(0.5f, 0.6f)`). None of them know `Gameplay` types,
event structs, or `MatchFeedbackController` exist. If the reference ran the
other way — a leaf service reaching up to call into a coordinator, or into
`Gameplay` directly — *that* would be the same mistake the `RodController` /
`AudioManager` cycle was, just moved one layer over.

**Optional next rung, not a recommendation:** introducing interfaces
(`IAudioService`, `IVfxService`, ...) so `MatchFeedbackController` depends on
abstractions instead of concrete manager classes would satisfy full
Dependency Inversion and make the controller unit-testable without Unity.
Worth doing if a second implementation (a mock for tests, a "silent mode")
ever becomes a real need — not worth the ceremony before then.

## Maintenance advice

- **Before adding any `using Foosball.X`, check the arrow.** Allowed:
  `Presentation → Gameplay → Core`, anything `→ Data`, anything `→
  Infrastructure`, `App →` everything. Anything else is the same smell that
  caused this refactor — replace it with an event, don't add the reference.
- **New cross-cutting feedback** (a new SFX/VFX/rumble reaction to an
  existing gameplay moment) is a new `Subscribe<T>` + handler in
  `MatchFeedbackController`, not a new direct call from `Gameplay`.
- **New event types** go in `Gameplay/GameEvents.cs` as plain structs with no
  method bodies, and aren't "real" until something actually publishes them.
  This session found a stale field (`BlockEvent.JuiceIntensity` from an
  earlier draft, never published, superseded by `BallVelocityX`/`BallSpeed`)
  that survived a rewrite unnoticed — grep for `Publish(new EventName` before
  trusting a struct's shape.
- **`Foosball.Core` keeps `noEngineReferences: true`.** If something in
  `Core/` ever fails to compile because of that flag, the fix is to move the
  Unity-touching part to `Gameplay` — never relax the flag.
- **No new singletons.** A new Presentation service gets a `[SerializeField]`
  and an `Init(...)` call wired from `GameBootstrap`, same as the existing
  six managers.
- **A new top-level folder needs its own `.asmdef` from day one** — retrofit
  attempts are how the compile-order trap gets hit again. Copy the reference
  list of whichever existing layer it's conceptually closest to.

## Verify in Unity

- Recompile — this is a new markdown file, not code, so there's nothing to
  break, but Unity will generate a `.meta` for it on next import. Nothing to
  wire in the Inspector.
