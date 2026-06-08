using DemoProject.Entities;

namespace DemoProject.Repositories;

public class PersonRepository : IPersonRepository
{
    private List<Person> _persons = new()
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

    public async Task<IEnumerable<Person>> GetAllAsync()
    {
        return _persons;
    }

    public async Task<Person> AddAsync(Person newPerson)
    {
        newPerson.Id = _persons.Any() ? _persons.Max(x => x.Id) + 1 : 1;
        _persons.Add(newPerson);
        return newPerson;
    }
}
