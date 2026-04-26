using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests.Evaluation;

public class SinglePropertyEvaluationContextConfirmationsTests
{
    [Fact]
    public void Confirmations_ReturnsEveryConfirmedProperty()
    {
        var confirmedProperties = typeof(SinglePropertyEvaluationContext)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.Name.StartsWith("Confirmed"))
            .ToList();

        var instance = (SinglePropertyEvaluationContext)RuntimeHelpers
            .GetUninitializedObject(typeof(SinglePropertyEvaluationContext));

        var sentinels = confirmedProperties.ToDictionary(
            p => p,
            p => RuntimeHelpers.GetUninitializedObject(p.PropertyType));

        foreach (var (property, sentinel) in sentinels)
        {
            property.SetValue(instance, sentinel);
        }

        var yielded = instance.Confirmations.ToList();
        var missing = sentinels
            .Where(kvp => !yielded.Contains(kvp.Value))
            .Select(kvp => kvp.Key.Name)
            .ToList();

        missing.Should().BeEmpty(
            because: $"Confirmations must yield every property on SinglePropertyEvaluationContext " +
                     "whose type name starts with 'Confirmed'. If this test fails after adding a new " +
                     $"Confirmed* property, also add it to the Confirmations list. Missing: " +
                     $"{string.Join(", ", missing)}");
    }
}
