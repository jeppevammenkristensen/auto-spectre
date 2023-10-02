// using Microsoft.EntityFrameworkCore;
//
// namespace Autospectre.Examples.Examples.Database;
//
// public class CompanyDbContext : DbContext
// {
//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//     {
//         optionsBuilder.UseInMemoryDatabase("CompanyDb");
//     }
//
//     public DbSet<Employee> Employees { get; set; }
//     public DbSet<Title> Titles { get; set; }
// }
//
// public class Employee
// {
//     public Employee(string name)
//     {
//         Name = name;
//     }
//
//     public int Id { get; set; }
//     public string Name { get; set; }
//     
//     public int TitleId { get; set; }
//     public Title Title { get; set; }
// }
//
// public class Title
// {
//     public Title(string name)
//     {
//         Name = name;
//     }
//
//     public int Id { get; set; }
//     public string Name { get; set; }
//     
//     public Paygroup Paygroup { get; set; }
// }
//
// public enum Paygroup
// {
//     Group1,
//     Group2,
//     Group3
// }