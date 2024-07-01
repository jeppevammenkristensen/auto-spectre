using AutoSpectre.SourceGeneration.Evaluation;
using AutoSpectre.SourceGeneration.Extensions;
using FluentAssertions;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests.Extensions;

public class ConfirmedExtensionsTest
{
    [Fact]
    public void GetSearchString_SearchEnabledFalse_ReturnsEmptyString()
    {
        var search = new ConfirmedSearchEnabled(false, "placeholder");

        var result = search.GetSearchString();

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetSearchString_SearchEnabledTrueAndNoPlaceholderText_ReturnsOnlyEnableSearch()
    {
        var search = new ConfirmedSearchEnabled(true, null);

        var result = search.GetSearchString();

        result.Should().Contain(".EnableSearch()");
    }

    [Fact]
    public void GetSearchString_SearchEnabledTrueAndEmptyPlaceholderText_ReturnsEnableSearchWithEmptyPlaceholderText()
    {
        var search = new ConfirmedSearchEnabled(true, "");

        var result = search.GetSearchString();

        //Please note that empty text within quotes depends on Implementation of GetSafeTextWithQuotes() method
        result.Should().Contain(".EnableSearch()").And.Contain("SearchPlaceholderText(");
    }

    [Fact]
    public void GetSearchString_SearchEnabledTrueAndPlaceholderText_ReturnsEnableSearchWithPlaceholderText()
    {
        var search = new ConfirmedSearchEnabled(true, "search");

        var result = search.GetSearchString();

        result.Should().Contain(".EnableSearch()").And.Contain(".SearchPlaceholderText(\"search\")");
    }

    [Fact]
    public void GetSearchString_NullConfirmedSearchEnabled_ReturnsEmptyString()
    {
        ConfirmedSearchEnabled? search = null;

        var result = search.GetSearchString();

        result.Should().BeEmpty();
    }
}