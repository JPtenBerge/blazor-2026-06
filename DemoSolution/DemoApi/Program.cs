using DemoProject.Apis;
using DemoProject.DataAccess;
using DemoProject.Repositories;
using DemoProject.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IPersonRepository, PersonDbRepository>();
builder.Services.AddOpenApi();
builder.Services.AddDbContextFactory<DemoContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DemoContext"));
}, ServiceLifetime.Transient);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapPersonEndpoints();
app.Run();
