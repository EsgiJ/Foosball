---
paths:
  - "Assets/_Project/Scripts/Gameplay/**/*.cs"
  - "Assets/_Project/Scripts/Presentation/**/*.cs"
---

# MonoBehaviour Rules

Applies to gameplay and presentation code that lives on GameObjects.

## Responsibility

A MonoBehaviour does three things and nothing else:

1. Holds serialized references and settings assets
2. Translates Unity lifecycle callbacks into calls on plain C# objects
3. Reflects state onto the scene (transforms, renderers, audio, UI)

Decision-making, rules, and scoring belong in `Core/`. If a MonoBehaviour has a
method longer than ~25 lines that is not a lifecycle callback, that logic
probably belongs in a plain class.

## Lifecycle

- `Awake` — cache own components, build internal objects. No cross-object access.
- `OnEnable` — subscribe to events.
- `Start` — cross-object initialization, after everything has awoken.
- `OnDisable` — unsubscribe. Every subscription in `OnEnable` has a match here.
- `OnDestroy` — release pooled or unmanaged resources.

Never rely on script execution order. If ordering matters, make it explicit
with an `Init(...)` call from a composition root.

## Physics

- All `Rigidbody` reads and writes happen in `FixedUpdate`.
- Use `Rigidbody.MovePosition` / `MoveRotation`, never direct `transform`
  assignment on a physics body.
- Use `Time.fixedDeltaTime` in `FixedUpdate`, `Time.deltaTime` in `Update`.
- Interpolate visuals separately from physics state so rendering stays smooth
  at a fixed timestep.

## Forbidden

- `FindObjectOfType`, `GameObject.Find`, `Resources.Load`, `SendMessage`
- `Camera.main` outside `Awake`
- `GetComponent` in any per-frame method
- Static mutable state
- `Instantiate` / `Destroy` at runtime — use the object pool
- Empty `Update` methods (they still cost a managed call every frame)

## Serialized references

Prefer `[SerializeField] private` over public. Mark fields that must be
assigned with a clear name and, where the type allows it, validate in
`OnValidate` and log which GameObject is missing the reference.

When you add or change a serialized field, list it explicitly so I can wire it
in the Inspector. Unity will not populate it for me.
