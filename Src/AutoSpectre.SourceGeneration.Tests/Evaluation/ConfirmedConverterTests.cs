using System;
using System.Text;
using AutoSpectre.SourceGeneration.BuildContexts;
using AutoSpectre.SourceGeneration.Evaluation;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests.Evaluation;

[TestSubject(typeof(ConfirmedConverter))]
public class ConfirmedConverterTests
{
    [Fact]
    public void WriteToSummary_WritesConverterLineWithBreak()
    {
        var converter = new ConfirmedConverter("ToUpper", isStatic: false);
        var builder = new StringBuilder();

        converter.WriteToSummary(new SummaryLineWriter(builder));

        builder.ToString().Should().Be($"/// Converter: ToUpper<br/>{Environment.NewLine}");
    }

    [Fact]
    public void WriteToSummary_StaticConverter_OutputIsIdenticalToInstance()
    {
        var staticConverter = new ConfirmedConverter("ToUpper", isStatic: true);
        var builder = new StringBuilder();

        staticConverter.WriteToSummary(new SummaryLineWriter(builder));

        builder.ToString().Should().Be($"/// Converter: ToUpper<br/>{Environment.NewLine}");
    }
}
