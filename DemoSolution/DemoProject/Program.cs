using DemoProject.Components;
using DemoProject.DataAccess;
using DemoProject.Repositories;
using DemoProject.Shared.Entities;
using DemoProject.Shared.Repositories;
using DemoProject.Shared.Validators;
using Duende.Bff;
using Duende.Bff.Blazor;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddOpenApi();
builder.Services.AddTransient<IValidator<Person>, PersonValidator>();
builder.Services.AddTransient<IPersonRepository, PersonDbRepository>();
builder.Services.AddMudServices();
builder.Services.AddDbContextFactory<DemoContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DemoContext"));
}, ServiceLifetime.Transient);

// CORS is nu nog niet nodig, maar mocht je je WASM standalone hosten dan wordt dit nodig:
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("blazorfrontend", policy =>
//    {
//        policy.WithOrigins("https://localhost:7085").AllowAnyHeader().AllowCredentials().AllowAnyMethod();
//    });
//});

builder.Services.AddBff()
    .AddServerSideSessions() // Add in-memory implementation of server side sessions
    .AddBlazorServer();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "cookie";
    options.DefaultChallengeScheme = "oidc";
    options.DefaultSignOutScheme = "oidc";
})
    .AddCookie("cookie", options =>
    {
        options.Cookie.Name = "__Host-blazor";

        // Because we use an identity server that's configured on a different site
        // (duendesoftware.com vs localhost), we need to configure the SameSite property to Lax. 
        // Setting it to Strict would cause the authentication cookie not to be sent after loggin in.
        // The user would have to refresh the page to get the cookie.
        // Recommendation: Set it to 'strict' if your IDP is on the same site as your BFF.
        options.Cookie.SameSite = SameSiteMode.Lax;
    })
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = "https://localhost:5001";
        options.ClientId = "blazorfrontend";
        options.ClientSecret = "304F15BB-6C25-4BCC-A741-0BED7C0DCB10";
        options.ResponseType = "code";
        options.ResponseMode = "query";

        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = true;
        options.MapInboundClaims = false;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        //options.Scope.Add("api");
        options.Scope.Add("offline_access");

        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "role";
    });

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(opts =>
    {
        opts.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        opts.DisableMcp();
        opts.DisableAgent();
    });
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();

// Add the BFF middleware which performs anti forgery protection
app.UseBff();
app.UseAuthorization();


app.UseAntiforgery();
app.MapBffManagementEndpoints();


//app.UseCors("blazorfrontend");

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorApp1.Client._Imports).Assembly);


app.Run();
