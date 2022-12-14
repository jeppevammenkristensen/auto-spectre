namespace AutoSpectre.SourceGeneration.Models
{
    public class Constants
    {
        /// <summary>
        /// Returns the fullyqualified name for <see cref="AskAttributeFullyQualifiedName"/>
        /// </summary>
        public static readonly string AskAttributeFullyQualifiedName;

        public static readonly string AutoSpectreFormAttributeFullyQualifiedName;

        static Constants()
        {
            AskAttributeFullyQualifiedName = "AutoSpectre.AskAttribute";
            AutoSpectreFormAttributeFullyQualifiedName = "AutoSpectre.AutoSpectreForm";
        }
    }

    internal enum AskTypeCopy
    {
        /// <summary>
        /// Default. 
        /// </summary>
        Normal,
        /// <summary>
        /// Presents a selection dialog
        /// </summary>
        Selection
    }
}