namespace AutoSpectre.SourceGeneration.Models
{
    public class Constants
    {
        public static readonly string AskAttributeFullyQualifiedName;

        static Constants()
        {
            AskAttributeFullyQualifiedName = typeof(AskAttribute).FullName;
        }
    }
}