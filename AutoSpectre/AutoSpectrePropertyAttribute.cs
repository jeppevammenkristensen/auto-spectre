using System;

namespace AutoSpectre;

public class AutoSpectrePropertyAttribute : Attribute
{
    public string? Title { get; set; }

    public string? Condition { get; set; }

    public bool NegateCondition { get; set; }
}