using System;
using System.Text;
using AutoSpectre.SourceGeneration.BuildContexts;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests.BuildContexts;

[TestSubject(typeof(SummaryLineWriter))]
public class SummaryLineWriterTests
{
    [Fact]
    public void AppendLine_PrependsTripleSlash()
    {
        var builder = new StringBuilder();
        var writer = new SummaryLineWriter(builder);

        writer.AppendLine(" hello", addLineBreak: false);

        builder.ToString().Should().Be($"/// hello{Environment.NewLine}");
    }

    [Fact]
    public void AppendLine_AddLineBreak_AppendsBrTagBeforeNewLine()
    {
        var builder = new StringBuilder();
        var writer = new SummaryLineWriter(builder);

        writer.AppendLine(" hello", addLineBreak: true);

        builder.ToString().Should().Be($"/// hello<br/>{Environment.NewLine}");
    }

    [Fact]
    public void AppendLine_StripsLeadingSlashesBeforePrepending()
    {
        var builder = new StringBuilder();
        var writer = new SummaryLineWriter(builder);

        writer.AppendLine("/// already commented", addLineBreak: false);

        builder.ToString().Should().Be($"/// already commented{Environment.NewLine}");
    }

    [Fact]
    public void AppendLine_StripsAnyLeadingSlashRunRegardlessOfLength()
    {
        var builder = new StringBuilder();
        var writer = new SummaryLineWriter(builder);

        writer.AppendLine("/////// many slashes", addLineBreak: false);

        builder.ToString().Should().Be($"/// many slashes{Environment.NewLine}");
    }

    [Fact]
    public void AppendLine_ReturnsSameInstanceForChaining()
    {
        var writer = new SummaryLineWriter(new StringBuilder());

        var result = writer.AppendLine("line", addLineBreak: false);

        result.Should().BeSameAs(writer);
    }

    [Fact]
    public void AppendLine_MultipleCalls_AppendOneLinePerCall()
    {
        var builder = new StringBuilder();
        var writer = new SummaryLineWriter(builder);

        writer.AppendLine(" first", addLineBreak: false)
              .AppendLine(" second", addLineBreak: false);

        builder.ToString().Should().Be(
            $"/// first{Environment.NewLine}/// second{Environment.NewLine}");
    }

    [Fact]
    public void AppendLine_PreservesExistingBuilderContent()
    {
        var builder = new StringBuilder("preexisting");
        var writer = new SummaryLineWriter(builder);

        writer.AppendLine(" line", addLineBreak: false);

        builder.ToString().Should().Be($"preexisting/// line{Environment.NewLine}");
    }

    [Fact]
    public void AppendLine_EmptyString_WritesTripleSlashAndNewLine()
    {
        var builder = new StringBuilder();
        var writer = new SummaryLineWriter(builder);

        writer.AppendLine(string.Empty, addLineBreak: false);

        builder.ToString().Should().Be($"///{Environment.NewLine}");
    }

    [Fact]
    public void Append_IsFirstPartOfLineFalse_WritesRawWithoutTripleSlash()
    {
        var builder = new StringBuilder();
        var writer = new SummaryLineWriter(builder);

        writer.Append("raw");

        builder.ToString().Should().Be("raw");
    }

    [Fact]
    public void Append_IsFirstPartOfLineTrue_PrependsTripleSlashAndStripsLeadingSlashes()
    {
        var builder = new StringBuilder();
        var writer = new SummaryLineWriter(builder);

        writer.Append("//// hello", isFirstPartOfLine: true);

        builder.ToString().Should().Be("/// hello");
    }

    [Fact]
    public void Append_DoesNotAppendNewLine()
    {
        var builder = new StringBuilder();
        var writer = new SummaryLineWriter(builder);

        writer.Append("a").Append("b");

        builder.ToString().Should().Be("ab");
    }

    [Fact]
    public void Append_ReturnsSameInstanceForChaining()
    {
        var writer = new SummaryLineWriter(new StringBuilder());

        var result = writer.Append("x");

        result.Should().BeSameAs(writer);
    }

    [Fact]
    public void NewLine_AddLineBreakTrue_WritesBrTagThenNewLine()
    {
        var builder = new StringBuilder();
        var writer = new SummaryLineWriter(builder);

        writer.NewLine();

        builder.ToString().Should().Be($"<br/>{Environment.NewLine}");
    }

    [Fact]
    public void NewLine_AddLineBreakFalse_WritesOnlyNewLine()
    {
        var builder = new StringBuilder();
        var writer = new SummaryLineWriter(builder);

        writer.NewLine(addLineBreak: false);

        builder.ToString().Should().Be(Environment.NewLine);
    }

    [Fact]
    public void NewLine_ReturnsSameInstanceForChaining()
    {
        var writer = new SummaryLineWriter(new StringBuilder());

        var result = writer.NewLine();

        result.Should().BeSameAs(writer);
    }

    [Fact]
    public void Builder_ExposesProvidedInstance()
    {
        var builder = new StringBuilder();
        var writer = new SummaryLineWriter(builder);

        writer.Builder.Should().BeSameAs(builder);
    }

    [Fact]
    public void ImplicitConversionToStringBuilder_ReturnsBackingBuilder()
    {
        var builder = new StringBuilder();
        var writer = new SummaryLineWriter(builder);

        StringBuilder converted = writer;

        converted.Should().BeSameAs(builder);
    }

    [Fact]
    public void ToString_ReturnsBackingBuilderContent()
    {
        var builder = new StringBuilder("content");
        var writer = new SummaryLineWriter(builder);

        writer.ToString().Should().Be("content");
    }
}
