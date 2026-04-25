using System;
using System.Text;
using AutoSpectre.SourceGeneration.BuildContexts;
using AutoSpectre.SourceGeneration.Evaluation;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests.Evaluation;

[TestSubject(typeof(ConfirmedChoices))]
public class ConfirmedChoicesTests
{
    [Fact]
    public void WriteToSummary_NoInvalidErrorText_WritesChoicesLineWithBreak()
    {
        var choices = new ConfirmedChoices("Names", ChoiceSourceType.Property, invalidErrorText: null, isStatic: false);
        var builder = new StringBuilder();

        choices.WriteToSummary(new SummaryLineWriter(builder));

        builder.ToString().Should().Be($"/// Choices: Names<br/>{Environment.NewLine}");
    }

    [Fact]
    public void WriteToSummary_WithInvalidErrorText_AppendsErrorTextBeforeBreak()
    {
        var choices = new ConfirmedChoices("Names", ChoiceSourceType.Method, invalidErrorText: "not allowed", isStatic: true);
        var builder = new StringBuilder();

        choices.WriteToSummary(new SummaryLineWriter(builder));

        builder.ToString().Should().Be(
            $"/// Choices: Names InvalidErrorText: not allowed<br/>{Environment.NewLine}");
    }
}
