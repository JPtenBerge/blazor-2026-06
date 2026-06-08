using System.ComponentModel.DataAnnotations;

namespace DemoProject.Entities;

public class Person
{
    public string Name { get; set; }

    public int? Age { get; set; }
    public string PhotoUrl { get; set; }
}
