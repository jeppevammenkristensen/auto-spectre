using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests.Evaluation;

public class SingleMethodEvaluationContextConfirmationsTests
{
    [Fact]
    public void Confirmations_ReturnsEveryConfirmedProperty()
    {
        var confirmedProperties = typeof(SingleMethodEvaluationContext)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.Name.StartsWith("Confirmed"))
            .ToList();

        var instance = (SingleMethodEvaluationContext)RuntimeHelpers
            .GetUninitializedObject(typeof(SingleMethodEvaluationContext));

        var sentinels = confirmedProperties.ToDictionary(
            p => p,
            p => RuntimeHelpers.GetUninitializedObject(p.PropertyType));

        foreach (var (property, sentinel) in sentinels)
        {
            property.SetValue(instance, sentinel);
        }

        instance.Confirmations.Should().BeEquivalentTo(
            sentinels.Values,
            because: "Confirmations must yield every property on SingleMethodEvaluationContext " +
                     "whose type name starts with 'Confirmed'. If this test fails after adding a new " +
                     "Confirmed* property, also add it to the Confirmations list.");
    }
}
