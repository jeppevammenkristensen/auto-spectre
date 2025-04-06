using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.Tests.TestUtils;

public static class ResultExtensions
{
    public static SyntaxTree GetOriginalSourceCode(this IEnumerable<SyntaxTree> trees)
    {
        return trees.ElementAt(0);
    }
    
    public static SyntaxTree GetGeneratedAutoSpectreFactoryCode(this IEnumerable<SyntaxTree> trees)
    {
        // Return the second last tree
        return trees.ToArray()[^2];
    }
    
    public static SyntaxTree GetSpectreFactory(this IEnumerable<SyntaxTree> trees)
    {
        return trees.ToArray()[^1];
    }
}