# Feature Refactoring Guidelines

## STEP 1: Discovery (Do this FIRST - no code changes yet)

1. Search codebase for ALL related code:
   - Use semantic search: "How does [feature] work?"
   - Search for class names, method names, interfaces
   - Find all files that import/use related classes
   - List every file that touches this feature

2. Map the flow:
   - What triggers the feature?
   - What processes happen?
   - What is the end result?
   - Draw the data flow between classes

3. Find violations:
   - Search for "GetComponent" in all related files
   - Search for "Find" in all related files
   - List all violations found

## STEP 2: Component Access Rules (STRICT - Zero Exceptions)

### FORBIDDEN (Never use):
- `GetComponent<T>()` / `GetComponents<T>()`
- `GetComponentInParent<T>()` / `GetComponentInChildren<T>()`
- `FindObjectOfType<T>()` / `FindObjectsOfType<T>()`
- `GameObject.Find()` / `transform.Find()`

## STEP 3: Refactoring Each File

For EACH related file found in Step 1:

1. Remove ALL GetComponent/Find calls
2. Replace with: DI or SerializedField (no exceptions)
3. Remove duplicate logic (DRY)
4. Simplify complex methods
5. Remove inline comments (keep XML docs)
6. Remove Debug.Log statements

## STEP 4: Code Quality Rules

- **DRY**: No duplicate code
- **SOLID**: Single responsibility per class
- **Simplify**: Remove unnecessary complexity
- **Modular**: Clear separation of concerns

## STEP 5: Final Check

Before finishing, verify:
- [ ] All related files were modified (not just one)
- [ ] Zero GetComponent/Find calls remain
- [ ] All dependencies are injected or serialized
- [ ] No code duplication
- [ ] Behavior unchanged (same functionality)

## STEP 6: Unity Changes (if needed)

If Inspector changes required:
- List exact steps
- List which GameObjects need references
- List prefab changes needed

---

## Quick Reference: What to Do When You See GetComponent

1. **Unity Component (any GameObject)?** → SerializedField (Pattern 2)
2. **Service/Manager/Cross-object dependency?** → Dependency Injection (Pattern 1)
3. **Collision object?** → Use collision data directly (Pattern 3)
4. **Never use GetComponent** - Zero exceptions

## Critical Reminder

- Refactor ALL related files, not just one
- Map the complete feature first
- Zero GetComponent/Find calls allowed (no exceptions, use SerializedField for all components)
- Preserve all functionality
