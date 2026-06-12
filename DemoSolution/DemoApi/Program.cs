using DemoApi.Apis;
using DemoProject.Apis;
using DemoProject.DataAccess;
using DemoProject.Repositories;
using DemoProject.Shared.Auth;
using DemoProject.Shared.Repositories;
using Duende.IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.Authority = "https://localhost:5001";
    options.TokenValidationParameters = new()
    {
        ValidateAudience = false,
        ValidateIssuer = false,
        NameClaimType = JwtClaimTypes.Name
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("alleenbobmeth", policy =>
    {
        policy.AddRequirements(new BobMensenMetHRequirement());
    });

    options.AddPolicy("alleencoolemensen", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("name", "Alice Smith");
    });
}); 

builder.Services.AddSingleton<IAuthorizationHandler, BobMensenMetHHandler>();
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


app.Use(async (context, next) =>
{
    if (context.Request.Headers.ContainsKey("Authorization"))
    {
        Console.WriteLine($"Auth: {context.Request.Headers.Authorization}");
    }
    else
    {
        Console.WriteLine("Geen auth header");
    }
    await next(context);
});



app.UseHttpsRedirection();
app.MapPersonEndpoints();
app.MapTestEndpoints();

app.Run();
