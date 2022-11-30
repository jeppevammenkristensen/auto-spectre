![Nuget](https://img.shields.io/nuget/v/AutoSpectre.SourceGeneration)

# Auto Spectre
Source generator project to generate classes that can be used in a console to prompt for values using Spectre.Console

## Short Guide
Decorate properties with AskAttribute for intance

```csharp
 [AutoSpectreForm]
 public class Someclass
    {
        [Ask(title: "[Green]Enter first name[/]")]   
        public string FirstName { get; set; }

        [Ask()]
        public string LastName { get; set; }

        [Ask()]
        public int Age { get; set; }        
    }
```

Behind the scenes this will generate an interface factory and implentation using `Spectre.Console` to prompt for the values. 

**Example output:**
```csharp
    public interface ISomeclassSpectreFactory
    {
        Someclass Get(Someclass destination = null);
    }
    
    public class SomeclassSpectreFactory : ISomeclassSpectreFactory
    {
        public Someclass Get(Someclass destination = null)
        {
           destination ??= new Test.Someclass();
           destination.FirstName = AnsiConsole.Ask<String>("[Green]Enter first name[/] ");
           destination.LastName = AnsiConsole.Ask<String>("Enter [green]LastName [/] ");
           destination.Age = AnsiConsole.Ask<Int32>("Enter [green]Age [/] ");
           return destination;
        }
    }

```
