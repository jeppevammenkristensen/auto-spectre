using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AutoSpectre.SourceGeneration.BuildContexts;
using AutoSpectre.SourceGeneration.Evaluation;
using AutoSpectre.SourceGeneration.Extensions;
using AutoSpectre.SourceGeneration.Extensions.Specification;
using AutoSpectre.SourceGeneration.Models;
using Microsoft.CodeAnalysis;
using static AutoSpectre.SourceGeneration.Extensions.Specification.SpecificationRecipes;

namespace AutoSpectre.SourceGeneration;

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

    /// <summary>
    /// This loops through the members (properties or methods) evaluates
    /// and return valid candidates
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public List<IStepContext> GetStepContexts()
    {
        var types = new LazyTypes(SyntaxContext.SemanticModel.Compilation);

        List<IStepContext> result = new();

        foreach (var (property, method, attributeData) in StepCandidates)
        {
            if (property is { })
            {
                HandleProperty(property, attributeData, result, types);
            }
            else if (method is { })
            {
                HandleMethod(method, attributeData, result,types);
            }
            else
            {
                throw new InvalidOperationException($"Both {nameof(property)} and {nameof(method)} are unexpectedly null");
            }
        }

        return result;
    }

    private void HandleMethod(IMethodSymbol method, TranslatedMemberAttributeData memberAttributeData, List<IStepContext> stepContexts, LazyTypes types)
    {
        if (!method.IsPublicInstance())
        {
            ProductionContext.ReportDiagnostic(Diagnostic.Create(
                new(DiagnosticIds.Id0021_CandidateMustBePublicInstance,
                    "Method must be public instance",
                    $"The method {method.Name} is not public instance",
                    "General", DiagnosticSeverity.Warning, true),
                method.Locations.FirstOrDefault()));
            return;
        }
        
        if (EvaluateSingleMethodEvaluationContext(method) is not {} singleMethodEvaluationContext)
        {
            return;
        }

        EvaluateCondition(singleMethodEvaluationContext, memberAttributeData);
        EvaluateStatus(memberAttributeData, singleMethodEvaluationContext);

        stepContexts.Add(new MethodContext(singleMethodEvaluationContext,
            new TaskStepBuildContext(memberAttributeData.Title, singleMethodEvaluationContext)));
    }

    private void EvaluateStatus(TranslatedMemberAttributeData memberAttributeData, SingleMethodEvaluationContext singleMethodEvaluationContext)
    {
        if (memberAttributeData.UseStatus == false)
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


        if (memberAttributeData.StatusText is { })
        {
            singleMethodEvaluationContext.ConfirmedStatus = new ConfirmedStatusWrap(memberAttributeData.StatusText);
            EvaluateSpinnerStyle(memberAttributeData, singleMethodEvaluationContext);
            EvaluateSpinnerType(memberAttributeData, singleMethodEvaluationContext);

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

    private void EvaluateSpinnerType(TranslatedMemberAttributeData memberAttributeData, SingleMethodEvaluationContext singleMethodEvaluationContext)
    {
        if (memberAttributeData.SpinnerType is { })
        {
            singleMethodEvaluationContext.SpinnerKnownType = memberAttributeData.SpinnerType.ToString();
        }
    }

    private void EvaluateSpinnerStyle(TranslatedMemberAttributeData memberAttributeData, SingleMethodEvaluationContext singleMethodEvaluationContext)
    {
        if (memberAttributeData.SpinnerStyle is { } style)
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

    private void HandleProperty(IPropertySymbol property, TranslatedMemberAttributeData memberAttributeData, List<IStepContext> stepContexts,
        LazyTypes types)
    {
        if (IsPublicAndInstanceSpec != property)
        {
            ProductionContext.ReportDiagnostic(Diagnostic.Create(
                new(DiagnosticIds.Id0021_CandidateMustBePublicInstance, "Property must be public instance",
                    $"The property {property.Name} was not public instance",
                    "General", DiagnosticSeverity.Error, true),
                property.Locations.FirstOrDefault()));
            return;
        }
        
        var propertyContext = SinglePropertyEvaluationContext.GenerateFromPropertySymbol(property, TargetType);

        EvaluateCondition(propertyContext, memberAttributeData);
        
        if (memberAttributeData.AskType == AskTypeCopy.Normal)
        {
            EvaluateNamedType(propertyContext, memberAttributeData);
            EvaluateDefaultValue(propertyContext, memberAttributeData);
            EvaluatePromptStyle(propertyContext, memberAttributeData);
            EvaluateValidation(propertyContext, memberAttributeData);
            EvaluateChoices(propertyContext,memberAttributeData);
            
            if (!propertyContext.IsEnumerable)
            {
                if (GetTextPromptBuildContext(memberAttributeData, propertyContext) is
                    { } promptBuildContext)
                {
                    stepContexts.Add(new PropertyContext(property.Name, property,
                        promptBuildContext));
                }
            }
            else
            {
                if (GetTextPromptBuildContext(memberAttributeData, propertyContext) is
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

        if (memberAttributeData.AskType == AskTypeCopy.Selection)
        {
            EvaluateSelectionSource(memberAttributeData, propertyContext);
            EvaluateSelectionConverter(memberAttributeData, propertyContext);
            EvaluatePageSize(memberAttributeData, ref propertyContext);
            EvaluateWrapAround(memberAttributeData, ref propertyContext);
            EvaluateMoreChoicesText(memberAttributeData, ref propertyContext);
            EvaluateInstructionText(memberAttributeData, ref propertyContext);
            EvaluateHighlightStyle(propertyContext, memberAttributeData);
            
            if (propertyContext.ConfirmedSelectionSource is { })
            {
                if (!propertyContext.IsEnumerable)
                {
                    stepContexts.Add(new PropertyContext(property.Name, property,
                        new SelectionPromptBuildContext(memberAttributeData.Title, propertyContext)));
                }
                else
                {
                    stepContexts.Add(new PropertyContext(property.Name, property,
                        new MultiSelectionBuildContext(title: memberAttributeData.Title,
                            propertyContext,
                             types)));
                }
            }
            else
            {
                // Diagnostic should have been added before this point
                return;
            }
        }
    }

    private void EvaluateChoices(SinglePropertyEvaluationContext propertyEvaluationContext,
        TranslatedMemberAttributeData memberAttributeData)
    {
        if (memberAttributeData.ChoicesStyle is {} style && style.EvaluateStyle())
        {
            propertyEvaluationContext.ConfirmedChoicesStyle = new(style);
        }
        
        var nameCandidate = memberAttributeData.ChoicesSource ?? $"{propertyEvaluationContext.Property.Name}Choices";
        var isGuess = memberAttributeData.ChoicesSource == null;

        TypeFieldMethodPropertySpecifiations spec = new(EnumerableOfTypeSpec(propertyEvaluationContext.Type));

        var candidates = TargetType
            .GetMembers()
            .Where(x => x.Name == nameCandidate)
            .ToList();
        
        var publicMethodOrPropertyOrFieldMatchSpec = (spec.Property | spec.Method | spec.Field) & IsPublic;

        if (candidates.Count > 0)
        {
            if (candidates.FirstOrDefault(publicMethodOrPropertyOrFieldMatchSpec) is { } match)
            {
                
                string? choiceInvalidText = null;

                

                ChoiceSourceType choiceSourceType = default;

                if (spec.Method == match)
                    choiceSourceType = ChoiceSourceType.Method;
                else if (spec.PropertyOrField == match)
                    choiceSourceType = ChoiceSourceType.Property;
                else
                    throw new InvalidOperationException(
                        "This should not occur. But was not able to determine if source was a method or a property");

                if (memberAttributeData.ChoicesInvalidText is { } invalidText)
                {
                    choiceInvalidText = invalidText;
                }

                propertyEvaluationContext.ConfirmedChoices = new ConfirmedChoices(nameCandidate, choiceSourceType
                    , choiceInvalidText, match.IsStatic);
            }
            else
            {
                ProductionContext.ReportDiagnostic(Diagnostic.Create(
                    new(DiagnosticIds.Id0023_ChoiceCandidate_NotValid, $"The match for Choice source {nameCandidate} was not valid",
                        $"The source with name {nameCandidate} must be public instance and be either a method with no parameters or a property returning any kind of enumerable of type {propertyEvaluationContext.GetSingleType()}",
                        "General", DiagnosticSeverity.Warning, true),
                    propertyEvaluationContext.Property.Locations.FirstOrDefault()));
            }
        }
        else
        {
            if (!isGuess)
            {
                ProductionContext.ReportDiagnostic(Diagnostic.Create(
                    new(DiagnosticIds.Id0024_ChoiceCandidate_NotFound, $"No matches found for Choice source {nameCandidate} ",
                        $"No candidate with name {nameCandidate} was found",
                        "General", DiagnosticSeverity.Warning, true),
                    propertyEvaluationContext.Property.Locations.FirstOrDefault()));
            }
        }
    }
    
    private void EvaluateNamedType(SinglePropertyEvaluationContext propertyContext, TranslatedMemberAttributeData memberAttributeData)
    {
        var namedTypeAnalysis = EvaluateType(propertyContext);
        if (namedTypeAnalysis == null)
            return;

        var initializer = EvaluateAndReturnTypeInitializer(propertyContext, memberAttributeData, namedTypeAnalysis);

        if (namedTypeAnalysis is { IsDecoratedWithValidAutoSpectreForm: true, HasEmptyConstructor: false } && initializer is null)
        {
            ProductionContext.ReportDiagnostic(Diagnostic.Create(
                new(DiagnosticIds.Id0020_InitializerNeeded, $"The type needs a method to initialize it",
                    $"The type {namedTypeAnalysis.NamedTypeSymbol.Name} decorated with AutoSpectreForm does not have an empty constructor. You need to provide a method that can initalize it",
                    "General", DiagnosticSeverity.Error, true),
                propertyContext.Property.Locations.FirstOrDefault()));
            return;
        }
        propertyContext.ConfirmedNamedTypeSource = new ConfirmedNamedTypeSource(namedTypeAnalysis, initializer?.Name, initializer?.IsStatic ?? false);
    }
    
    private IMethodSymbol? EvaluateAndReturnTypeInitializer(SinglePropertyEvaluationContext propertyContext, TranslatedMemberAttributeData memberAttributeData, NamedTypedAnalysis? namedTypedAnalysis)
    {
        var typeInitializer = memberAttributeData.TypeInitializer ?? $"Init{propertyContext.Type.Name}";
        
        
        if (namedTypedAnalysis is {})
        {
            var methodSpec = 
                MethodWithNoParametersSpec.WithReturnType(namedTypedAnalysis.NamedTypeSymbol) & IsPublic;
            
            var methodMatch = TargetType.GetAllMembers()
                .OfType<IMethodSymbol>()
                .Where(x => x.Name == typeInitializer)
                .FirstOrDefault(methodSpec);
                
            if (methodMatch is { })
            {
                return methodMatch;
            }
        }

        return null;
    }
    private void EvaluateSelectionSource(TranslatedMemberAttributeData memberAttributeData,
        SinglePropertyEvaluationContext propertyContext)
    {
        var selectionSource = memberAttributeData.SelectionSource ?? $"{propertyContext.Property.Name}Source";
        TypeFieldMethodPropertySpecifiations specs =
            new TypeFieldMethodPropertySpecifiations(EnumerableOfTypeSpec(propertyContext.GetSingleType().type));
            
        var match = TargetType
            .GetAllMembers()
            .Where(x => x.Name == selectionSource)
            .Where(specs.Field.Or(specs.Method).Or(specs.Property))
            .FirstOrDefault(x => x.IsPublic());

        if (match is { })
        {
            bool isStatic = match.IsStatic;

            if (specs.Method == match)
                propertyContext.ConfirmedSelectionSource =
                    new ConfirmedSelectionSource(selectionSource, SelectionPromptSelectionType.Method, isStatic);
            else if (specs.Field == match || specs.Property == match)
                propertyContext.ConfirmedSelectionSource =
                    new ConfirmedSelectionSource(selectionSource, SelectionPromptSelectionType.Property, isStatic);
            else
                throw new NotSupportedException();
        }
        else if(memberAttributeData.SelectionSource is {})
        {
            ProductionContext.ReportDiagnostic(Diagnostic.Create(
                new(DiagnosticIds.Id0005_SelectionSourceNotFound,
                    "Not a valid selection source",
                    $"The selection source {memberAttributeData.SelectionSource} was not found on type",
                    "General", DiagnosticSeverity.Error, true),
                propertyContext.Property.Locations.FirstOrDefault()));
        }
        else
        {
            ProductionContext.ReportDiagnostic(Diagnostic.Create(
                new(DiagnosticIds.Id0026_NoSelectionSource,
                    "No selection source set",
                    $"No selection source was set. You can fix this by setting the source or following the name convention for a valid source",
                    "General", DiagnosticSeverity.Error, true),
                propertyContext.Property.Locations.FirstOrDefault()));
        }
    }


    // Note. It might seems extra ceremonious for these methods that just transfers the value. But it's just in
    // case they get extra "complex". They probably won't.

    private void EvaluateInstructionText(TranslatedMemberAttributeData memberAttributeData,
        ref SinglePropertyEvaluationContext propertyContext)
    {
        if (memberAttributeData.InstructionsText is { })
        {
            propertyContext.InstructionsText = memberAttributeData.InstructionsText;
        }
    }

    private void EvaluateMoreChoicesText(TranslatedMemberAttributeData memberAttributeData,
        ref SinglePropertyEvaluationContext propertyContext)
    {
        if (memberAttributeData.MoreChoicesText is { })
        {
            propertyContext.MoreChoicesText = memberAttributeData.MoreChoicesText;
        }
    }

    private void EvaluateWrapAround(TranslatedMemberAttributeData memberAttributeData,
        ref SinglePropertyEvaluationContext propertyContext)
    {
        if (memberAttributeData.WrapAround is { })
        {
            propertyContext.WrapAround = memberAttributeData.WrapAround;
        }
    }

    private void EvaluatePageSize(TranslatedMemberAttributeData memberAttributeData,
        ref SinglePropertyEvaluationContext propertyContext)
    {
        if (memberAttributeData.PageSize is { } pageSize)
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


            propertyContext.PageSize = memberAttributeData.PageSize;
        }
    }

    private void EvaluatePromptStyle(SinglePropertyEvaluationContext propertyContext, TranslatedMemberAttributeData memberAttribute)
    {
        propertyContext.PromptStyle =
            EvaluateStyle(memberAttribute.PromptStyle, nameof(propertyContext.PromptStyle), propertyContext);
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
        TranslatedMemberAttributeData memberAttributeData)
    {
        propertyContext.HighlightStyle = EvaluateStyle(memberAttributeData.HighlightStyle,
            nameof(memberAttributeData.HighlightStyle), propertyContext);
    }


    /// <summary>
    /// Evaluates to see if the property has an initialized
    /// Like for instance public bool Confirm {get;set;} = true
    /// </summary>
    /// <param name="propertyContext"></param>
    /// <param name="memberAttributeData"></param>
    private void EvaluateDefaultValue(SinglePropertyEvaluationContext propertyContext,
        TranslatedMemberAttributeData memberAttributeData)
    {
        var parsedStyle = EvaluateStyle(memberAttributeData.DefaultValueStyle, nameof(memberAttributeData.DefaultValueStyle),
            propertyContext);
        if (parsedStyle is {})
            propertyContext.ConfirmedDefaultStyle = new ConfirmedDefaultStyle(parsedStyle);

        var defaultValue = memberAttributeData.DefaultValue ?? $"{propertyContext.Property.Name}DefaultValue";
        var isGuess = memberAttributeData.DefaultValue == null;
        
        var candidates = TargetType
            .GetMembers()
            .Where(x => x.Name.ToString() == defaultValue)
            .ToList();

        var singleReturnType = propertyContext.GetSingleType().type;
        var method = MethodWithNoParametersSpec.WithReturnType(singleReturnType);
        var property = PropertyOfTypeSpec(singleReturnType);
        var field = FieldOfTypeSpec(singleReturnType);
        
        var filter = (method | property | field) & IsPublic;

        if (candidates.FirstOrDefault(filter) is { } candidate)
        {
            DefaultValueType? valueType = null;
            var instance = IsInstance == candidate;
            if (method == candidate)
            {
                valueType = DefaultValueType.Method;
            }
            else if ((property | field) == candidate)
            {
                valueType = DefaultValueType.Property;
            }

            if (valueType == null)
                throw new InvalidOperationException(
                    $"Could not derive the value type of candidate with name {defaultValue}");

            propertyContext.ConfirmedDefaultValue =
                new ConfirmedDefaultValue(valueType.Value, defaultValue, parsedStyle, instance);
            return;
        }
        else
        {
            if (!isGuess)
            {
                ProductionContext.ReportDiagnostic(Diagnostic.Create(
                    new(DiagnosticIds.Id0025_DefaultValueSource_NotFound, $"Default value {memberAttributeData.DefaultValue} was not found. It should be a method with no parameters or a property returning a single type",
                        $"Could not find a valid correct method, property or field with name {memberAttributeData.DefaultValue} in the target class", "General",
                        DiagnosticSeverity.Error, true),
                    propertyContext.Property.Locations.FirstOrDefault()));
            }
        }
    }

    /// <summary>
    /// Evaluates the Converter set on the attributeData. If it's correct a valid converter is set on the context.
    /// if it is set but not valid a warning is reported.
    /// </summary>
    /// <param name="memberAttributeData"></param>
    /// <param name="context"></param>
    private void EvaluateSelectionConverter(TranslatedMemberAttributeData memberAttributeData,
        SinglePropertyEvaluationContext context)
    {
        bool guessed = memberAttributeData.Converter == null;
        var converterName = memberAttributeData.Converter ?? $"{context.Property.Name}Converter";
        

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
            .GetAllMembers()
            .Where(x => x.Name == converterName)
            .Where(x => x.IsPublic())
            .ToList();

        var match = candidates.FirstOrDefault(ConverterMethodOrProperty);

        if (match is { })
        {
            context.ConfirmedConverter = new ConfirmedConverter(converterName, match.IsStatic);
        }
        else if (!guessed || candidates.Count > 0)
        {
            ProductionContext.ReportDiagnostic(Diagnostic.Create(
                new("AutoSpectre_JJK0008",
                    $"Converter {memberAttributeData.Converter} should be a method taking a {context.UnderlyingType} as input and return string on the class",
                    $"Could not find a correct method to match {converterName} supported", "General",
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
                .GetAllMembers()
                .Where(x => x.IsPublicInstance())
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

    public PromptBuildContext? GetTextPromptBuildContext(TranslatedMemberAttributeData memberAttributeData,
        SinglePropertyEvaluationContext evaluationContext)
    {
        var type = evaluationContext.IsEnumerable ? evaluationContext.UnderlyingType : evaluationContext.Type;

        if (type.SpecialType == SpecialType.System_Boolean)
        {
            return new ConfirmPromptBuildContext(memberAttributeData.Title, evaluationContext.IsNullable,
                evaluationContext);
        }
        else if (type.TypeKind == TypeKind.Enum)
        {
            return new EnumPromptBuildContext(memberAttributeData.Title, type, evaluationContext.IsNullable,
                evaluationContext);
        }
        else if (type.SpecialType == SpecialType.None)
        {
            if (evaluationContext.ConfirmedNamedTypeSource is { } confirmedNamedTypeSource)
            {
                if (confirmedNamedTypeSource.NamedTypeAnalysis is {IsDecoratedWithValidAutoSpectreForm: true})
                {
                    return new ReuseExistingAutoSpectreFactoryPromptBuildContext(memberAttributeData.Title, confirmedNamedTypeSource.NamedTypeAnalysis.NamedTypeSymbol,
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
            return new TextPromptBuildContext(memberAttributeData, type, evaluationContext.IsNullable, evaluationContext);
        }
    }

    private void EvaluateCondition(IConditionContext propertyContext,
        TranslatedMemberAttributeData memberAttributeData)
    {
        bool isGuess = memberAttributeData.Condition == null;
        string condition = memberAttributeData.Condition ?? $"{propertyContext.Name}Condition";
        bool negateCondition = memberAttributeData.ConditionNegated;

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
        TranslatedMemberAttributeData memberAttributeData)
    {
        bool isGuess = memberAttributeData.Validator == null;
        string validator = memberAttributeData.Validator ?? $"{propertyContext.Property.Name}Validator";
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
            TargetType
                .GetAllMembers()
                .Where(x => x.IsPublic() && x.Name == validator)
                .ToList();

        var match = candidates.FirstOrDefault(IsMethodMatch);

        if (match is { })
        {
            propertyContext.ConfirmedValidator = new ConfirmedValidator(validator, !propertyContext.IsEnumerable, match.IsStatic);
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

    public class TypeFieldMethodPropertySpecifiations
    {
        private readonly Specification<ITypeSymbol> _type;

        public TypeFieldMethodPropertySpecifiations(Specification<ITypeSymbol> type)
        {
            _type = type;

            Method = MethodWithNoParametersSpec.WithTypeSpec(_type);
            Property = PropertyOfTypeSpec(_type);
            Field = FieldOfTypeSpec(_type);
        }

        public FieldSpecification<ISymbol> Field { get;  }

        public Specification<ISymbol> Property { get;  }

        public Specification<ISymbol> PropertyOrField => Property | Field;

        public MethodSpecification<ISymbol> Method { get; }
    }
}