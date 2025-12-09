Read following instructions carefully:

Go through the entire project and identify all code related to this feature. Build a full picture of the flow and how the modules interact. Refactor the relevant scripts to:

- Make sure you understand game logic and related code carefully
- Remove bad practices, potential bugs, and unnecessary complexity.
- Dramatically simplify and deduplicate logic while preserving behavior.
- Refactor code to be more modular, scalable, and readable with best practices
- Focus on SOLID and DRY
- Improve structure, naming, and overall readability and maintainability.
- Never add inline comments and debug logs
- Remove old scripts and logic that was replaced (if exists)
- Never use GetComponent or FindObject type of methods
- If changes inside unity are required, write instructions

**CRITICAL: NEVER delete XML `<summary>` blocks or other structured documentation comments. Always preserve existing summaries and add summaries to new public members.**

---

## Architecture Compliance (MANDATORY)

Before making ANY changes, reference `Assets/_Docs/Architecture.md` and `Assets/_Docs/DevelopmentPlan.md`.

### Layer Rules (STRICT)

| Layer | CAN Reference | CANNOT Reference |
|-------|---------------|------------------|
| **Core** | Nothing (pure C#) | Unity, Infrastructure, App, Presentation |
| **Infrastructure** | Core only | App, Presentation |
| **App** | Core, Infrastructure | Presentation |
| **Presentation** | Core, App | **Infrastructure** |

⚠️ Presentation layer must NEVER directly reference Infrastructure. Use dependency injection via VContainer.

### File Placement

| Component Type | Location |
|----------------|----------|
| Interfaces | `Scripts/Core/Abstractions/` |
| Data structs | `Scripts/Core/Data/` |
| Pure C# logic | `Scripts/Core/Domain/` |
| Event definitions | `Scripts/Core/Events/` |
| ScriptableObjects | `Scripts/Infrastructure/` |
| Services | `Scripts/App/Services/` |
| MonoBehaviours | `Scripts/Presentation/` |

### Naming Rules

- **Namespace MUST match folder path** (e.g., file in `Core/Abstractions/` → `namespace Core.Abstractions`)
- One class per file, file name matches class name
- Interfaces: `IInterfaceName`
- Private fields: `_camelCase`
- MonoBehaviours: suffix with `View` or `Component`

### Dependency Injection

- Always depend on interfaces, not concrete types
- Register dependencies in `GameplayInstaller` only
- In Presentation, use `[Inject]` for dependencies, NOT `[SerializeField]` for Infrastructure types

```csharp
// ✅ CORRECT
[Inject] private IEventChannel<KnifeSpawnRequestData> _spawnChannel;

// ❌ WRONG - direct Infrastructure reference
[SerializeField] private KnifeSpawnRequestEventChannel spawnRequestEventChannel;
```

### Core Layer (Pure C#)

- NO `using UnityEngine;` in Core layer
- NO Unity types (Vector2, MonoBehaviour, etc.)
- Use primitives and convert at layer boundaries

### Anti-Patterns to Fix

- [ ] Presentation referencing Infrastructure directly
- [ ] Concrete type dependencies instead of interfaces
- [ ] Namespace not matching folder path
- [ ] Unity types in Core layer
- [ ] Magic numbers (use ScriptableObject configs)
- [ ] GetComponent/FindObject methods

### Before Completing

Verify:
- [ ] All interfaces in `Core/Abstractions/`
- [ ] Namespace matches folder path in all files
- [ ] Core has no Unity dependencies
- [ ] Presentation does not reference Infrastructure
- [ ] Dependencies injected via VContainer interfaces
