using System;
using System.Reflection;
using AutoSpectre.SourceGeneration.Extensions;
using AutoSpectre.SourceGeneration.Models;
using Microsoft.CodeAnalysis;
using Spectre.Console;

namespace AutoSpectre.SourceGeneration;

public class PropertyWithAskAttributeData
{
    public IPropertySymbol Property { get; }

    public TranslatedAttributeData TranslatedAttribute { get; }

    public PropertyWithAskAttributeData(IPropertySymbol property, AttributeData attributeData)
    {
        Property = property;

        if (attributeData.AttributeClass == null)
            throw new("Attribute class was null");

        var title = attributeData.GetValue<string?>("Title") ??
                    $"Enter [green]{property.Name}[/]";

#pragma warning disable CS0618 // Type or member is obsolete
        var condition = attributeData.GetValue<string>(nameof(AskAttribute.Condition));

        var conditionNegated = attributeData.GetValue<bool>(nameof(AskAttribute.NegateCondition));


        if (attributeData.AttributeClass.Name == nameof(AskAttribute))
        {
            var askType = attributeData.GetValue<AskTypeCopy>("AskType");
            var selectionSource = attributeData.GetValue<string?>("SelectionSource") ?? null;
            var converter = attributeData.GetValue<string?>("Converter") ?? null;
            var validator = attributeData.GetValue<string>(nameof(AskAttribute.Validator));
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
            var validator = attributeData.GetValue<string>(nameof(TextPromptAttribute.Validator));
            var secret = attributeData.GetValue<bool>(nameof(TextPromptAttribute.Secret));
            var mask = attributeData.GetValue<char?>(nameof(TextPromptAttribute.Mask), '*');
            string? defaultValueStyle = attributeData.GetValue<string>(nameof(TextPromptAttribute.DefaultValueStyle));
            string? promptStyle = attributeData.GetValue<string>(nameof(TextPromptAttribute.PromptStyle));
            
            TranslatedAttribute = TranslatedAttributeData.TextPrompt(title,validator, condition, conditionNegated, secret, mask, defaultValueStyle, promptStyle);

        }
        else if (attributeData.AttributeClass.Name == nameof(SelectPromptAttribute))
        {
            var selectionSource = attributeData.GetValue<string?>(nameof(SelectPromptAttribute.Source)) ?? null;
            var converter = attributeData.GetValue<string?>(nameof(SelectPromptAttribute.Converter)) ?? null;

            TranslatedAttribute = TranslatedAttributeData.SelectPrompt(title, selectionSource, converter, condition, conditionNegated);
        }
        else
        {
            throw new InvalidOperationException("Unexpected attribute being processed");
        }
    }

    public void Deconstruct(out IPropertySymbol property, out TranslatedAttributeData attributeData)
    {
        property = Property;
        attributeData = TranslatedAttribute;
    }
}