using System;

namespace AutoSpectre;

/// <summary>
/// Decorate a method with this attribute to break out of the code generation if a given condition is met.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class BreakAttribute : MethodBasedAttribute, IConditionAttribute
{
}