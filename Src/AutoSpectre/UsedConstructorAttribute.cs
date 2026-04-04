using System;

namespace AutoSpectre;

/// <summary>
/// Marks a constructor to be used by the generated factory when creating an instance of the form.
/// Use this when the form type has multiple constructors and you want to specify which one to use.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public class UsedConstructorAttribute : Attribute
{
    
}