# AGENTS.md

Guidance for agents (human or AI) contributing to AutoSpectre. Focus: extending the source generator when Spectre.Console ships new prompt features.

## Project in one paragraph

AutoSpectre is a Roslyn incremental source generator. Users decorate a class with `[AutoSpectreForm]` and its properties with `[TextPrompt]` / `[SelectPrompt]` / `[TaskStep]`. At compile time the generator emits a factory that, when invoked at runtime, drives Spectre.Console prompts to populate the class. Attribute properties on the user side map 1:1 to fluent calls on the Spectre.Console prompt builder (e.g. `[SelectPrompt(CancelResult = ...)]` → `.AddCancelResult(...)`).

## Key directories

- `Src/AutoSpectre/` — public attribute classes (consumer-facing).
- `Src/AutoSpectre.SourceGeneration/` — the generator.
  - `BuildContexts/` — emit generated C# for each prompt type.
  - `Evaluation/` — "confirmed" state models after validation (e.g. `ConfirmedDefaultValue`).
- `Src/AutoSpectre.SourceGeneration.Tests/` — generator tests; base class provides `GetOutput(code).OutputShouldContain(...)` / `.ShouldHaveSourceGeneratorDiagnosticOnlyOnce(...)`.
- `ReleaseNotes.md`, `README.md` — user-facing docs at repo root.

## Adding a new attribute property (worked example: `SelectPromptAttribute.DefaultValue`)

Wrapping a new upstream Spectre.Console feature means touching ~8 source files plus tests and docs, in this order:

### 1. Attribute class — `Src/AutoSpectre/SelectPromptAttribute.cs`

Add the property with an XML doc. Name convention on `SelectPromptAttribute` drops the `...Source` suffix (match siblings like `CancelResult`, not `TextPromptAttribute.DefaultValueSource`):

```csharp
/// <summary>
/// Name of a member (parameterless method, property, or field) whose value becomes the
/// pre-highlighted item when the prompt is shown. Convention: {PropertyName}DefaultValue.
/// </summary>
public string? DefaultValue { get; set; }
```

### 2. Name constant — `Src/AutoSpectre.SourceGeneration/AutoSpectreNames.cs`

Inside `SelectPromptAttributeNames` (or `TextPromptAttributeNames`):

```csharp
public const string DefaultValue = "DefaultValue";
```

### 3. Data carrier — `Src/AutoSpectre.SourceGeneration/TranslatedMemberAttributeData.cs`

If the field already exists (e.g. `DefaultValue` is shared with `TextPrompt`), just extend the factory. Otherwise add a property on the class.

```csharp
public static TranslatedMemberAttributeData SelectPrompt(
    /* existing params ... */,
    string? defaultValue)
{
    return new(/* ... */)
    {
        /* existing initializers ... */,
        DefaultValue = defaultValue
    };
}
```

### 4. Attribute parsing — `Src/AutoSpectre.SourceGeneration/StepWithAttributeData.cs`

In the branch matching the target attribute:

```csharp
var defaultValue =
    attributeData.GetAttributePropertyValue<string?>(SelectPromptAttributeNames.DefaultValue) ?? null;
// ... then pass defaultValue into the factory call
```

### 5. Evaluator — `Src/AutoSpectre.SourceGeneration/StepContextBuilderOperation.cs`

Find the `AskTypeCopy.Selection` (or `.Normal` for TextPrompt) branch and call your evaluator there. For member-name lookups, reuse the existing machinery:

- `EvaluateCancelResult` — type-exact match; separate `Confirmed*` state.
- `EvaluateDefaultValue` — uses `propertyContext.GetSingleType().type` (returns element type for enumerables, property type otherwise), so the same evaluator works for single-select and multi-select.

If you can reuse an existing evaluator, call it directly — no new method required:

```csharp
EvaluateDefaultValue(propertyContext, memberAttributeData);
```

The `Confirmed*` state is stored on `SinglePropertyEvaluationContext` (`Src/AutoSpectre.SourceGeneration/SinglePropertyEvaluationContext.cs`).

### 6. Emission — `Src/AutoSpectre.SourceGeneration/BuildContexts/`

For select prompts, shared emitters live on `SelectionBaseBuildContext`. Add a protected `Generate*` that returns either the fluent call or `string.Empty`:

```csharp
protected string GenerateDefaultValue()
{
    if (Context.ConfirmedDefaultValue is { } confirmed)
    {
        var prepend = GetStaticOrInstancePrepend(!confirmed.Instance); // polarity flip — see gotcha
        var suffix = confirmed.Type == DefaultValueType.Method ? "()" : string.Empty;
        return $".DefaultValue({prepend}.{confirmed.Name}{suffix})";
    }
    return string.Empty;
}
```

Then chain it into `PromptPart` in both `SelectionPromptBuildContext.cs` and `MultiSelectionBuildContext.cs`. Place calls in logical prompt-config order (title → page/wrap → search → defaults → cancel → choices).

`GetStaticOrInstancePrepend(bool isStatic)` returns `form.` for instance members and the fully-qualified target type name for static members.

### 7. Tests — `Src/AutoSpectre.SourceGeneration.Tests/`

Template: `CancelResultTests.cs` or the new `DefaultValueSelectTests.cs`. Minimum coverage:

- Convention match (`{PropertyName}DefaultValue`) with a theory across property/method/field × instance/static.
- Explicit override (`[SelectPrompt(DefaultValue = nameof(X))]`).
- Enumerable property variant (for select/multi features).
- Missing explicit source → assert the expected diagnostic ID fires once.
- Convention-miss → assert no diagnostic.

### 8. Docs

- `ReleaseNotes.md` — add a bullet under the unreleased version.
- `README.md` — add a subsection under the relevant attribute. Link diagnostic IDs by name (`AutoSpectre_JJK0xx`).

### 9. Diagnostics — `Src/AutoSpectre.SourceGeneration/DiagnosticIds.cs`

Reuse existing IDs when the semantic already fits (e.g. `Id0025_DefaultValueSource_NotFound` covers "named source member missing" across both Text and Select prompts). Only add a new ID when you need a genuinely new failure mode. New IDs are sequential (`JJK031`, `JJK032`, ...).

## Gotchas

- **Polarity flip**: `ConfirmedCancelResult.IsStatic` (bool) vs `ConfirmedDefaultValue.Instance` (bool) are inverses. `GetStaticOrInstancePrepend` expects `isStatic`, so from `ConfirmedDefaultValue` pass `!d.Instance`.
- **`ConfirmedDefaultValue` ctor takes a `style` param but does not store it** — see `Evaluation/ConfirmedDefaultValue.cs`. Harmless but surprising.
- **`EvaluateDefaultValue` reads `memberAttributeData.DefaultValueStyle`**. When called from prompt types that don't expose a style (e.g. SelectPrompt), the field stays null and the style branch silently no-ops — safe to reuse across prompt types.
- **There is no `MultiSelectPromptAttribute`.** `SelectPromptAttribute` covers both; the property being enumerable at codegen time decides which `BuildContext` is used.
- **Upstream multi-select semantics for `DefaultValue`**: `MultiSelectionPrompt<T>.DefaultValue(T)` takes a single `T` and only *highlights* one item — it does not pre-select a collection. Document this on any multi-select default-value feature.
- **`EditableDefaultValue` is TextPrompt-only.** Don't call `EvaluateEditableDefaultValue` from the Selection branch.
- **Naming split between attributes**: `TextPromptAttribute` uses `...Source` suffixes (`DefaultValueSource`, `ChoicesSource`) for member-name properties; `SelectPromptAttribute` drops the suffix (`CancelResult`, `Source`, `DefaultValue`). When in doubt, match the sibling properties on the same attribute class.

## End-to-end sanity check

After implementing, the quickest confidence check is to build a minimal form in a sample consumer, inspect the generated `.g.cs`, and grep for the new fluent call:

```csharp
[AutoSpectreForm]
public class Demo
{
    [SelectPrompt(Source = nameof(Items))]
    public string Pick { get; set; } = "";

    public List<string> Items => new() { "a", "b", "c" };
    public string PickDefaultValue => "b";
}
```

Expected output in the generated source: `.DefaultValue(form.PickDefaultValue)` in the `new SelectionPrompt<string>()` fluent chain.
