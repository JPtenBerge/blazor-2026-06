using BlazorApp1.Client.Repositories;
using DemoProject.Shared.Repositories;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddSingleton<IPersonRepository, PersonRepository>();

await builder.Build().RunAsync();
