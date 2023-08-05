namespace AutoSpectre.SourceGeneration;

public class ConfirmedValidator
{
    public ConfirmedValidator(string name, bool singleValidation)
    {
        Name = name;
        SingleValidation = singleValidation;
    }

    public string Name { get; set; }
    public bool SingleValidation { get; }
}