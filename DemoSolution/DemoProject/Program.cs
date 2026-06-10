using DemoProject.Components;
using DemoProject.DataAccess;
using DemoProject.Shared.Entities;
using DemoProject.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using DemoProject.Shared.Repositories;
using DemoProject.Shared.Validators;
using DemoProject.Apis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddTransient<IValidator<Person>, PersonValidator>();
builder.Services.AddTransient<IPersonRepository, PersonDbRepository>();
builder.Services.AddMudServices();
builder.Services.AddDbContextFactory<DemoContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DemoContext"));
}, ServiceLifetime.Transient);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof (BlazorApp1.Client._Imports).Assembly);

app.MapPersonEndpoints();

app.Run();
