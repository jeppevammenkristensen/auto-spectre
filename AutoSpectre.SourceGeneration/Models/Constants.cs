namespace AutoSpectre.SourceGeneration.Models
{
    public class Constants
    {
        /// <summary>
        /// Returns hte fullyqualified name for <see cref="AskAttributeFullyQualifiedName"/>
        /// </summary>
        public static readonly string AskAttributeFullyQualifiedName;

        /// <summary>
        /// The type name for Spectre.Console
        /// </summary>
        public static readonly string SpectreConsoleTypeName = "Spectre.Console";

        public static readonly string AutoSpectreFormAttributeFullyQualifiedName;

        static Constants()
        {
            AskAttributeFullyQualifiedName = "AutoSpectre.AskAttribute";
            AutoSpectreFormAttributeFullyQualifiedName = "AutoSpectre.AutoSpectreForm";

        }
    }
}