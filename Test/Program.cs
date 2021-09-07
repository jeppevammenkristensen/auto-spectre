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
            factory.Get();
            
            Console.WriteLine(item.FirstName);
        }
    }

    public class Someclass
    {
        [Ask(title: "[Green]Enter first name[/]")]   
        public string FirstName { get; set; }

        [Ask()]
        public string LastName { get; set; }

        [Ask()]
        public int Age { get; set; }
        
    }
}
