using DemoProject.Entities;

namespace DemoProject.Components.Pages;

public partial class Home
{
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
}
