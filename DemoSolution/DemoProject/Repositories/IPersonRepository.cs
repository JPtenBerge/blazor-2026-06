using DemoProject.Entities;

namespace DemoProject.Repositories;

public interface IPersonRepository
{
    Task<Person> AddAsync(Person newPerson);
    Task<IEnumerable<Person>> GetAllAsync();
}