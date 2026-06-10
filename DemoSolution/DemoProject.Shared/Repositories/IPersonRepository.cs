using DemoProject.Shared.Entities;

namespace DemoProject.Shared.Repositories;

public interface IPersonRepository
{
    Task<Person> AddAsync(Person newPerson);
    Task<IEnumerable<Person>> GetAllAsync();
}