using System.ComponentModel.DataAnnotations;

namespace DemoProject.Entities;

public class Person
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int? Age { get; set; }
    public string PhotoUrl { get; set; }
}
