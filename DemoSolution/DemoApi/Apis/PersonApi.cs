using DemoProject.Shared.Entities;
using DemoProject.Shared.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DemoProject.Apis;

public static class PersonApi
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public void MapPersonEndpoints()
        {
            var group = endpoints.MapGroup("api/persons");
            group.MapGet("/", GetAll);
            group.MapGet("/{id:int}", Get);
            group.MapPost("/", Post);
        }
    }

    public static async Task<IEnumerable<Person>> GetAll(IPersonRepository repo)
    {
        return await repo.GetAllAsync();
    }

    public static async Task<Results<NotFound<string>, Ok<Person>>> Get(int id, IPersonRepository repo)
    {
        var veelTeVeel = await repo.GetAllAsync();
        var person = veelTeVeel.SingleOrDefault(x => x.Id == id);
        return person is null ? TypedResults.NotFound($"Person with id {id} does not exist.") : TypedResults.Ok(person);
    }

    public static async Task<Created<Person>> Post(IPersonRepository repo, Person newPerson)
    {
        var updatedPerson = await repo.AddAsync(newPerson);
        return TypedResults.Created("", updatedPerson);
    }
}
