using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Formatting;

namespace AutoSpectreAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AskAnalyzerCodeFixProvider)), Shared]
    public class AskAnalyzerCodeFixProvider : CodeFixProvider
    {
        private static string CodeFixTitle = "Add ask attribute";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(MissingAskAttributeAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf()
                .OfType<PropertyDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixTitle,
                    createChangedDocument: c => AddAskAttribute(context.Document, declaration, c),
                    equivalenceKey: nameof(CodeFixTitle)),
                diagnostic);
        }

        private async Task<Document> AddAskAttribute(Document document, PropertyDeclarationSyntax property,
            CancellationToken cancellationToken)
        {
            // Add the attribute "Ask" to the property declaration
            var askAttribute = SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Ask"))
                )
            );

            var root = await document.GetSyntaxRootAsync();
            if (root is null)
                return document;
            
            var newRoot = Formatter.Format(root.ReplaceNode(property,property.AddAttributeLists(askAttribute)).NormalizeWhitespace(), Formatter.Annotation, document.Project.Solution.Workspace);

            // Get the original document
            var originalDocument = document;

            // Replace the old root with the new root to get a new document with the code fix applied
            var newDocument = originalDocument.WithSyntaxRoot(newRoot);

            // Return the updated document
            return newDocument;


        }

        // Example of modifying a document adding an attribute 

        //private async Task<Document> AddAttributeToClass(Document document, TypeDeclarationSyntax typeDeclaration,
        //    CancellationToken cancellationToken)
        //{
        //    var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        //    editor.ReplaceNode(typeDeclaration, AddTraitAttribute(typeDeclaration));
        //    return editor.GetChangedDocument();
        //}

        //private TypeDeclarationSyntax AddTraitAttribute(TypeDeclarationSyntax source)
        //{
        //    var attributeListSyntax = AttributeList(
        //        SingletonSeparatedList<AttributeSyntax>(
        //            Attribute(IdentifierName("Trait"))));

        //    return source.AddAttributeLists(
        //        attributeListSyntax);
        //}
    }
}