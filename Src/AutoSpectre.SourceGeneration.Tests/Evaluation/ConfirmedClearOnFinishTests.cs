using System.Text;
using AutoSpectre.SourceGeneration.BuildContexts;
using AutoSpectre.SourceGeneration.Evaluation;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests.Evaluation;

[TestSubject(typeof(ConfirmedClearOnFinish))]
public class ConfirmedClearOnFinishTests
{
    [Fact]
    public void WriteToSummary_WritesClearOnFinishLineWithBreak()
    {
        var clearOnFinish = new ConfirmedClearOnFinish();
        var builder = new StringBuilder();

        clearOnFinish.WriteToSummary(new SummaryLineWriter(builder));

        builder.ToString().Should().Be($"/// Clear on finish: true<br/>{System.Environment.NewLine}");
    }
}
