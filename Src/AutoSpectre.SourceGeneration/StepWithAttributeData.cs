using System;
using System.Reflection;
using AutoSpectre.SourceGeneration.Extensions;
using AutoSpectre.SourceGeneration.Models;
using Microsoft.CodeAnalysis;
using Spectre.Console;

namespace AutoSpectre.SourceGeneration;

public enum StepSource
{
    Property,
    Method
}

public class StepWithAttributeData
{
    public StepSource Source { get; }
    
    public IPropertySymbol? Property { get; }
    
    public IMethodSymbol? Method { get; }

    public TranslatedAttributeData TranslatedAttribute { get; }

    public StepWithAttributeData(IMethodSymbol method, AttributeData attributeData)
    {
        Method = method;
        Source = StepSource.Method;
        
        if (attributeData.AttributeClass == null)
            throw new("Attribute class was null");

        var title = attributeData.GetAttributePropertyValue<string?>("Title");

        #pragma warning disable CS0618 // Type or member is obsolete
        var condition = attributeData.GetAttributePropertyValue<string>(nameof(AskAttribute.Condition));

        var conditionNegated = attributeData.GetAttributePropertyValue<bool>(nameof(AskAttribute.NegateCondition));

        var useStatus = attributeData.GetAttributePropertyValue<bool>(nameof(TaskStepAttribute.UseStatus));
        var statusText = attributeData.GetAttributePropertyValue<string?>(nameof(TaskStepAttribute.StatusText));

        var spinnerStyle = attributeData.GetAttributePropertyValue<string?>(nameof(TaskStepAttribute.SpinnerStyle));
        SpinnerKnownTypesCopy? spinnerType = null;
        if (attributeData.TryGetAttributePropertyValue<SpinnerKnownTypesCopy>(nameof(TaskStepAttribute.SpinnerType), out var value))
        {
            spinnerType = value;
        } 
        
        TranslatedAttribute = TranslatedAttributeData.TaskPrompt(title, condition, conditionNegated, useStatus, statusText, spinnerStyle, spinnerType);
    }
    
    public StepWithAttributeData(IPropertySymbol property, AttributeData attributeData)
    {
        Property = property;
        Source = StepSource.Property;

        if (attributeData.AttributeClass == null)
            throw new("Attribute class was null");

        var title = attributeData.GetAttributePropertyValue<string?>("Title") ??
                    $"Enter [green]{property.Name}[/]";

#pragma warning disable CS0618 // Type or member is obsolete
        var condition = attributeData.GetAttributePropertyValue<string>(nameof(AskAttribute.Condition));

        var conditionNegated = attributeData.GetAttributePropertyValue<bool>(nameof(AskAttribute.NegateCondition));


        if (attributeData.AttributeClass.Name == nameof(AskAttribute))
        {
            var askType = attributeData.GetAttributePropertyValue<AskTypeCopy>("AskType");
            var selectionSource = attributeData.GetAttributePropertyValue<string?>("SelectionSource") ?? null;
            var converter = attributeData.GetAttributePropertyValue<string?>("Converter") ?? null;
            var validator = attributeData.GetAttributePropertyValue<string>(nameof(AskAttribute.Validator));
#pragma warning restore CS0618 // Type or member is obsolete            
            TranslatedAttribute =
                new TranslatedAttributeData(
                    askType: askType,
                    selectionSource: selectionSource,
                    title: title,
                    converter: converter,
                    validator: validator,
                    condition: condition, 
                    conditionNegated);
        }
        else if (attributeData.AttributeClass.Name == nameof(TextPromptAttribute))
        {
            var validator = attributeData.GetAttributePropertyValue<string>(nameof(TextPromptAttribute.Validator));
            var secret = attributeData.GetAttributePropertyValue<bool>(nameof(TextPromptAttribute.Secret));
            var mask = attributeData.GetAttributePropertyValue<char?>(nameof(TextPromptAttribute.Mask), '*');
            string? defaultValueStyle = attributeData.GetAttributePropertyValue<string>(nameof(TextPromptAttribute.DefaultValueStyle));
            string? promptStyle = attributeData.GetAttributePropertyValue<string>(nameof(TextPromptAttribute.PromptStyle));
            string? typeInitializer =
                attributeData.GetAttributePropertyValue<string>(nameof(TextPromptAttribute.TypeInitializer));
            
            TranslatedAttribute = TranslatedAttributeData.TextPrompt(title,validator, condition, conditionNegated, secret, mask, defaultValueStyle, promptStyle, typeInitializer);

        }
        
        else if (attributeData.AttributeClass.Name == nameof(SelectPromptAttribute))
        {
            var selectionSource = attributeData.GetAttributePropertyValue<string?>(nameof(SelectPromptAttribute.Source)) ?? null;
            var converter = attributeData.GetAttributePropertyValue<string?>(nameof(SelectPromptAttribute.Converter)) ?? null;
            var pageSize = attributeData.GetAttributePropertyValue<int?>(nameof(SelectPromptAttribute.PageSize)) ?? null;
            var wrapAround = attributeData.GetAttributePropertyValue<bool?>(nameof(SelectPromptAttribute.WrapAround)) ?? null;
            var moreChoicesText = attributeData.GetAttributePropertyValue<string?>(nameof(SelectPromptAttribute.MoreChoicesText)) ?? null;
            var instructionsText = attributeData.GetAttributePropertyValue<string?>(nameof(SelectPromptAttribute.InstructionsText)) ?? null;
            var highlightStyle = attributeData.GetAttributePropertyValue<string?>(nameof(SelectPromptAttribute.HighlightStyle)) ?? null;

            TranslatedAttribute = TranslatedAttributeData.SelectPrompt(title, selectionSource, converter, condition, conditionNegated, pageSize, wrapAround, moreChoicesText, instructionsText, highlightStyle);
        }
        
        else
        {
            throw new InvalidOperationException("Unexpected attribute being processed");
        }
    }

    public void Deconstruct(out IPropertySymbol? property, out IMethodSymbol? method,  out TranslatedAttributeData attributeData)
    {
        property = Property;
        attributeData = TranslatedAttribute;
        method = Method;
    }
}