using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration
{
    public class PropertyContext
    {
        public string PropertyName { get; }
        public IPropertySymbol PropertySymbol { get; }
        public PromptBuildContext BuildContext { get; }

        public PropertyContext(string propertyName, IPropertySymbol propertySymbol, PromptBuildContext buildContext)
        {
            PropertyName = propertyName;
            PropertySymbol = propertySymbol;
            BuildContext = buildContext;
        }
    }
}