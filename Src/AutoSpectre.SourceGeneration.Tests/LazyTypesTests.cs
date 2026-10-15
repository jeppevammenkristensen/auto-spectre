using System.Threading;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests;

public class LazyTypesTests
{
    [Fact]
    public void CancellationToken_ResolvesAndCachesCompilationSymbol()
    {
        var compilation = CSharpCompilation.Create("Test",
            references: [MetadataReference.CreateFromFile(typeof(CancellationToken).Assembly.Location)]);
        var types = new LazyTypes(compilation);

        var symbol = types.CancellationToken;

        symbol.Should().NotBeNull();
        symbol.Should().BeSameAs(compilation.GetTypeByMetadataName("System.Threading.CancellationToken"));
        types.CancellationToken.Should().BeSameAs(symbol);
    }

    [Fact]
    public void CancellationToken_ReturnsNullWhenTypeIsUnavailable()
    {
        var types = new LazyTypes(CSharpCompilation.Create("Test"));

        types.CancellationToken.Should().BeNull();
    }
}
