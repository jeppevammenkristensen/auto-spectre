using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoSpectre.SourceGeneration
{
    public class SyntaxReceiver : ISyntaxReceiver
    {
        public HashSet<ClassDeclarationSyntax> Classes = new HashSet<ClassDeclarationSyntax>(); 
        
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is not AttributeSyntax attribute)
            {
                return;
            }

            var name = ExtractName(attribute.Name);
            if (name == "AskAttribute" || name == "Ask")
            {
                var cls = attribute.Ancestors().OfType<ClassDeclarationSyntax>()?.FirstOrDefault();
                if (cls is not null)
                {
                    Classes.Add(cls);
                }
            }

        }

        private string ExtractName(NameSyntax type)
        {
            while (type != null)
            {
                switch (type)
                {
                    case IdentifierNameSyntax ins:
                        return ins.Identifier.Text;

                    case QualifiedNameSyntax qns:
                        type = qns.Right;
                        break;

                    default:
                        return null;
                }
            }

            return null;
        }
    }
}