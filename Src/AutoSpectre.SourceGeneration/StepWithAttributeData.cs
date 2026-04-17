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

    public TranslatedMemberAttributeData TranslatedMemberAttribute { get; }

    public StepWithAttributeData(IMethodSymbol method, AttributeData attributeData)
    {
        Method = method;
        Source = StepSource.Method;
        
        if (attributeData.AttributeClass == null)
            throw new("Attribute class was null");

        var title = attributeData.GetAttributePropertyValue<string?>("Title");

        #pragma warning disable CS0618 // Type or member is obsolete
        var condition = attributeData.GetAttributePropertyValue<string>(IConditionAttributeNames.Condition);

        var conditionNegated = attributeData.GetAttributePropertyValue<bool>(IConditionAttributeNames.NegateCondition);

        var useStatus = attributeData.GetAttributePropertyValue<bool>(MethodBasedAttributeNames.UseStatus);
        var statusText = attributeData.GetAttributePropertyValue<string?>(MethodBasedAttributeNames.StatusText);

        var spinnerStyle = attributeData.GetAttributePropertyValue<string?>(MethodBasedAttributeNames.SpinnerStyle);
        SpinnerKnownTypesCopy? spinnerType = null;
        if (attributeData.TryGetAttributePropertyValue<SpinnerKnownTypesCopy>(MethodBasedAttributeNames.SpinnerType, out var value))
        {
            spinnerType = value;
        }


        if (attributeData.AttributeClass.Name == TaskStepAttributeNames.AttributeName)
        {
            TranslatedMemberAttribute = TranslatedMemberAttributeData.TaskPrompt(title, condition, conditionNegated, useStatus, statusText, spinnerStyle, spinnerType);    
        }
        else
        {
            TranslatedMemberAttribute = TranslatedMemberAttributeData.BreakPrompt(condition, conditionNegated, useStatus, statusText, spinnerStyle, spinnerType);   
        }
        
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
        var condition = attributeData.GetAttributePropertyValue<string>(IConditionAttributeNames.Condition);

        var conditionNegated = attributeData.GetAttributePropertyValue<bool>(IConditionAttributeNames.NegateCondition);
        
        if (attributeData.AttributeClass.Name == TextPromptAttributeNames.AttributeName)
        {
            var validator = attributeData.GetAttributePropertyValue<string>(TextPromptAttributeNames.Validator);
            var secret = attributeData.GetAttributePropertyValue<bool>(TextPromptAttributeNames.Secret);
            var mask = attributeData.GetAttributePropertyValue<char?>(TextPromptAttributeNames.Mask, '*');
            string? defaultValueStyle = attributeData.GetAttributePropertyValue<string>(TextPromptAttributeNames.DefaultValueStyle);
            string? promptStyle = attributeData.GetAttributePropertyValue<string>(TextPromptAttributeNames.PromptStyle);
            string? typeInitializer = attributeData.GetAttributePropertyValue<string>(TextPromptAttributeNames.TypeInitializer);
            var choicesSource = attributeData.GetAttributePropertyValue<string>(TextPromptAttributeNames.ChoicesSource);
            var choicesStyle = attributeData.GetAttributePropertyValue<string>(TextPromptAttributeNames.ChoicesStyle);
            var choicesInvalidText = attributeData.GetAttributePropertyValue<string>(TextPromptAttributeNames.ChoicesInvalidText);
            var defaultValueSource =
                attributeData.GetAttributePropertyValue<string>(TextPromptAttributeNames.DefaultValueSource);
            var searchEnabled = attributeData.GetAttributePropertyValue<bool?>(SelectPromptAttributeNames.SearchEnabled) ?? null;
            var searchPlaceholderText =
                attributeData.GetAttributePropertyValue<string?>(SelectPromptAttributeNames.SearchPlaceholderText) ??
                null;


            TranslatedMemberAttribute = TranslatedMemberAttributeData.TextPrompt(title,
                validator,
                condition,
                conditionNegated,
                secret,
                mask,
                defaultValueStyle,
                promptStyle,
                typeInitializer,
                choicesSource,
                choicesStyle,
                choicesInvalidText,
                defaultValueSource, 
                searchEnabled, 
                searchPlaceholderText);
        }
        
        else if (attributeData.AttributeClass.Name == SelectPromptAttributeNames.AttributeName)
        {
            var selectionSource = attributeData.GetAttributePropertyValue<string?>(SelectPromptAttributeNames.Source) ?? null;
            var converter = attributeData.GetAttributePropertyValue<string?>(SelectPromptAttributeNames.Converter) ?? null;
            var pageSize = attributeData.GetAttributePropertyValue<int?>(SelectPromptAttributeNames.PageSize) ?? null;
            var wrapAround = attributeData.GetAttributePropertyValue<bool?>(SelectPromptAttributeNames.WrapAround) ?? null;
            var moreChoicesText = attributeData.GetAttributePropertyValue<string?>(SelectPromptAttributeNames.MoreChoicesText) ?? null;
            var instructionsText = attributeData.GetAttributePropertyValue<string?>(SelectPromptAttributeNames.InstructionsText) ?? null;
            var highlightStyle = attributeData.GetAttributePropertyValue<string?>(SelectPromptAttributeNames.HighlightStyle) ?? null;
            var searchEnabled = attributeData.GetAttributePropertyValue<bool?>(SelectPromptAttributeNames.SearchEnabled) ?? null;
            var searchPlaceholderText =
                attributeData.GetAttributePropertyValue<string?>(SelectPromptAttributeNames.SearchPlaceholderText) ??
                null;            
            var cancelResult =
                attributeData.GetAttributePropertyValue<string?>(SelectPromptAttributeNames.CancelResult) ?? null;

            

            TranslatedMemberAttribute = TranslatedMemberAttributeData.SelectPrompt(title,
                selectionSource,
                converter,
                condition,
                conditionNegated,
                pageSize,
                wrapAround,
                moreChoicesText,
                instructionsText,
                highlightStyle, 
                searchEnabled,
                searchPlaceholderText,
                cancelResult);
        }
        
        else
        {
            throw new InvalidOperationException("Unexpected attribute being processed");
        }
    }

    public void Deconstruct(out IPropertySymbol? property, out IMethodSymbol? method,  out TranslatedMemberAttributeData memberAttributeData)
    {
        property = Property;
        memberAttributeData = TranslatedMemberAttribute;
        method = Method;
    }
}