# Releases

## 0.14.0

* **Breaking:** Dropped `netstandard2.0`. `AutoSpectre` now targets `net8.0` and `net10.0` only.
* Added partial-class summary generation. When an `[AutoSpectreForm]` class is declared `partial` and contains `partial` properties (decorated with `TextPrompt`/`SelectPrompt`) or `partial` methods (decorated with `TaskStep`/`Break`), the generator emits an additional `*PartialSummaries.g.cs` file alongside the factory. That file contains the partial member declarations with XML `<summary>` documentation derived from each step's configuration (title, condition, default value, choices, cancel result, clear-on-finish, search, etc.), so IntelliSense surfaces the prompt behavior at the call site. Method bodies are stripped — only the declaration is emitted.

## 0.13.0

* Upgraded `Spectre.Console` dependency to 0.55.2.
* Added support for new CancelResult in AutoSpectre
* Added support for `DefaultValue` on `SelectPromptAttribute`. Points at a public member (method/property/field, instance or static) whose value pre-highlights an item when the prompt is shown. Convention: `{PropertyName}DefaultValue` is picked up automatically. Works for both single- and multi-select; the source returns the element type (upstream `MultiSelectionPrompt<T>.DefaultValue(T)` highlights one item, it does not pre-select a collection). Maps to Spectre.Console 0.55.2's `SelectionPrompt<T>.DefaultValue` / `MultiSelectionPrompt<T>.DefaultValue`.
* Added support for `EditableDefaultValue` on `TextPromptAttribute`. When set alongside a `DefaultValueSource`, the default is pre-injected into the input field so the user can edit it instead of retyping. Emits diagnostic `AutoSpectre_JJK030` when used without a resolvable default or on a bool property (which becomes a `ConfirmationPrompt`).
* Added support for `ClearOnFinish` on `TextPromptAttribute`
* Fixed bug where a form class declared outside any namespace (e.g. in a top-level/file-based program) produced invalid generated code containing `namespace <global namespace>`. The generator now emits the factory at top level and skips the `using <namespace>;` directive in the partial `SpectreFactory`.

## 0.12.0

* IMPORTANT: From this version and forward. The AutoSpectre.SourceGeneration package will be integrated directly into AutoSpectre, so you only need to install AutoSpectre. In a near future the AutoSpectre.SourceGeneration nuget package will be made obsolete
* AutoSpectre now target .netStandard, net8.0 and net10.0
* Rewrote Analyzers to be included directly in the AutoSpectre package. 
  * Added a new analyzer to check for empty AutoSpectreForm classes (classes decorated with the AutoSpectreForm attribute that does not have any step attributes)

## 0.11.0

* Added the BreakAttribute to allow for breaking the prompt flow

## 0.10.0

### AutoSpectre

* Interfaces `ISpectreFactory<T>` and `IAutoSpectreFactory<T>` has been added to add a shared interface for the generated classes.
* Added `SpectreFactory`as an extension point for source generation
* Added some extensions methods for creating a prompt from a ISpectreFactory
* Removed `DisableDump`

### AutoSpectre.SourceGeneration

* The logic for generating the implementation has been changed so it always required to call `Prompt` or `PromptAsync` with a not null input
* Removed support for `Dump` extensions methods and prompt extension methods.
* Much helper logic for generating prompt has been moved from custom extensions methods to the `SpectreFactory` class (introducing both non extension methods and extension methods).

## 0.9.0

### AutoSpectre.SourceGeneration

* Added support for the EnableSearch
* Removed AskAttribute
* Made SpectreDump method obsolete as better alternatives exist.

## 0.8.0

### AutoSpectre

### AutoSpectre.SourceGeneration

* An extension method is now exposed so it's possible to call the Prompt method on an instance without having to
instantiate the factory class.
* An extension method will be generated per decorated class to allow to Dump the values to the console.
* The dump method can be deactiveated
* Added support for defining DefaultValueStyle and ChoiceStyle on bool values decorated with the [TextPrompt] attribute. (this will generate a ConfirmationPrompt in Spectre)

## 0.7.0

### AutoSpectre

* TextPrompt has three new properties for defining choices.
* DefaultValueSource added on TextPrompt

### AutoSpectre.SourceGeneration

* Based on the new properties in TextPrompt, the source generation has been updated to support these new properties.
* New solution for defining DefaultValueSource. Old way discarded.
* Fixed double diagnostics if source was missing
* Added support for defining Sources that are Static. And allow them to be fields
    1. Source on SelectPrompt now supports fields and static
    2. Converter on SelectPrompt now supports static method
    3. Validator on TextPrompt now supports static method.
    4. TypeInitializer on TextPrompt now supports static method.
    5. Choices on TextPrompt now supports static and has added fields

## 0.6.0

### Autospectre

* Added an extended Text prompt to allow for passing in a delegate to convert from string to the given type of the prompt
* Added extension method to allow setting the culture "fluently" used by source generation
* Added possibility to add custom extra text to TextPrompt title.

### AutoSpectre.SourceGeneration

* Fix to allow inheritance for form classes (classes decorated with AutoSpectreForm)
* Allow setting culture to use for source generation. The culture is initalized as a culture variable. Per default it will be the CurrentUICulture
* Added ability to point to a method to initialize anohter referenced form. This is useful if that class has a constructor that is not empty.


