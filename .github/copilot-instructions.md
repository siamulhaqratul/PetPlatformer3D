
# Copilot Instructions

This project follows strict coding principles for maintainable, scalable, and clean C# code in Unity.


## Field Naming
- All class-level fields must be renamed to `_camelCase` (e.g., `_health`, `_score`) as part of Hamba Structure. Always update all usages throughout the file when applying Hamba Structure.
- Constants use `ALL_CAPS_WITH_UNDERSCORES` (e.g., `MAX_HEALTH`, `TARGET_SCAN_INTERVAL`). Always update all usages throughout the file when applying Hamba Structure.
- Readonly fields must use `_camelCase` naming, must NOT be static, and must be placed in a dedicated section: `// ========================= READONLY FIELDS ======================`. Do not remove user comments (such as region comments) when refactoring or reorganizing fields.



## Code Structure

-- For any fields (including constants and readonly), place a descriptive comment on its own line directly above the field declaration (not inline) only if documentation is requested. Do not do this for properties or events.
-- Use visually distinct section headers: a single `//` comment line with **long runs of `=`** padding the label on both sides (not `#region`). Copy the exact strings from the section list below — spacing and equals count must match. Example: `// ================= SERIALIZED FIELDS (INSPECTOR) =================`. Add a blank line after every section header.
-- For any fields (including constants and readonly), do NOT include a descriptive comment above the field declaration unless you specifically request documentation (e.g., by saying "Do Hamba Documentation"). Never add field comments by default.
-- Only include section headers if there are actual fields or methods in that section.
-- For serialized fields, always use `[Tooltip]` attributes for Inspector documentation. Do not add single-line comments for `[SerializeField]` fields if a `[Tooltip]` is present.
-- Use one-line XML summary comments for properties, methods, and events (e.g., `/// <summary>Damage dealt per attack.</summary>`), except for Unity callback methods (e.g., Start, Awake, Update, FixedUpdate).
-- For methods with parameters, include `<param>` tags for each parameter.
-- Use clear, descriptive variable names (avoid single-letter names).
-- Organize code into the following clear sections (only include if used):
  - // =========================== CONSTANTS ===========================
  - // ======================= STATIC FIELDS ==========================
  - // ========================= READONLY FIELDS ======================
  - // ================= SERIALIZED FIELDS (INSPECTOR) =================
  - // =================== PRIVATE/PROTECTED FIELDS ====================
  - // ========================= INITIALIZE METHODS ========================
  - // =========================== PROPERTIES ==========================
  - // ============================ EVENTS =============================
  - // ======================= UNITY CALLBACKS =========================
  - // ========================= PUBLIC METHODS ========================
  - // ========================= STATIC METHODS ========================
  - // ========================= PRIVATE METHODS =======================
  - // ======================== ABSTRACT METHODS =======================

### Example

See `Assets/Scripts/SampleCode.cs` for a complete sample to follow for Hamba Structure and Documentation.

```csharp
// ...see full example in Assets/Scripts/SampleCode.cs...
```

## General Practices
Do not add any documentation comments (including XML summary, method, or property comments) in scripts unless the user specifically requests documentation. Only add documentation if the user says "Do Hamba Documentation" or otherwise asks for it.
-- Use `[Header]` for inspector grouping.
-- Use `[Tooltip]` for all serialized fields.
-- Use properties instead of public fields.
-- Avoid magic numbers and strings; use named constants.
- Use the most restrictive access modifier possible.
- Keep methods short and focused.

## Unity-Specific Clean Code Best Practices
- Use `[Header]` to organize serialized fields in the Inspector.
- Cache components and frequently used GameObjects in `Start`.
- Cache `transform` instead of accessing it repeatedly.
- Use `Start` for initialization; avoid `Awake` unless necessary.
- Avoid using `Find` or `FindObjectOfType` for lookups.
- Avoid heavy logic in `Update`.
- Use `FixedUpdate` for physics-related logic.
- Use events instead of polling every frame.
- Separate input handling from movement logic.
- Use `ScriptableObject` for data-driven design.
- Use object pooling for frequently spawned objects.
- Use coroutines for delayed or repeated actions.
- Follow component-based architecture.
- Use curly braces for all control statements, except guard clauses.
- Extract complex conditions/expressions into well-named variables or methods.
- Use string interpolation (`$""`) for clarity.
- Use `var` wisely.
- Use clear, descriptive variable names (avoid single-letter names).
- One class, enum, interface, or abstract per file.

## Tooling
- Always prefer the Unity MCP tools when they are available (scene/GameObject/asset/prefab edits, script validation, reading the console, running tests, refreshing the editor). Fall back to direct file edits or other tools only when Unity MCP is unavailable or can't do the task.

## How to Apply
- **Cursor Agent:** `.cursor/rules/hamba-structure.mdc` applies automatically when working on `*.cs` files.
- **VS Code + Copilot:** this file is loaded automatically for the workspace.
- **Claude Code:** `CLAUDE.md` at the repo root is loaded automatically for every session.
- For refactoring, say: "Do Hamba Structure".
- For documentation, say: "Do Hamba Documentation" (documentation will only be added or updated if you give this command).

---
For more details or updates, edit this file.
