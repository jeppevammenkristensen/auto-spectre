using System;
using AutoSpectre;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            ISomeclassSpectreFactory factory = new SomeclassSpectreFactory();
            var item = new Someclass();
            factory.Get(item);
            
            Console.WriteLine(item.MiddleName);
        }
    }

    public class Someclass
    {
        [Ask(title: "[Green]Enter first name[/]")]   
        public string FirstName { get; set; }

        [Ask()]
        public string? MiddleName { get; set; }

        [Ask()]
        public string LastName { get; set; }

        [Ask()]
        public int Age { get; set; }
        
    }
}
