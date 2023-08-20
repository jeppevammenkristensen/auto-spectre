using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoSpectre.SourceGeneration;

internal class MethodWalker : CSharpSyntaxWalker
{

    public bool HasPrompting => false;
    private readonly SemanticModel _model;

    public MethodWalker(SemanticModel model)
    {
        _model = model;
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        // This can be code to get started. We need to determine if an

        //if (node.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
        //{
        //    if (memberAccessExpressionSyntax.Name.Identifier.Text == "Prompt")
        //    {
        //        var methodSymbol = _model.GetSymbolInfo(memberAccessExpressionSyntax).Symbol;
        //        if (methodSymbol is IMethodSymbol method)
        //        {
                    
                    
        //        }
        //    }
        //}
    }
}