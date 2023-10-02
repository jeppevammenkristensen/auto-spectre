// using AutoSpectre;
// using Microsoft.EntityFrameworkCore;
// using Spectre.Console;
//
// namespace Autospectre.Examples.Examples.Database;
//
// [AutoSpectreForm]
// public class CreateEmployeesForm
// {
//     private readonly CompanyDbContext _context;
//
//     public CreateEmployeesForm(CompanyDbContext context)
//     {
//         _context = context;
//     }
//
//     [TaskStep(UseStatus = true, StatusText = "Initializing",SpinnerStyle = "slowblink",SpinnerType = SpinnerKnownTypes.CircleQuarters)]
//     public async Task Initalizing(IAnsiConsole console)
//     {
//         var employees = await _context.Employees
//             .Include(x => x.Title)
//             .ToListAsync();
//
//         foreach (var employee in employees)
//         {
//             console.MarkupLineInterpolated($"Id:{employee.Id} Name: {employee.Name} Title:{employee.Title.Name} Paygroup:{employee.Title.Paygroup}");
//         }
//     }
//     
//     [TextPrompt]
//     public string Name { get; set; }
//
//     [TextPrompt(Title = "Use an existing title")]
//     public bool UseExistingTitle
//     {
//         get; set;
//     }
//
//     [TaskStep(UseStatus = true, StatusText = "Loading titles", Condition = nameof(UseExistingTitle))]
//     public async Task LoadTitles(IAnsiConsole console)
//     {
//         var titles = await _context.Titles.ToListAsync();
//         if (titles.Count == 0)
//         {
//             console.MarkupLine("[red]No titles available[/]");
//         }
//
//         Titles = titles;
//     }
//
//     public List<Title> Titles { get; set; }
//
//     [SelectPrompt(Source = nameof(Titles))]
//     public Title ExistingTitle { get; set; }
//
//     public string ExistingTitleConverter(Title title)
//     {
//         return $"[red]{title.Name}-{title.Paygroup}[/]";
//     }
//     
//     public bool ExistingTitleCondition => UseExistingTitle && Titles.Count > 0;
//
//     [TextPrompt(Title = "Add data for new title",Condition = nameof(ExistingTitleCondition), NegateCondition = true)]
//     public CreateTitle NewTitle { get; set; }
//
//     public string? NewTitleValidator(CreateTitle title)
//     {
//         return _context.Titles
//             .Any(x => x.Name == title.Name) ? $"The title {title.Name} already exist" : null;
//     }
//
//     [TaskStep(UseStatus = true, StatusText = "Creating title", Condition = nameof(ExistingTitleCondition), NegateCondition = true)]
//     public async Task CreateTitle()
//     {
//         var title = new Title(NewTitle.Name)
//         {
//             Paygroup = NewTitle.Paygroup
//         };
//         _context.Titles.Add(title);
//         _ = await _context.SaveChangesAsync();
//         CreatedTitle = title;
//     }
//     
//     
//
//     public Title CreatedTitle { get; set; }
//
//     [TaskStep]
//     public async Task Save(IAnsiConsole console)
//     {
//         var newEmployee = new Employee(Name);
//         var title = ExistingTitleCondition ? ExistingTitle : CreatedTitle;
//         newEmployee.Title = title;
//         _context.Employees.Add(newEmployee);
//         var _ = await _context.SaveChangesAsync();
//         console.MarkupLine("[green]Employee succesfully created[/]");
//     }
// }
//
// [AutoSpectreForm]
// public class CreateTitle
// {
//     [TextPrompt]
//     public string Name { get; set; }
//     
//     [TextPrompt]
//     public Paygroup Paygroup { get; set; }
// }
//
