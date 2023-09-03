using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AutoSpectre.SourceGeneration.BuildContexts;
using AutoSpectre.SourceGeneration.Extensions;
using AutoSpectre.SourceGeneration.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoSpectre.SourceGeneration;

public class NamedTypedAnalysis
{
    
    
    public NamedTypedAnalysis(INamedTypeSymbol namedTypeSymbol, bool isDecoratedWithValidAutoSpectreForm, bool hasAnyAsyncDecoratedMethods, bool hasEmptyConstructor)
    {
        NamedTypeSymbol = namedTypeSymbol;
        IsDecoratedWithValidAutoSpectreForm = isDecoratedWithValidAutoSpectreForm;
        HasAnyAsyncDecoratedMethods = hasAnyAsyncDecoratedMethods;
        HasEmptyConstructor = hasEmptyConstructor;
    }

    public INamedTypeSymbol NamedTypeSymbol { get; }
    
    /// <summary>
    /// This return true if the given type is decorated with AutoSpectreForm and has at least
    /// one property or method decorated with a relevant attribute. It does not do a fuller analysis than
    /// that. So there can be unique scenarios where a factory has not been generated for the given type but
    /// this still is set to true. 
    /// </summary>
    public bool IsDecoratedWithValidAutoSpectreForm { get; }
    public bool HasAnyAsyncDecoratedMethods { get; }
    public bool HasEmptyConstructor { get; }
}

internal class StepContextBuilderOperation
{
    private readonly ConcurrentDictionary<INamedTypeSymbol, NamedTypedAnalysis> _analysedTypes =
        new (SymbolEqualityComparer.Default);
    
    public GeneratorAttributeSyntaxContext SyntaxContext { get; }
    
    /// <summary>
    /// Methods or Properties on the <see cref="TargetType"/> that are
    /// candidates to have a prompt or task executed.
    /// </summary>
    public IReadOnlyList<StepWithAttributeData> StepCandidates { get; }
    
    /// <summary>
    /// The target type (the type that has been decoreated with the AutoSpectreFormAttribute
    /// </summary>
    public INamedTypeSymbol TargetType { get; }
    public SourceProductionContext ProductionContext { get; }

    public LazyTypes Types { get; }

    internal StepContextBuilderOperation(
        GeneratorAttributeSyntaxContext syntaxContext,
        IReadOnlyList<StepWithAttributeData> stepCandidates,
        INamedTypeSymbol targetType, SourceProductionContext productionContext)
    {
        SyntaxContext = syntaxContext;
        StepCandidates = stepCandidates;
        TargetType = targetType;
        ProductionContext = productionContext;
        Types = new(SyntaxContext.SemanticModel.Compilation);
    }

    public static List<IStepContext> GetStepContexts(
        GeneratorAttributeSyntaxContext syntaxContext,
        IReadOnlyList<StepWithAttributeData> candidates,
        INamedTypeSymbol targetNamedType,
        SourceProductionContext productionContext)
    {
        StepContextBuilderOperation operation = new(syntaxContext, candidates, targetNamedType, productionContext);
        return operation.GetStepContexts();
    }

    public List<IStepContext> GetStepContexts()
    {
        var types = new LazyTypes(SyntaxContext.SemanticModel.Compilation);

        List<IStepContext> stepContexts = new();

        foreach (var (property, method, attributeData) in StepCandidates)
        {
            if (property is { })
            {
                HandleProperty(property, attributeData, stepContexts, types);
            }
            else if (method is { })
            {
                HandleMethod(method, attributeData, stepContexts,types);
            }
            else
            {
                throw new InvalidOperationException($"Both {nameof(property)} and {nameof(method)} are unexpectedly null");
            }
        }

        return stepContexts;
    }

    private void HandleMethod(IMethodSymbol method, TranslatedAttributeData attributeData, List<IStepContext> stepContexts, LazyTypes types)
    {


        if (EvaluateSingleMethodEvaluationContext(method) is not {} singleMethodEvaluationContext)
        {
            return;
        }

        EvaluateCondition(singleMethodEvaluationContext, attributeData);

        EvaluateStatus(attributeData, singleMethodEvaluationContext);



        stepContexts.Add(new MethodContext(singleMethodEvaluationContext,
            new TaskStepBuildContext(attributeData.Title, singleMethodEvaluationContext)));
    }

    private void EvaluateStatus(TranslatedAttributeData attributeData, SingleMethodEvaluationContext singleMethodEvaluationContext)
    {
        if (attributeData.UseStatus == false)
            return;

        var methodWalker = new MethodWalker(SyntaxContext.SemanticModel);
        methodWalker.Visit(singleMethodEvaluationContext.MethodSyntax);
        if (methodWalker.HasPrompting)
        {
            ProductionContext.ReportDiagnostic(Diagnostic.Create(
                new(DiagnosticIds.Id0017_TaskStepStatusWithPrompting,
                    "When using status. Prompting is not allowed. It appears that the method body does this",
                    $"When using status. Prompting is not allowed. It appears that the method body does this",
                    "General", DiagnosticSeverity.Warning, true),
                singleMethodEvaluationContext.Method.Locations.FirstOrDefault()));
        }


        if (attributeData.StatusText is { })
        {
            singleMethodEvaluationContext.ConfirmedStatus = new ConfirmedStatusWrap(attributeData.StatusText);
            EvaluateSpinnerStyle(attributeData, singleMethodEvaluationContext);
            EvaluateSpinnerType(attributeData, singleMethodEvaluationContext);

        }
        else
        {
            ProductionContext.ReportDiagnostic(Diagnostic.Create(
                new(DiagnosticIds.Id0018_StatusTextIsRequired,
                    "StatusText must be set when UseStatus has been set to true",
                    $"StatusText must be set when UseStatus has been set to true",
                    "General", DiagnosticSeverity.Error, true),
                singleMethodEvaluationContext.Method.Locations.FirstOrDefault()));
        }
    }

    private void EvaluateSpinnerType(TranslatedAttributeData attributeData, SingleMethodEvaluationContext singleMethodEvaluationContext)
    {
        if (attributeData.SpinnerType is { })
        {
            singleMethodEvaluationContext.SpinnerKnownType = attributeData.SpinnerType.ToString();
        }
    }

    private void EvaluateSpinnerStyle(TranslatedAttributeData attributeData, SingleMethodEvaluationContext singleMethodEvaluationContext)
    {
        if (attributeData.SpinnerStyle is { } style)
        {
            if (style.EvaluateStyle())
            {
                singleMethodEvaluationContext.SpinnerStyle = style;
            }
            else
            {
                ProductionContext.ReportDiagnostic(Diagnostic.Create(new (DiagnosticIds.Id0019_SpinnerStyleNotValid,
                        "The Spinnerstyle is not correct",
                        "The SpinnerStyle is not correct",
                        "General",
                        DiagnosticSeverity.Warning,
                        true), singleMethodEvaluationContext.Method.Locations.FirstOrDefault()));
            }
        }
        
    }

    /// <summary>
    /// Evaluates the signature of the method. If it is incorrect a diagnostic is reported
    /// and null is returned
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private SingleMethodEvaluationContext? EvaluateSingleMethodEvaluationContext(IMethodSymbol method)
    {
        var typesByMetadataName =
            this.SyntaxContext.SemanticModel.Compilation.GetTypesByMetadataName("System.Threading.Tasks.Task");
        if (typesByMetadataName.Length == 0)
        {
            throw new InvalidOperationException("No types found");
        }

        // Check if the method is void or returning a Task. We will accept with or without async
        if (method.ReturnsVoid || method.ReturnType.Equals(this.Types.Task, SymbolEqualityComparer.Default))
        {
            bool validParameters = method.Parameters.Length == 0 || (method.Parameters.Length == 1 &&
                                                                     method.Parameters[0].Type
                                                                         .Equals(Types.IAnsiConsole,
                                                                             SymbolEqualityComparer.Default));
            if (validParameters)
            {
                var hasAnsiConsole = method.Parameters.FirstOrDefault() is { };
                var isAsync = method.ReturnType.Equals(this.Types.Task, SymbolEqualityComparer.Default);
                return new SingleMethodEvaluationContext(method, isAsync, hasAnsiConsole);
                
            }
            else
            {
                ProductionContext.ReportDiagnostic(Diagnostic.Create(
                    new(DiagnosticIds.Id0015_IncorrectMethodType,
                        "The method decorated with the TaskStepAttribute does not meet the requiremnts ",
                        $"The return type must be void or a Task. IAnsiConsole is allowed a parameter, but can be omitted",
                        "General", DiagnosticSeverity.Error, true),
                    method.Locations.FirstOrDefault()));
            }
        }
        else
        {
            ProductionContext.ReportDiagnostic(Diagnostic.Create(
                new(DiagnosticIds.Id0016_TaskStepMethodInvalidReturnType,
                    "The return type of the method must be either void or Task",
                    $"The return type was {method.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)} but only void or Task are allowed",
                    "General", DiagnosticSeverity.Error, true),
                method.Locations.FirstOrDefault()));
        }

        return null;
    }

    private void HandleProperty(IPropertySymbol property, TranslatedAttributeData attributeData, List<IStepContext> stepContexts,
        LazyTypes types)
    {
        var propertyContext = SinglePropertyEvaluationContext.GenerateFromPropertySymbol(property);

        EvaluateCondition(propertyContext, attributeData);
       

        if (attributeData.AskType == AskTypeCopy.Normal)
        {
            EvaluateNamedType(propertyContext, attributeData);
            EvaluateDefaultValue(propertyContext, attributeData);
            EvaluatePromptStyle(propertyContext, attributeData);
            EvaluateValidation(propertyContext, attributeData);

            if (!propertyContext.IsEnumerable)
            {
                if (GetTextPromptBuildContext(attributeData, propertyContext) is
                    { } promptBuildContext)
                {
                    stepContexts.Add(new PropertyContext(property.Name, property,
                        promptBuildContext));
                }
            }
            else
            {
                if (GetTextPromptBuildContext(attributeData, propertyContext) is
                    { } promptBuildContext)
                {
                    stepContexts.Add(new PropertyContext(property.Name,
                        property,
                        new MultiAddBuildContext(propertyContext.Type,
                            propertyContext.GetSingleType().type,
                            types,
                            promptBuildContext,
                            propertyContext)));
                }
            }
        }

        if (attributeData.AskType == AskTypeCopy.Selection)
        {
            EvaluateSelectionSource(attributeData, propertyContext);
            EvaluateSelectionConverter(attributeData, propertyContext);
            EvaluatePageSize(attributeData, ref propertyContext);
            EvaluateWrapAround(attributeData, ref propertyContext);
            EvaluateMoreChoicesText(attributeData, ref propertyContext);
            EvaluateInstructionText(attributeData, ref propertyContext);
            EvaluateHighlightStyle(propertyContext, attributeData);

            var selectionSource = attributeData.SelectionSource ?? $"{propertyContext.Property.Name}Source";

            var match = TargetType
                .GetMembers()
                .Where(x => x.Name == selectionSource)
                .FirstOrDefault(x => x is IMethodSymbol
                {
                    Parameters.Length: 0
                } or IPropertySymbol { GetMethod: { } });

            if (match is { })
            {
                SelectionPromptSelectionType selectionType = match switch
                {
                    IMethodSymbol => SelectionPromptSelectionType.Method,
                    IPropertySymbol => SelectionPromptSelectionType.Property,
                    _ => throw new NotSupportedException(),
                };
                if (!propertyContext.IsEnumerable)
                {
                    stepContexts.Add(new PropertyContext(property.Name, property,
                        new SelectionPromptBuildContext(attributeData.Title, propertyContext,
                            selectionSource, selectionType)));
                }
                else
                {
                    stepContexts.Add(new PropertyContext(property.Name, property,
                        new MultiSelectionBuildContext(title: attributeData.Title,
                            propertyContext,
                            selectionTypeName: selectionSource,
                            selectionType: selectionType, types)));
                }
            }
            else
            {
                ProductionContext.ReportDiagnostic(Diagnostic.Create(
                    new("AutoSpectre_JJK0005",
                        "Not a valid selection source",
                        $"The source {attributeData.SelectionSource} was not found on type",
                        "General", DiagnosticSeverity.Warning, true),
                    property.Locations.FirstOrDefault()));
            }
        }
    }
    
    private void EvaluateNamedType(SinglePropertyEvaluationContext propertyContext, TranslatedAttributeData attributeData)
    {
        var namedTypeAnalysis = EvaluateType(propertyContext);
        if (namedTypeAnalysis == null)
            return;

        var initializer = EvaluateAndReturnTypeInitializer(propertyContext, attributeData, namedTypeAnalysis);


        if (namedTypeAnalysis is { IsDecoratedWithValidAutoSpectreForm: true, HasEmptyConstructor: false } && initializer is null)
        {
            
            ProductionContext.ReportDiagnostic(Diagnostic.Create(
                new(DiagnosticIds.Id0020_InitializerNeeded, $"The type needs a method to initialize it",
                    $"The type {namedTypeAnalysis.NamedTypeSymbol.Name} decorated with AutoSpectreForm does not have an empty constructor. You need to provide a method that can initalize it",
                    "General", DiagnosticSeverity.Error, true),
                propertyContext.Property.Locations.FirstOrDefault()));
        }

        propertyContext.ConfirmedNamedTypeSource = new ConfirmedNamedTypeSource(namedTypeAnalysis, initializer);
    }

    private string? EvaluateAndReturnTypeInitializer(SinglePropertyEvaluationContext propertyContext, TranslatedAttributeData attributeData, NamedTypedAnalysis? namedTypedAnalysis)
    {
        if (attributeData.TypeInitializer is { } typeInitializer && namedTypedAnalysis is {})
        {
            var methodMatch = TargetType.GetAllMembers()
                .Where(x => x.Kind == SymbolKind.Method && x.Name == typeInitializer)
                .OfType<IMethodSymbol>()
                .FirstOrDefault(x =>
                    x.ReturnType.Equals(namedTypedAnalysis.NamedTypeSymbol,
                        SymbolEqualityComparer.Default) && x.Parameters.Length == 0);

            if (methodMatch is { })
            {
                return methodMatch.Name;
            }
        }

        return null;
    }

    private void EvaluateSelectionSource(TranslatedAttributeData attributeData,
        SinglePropertyEvaluationContext propertyContext)
    {
        var selectionSource = attributeData.SelectionSource ?? $"{propertyContext.Property.Name}Source";

        var match = TargetType
            .GetMembers()
            .Where(x => x.Name == selectionSource)
            .FirstOrDefault(x => x is IMethodSymbol
            {
                Parameters.Length: 0
            } or IPropertySymbol { GetMethod: { } });

        if (match is { })
        {
            propertyContext.ConfirmedSelectionSource = match switch
            {
                IMethodSymbol => new ConfirmedSelectionSource(selectionSource, SelectionPromptSelectionType.Method),
                IPropertySymbol => new ConfirmedSelectionSource(selectionSource, SelectionPromptSelectionType.Property),
                _ => throw new NotSupportedException(),
            };
        }
        else
        {
            ProductionContext.ReportDiagnostic(Diagnostic.Create(
                new("AutoSpectre_JJK0005",
                    "Not a valid selection source",
                    $"The selectionsource {attributeData.SelectionSource} was not found on type",
                    "General", DiagnosticSeverity.Warning, true),
                propertyContext.Property.Locations.FirstOrDefault()));
        }
    }


    // Note. It might seems extra ceremonious for these methods that just transfers the value. But it's just in
    // case they get extra "complex". They probably won't.

    private void EvaluateInstructionText(TranslatedAttributeData attributeData,
        ref SinglePropertyEvaluationContext propertyContext)
    {
        if (attributeData.InstructionsText is { })
        {
            propertyContext.InstructionsText = attributeData.InstructionsText;
        }
    }

    private void EvaluateMoreChoicesText(TranslatedAttributeData attributeData,
        ref SinglePropertyEvaluationContext propertyContext)
    {
        if (attributeData.MoreChoicesText is { })
        {
            propertyContext.MoreChoicesText = attributeData.MoreChoicesText;
        }
    }

    private void EvaluateWrapAround(TranslatedAttributeData attributeData,
        ref SinglePropertyEvaluationContext propertyContext)
    {
        if (attributeData.WrapAround is { })
        {
            propertyContext.WrapAround = attributeData.WrapAround;
        }
    }

    private void EvaluatePageSize(TranslatedAttributeData attributeData,
        ref SinglePropertyEvaluationContext propertyContext)
    {
        if (attributeData.PageSize is { } pageSize)
        {
            if (pageSize < 3)
            {
                ProductionContext.ReportDiagnostic(Diagnostic.Create(
                    new(DiagnosticIds.Id0013_PageSizeMustBeThreeOrLarger,
                        "Pagesize must be greater than or equal to 3",
                        $"PageSize {pageSize} is less than 3",
                        "General", DiagnosticSeverity.Error, true),
                    propertyContext.Property.Locations.FirstOrDefault()));
            }


            propertyContext.PageSize = attributeData.PageSize;
        }
    }

    private void EvaluatePromptStyle(SinglePropertyEvaluationContext propertyContext, TranslatedAttributeData attribute)
    {
        propertyContext.PromptStyle =
            EvaluateStyle(attribute.PromptStyle, nameof(propertyContext.PromptStyle), propertyContext);
    }

    private string? EvaluateStyle(string? style, string propertyName, SinglePropertyEvaluationContext propertyContext)
    {
        if (style == null)
            return null;

        if (style.EvaluateStyle())
            return style;
        else
        {
            ProductionContext.ReportDiagnostic(Diagnostic.Create(
                new(DiagnosticIds.Id0014_StyleNotValid,
                    "Not a valid style",
                    $"The style {style} on property {propertyName} is not a valid style",
                    "General", DiagnosticSeverity.Error, true),
                propertyContext.Property.Locations.FirstOrDefault()));
            return null;
        }
    }

    private void EvaluateHighlightStyle(SinglePropertyEvaluationContext propertyContext,
        TranslatedAttributeData attributeData)
    {
        propertyContext.HighlightStyle = EvaluateStyle(attributeData.HighlightStyle,
            nameof(attributeData.HighlightStyle), propertyContext);
    }


    /// <summary>
    /// Evaluates to see if the property has an initialized
    /// Like for instance public bool Confirm {get;set;} = true
    /// </summary>
    /// <param name="propertyContext"></param>
    /// <param name="translatedAttributeData"></param>
    /// <param name="attributeData"></param>
    private void EvaluateDefaultValue(SinglePropertyEvaluationContext propertyContext,
        TranslatedAttributeData attributeData)
    {
        var parsedDefaultValue = EvaluateStyle(attributeData.DefaultValueStyle, nameof(attributeData.DefaultValueStyle),
            propertyContext);

        if (propertyContext.PropertySyntax is { Initializer: { } equal })
        {
            // We try to catch a property initialized with for instance = "Hello", = 5, = true
            if (equal.Value is LiteralExpressionSyntax)
            {
                propertyContext.ConfirmedDefaultValue =
                    new ConfirmedDefaultValue(DefaultValueType.Literal,
                        equal.Value.ToString(),
                        parsedDefaultValue);
            }
            else if (equal.Value is IdentifierNameSyntax or InvocationExpressionSyntax
                     {
                         Expression: IdentifierNameSyntax
                     })
            {
                // Note that it can be necessary to make this more robust.
                propertyContext.ConfirmedDefaultValue =
                    new ConfirmedDefaultValue(DefaultValueType.Call,
                        equal.Value.ToString(),
                        parsedDefaultValue);
            }
            else if (equal.Value.ToString().Equals("string.empty", StringComparison.OrdinalIgnoreCase))
            {
                propertyContext.ConfirmedDefaultValue =
                    new ConfirmedDefaultValue(DefaultValueType.Literal,
                        equal.Value.ToString(),
                        parsedDefaultValue);
            }
            else
            {
                ProductionContext.ReportDiagnostic(Diagnostic.Create(
                    new(DiagnosticIds.Id0012_UnsupportedDefaultValue,
                        $"The default value defined for property {propertyContext.Property.Name} is not supported",
                        $"The supported initializers for a properties are literals (for instance 5, 70), simple public static constants, properties, fields, method invocations",
                        "General",
                        DiagnosticSeverity.Info, true),
                    propertyContext.PropertySyntax.Initializer!.GetLocation()));
            }
        }
    }

    /// <summary>
    /// Evalutes the Converter set on the attributeData. If it's correct a valid converter is set on the context.
    /// if it is set but not valid a warning is reported.
    /// </summary>
    /// <param name="attributeData"></param>
    /// <param name="context"></param>
    private void EvaluateSelectionConverter(TranslatedAttributeData attributeData,
        SinglePropertyEvaluationContext context)
    {
        bool guessed = attributeData.Converter == null;
        var converterName = attributeData.Converter ?? $"{context.Property.Name}Converter";

        bool ConverterMethodOrProperty(ISymbol symbol)
        {
            if (symbol is IMethodSymbol method)
            {
                if (method.Parameters.FirstOrDefault() is { } parameter)
                {
                    if (SymbolEqualityComparer.Default.Equals(parameter.Type, context.UnderlyingType ?? context.Type))
                    {
                        if (method.ReturnType.SpecialType == SpecialType.System_String)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        var candidates = TargetType
            .GetMembers()
            .Where(x => x.Name == converterName)
            .ToList();

        var match = candidates.FirstOrDefault(ConverterMethodOrProperty);

        if (match is { })
        {
            context.ConfirmedConverter = new ConfirmedConverter(converterName);
        }
        else if (!guessed || candidates.Count > 0)
        {
            ProductionContext.ReportDiagnostic(Diagnostic.Create(
                new("AutoSpectre_JJK0008",
                    $"Converter {attributeData.Converter} should be a method taking a {context.UnderlyingType} as input and return string on the class",
                    $"Could not find a correct method to match {attributeData.Converter} supported", "General",
                    DiagnosticSeverity.Warning, true),
                context.Property.Locations.FirstOrDefault()));
        }
    }
    
    private NamedTypedAnalysis? EvaluateType(SinglePropertyEvaluationContext evaluationContext)
    {
        if (evaluationContext.GetSingleType().type is not INamedTypeSymbol namedTypeSymbol)
            return null;

        var namedType = _analysedTypes.GetOrAdd(namedTypeSymbol, symbol =>
        {
            var isDecoratedWithAutoSpectreForm = symbol.IsDecoratedWithAttribute(Types.AutoSpectreForm!);
            if (!isDecoratedWithAutoSpectreForm)
                return new NamedTypedAnalysis(symbol, isDecoratedWithAutoSpectreForm, false, false);

            var decoratedMethods = symbol
                .GetMembers()
                .FilterDecoratedWithAnyAttribute(Types.TaskStepPrompt!, Types.TextPrompt!, Types.SelectPrompt!)
                .ToList();
            
            // if (decoratedMethods.Count == 0)
            //     return new NamedTypedAnalysis(symbol, false, false, false);
            
            var hasAnyAsyncDecoratedMethods = decoratedMethods
                .OfType<IMethodSymbol>()
                .Where(x => x.ReturnType.Equals(Types.Task, SymbolEqualityComparer.Default))
                .FilterDecoratedWithAnyAttribute(Types.TaskStepPrompt!)
                .Any();

            var hasEmptyConstructor = symbol.InstanceConstructors.Any(x => x.Parameters.Length == 0);
            return 
                new NamedTypedAnalysis(symbol, isDecoratedWithAutoSpectreForm, hasAnyAsyncDecoratedMethods,
                hasEmptyConstructor);
        });

        return namedType;
    }

    public PromptBuildContext? GetTextPromptBuildContext(TranslatedAttributeData attributeData,
        SinglePropertyEvaluationContext evaluationContext)
    {
        var type = evaluationContext.IsEnumerable ? evaluationContext.UnderlyingType : evaluationContext.Type;

        if (type.SpecialType == SpecialType.System_Boolean)
        {
            return new ConfirmPromptBuildContext(attributeData.Title, type, evaluationContext.IsNullable,
                evaluationContext);
        }
        else if (type.TypeKind == TypeKind.Enum)
        {
            return new EnumPromptBuildContext(attributeData.Title, type, evaluationContext.IsNullable,
                evaluationContext);
        }
        else if (type.SpecialType == SpecialType.None)
        {
            if (evaluationContext.ConfirmedNamedTypeSource is { } confirmedNamedTypeSource)
            {
                if (confirmedNamedTypeSource.NamedTypeAnalysis is {IsDecoratedWithValidAutoSpectreForm: true})
                {
                    return new ReuseExistingAutoSpectreFactoryPromptBuildContext(attributeData.Title, confirmedNamedTypeSource.NamedTypeAnalysis.NamedTypeSymbol,
                        evaluationContext.IsNullable, evaluationContext);
                }
                else
                {
                    ProductionContext.ReportDiagnostic(Diagnostic.Create(
                        new("AutoSpectre_JJK0007", $"Type currently not supported",
                            $"Only classes with {Constants.AutoSpectreFormAttributeFullyQualifiedName} supported",
                            "General", DiagnosticSeverity.Warning, true),
                        evaluationContext.Property.Locations.FirstOrDefault()));
                    return null;
                }
            }
            else
            {
                ProductionContext.ReportDiagnostic(Diagnostic.Create(
                    new("AutoSpectre_JJK0006", "Unsupported type",
                        $"Type {evaluationContext.Type} is not supported", "General", DiagnosticSeverity.Warning, true),
                    evaluationContext.Property.Locations.FirstOrDefault()));
                return null;
            }
        }

        else
        {
            return new TextPromptBuildContext(attributeData, type, evaluationContext.IsNullable, evaluationContext);
        }
    }

    private void EvaluateCondition(IConditionContext propertyContext,
        TranslatedAttributeData attributeData)
    {
        bool isGuess = attributeData.Condition == null;
        string condition = attributeData.Condition ?? $"{propertyContext.Name}Condition";
        bool negateCondition = attributeData.ConditionNegated;

        bool IsConditionMatch(ISymbol symbol)
        {
            if (symbol is IMethodSymbol methodSymbol)
            {
                if (methodSymbol.Parameters.Length > 0)
                {
                    return false;
                }

                if (methodSymbol.ReturnType.SpecialType == SpecialType.System_Boolean)
                {
                    return true;
                }
            }
            else if (symbol is IPropertySymbol propertySymbol)
            {
                if (propertySymbol.Type.SpecialType == SpecialType.System_Boolean &&
                    propertySymbol.GetMethod is not null)
                {
                    return true;
                }
            }

            return false;
        }

        var candidates =
            TargetType.GetMembers(condition)
                .ToList();

        var match = candidates.FirstOrDefault(IsConditionMatch);

        if (match is { })
        {
            propertyContext.ConfirmedCondition = match switch
            {
                IMethodSymbol method => new ConfirmedCondition(method.Name, ConditionSource.Method, negateCondition),
                IPropertySymbol property => new ConfirmedCondition(property.Name, ConditionSource.Property,
                    negateCondition),
                _ => throw new InvalidOperationException("Expected a Method or Property symbol")
            };
        }
        else if (candidates.Count > 0)
        {
            ProductionContext.ReportDiagnostic(Diagnostic.Create(
                new(DiagnosticIds.Id0010_ConditionNameMatchInvalid,
                    $"Found name matches for {condition} but they were not valid",
                    $"{candidates.Count} matches where found. But they did not match a property or method (with no arguments) named {condition} return a boolean",
                    "General", DiagnosticSeverity.Warning, true),
                propertyContext.Symbol.Locations.FirstOrDefault()));
        }
        else if (!isGuess)
        {
            ProductionContext.ReportDiagnostic(Diagnostic.Create(
                new(DiagnosticIds.Id0011_ConditionNameNotFound, $"Did not find name matches for {condition}",
                    $"No candiates where found with name {condition}", "General", DiagnosticSeverity.Warning, true),
                propertyContext.Symbol.Locations.FirstOrDefault()));
        }
    }

    private void EvaluateValidation(SinglePropertyEvaluationContext propertyContext,
        TranslatedAttributeData attributeData)
    {
        bool isGuess = attributeData.Validator == null;
        string validator = attributeData.Validator ?? $"{propertyContext.Property.Name}Validator";
        var type = propertyContext.IsEnumerable ? propertyContext.UnderlyingType : propertyContext.Type;

        bool IsMethodMatch(ISymbol symbol)
        {
            if (symbol is IMethodSymbol methodSymbol)
            {
                if (propertyContext.IsEnumerable)
                {
                    if (methodSymbol.Parameters.Length == 2)
                    {
                        var first = methodSymbol.Parameters[0];
                        var second = methodSymbol.Parameters[1];

                        var (isEnumerable, underlyingType) = first.Type.IsEnumerableOfTypeButNotString();
                        if (isEnumerable && SymbolEqualityComparer.Default.Equals(underlyingType, type))
                        {
                            if (SymbolEqualityComparer.Default.Equals(second.Type, type))
                            {
                                if (methodSymbol.ReturnType.SpecialType == SpecialType.System_String)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                else if (methodSymbol.Parameters.FirstOrDefault() is { } firstParameter)
                {
                    if (SymbolEqualityComparer.Default.Equals(firstParameter.Type,
                            type))
                    {
                        if (methodSymbol.ReturnType.SpecialType == SpecialType.System_String)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        var candidates =
            TargetType.GetMembers(validator)
                .ToList();

        var match = candidates.FirstOrDefault(IsMethodMatch);

        if (match is { })
        {
            propertyContext.ConfirmedValidator = new ConfirmedValidator(validator, !propertyContext.IsEnumerable);
        }
        else if (candidates.Count > 0)
        {
            ProductionContext.ReportDiagnostic(Diagnostic.Create(
                new(DiagnosticIds.Id0008_ValidatorNameMatchInvalid,
                    $"Found name matches for {validator} but they were not valid",
                    $"{candidates.Count} matches where found. But they did not match having a parameter of type {propertyContext.Type} and return type string",
                    "General", DiagnosticSeverity.Warning, true),
                propertyContext.Property.Locations.FirstOrDefault()));
        }
        else if (!isGuess)
        {
            ProductionContext.ReportDiagnostic(Diagnostic.Create(
                new(DiagnosticIds.Id0009_ValidatorNameNotFound, $"Did not find name matches for {validator}",
                    $"No candiates where found with name {validator}", "General", DiagnosticSeverity.Warning, true),
                propertyContext.Property.Locations.FirstOrDefault()));
        }
    }
}