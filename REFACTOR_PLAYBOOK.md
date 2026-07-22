# Refactor Playbook — Foosball

Working document. Not project memory — keep it out of `CLAUDE.md` so it does
not consume context every session. Copy prompts from here into Claude Code.

## Ground rules

- Start every stage in **plan mode** (`/plan` or Shift+Tab twice).
- One stage per session. Run `/clear` between stages.
- After every stage: recompile in Unity, play-test, commit, then move on.
- If a diff touches more than ~5 files, stop and split it.

## Before you start

```bash
git checkout -b refactor/architecture
git tag before-refactor
```

In Unity, Project Settings → Editor:
- Asset Serialization → **Force Text**
- Version Control → **Visible Meta Files**

Confirm `.gitignore` ignores `Library/`, `Temp/`, `Logs/`, `Build/`, `obj/`
and does **not** ignore `*.meta`.

---

## Stage 1 — Map the codebase

No code changes. The output is a document.

> Analyze this Unity project and produce a dependency map. For every
> MonoBehaviour and manager class, list: the types it references, any static or
> singleton access it performs, and any use of FindObjectOfType,
> GameObject.Find, Resources.Load, or SendMessage. Present it as a table.
> Then list every circular dependency you find, and every class that more than
> three other classes depend on. Write the result to
> docs/architecture-before.md. Do not modify any other file.

Keep this file. The before/after comparison is portfolio and blog material.

---

## Stage 2 — Extract data into ScriptableObjects

Lowest risk, immediate payoff. Do this before touching dependencies.

> Find every hardcoded tuning value and inspector-tuned constant in the ball
> and rod gameplay code. Group them into two ScriptableObject classes,
> PhysicsSettings and RodSettings, under Assets/_Project/Scripts/Data/.
> The MonoBehaviours should reference these via [SerializeField] private
> fields. Do not change any behavior — only change where the values come from.
> When done, list the exact CreateAssetMenu paths I need to use and every
> Inspector field I need to wire up.

Repeat for each of these, one session each:

| ScriptableObject | Contents |
|---|---|
| `MatchSettings` | Win score, time limit, ball count, serve rules |
| `AIDifficulty` | Reaction delay, prediction accuracy, aggression — one asset per level |
| `AudioBank` | Clips plus per-event variation and volume |
| `JuiceProfile` | Screen shake, hit-stop, camera punch parameters |

`JuiceProfile` is the seed of the GameJuice package. Design it as if it will be
extracted later, because it will be.

---

## Stage 3 — Make dependencies explicit

Order matters: kill lookups first, then singletons, then introduce the bus.

**3a — Remove runtime lookups**

> Find every call to FindObjectOfType, FindObjectsOfType, GameObject.Find, and
> Resources.Load in the project. For each one, replace it with an explicit
> [SerializeField] reference or a parameter passed into an Init method.
> Do not introduce a service locator. List every reference I need to assign in
> the Inspector, grouped by prefab and scene object.

**3b — Remove the singleton**

> Remove the GameManager singleton. Identify every class that reads
> GameManager.Instance and make the dependency explicit: either a serialized
> reference or an Init(GameManager) call from a composition root. Add a single
> GameBootstrap MonoBehaviour that constructs and wires the systems in Start.
> Behavior must be identical. List everything I need to wire in the Inspector.

**3c — Add the event bus**

> Create a type-safe event bus in Assets/_Project/Scripts/Infrastructure/.
> Requirements: generic type keys, no string identifiers, struct event payloads
> with no per-publish allocation, and automatic unsubscribe when a subscribing
> MonoBehaviour is destroyed. Add a small extension so a MonoBehaviour can
> subscribe with one call and have teardown handled for it.
> Then convert these to events: goal scored, match ended, ball reset,
> rod contact. Keep the direct references for everything else for now.

This class becomes the standalone Event Bus package later. Keep it free of
Foosball-specific types.

---

## Stage 4 — Split into layers

> Reorganize Assets/_Project/Scripts into Data, Core, Gameplay, Presentation,
> Infrastructure, and Editor folders per CLAUDE.md. Move each file to the
> folder matching its responsibility and update namespaces to match.
> Critical: move each .meta file together with its .cs file in the same
> operation. Core must not contain any using UnityEngine — if a file cannot
> satisfy that, tell me instead of moving it.
> Give me the full list of moves before executing anything.

Do this one folder at a time, not all at once. Recompile after each.

**Then extract the rules:**

> Extract match rules, scoring, and win-condition logic out of the
> MonoBehaviours into plain C# classes in Core/. They should take
> MatchSettings as a constructor argument and expose state through properties
> and events. No UnityEngine references. Then write NUnit edit-mode tests
> covering: scoring increments, win detection at the score threshold, time
> expiry, and reset between matches.

Unit-testable match rules is a concrete thing to point at in an interview.

---

## Stage 5 — Extract the packages

Once Foosball is clean, pull the reusable pieces out into UPM packages:

- Object pool + pool inspector window
- Event bus + subscriber inspector window
- GameJuice + `JuiceProfile` preview
- In-game debug console
- Versioned save system

Each package needs: `package.json`, `Samples~/` with a working scene,
a README with a GIF, semantic versioning, and a CHANGELOG.

Foosball then consumes them as packages rather than containing them. That
round trip — written in a real game, extracted, consumed back — is the story
worth telling in the README.

---

## Verification checklist

Run after every stage before committing:

- [ ] Project compiles with zero errors and no new warnings
- [ ] No missing script references in any scene or prefab
- [ ] Game plays through a full match without exceptions
- [ ] `git status` shows no unexpected deletions, especially `.meta` files
- [ ] Staged files are explicit paths, not `git add -A`
