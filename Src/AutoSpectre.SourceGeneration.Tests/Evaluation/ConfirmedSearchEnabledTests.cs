using System;
using System.Text;
using AutoSpectre.SourceGeneration.BuildContexts;
using AutoSpectre.SourceGeneration.Evaluation;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests.Evaluation;

[TestSubject(typeof(ConfirmedSearchEnabled))]
public class ConfirmedSearchEnabledTests
{
    [Fact]
    public void WriteToSummary_NoPlaceholderText_WritesEnabledLineWithBreak()
    {
        var search = new ConfirmedSearchEnabled(searchEnabled: true, searchPlaceholderText: null);
        var builder = new StringBuilder();

        search.WriteToSummary(new SummaryLineWriter(builder));

        builder.ToString().Should().Be($"/// Search is enabled<br/>{Environment.NewLine}");
    }

    [Fact]
    public void WriteToSummary_WithPlaceholderText_AppendsPlaceholderBeforeBreak()
    {
        var search = new ConfirmedSearchEnabled(searchEnabled: true, searchPlaceholderText: "type to filter");
        var builder = new StringBuilder();

        search.WriteToSummary(new SummaryLineWriter(builder));

        builder.ToString().Should().Be(
            $"/// Search is enabled Placeholder: type to filter<br/>{Environment.NewLine}");
    }
}
