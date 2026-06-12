using DemoProject.Shared.Entities;
using DemoProject.Shared.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DemoApi.Apis;

public static class TestApi
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public void MapTestEndpoints()
        {
            var group = endpoints.MapGroup("api/test");
            group.MapGet("/", Get);
        }
    }

    public static string Get(IPersonRepository repo)
    {
        return $"Werkt! {DateTime.Now.ToShortTimeString()}";
    }

}
