---
paths:
  - "Assets/_Project/Scripts/Editor/**/*.cs"
---

# Editor Tooling Rules

Applies to editor-only code. These files must live inside a folder named
`Editor` or they will be included in player builds and break compilation.

## Constraints

- Wrap anything referencing `UnityEditor` in `#if UNITY_EDITOR` as a second
  line of defense, even inside an `Editor` folder.
- Editor code may reference runtime code. Runtime code may **never** reference
  editor code.
- Menu items go under `Foosball/` — for example
  `[MenuItem("Foosball/Tools/Find Missing References")]`.

## Preferred APIs

- `EditorWindow` for standalone tools; `Editor` + `[CustomEditor]` for
  inspectors; `PropertyDrawer` for reusable field widgets.
- Use `SerializedObject` / `SerializedProperty` rather than writing to target
  fields directly. This preserves undo, multi-object editing, and prefab
  override tracking.
- Wrap destructive operations in `Undo.RecordObject` before mutating, and call
  `EditorUtility.SetDirty` after.
- Use `AssetDatabase.FindAssets` with a type filter for project-wide scans, and
  show `EditorUtility.DisplayProgressBar` for anything scanning many assets.

## Tools planned for this project

These are being written here first, then extracted into standalone UPM
packages later. Keep them dependency-free so extraction stays cheap.

- **Missing reference finder** — scan scenes and prefabs for null or broken
  object references, list them with a click-to-select result row.
- **Build size analyzer** — parse the build report, group assets by type and
  size, highlight the largest contributors.
- **TODO collector** — scan `.cs` files for `TODO:` / `FIXME:` / `HACK:`,
  group by file, click to open at the line.
- **Pool inspector** — live view of active/idle counts and peak usage per pool.
- **Event bus inspector** — live view of subscribers per event type and a
  timestamped log of recent events.

## Style

Editor tools are internal-facing but treat them as products: name things
clearly, keep the window usable at small sizes, and never block the main thread
for more than a second without a progress bar.
