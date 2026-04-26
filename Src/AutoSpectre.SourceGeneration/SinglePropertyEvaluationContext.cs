using System;
using System.Collections.Generic;
using AutoSpectre.SourceGeneration.Evaluation;
using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoSpectre.SourceGeneration;

/// <summary>
/// Represents the evaluation context for a single method during source generation analysis.
/// Encapsulates method metadata, async/task semantics, and optional status/spinner configuration
/// needed to generate a task step invocation for a user interaction flow.
/// Implements <see cref="IConditionContext"/> to support conditional step execution.
/// </summary>
public class SingleMethodEvaluationContext : IConditionContext
{
    /// <summary>The method symbol this context describes.</summary>
    public IMethodSymbol Method { get; }

    /// <summary>True if the method returns <see cref="System.Threading.Tasks.Task"/> or <see cref="System.Threading.Tasks.Task{TResult}"/>.</summary>
    public bool ReturnTypeIsTask { get; }

    /// <summary>True if the method accepts an <c>IAnsiConsole</c> parameter that the generator must pass through.</summary>
    public bool HasAnsiConsoleParameter { get; }

    /// <summary>True when the method is rendered as a task step (e.g. wrapped in a Spectre status/progress block).</summary>
    public bool IsTaskStep { get; private set; }

    private Lazy<MethodDeclarationSyntax> _methodSyntaxLazy;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleMethodEvaluationContext"/> class.
    /// </summary>
    /// <param name="method">The method symbol being analyzed.</param>
    /// <param name="returnTypeIsTask">Whether the method returns <see cref="System.Threading.Tasks.Task"/>.</param>
    /// <param name="hasAnsiConsoleParameter">Whether the method has an <c>IAnsiConsole</c> parameter.</param>
    /// <param name="isTaskStep">Whether the method is treated as a task step in the generated flow.</param>
    public SingleMethodEvaluationContext(IMethodSymbol method, bool returnTypeIsTask, bool hasAnsiConsoleParameter, bool isTaskStep)
    {
        Method = method;
        ReturnTypeIsTask = returnTypeIsTask;
        HasAnsiConsoleParameter = hasAnsiConsoleParameter;
        IsTaskStep = isTaskStep;
        _methodSyntaxLazy = new Lazy<MethodDeclarationSyntax>(() =>
            Method.DeclaringSyntaxReferences[0].GetSyntax() as MethodDeclarationSyntax ?? throw new InvalidOperationException());
    }

    /// <summary>
    /// Gets or sets the confirmed status (spinner text wrapper) used while the method executes.
    /// </summary>
    public ConfirmedStatusWrap? ConfirmedStatus { get; set; }

    /// <summary>
    /// Gets the method's declaration syntax resolved lazily from the symbol's syntax references.
    /// </summary>
    public MethodDeclarationSyntax MethodSyntax => _methodSyntaxLazy.Value;

    /// <summary>
    /// Gets or sets the style string applied to the spinner shown during execution.
    /// </summary>
    public string? SpinnerStyle { get; set; }

    /// <summary>
    /// Gets or sets the known spinner type name (e.g., a Spectre.Console spinner preset).
    /// </summary>
    public string? SpinnerKnownType { get; set; }

    string IConditionContext.Name => Method.Name;
    ISymbol IConditionContext.Symbol => Method;

    /// <summary>
    /// Gets or sets the optional condition that determines whether this method step is executed.
    /// </summary>
    public ConfirmedCondition? ConfirmedCondition { get; set; }

    public bool IsPartial => Method.IsPartial();

    public IEnumerable<object?> Confirmations => new List<object?>()
    {
        ConfirmedCondition, ConfirmedStatus
    };
}

/// <summary>
/// Wraps a confirmed status text that is displayed (typically with a spinner) while
/// a task step executes.
/// </summary>
public class ConfirmedStatusWrap
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmedStatusWrap"/> class.
    /// </summary>
    /// <param name="statusText">The status text to display while the step executes.</param>
    public ConfirmedStatusWrap(string statusText)
    {
        StatusText = statusText;
    }

    /// <summary>
    /// Gets the status text shown to the user during execution.
    /// </summary>
    public string StatusText { get; }
    
}

/// <summary>
/// Common shape for any evaluation context (property or method) that can carry a
/// conditional-execution flag, allowing the generator to share condition-handling logic.
/// </summary>
public interface IConditionContext
{
    /// <summary>
    /// Gets the name of the underlying member.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the underlying symbol that this context describes.
    /// </summary>
    public ISymbol Symbol { get; }

    /// <summary>
    /// Gets or sets the optional condition that gates the member's participation in the flow.
    /// </summary>
    public ConfirmedCondition? ConfirmedCondition { get; set; }
    
    public bool IsPartial { get; }
}

/// <summary>
/// Represents shared evaluation context for an entire generated form, including culture settings
/// and the constructor used to instantiate the target type.
/// </summary>
public class SingleFormEvaluationContext
{
    /// <summary>
    /// Gets or sets the confirmed culture used when formatting or parsing values in the form.
    /// </summary>
    public ConfirmedCulture? ConfirmedCulture { get; set; }
    //public bool DisableDumpMethod { get; set; }

    /// <summary>
    /// Gets or sets the constructor that is used to instantiate the target type for the form.
    /// </summary>
    public IMethodSymbol? UsedConstructor { get; set; }
}

/// <summary>
/// Represents a confirmed culture identifier used when formatting or parsing values during prompting.
/// </summary>
public class ConfirmedCulture
{
    /// <summary>
    /// Gets the culture identifier (e.g., "en-US").
    /// </summary>
    public string Culture { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmedCulture"/> class.
    /// </summary>
    /// <param name="culture">The culture identifier.</param>
    public ConfirmedCulture(string culture)
    {
        Culture = culture;
    }
}

/// <summary>
/// Represents the evaluation context for a single property during source generation analysis.
/// This context encapsulates all information needed to generate prompt code for a property,
/// including type information, nullability, validators, converters, and UI configuration.
/// Implements <see cref="IConditionContext"/> to support conditional property rendering.
/// </summary>
/// <remarks>
/// This class serves as a comprehensive container for property metadata and configuration
/// gathered during the source generation process. It tracks whether the property requires
/// async operations, handles both simple and enumerable types, and maintains references
/// to confirmed sources for selections, validators, converters, and other prompt behaviors.
/// The context is used throughout the generation pipeline to make decisions about how to
/// generate appropriate prompt code for user input collection.
/// </remarks>
public class SinglePropertyEvaluationContext : IConditionContext
{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    /// <summary>
    /// A sentinel empty context instance used where a non-null reference is required but no real
    /// property is available (e.g., placeholder construction paths).
    /// </summary>
    public static SinglePropertyEvaluationContext Empty = new(default(IPropertySymbol),
        false, default(ITypeSymbol), false, default(ITypeSymbol), default(INamedTypeSymbol));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    
    private Lazy<PropertyDeclarationSyntax?> _propertySyntaxLazy;

    /// <summary>
    /// Gets a value indicating whether the property requires async code generation,
    /// based on whether the confirmed named-type source reports itself as async.
    /// </summary>
    public bool RequiresAsync => ConfirmedNamedTypeSource?.IsAsync ?? false;

    /// <summary>
    /// Initializes a new instance of the <see cref="SinglePropertyEvaluationContext"/> class.
    /// </summary>
    /// <param name="property">The property symbol being analyzed.</param>
    /// <param name="isNullable">Whether the property type is nullable.</param>
    /// <param name="type">The property's type, with nullability stripped.</param>
    /// <param name="isEnumerable">Whether the property type is an enumerable (non-string).</param>
    /// <param name="underlyingType">The underlying element type when <paramref name="isEnumerable"/> is true.</param>
    /// <param name="targetType">The containing target type the property belongs to.</param>
    public SinglePropertyEvaluationContext(IPropertySymbol property, bool isNullable, ITypeSymbol type, bool isEnumerable, ITypeSymbol? underlyingType, INamedTypeSymbol targetType)
    {
        Property = property;
        IsNullable = isNullable;
        Type = type;
        IsEnumerable = isEnumerable;
        TargetType = targetType;
        if (underlyingType is { })
        {
            var (underlyingCanBeNull, underlying) = underlyingType.GetTypeWithNullableInformation();
            UnderlyingType = underlying;
            UnderlyingTypeIsNullable = underlyingCanBeNull;
        }
        
        _propertySyntaxLazy = new Lazy<PropertyDeclarationSyntax?>(() =>
            Property.DeclaringSyntaxReferences[0].GetSyntax() as PropertyDeclarationSyntax);
    }

    /// <summary>
    /// Gets a value indicating whether the underlying (element) type of an enumerable property is nullable.
    /// </summary>
    public bool UnderlyingTypeIsNullable { get;  }

    /// <summary>
    /// Gets the property symbol that this context describes.
    /// </summary>
    public IPropertySymbol Property { get; }

    string IConditionContext.Name => Property.Name;
    ISymbol IConditionContext.Symbol => Property;


    /// <summary>
    /// Gets or sets a value indicating whether the property type is nullable.
    /// </summary>
    public bool IsNullable { get; internal set; }

    /// <summary>
    /// Gets the property's type (with nullability stripped to the underlying declared type).
    /// </summary>
    public ITypeSymbol Type { get; }

    /// <summary>
    /// Gets a value indicating whether the property is an enumerable (non-string) collection.
    /// </summary>
    public bool IsEnumerable { get; }

    /// <summary>
    /// Gets the type that contains this property (the form's target type).
    /// </summary>
    public INamedTypeSymbol TargetType { get; }

    /// <summary>
    /// Gets the element type when the property is enumerable; otherwise <c>null</c>.
    /// </summary>
    public ITypeSymbol? UnderlyingType { get; }
    
    public ITypeSymbol RequiredUnderlyingType => UnderlyingType ?? throw new InvalidOperationException("This should not be null");

    /// <summary>
    /// Gets or sets the confirmed selection source that provides choices for this property.
    /// </summary>
    public ConfirmedSelectionSource? ConfirmedSelectionSource { get; set; }

    /// <summary>
    /// Gets or sets the confirmed named-type source associated with this property.
    /// </summary>
    public ConfirmedNamedTypeSource? ConfirmedNamedTypeSource { get; set; }

    /// <summary>
    /// Gets or sets the confirmed converter used to transform user input into the property's type.
    /// </summary>
    public ConfirmedConverter? ConfirmedConverter { get; set; }

    /// <summary>
    /// Gets or sets the confirmed validator that validates the entered value for this property.
    /// </summary>
    public ConfirmedValidator? ConfirmedValidator { get; set; }

    /// <summary>
    /// Gets or sets the condition that determines whether this property is prompted.
    /// </summary>
    public ConfirmedCondition? ConfirmedCondition { get; set; }

    /// <summary>
    /// Gets or sets the confirmed default value source used to pre-fill the prompt.
    /// </summary>

    // <inheritdoc />
    public bool IsPartial => Property.IsPartial();

    /// <summary>Resolved default value supplied to the prompt.</summary>
    public ConfirmedDefaultValue? ConfirmedDefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the confirmed default style applied to the default value display.
    /// </summary>
    public ConfirmedDefaultStyle? ConfirmedDefaultStyle { get; set; }

    /// <summary>
    /// Gets or sets the confirmed choices set (the finite list of valid values for selection prompts).
    /// </summary>
    public ConfirmedChoices? ConfirmedChoices { get; set; }

    /// <summary>
    /// Gets the property's declaration syntax resolved lazily from the symbol's syntax references.
    /// </summary>
    public PropertyDeclarationSyntax? PropertySyntax => _propertySyntaxLazy.Value;

    /// <summary>
    /// Gets or sets the Spectre.Console style string applied to the prompt text.
    /// </summary>
    public string? PromptStyle { get; set; }

    /// <summary>
    /// Gets or sets the page size used for paginated selection prompts.
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether selection prompts wrap around at the ends of the list.
    /// </summary>
    public bool? WrapAround { get; set; }

    /// <summary>
    /// Gets or sets the text shown to indicate that more choices are available beyond the current page.
    /// </summary>
    public string? MoreChoicesText { get; set; }

    /// <summary>
    /// Gets or sets the instructional text displayed with the prompt.
    /// </summary>
    public string? InstructionsText { get; set; }

    /// <summary>
    /// Gets or sets the Spectre.Console style applied to the highlighted selection.
    /// </summary>

    /// <summary>Optional Spectre style applied to the highlighted item in selection prompts.</summary>
    public string? HighlightStyle { get; set; } //public NamedTypedAnalysis? NamedTypeAnalysis { get; set; }

    /// <summary>
    /// Gets or sets the confirmed choice style (e.g., prompt vs. selection) configured for this property.
    /// </summary>
    public ConfirmedChoiceStyle ConfirmedChoicesStyle { get; set; }

    /// <summary>
    /// Gets or sets the confirmed search-enabled configuration for selection prompts.
    /// </summary>
    public ConfirmedSearchEnabled? ConfirmedSearchEnabled { get; set; }

    /// <summary>
    /// Gets or sets the confirmed cancel-result configuration that defines behavior when the user cancels.
    /// </summary>
    public ConfirmedCancelResult? ConfirmedCancelResult { get; set; }
    /// <summary>
    /// All "Confirmed*" attribute-derived settings on this context, in declaration order.
    /// May contain nulls for entries that were not set.
    /// </summary>
    public IEnumerable<object?> Confirmations => new List<object?>()
    {
        ConfirmedSelectionSource,
        ConfirmedNamedTypeSource,
        ConfirmedConverter,
        ConfirmedValidator,
        ConfirmedCondition,
        ConfirmedDefaultValue,
        ConfirmedDefaultStyle,
        ConfirmedChoices,
        ConfirmedChoicesStyle,
        ConfirmedSearchEnabled,
        ConfirmedCancelResult,
        ConfirmedClearOnFinish
    };

    public ConfirmedClearOnFinish? ConfirmedClearOnFinish { get; set; }


    public IEnumerable<object> GetConfirmedSources()
    {
        if (ConfirmedCancelResult is { } cancelResult)
        {
            yield return cancelResult;
        }
    }
    
    
    /// <summary>
    /// Creates a new <see cref="SinglePropertyEvaluationContext"/> from the given property symbol,
    /// deriving nullability and enumerable/underlying type information from the property's type.
    /// </summary>
    /// <param name="property">The property to build the context for.</param>
    /// <param name="namedTypeSymbol">The target type that contains the property.</param>
    /// <returns>A fully initialized evaluation context for the property.</returns>
    public static SinglePropertyEvaluationContext GenerateFromPropertySymbol(IPropertySymbol property,
        INamedTypeSymbol namedTypeSymbol)
    {
        var (nullable, originalType) = property.Type.GetTypeWithNullableInformation();
        var (enumerable, underlying) = property.Type.IsEnumerableOfTypeButNotString();

        var propertyEvaluationContext =
            new SinglePropertyEvaluationContext(property: property, isNullable: nullable, type: originalType,
                isEnumerable: enumerable, underlyingType: underlying, namedTypeSymbol);
        return propertyEvaluationContext;
    }

    /// <summary>
    ///  Returns the Type is the type is not an enumerable or the underlying
    /// type is it is an enumerable
    /// </summary>
    public (bool nullable, ITypeSymbol type) GetSingleType() =>
        IsEnumerable
            ? (UnderlyingTypeIsNullable,
                UnderlyingType ?? throw new InvalidOperationException("This should not be null"))
            : (IsNullable, Type);

}