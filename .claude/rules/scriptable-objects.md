---
paths:
  - "Assets/_Project/Scripts/Data/**/*.cs"
---

# ScriptableObject Rules

Applies to every type under `Data/`.

## Definition

- Every SO gets `[CreateAssetMenu]` with a menu path under `Foosball/`:
  `[CreateAssetMenu(fileName = "RodSettings", menuName = "Foosball/Rod Settings")]`
- Fields are `[SerializeField] private` with public read-only properties.
  Nothing in `Data/` is writable from gameplay code at runtime.
- Annotate numeric fields with `[Range]`, `[Min]`, or `[Tooltip]` so the asset
  is tunable without reading the source.
- Group related fields with `[Header("...")]`.

## What belongs here

Configuration and tuning data only:

- Physics constants, movement speeds, limits, thresholds
- AI difficulty parameters
- Audio clip banks, VFX profiles, juice profiles
- Match rules (win score, time limit, ball count)

## What does not belong here

- Mutable runtime state. SOs are shared assets; writing to them at runtime
  persists in the Editor and leaks between play sessions.
- References to scene objects. SOs cannot hold scene references.
- Logic beyond trivial derived properties. No `Update`, no coroutines.

## Runtime state

If a system needs per-match mutable state, define a separate plain C# class in
`Core/` and initialize it from the SO:

```csharp
var state = new MatchState(matchSettings);
```

Never mutate the SO itself.

## Validation

Implement `OnValidate()` for any constraint that can be checked in the Editor
(min < max, non-null clip arrays, non-zero divisors). Log a clear warning
naming the asset. This catches misconfigured assets before play mode.

## Creating assets

You cannot create `.asset` files. After defining or changing an SO class,
output the exact menu path I should use and the field values to enter.
