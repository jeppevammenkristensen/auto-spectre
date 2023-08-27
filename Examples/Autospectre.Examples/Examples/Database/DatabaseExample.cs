namespace Autospectre.Examples.Examples.Database;

public class DatabaseExample : IExample
{
    public async Task Run()
    {
        var form = new CreateEmployeesForm(new CompanyDbContext());
        var factory = new CreateEmployeesFormSpectreFactory();
        await factory.GetAsync(form);
        
    }
}