using Spectre.Console;

namespace AutoSpectre;

/// <summary>
/// Use this on a Method or Task (can be async) to perform some kind of operation
/// It can be simple like doing some processing based on previously prompted values
/// or it could be a database lookup . 
/// </summary>

public class TaskStepAttribute : MethodBasedAttribute
{
    /// <summary>
    /// The title can be left out for a <see cref="TaskStepAttribute"/>
    /// </summary>
    public override string? Title { get; set; }

    
   
}