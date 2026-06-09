using DemoProject.Entities;
using DemoProject.Repositories;
using Microsoft.AspNetCore.Components;

namespace DemoProject.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject] public IPersonRepository PersonRepository { get; set; } = null!;

    [SupplyParameterFromForm(FormName = "AddPersonForm")] // ge-POSTe wordt hierin gebind
    public Person NewPerson { get; set; } = new();

    [SupplyParameterFromForm(FormName = "AddPersonForm")]
    public IFormFile CV { get; set; }

    public List<Person>? People { get; set; }

    public string Name { get; set; } = "Luuk";

    string DoeIets()
    {
        return $"Hallo, {Name}!";
    }

    async Task AddPerson()
    {
        Console.WriteLine("hier kom ik");
        if (NewPerson is not null)
        {
            Console.WriteLine("hey nieuw persoon aanmaken: " + NewPerson.Name);
            await PersonRepository.AddAsync(NewPerson);
            await RefreshPeople();
        }


        if (CV is not null)
        {
            var stream = CV.OpenReadStream();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync(); // 4GB?
            Console.WriteLine($"Geuploade content: {content}");
        }
    }

    protected override async Task OnInitializedAsync() => await RefreshPeople();

    private async Task RefreshPeople()
    {
        People = [.. await PersonRepository.GetAllAsync()];

    }
}
