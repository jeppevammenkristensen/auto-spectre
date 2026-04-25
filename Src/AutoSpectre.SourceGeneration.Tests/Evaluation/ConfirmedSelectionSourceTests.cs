using System;
using System.Text;
using AutoSpectre.SourceGeneration.BuildContexts;
using AutoSpectre.SourceGeneration.Evaluation;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests.Evaluation;

[TestSubject(typeof(ConfirmedSelectionSource))]
public class ConfirmedSelectionSourceTests
{
    [Fact]
    public void WriteToSummary_WritesSourceLineWithBreak()
    {
        var source = new ConfirmedSelectionSource("NamesSource", SelectionPromptSelectionType.Property, isStatic: false);
        var builder = new StringBuilder();

        source.WriteToSummary(new SummaryLineWriter(builder));

        builder.ToString().Should().Be($"/// Source: NamesSource<br/>{Environment.NewLine}");
    }

    [Fact]
    public void WriteToSummary_OutputDoesNotDependOnSourceType()
    {
        var source = new ConfirmedSelectionSource("NamesSource", SelectionPromptSelectionType.Method, isStatic: true);
        var builder = new StringBuilder();

        source.WriteToSummary(new SummaryLineWriter(builder));

        builder.ToString().Should().Be($"/// Source: NamesSource<br/>{Environment.NewLine}");
    }
}
