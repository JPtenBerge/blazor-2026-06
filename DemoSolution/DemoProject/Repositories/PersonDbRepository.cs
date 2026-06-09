using DemoProject.DataAccess;
using DemoProject.Entities;
using Microsoft.EntityFrameworkCore;

namespace DemoProject.Repositories;

public class PersonDbRepository(DemoContext context) : IPersonRepository
{
    public async Task<IEnumerable<Person>> GetAllAsync()
    {
        return await context.Persons.ToArrayAsync();
    }

    public async Task<Person> AddAsync(Person newPerson)
    {
        context.Persons.Add(newPerson);
        await context.SaveChangesAsync(); // koppelt de nieuwe Id
        return newPerson; // met nieuwe Id
    }
}
