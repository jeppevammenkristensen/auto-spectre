using System;
using System.Text;
using AutoSpectre.SourceGeneration.BuildContexts;
using AutoSpectre.SourceGeneration.Evaluation;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests.Evaluation;

[TestSubject(typeof(ConfirmedDefaultValue))]
public class ConfirmedDefaultValueTests
{
    [Fact]
    public void WriteToSummary_WritesDefaultValueLineWithBreak()
    {
        var defaultValue = new ConfirmedDefaultValue("DefaultName", SourceEvaluation.False);
        var builder = new StringBuilder();

        defaultValue.WriteToSummary(new SummaryLineWriter(builder));

        builder.ToString().Should().Be($"/// Default value: DefaultName<br/>{Environment.NewLine}");
    }

    [Fact]
    public void WriteToSummary_OutputDoesNotDependOnDefaultValueType()
    {
        var method = new ConfirmedDefaultValue("DefaultName", SourceEvaluation.False);
        var builder = new StringBuilder();

        method.WriteToSummary(new SummaryLineWriter(builder));

        builder.ToString().Should().Be($"/// Default value: DefaultName<br/>{Environment.NewLine}");
    }
}
