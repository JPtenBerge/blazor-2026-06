using BlazorApp1.Client.Repositories;
using DemoProject.Shared.Repositories;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

//builder.Services.AddSingleton<IPersonRepository, PersonRepository>();
builder.Services.AddSingleton<IPersonRepository, PersonHttpRepository>();

await builder.Build().RunAsync();
