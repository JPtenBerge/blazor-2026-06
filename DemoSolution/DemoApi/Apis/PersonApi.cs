using DemoProject.Shared.Auth;
using DemoProject.Shared.Entities;
using DemoProject.Shared.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using System.Security.Principal;

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
            group.MapPut("/{id:int}", Put);
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

    public static async Task<Results<Ok<Person>, ForbidHttpResult>> Put(int id, ClaimsPrincipal principal,
        IAuthorizationService authService, IPersonRepository repo, Person person)
    {
        var veelTeVeel = await repo.GetAllAsync();
        var personEntity = veelTeVeel.SingleOrDefault(x => x.Id == id);

        var result = await authService.AuthorizeAsync(principal, personEntity, "alleenbobmeth");
        if (!result.Succeeded)
        {
            return TypedResults.Forbid();
        }

        // TODO: actually update db
        return TypedResults.Ok(person);

    }
}
