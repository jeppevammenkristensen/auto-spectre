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
        try
        {
            return trees.ToArray()[^2];
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Failed to retrive generated code.", e);
        }
    }
    
    public static SyntaxTree GetSpectreFactory(this IEnumerable<SyntaxTree> trees)
    {
        return trees.ToArray()[^1];
    }
}