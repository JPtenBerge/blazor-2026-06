using BlazorApp1.Client.Repositories;
using DemoProject.Shared.Repositories;
using Duende.Bff.Blazor.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services
    .AddBffBlazorClient() // Provides auth state provider that polls the /bff/user endpoint
    .AddCascadingAuthenticationState();

//builder.Services.AddSingleton<IPersonRepository, PersonRepository>();
builder.Services.AddSingleton<IPersonRepository, PersonHttpRepository>();

await builder.Build().RunAsync();
