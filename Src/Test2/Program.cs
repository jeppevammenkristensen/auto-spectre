using AutoSpectre;


var someForm = new SomeForm();
someForm.Prompt();


[AutoSpectreForm]
public class SomeForm
{
    [TextPrompt(ClearOnFinish = true, Title = "Enter your name", EditableDefaultValue = true, DefaultValueSource = nameof(NameDefault))]
    public string Name {get;set;} = string.Empty;
    
    public readonly string NameDefault = "Jeppe";
}