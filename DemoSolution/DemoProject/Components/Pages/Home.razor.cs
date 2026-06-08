using DemoProject.Entities;
using Microsoft.AspNetCore.Components;

namespace DemoProject.Components.Pages;

public partial class Home : ComponentBase
{
    [SupplyParameterFromForm(FormName = "AddPersonForm")] // ge-POSTe wordt hierin gebind
    public Person NewPerson { get; set; } = new();

    public List<Person>? People { get; set; } = new()
    {
        new()
        {
            Age = 63,
            Name = "Bob",
            PhotoUrl = "https://example.com/photos/bob.jpg"
        },
        new()
        {
            Age = 30,
            Name = "Alice",
            PhotoUrl = "https://example.com/photos/alice.jpg"
        },
        new()
        {
            Age = 23,
            Name = "Eve",
            PhotoUrl = "https://example.com/photos/eve.jpg"
        }
    };

    public string Name { get; set; } = "Luuk";

    string DoeIets()
    {
        return $"Hallo, {Name}!";
    }

    void AddPerson()
    {
        Console.WriteLine("hier kom ik");
        if (NewPerson is not null)
        {
            Console.WriteLine("hey nieuw persoon aanmaken: " + NewPerson.Name);
        }
    }

}
