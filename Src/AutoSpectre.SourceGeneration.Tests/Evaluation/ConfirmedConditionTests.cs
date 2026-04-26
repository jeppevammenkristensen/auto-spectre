using System;
using System.Text;
using AutoSpectre.SourceGeneration.BuildContexts;
using AutoSpectre.SourceGeneration.Evaluation;
using FluentAssertions;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests.Evaluation;

public class ConfirmedConditionTests
{
    [Fact]
    public void WriteToSummary_NotNegated_WritesConditionLineWithoutNegated()
    {
        var condition = new ConfirmedCondition("IsReady", ConditionSource.Property, negate: false);
        var builder = new StringBuilder();

        condition.WriteToSummary(new SummaryLineWriter(builder));

        builder.ToString().Should().Be($"/// Condition: IsReady<br/>{Environment.NewLine}");
    }

    [Fact]
    public void WriteToSummary_Negated_WritesConditionLineWithNegated()
    {
        var condition = new ConfirmedCondition("IsReady", ConditionSource.Method, negate: true);
        var builder = new StringBuilder();

        condition.WriteToSummary(new SummaryLineWriter(builder));

        builder.ToString().Should().Be($"/// Condition: IsReady Negated: True<br/>{Environment.NewLine}");
    }

    [Fact]
    public void WriteToSummary_AppendsToExistingBuilderContent()
    {
        var condition = new ConfirmedCondition("HasValue", ConditionSource.Property, negate: false);
        var builder = new StringBuilder("existing");

        condition.WriteToSummary(new SummaryLineWriter(builder));

        builder.ToString().Should().Be($"existing/// Condition: HasValue<br/>{Environment.NewLine}");
    }
}
