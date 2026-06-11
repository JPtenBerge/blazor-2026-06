using DemoProject.Shared.Entities;
using DemoProject.Shared.Repositories;
using Flurl.Http;
using System.Net.Http.Json;

namespace BlazorApp1.Client.Repositories;

public class PersonHttpRepository : IPersonRepository
{
    public async Task<IEnumerable<Person>> GetAllAsync()
    {
        return await "https://localhost:7085/api/persons".GetJsonAsync<IEnumerable<Person>>();

        // typed http client
        // - GetFromJsonAsync ondersteunt geen headers meesturen
        // - PostAsJsonAsync kan de response niet parsen

        //var http = new HttpClient();
        //var persons = await http.GetFromJsonAsync<IEnumerable<Person>>("api/persons");

        //http.PostAsJsonAsync<>
        //http.SendAsync()
    }

    public async Task<Person> AddAsync(Person newPerson)
    {
        return await "https://localhost:7085/api/persons".PostJsonAsync(newPerson).ReceiveJson<Person>();

        //newPerson.Id = _persons.Any() ? _persons.Max(x => x.Id) + 1 : 1;
        //_persons.Add(newPerson);
        //return newPerson;
    }
}