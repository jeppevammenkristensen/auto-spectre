namespace AutoSpectre.SourceGeneration.Evaluation;

public class ConfirmedValidator
{
    public ConfirmedValidator(string name, bool singleValidation, bool isStatic)
    {
        Name = name;
        SingleValidation = singleValidation;
        IsStatic = isStatic;
    }

    public string Name { get; set; }
    public bool SingleValidation { get; }
    public bool IsStatic { get; }
}