using System;
using System.Text;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration;

/// <summary>
/// Uses a builder in  combination with disposable to wrap a namespace correctly
/// </summary>
internal readonly struct NamespaceScope : IDisposable
{
    private readonly StringBuilder? _builder;

    private NamespaceScope(StringBuilder? builder) => _builder = builder;

    public static NamespaceScope Begin(StringBuilder builder, INamespaceSymbol containingNamespace)
    {
        if (containingNamespace.IsGlobalNamespace)
            return new NamespaceScope(null);

        builder.Append("namespace ").AppendLine(containingNamespace.ToDisplayString());
        builder.AppendLine("{");
        return new NamespaceScope(builder);
    }

    public void Dispose()
    {
        _builder?.AppendLine("}");
    }
}
