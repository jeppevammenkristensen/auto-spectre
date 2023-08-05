using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using AutoSpectreAnalyzer.Extensions;

namespace AutoSpectreAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingValidationCodeFixProvider)), Shared]
    public class MissingValidationCodeFixProvider : CodeFixProvider
    {
        private static string CodeFixTitle = "Add validation method";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(MissingValidationAnalyzer.DiagnosticId); }
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
                    createChangedDocument: c => AddValidationMethod(context.Document, declaration, c),
                    equivalenceKey: nameof(CodeFixTitle)),
                diagnostic);
        }

        private async Task<Document> AddValidationMethod(Document document, PropertyDeclarationSyntax property,
            CancellationToken cancellationToken)
        {
           

            var root = await document.GetSyntaxRootAsync();
            if (root is null)
                return document;

            var classDeclaration = property.Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();
            var semanticModelAsync = await document.GetSemanticModelAsync(cancellationToken);
            var propertySymbol = semanticModelAsync.GetDeclaredSymbol(property);


            var method = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("string?"),$"{property.Identifier}Validator")
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBody(SyntaxFactory.Block(SyntaxFactory.ParseStatement("return null;")));

            var (isEnumerable, underlyingType) = propertySymbol.Type.IsEnumerableOfTypeButNotString();
            var type = underlyingType ?? propertySymbol.Type;
            var typeSyntax = SyntaxFactory.ParseTypeName(type.ToMinimalDisplayString(semanticModelAsync, NullableFlowState.None,0));
            if (isEnumerable)
            {
                var firstParameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("items"))
                    .WithType(SyntaxFactory.ArrayType(typeSyntax).AddRankSpecifiers(SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.OmittedArraySizeExpression()))));

                var secondParameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("item"))
                    .WithType(typeSyntax);

                method = method.WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(new[] { firstParameter, secondParameter })));
            }
            else
            {
                var firstParameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("item"))
                    .WithType(typeSyntax);

                method = method.WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(new[] { firstParameter})));
            }


            var newRoot = Formatter.Format(root.ReplaceNode(classDeclaration,classDeclaration.AddMembers(method)).NormalizeWhitespace(), Formatter.Annotation, document.Project.Solution.Workspace);

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