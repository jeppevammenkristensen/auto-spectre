﻿using AutoSpectre.SourceGeneration.Extensions;
using AutoSpectre.SourceGeneration.Models;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration;

public class PropertyWithAskAttributeData
{
    public void Deconstruct(out IPropertySymbol property, out TranslatedAskAttributeData attributeData)
    {
        property = Property;
        attributeData = TranslatedAskAttribute;
    }

    public IPropertySymbol Property { get; }
    public TranslatedAskAttributeData TranslatedAskAttribute { get; }

    public PropertyWithAskAttributeData(IPropertySymbol property, AttributeData attributeData)
    {
        Property = property;
           
        var title = attributeData.GetValue<string?>("Title") ??
                    $"Enter [green]{property.Name}[/]";
        var askType = attributeData.GetValue<AskTypeCopy>("AskType");
        var selectionSource = attributeData.GetValue<string?>("SelectionSource") ?? null;
        TranslatedAskAttribute =
            new TranslatedAskAttributeData(askType: askType, selectionSource: selectionSource, title: title);
    }
}