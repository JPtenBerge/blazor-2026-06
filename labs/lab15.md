# Lab 15: Distributed Blazor with OIDC

In this lab, you will implement a distributed Blazor application architecture that includes a separate Web API for managing comments and integrates OIDC-based authentication using Duende Identity Server. You’ll move the comments-related code from your Blazor Server project to a dedicated Web API project, secure the API using JWT bearer tokens, and configure your Blazor app to authenticate users and communicate with the API.

# IMPORTANT: This lab does not start from the end of lab 14, but rather from the end of lab 13. It is an alternative to lab 14, and you should choose one or the other. Lab 14 explores Authentication and Authorization in a monolithic Blazor application, while this lab focuses on a distributed architecture with a separate API and Identity Server.

### Step 1: Create the API Project

1. **Create a New ASP.NET Web API Project**:
   - **Project Name**: Choose a name such as `PhotoSharingApplication.CommentsApi`.
   - **Authentication**: None.
   - **HTTPS**: Enabled.
   - **Use Controllers**: Yes.

2. **Add Necessary NuGet Packages**:
   - Add the following packages:
     - `Microsoft.EntityFrameworkCore.SqlServer`
     - `Microsoft.EntityFrameworkCore.Tools`

3. **Move the CommentsController**:
   - From your existing Blazor Server project, move the `Controllers\CommentsController.cs` file to the API project.

4. **Copy Required Files**:
   - Copy the following files from your Blazor Server project to the API project:
     - `Core\Interfaces\ICommentsRepository.cs`
     - `Core\Models\Comment.cs`
     - `Infrastructure\Repositories\CommentsRepository.cs`
     - `Infrastructure\Data\PhotosDbContext.cs`
     - The ConnectionString from `appsettings.json`.

5. **Modify the Namespace**:
   - Adjust the namespace in each file to match the folder structure in your API project.

6. **Rename the DbContext**:
   - Rename `PhotosDbContext` to `CommentsDbContext` and remove the `Photo` entity and its configuration.

7. **Add the DbContext to the Service Collection**:
   - In `Program.cs`, register the `CommentsDbContext`:

   ```csharp
   builder.Services.AddDbContext<CommentsDbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("CommentsDbContext")));
   ```

8. **Update the Connection String**:
   - In `appsettings.json`, change the connection string to point to a new database, such as `CommentsDb`.

9. **Add a Migration and Update the Database**:
   - Use the following commands in the Package Manager Console:

   ```bash
   Add-Migration Initial -Project PhotoSharingApplication.CommentsApi -StartupProject PhotoSharingApplication.CommentsApi
   Update-Database -Project PhotoSharingApplication.CommentsApi -StartupProject PhotoSharingApplication.CommentsApi
   ```

### Step 2: Test the API

1. **Test Using Swagger UI**:
   - Start the API project and navigate to the Swagger UI. Test the endpoints to ensure they work correctly.
   
2. **Optional: Test Using .http Files**:
   - If you have `.http` files from your Blazor Server project, copy them to the API project and test the endpoints.

### Step 3: Clean Up the Blazor Server Project

Before adding authentication, clean up the Blazor Server project and ensure it can communicate with the new API.

1. **Modify the DbContext**:
   - Remove the `Comment` entity and its configuration from `PhotosDbContext`.

2. **Add a Migration and Update the Database**:
   - Use the following commands in the Package Manager Console:

   ```bash
   Add-Migration Initial -Project PhotoSharingApplication.Frontend.BlazorWebAssembly -StartupProject PhotoSharingApplication.Frontend.BlazorWebAssembly
   Update-Database -Project PhotoSharingApplication.Frontend.BlazorWebAssembly -StartupProject PhotoSharingApplication.Frontend.BlazorWebAssembly
   ```

3. **Update Program.cs**:
   - Remove the following lines from `Program.cs`:

   ```csharp
   builder.Services.AddControllers();
   builder.MapControllers();
   ```

4. **Move and Adjust CommentsRepository**:
   - Copy `CommentsRepository` from the Client project to the Server project, adjusting the namespace as needed.
   - Inject and configure `HttpClient` in `Program.cs`:

   ```csharp
   builder.Services.AddHttpClient<ICommentsRepository, CommentsRepository>(httpClient =>
   {
       httpClient.BaseAddress = new Uri("https://localhost:{{API_PORT}}");
   });
   ```

   - Replace `{{API_PORT}}` with the actual port of your API.

### Step 4: Forward API Calls Using YARP

1. **Add YARP Package**:
   - Add the `YARP` package to your Blazor Server project.

2. **Configure YARP in Program.cs**:
   - Add the YARP Forwarder services:

   ```csharp
   builder.Services.AddHttpForwarder();
   ```

   - Add the Forwarder Middleware:

   ```csharp
   app.MapForwarder("/api/{**catch-all}", "https://localhost:{{API_PORT}}");
   ```

   - Replace `{{API_PORT}}` with the actual port of your API.

### Step 5: Test the Blazor Server

1. **Set Multiple Startup Projects**:
   - Set both the API and Blazor Server projects as startup projects.

2. **Run the Application**:
   - Start the API and Blazor Server projects. Ensure that the Blazor app can communicate with the API, and that all features work as expected.

### Step 6: Add Security to the API

To facilitate security configuration, you’ll change the routes of the `POST`, `PUT`, and `DELETE` methods to a new path starting with `apiauth`. This allows you to add bearer token authentication to these routes while leaving `GET` methods accessible without a token.

1. **Update the CommentsController**:
   - Change the routes of the `POST`, `PUT`, and `DELETE` methods:

   ```csharp
   [HttpPut("/apiauth/comments/{id}")]
   public async Task<IActionResult> PutComment(int id, Comment comment)
   ```

   ```csharp
   [HttpPost("/apiauth/comments/")]
   public async Task<ActionResult<Comment>> PostComment(Comment comment)
   ```

   ```csharp
   [HttpDelete("/apiauth/comments/{id}")]
   public async Task<ActionResult<Comment>> DeleteComment(int id)
   ```

2. **Update CommentsRepository in Both Projects**:
   - Adjust the `POST`, `PUT`, and `DELETE` method calls in `CommentsRepository`:

   ```csharp
   public async Task<Comment> AddCommentAsync(Comment comment)
   {
       HttpResponseMessage response = await httpClient.PostAsJsonAsync("/apiauth/comments", comment);
       return await response.Content.ReadFromJsonAsync<Comment>();
   }

   public async Task DeleteCommentAsync(int id)
   {
       await httpClient.DeleteAsync($"/apiauth/comments/{id}");
   }

   public async Task<Comment> UpdateCommentAsync(Comment comment)
   {
       HttpResponseMessage response = await httpClient.PutAsJsonAsync($"/apiauth/comments/{comment.Id}", comment);
       return comment;
   }
   ```

3. **Test the Application**:
   - Run the application. The `POST`, `PUT`, and `DELETE` methods should not work anymore, while the `GET` methods should still function. This is because the secure routes require authentication, which will be implemented next.

### Step 7: Set Up Duende Identity Server

1. **Install Duende Identity Server Templates**:
   - Install the Duende templates using the following command:

   ```bash
   dotnet new -i Duende.IdentityServer.Templates
   ```

2. **Create a New Identity Server Project**:
   - In Visual Studio, create a new project using the `Duende Identity Server with In-Memory Stores and Test Users` template.
   - Name it `PhotoSharingApplication.DuendeIdentityProvider`.

### Step 8: Configure Duende Identity Server

1. **Define an API Scope**:
   - Open `Config.cs` and add an `ApiScope`:

   ```csharp
   public static IEnumerable<ApiScope> ApiScopes =>
   [
       new ApiScope(name: "comments", displayName: "Comments API")
   ];
   ```

2. **Define a Client**:
   - Add this client definition to `Config.cs`:

   ```csharp
   public static IEnumerable<Client> Clients =>
   [
       new Client()
       {
           ClientId = "photosharingapplication",
           ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

           AllowedGrantTypes = GrantTypes.Code,

           RedirectUris = { "https://localhost:{{YOUR_BLAZOR_SERVER_PORT}}/signin-oidc" },
           FrontChannelLogoutUri = "https://localhost:{{YOUR_BLAZOR_SERVER_PORT}}/signout-oidc",
           PostLogoutRedirectUris = { "https://localhost:{{YOUR_BLAZOR_SERVER_PORT}}/signout-callback-oidc" },

           AllowOfflineAccess = true,
           AllowedScopes = { 
               IdentityServerConstants.StandardScopes.OpenId,
               IdentityServerConstants.StandardScopes.Profile, 
               "comments" // Must match the ApiScope name
           }
       }
   ];
   ```

   - Replace `{{YOUR_BLAZOR_SERVER_PORT}}` with the actual port of your Blazor Server.

### Step 9: Secure the API with JWT Bearer Authentication

1. **Add JWT Bearer Authentication**:
   - Add the `Microsoft.AspNetCore.Authentication.JwtBearer` package to the API project.

2. **Configure JWT Authentication**:
   - Add authentication and authorization services in `Program.cs`:

   ```csharp
   builder.Services.AddAuthentication()
       .AddJwtBearer(options =>
       {
           options.Authority = "https://localhost:{{IDENTITY_SERVER_PORT}}";
           options.TokenValidationParameters.ValidateAudience = false;
       });
   builder.Services.AddAuthorization();
   ```

   - Replace `{{IDENTITY_SERVER_PORT}}` with the actual port of your Identity Server (likely 5001).

3. **Authorize Controller Actions**:
   - Add the `[Authorize]` attribute to the `POST`, `PUT`, and `DELETE` actions in `CommentsController`.

### Step 10: Configure Blazor Server for Authentication

1. **Download Blazor Samples**:
   - Download the Blazor samples from `https://github.com/dotnet/blazor-samples`.
   - Open the `BlazorWebAppOidcBff/BlazorWebAppWithOidc` sample.

2. **Add Authentication Packages**:
   - Add the following package to the Blazor Server project:
     - `Microsoft.AspNetCore.Authentication.OpenIdConnect`
   - Add the following packages to the Client project:
     - `Microsoft.AspNetCore.Components.WebAssembly.Authentication`
     - `Microsoft.Extensions.Http`

3. **Copy Authentication Files**:
   - From the `BlazorWebAppOidc` project, copy the following files to the Blazor Server project:
     - `BlazorWebAppOidc/CookieOidcRefresher.cs`
     - `CookieOidcServiceCollectionExtensions.cs`
     - `LoginLogoutEndpointRouteBuilderExtensions.cs`
     - `PersistingAuthenticationStateProvider.cs`
   - From the `BlazorWebAppOidc.Client` project, copy the following files to the Blazor Client project:
     - `PersistentAuthenticationStateProvider.cs`
     - `UserInfo.cs`
     - `Pages/UserClaims.razor`
   - From the `BlazorWebAppOidc.Client` project, copy the following files to the Blazor Server project:
     - `RedirectToLogin.razor`
     - `Layout/LoginOrOut.razor`

4. **Customize `LoginOrOut.razor`**:
   - Modify the style of the `LoginOrOut.razor` component to match the style of your `NavMenu` component:

   ```csharp
   <AuthorizeView>
       <Authorized>
           <ul class="navbar-nav mb-2 mb-md-0">
               <li class="nav-item">
                   <form class="d-flex" action="authentication/logout" method="post">
                       <AntiforgeryToken />
                       <input type="hidden" name="ReturnUrl" value="@currentUrl" />
                       <button type="submit" class="nav-link">
                           Logout @context.User.Identity?.Name
                       </button>
                   </form>
               </li>
           </ul>
       </Authorized>
       <NotAuthorized>
           <ul class="navbar-nav mb-2 mb-md-0">
               <li class="nav-item">
                   <NavLink class="nav-link" href="authentication/login">
                       Login
                   </NavLink>
               </li>
           </ul>
       </NotAuthorized>
   </AuthorizeView>
   ```

5. **Configure the Server**:
   - In `Program.cs` of the Blazor Server project, add:

   ```csharp
   const string MS_OIDC_SCHEME = "MicrosoftOidc";

   builder.Services.AddAuthentication(options =>
   {
       options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
       options.DefaultChallengeScheme = MS_OIDC_SCHEME;
   })
       .AddOpenIdConnect(MS_OIDC_SCHEME, oidcOptions =>
        {
            oidcOptions.Scope.Add("comments");

            oidcOptions.Authority = "https://localhost:5001";

            oidcOptions.ClientId = "photosharingapplication";
            oidcOptions.ClientSecret = "49C1A7E1-0C79-4A89-A3D6-A37998FB86B0";
            oidcOptions.ResponseType = OpenIdConnectResponseType.Code;
            
            oidcOptions.MapInboundClaims = false;
            
            oidcOptions.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
            oidcOptions.TokenValidationParameters.RoleClaimType = "role";
            
            oidcOptions.GetClaimsFromUserInfoEndpoint = true;
        })
       .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

   builder.Services.ConfigureCookieOidcRefresh(CookieAuthenticationDefaults.AuthenticationScheme, MS_OIDC_SCHEME);

   builder.Services.AddAuthorization();

   builder.Services.AddCascadingAuthenticationState();

   builder.Services.AddScoped<AuthenticationStateProvider, PersistingAuthenticationStateProvider>();

   builder.Services.AddHttpContextAccessor();
   ```

   - Before `app.Run();` add:

   ```csharp
   app.MapForwarder("/apiauth/{**catch-all}", "https://localhost:7123", transformBuilder =>
   {
       transformBuilder.AddRequestTransform(async transformContext =>
       {
           var accessToken = await transformContext.HttpContext.GetTokenAsync("access_token");
           transformContext.ProxyRequest.Headers.Authorization = new("Bearer", accessToken);
       });
   }).RequireAuthorization();

   app.MapGroup("/authentication").MapLoginAndLogout();
   ```

### Step 11: Configure the Blazor Client

1. **Update `_Imports.razor`**:
   - Add the following line:

   ```csharp
   @using Microsoft.AspNetCore.Components.Authorization
   ```

2. **Update `Program.cs`**:
   - Add the following services:

   ```csharp
   builder.Services.AddAuthorizationCore();
   builder.Services.AddCascadingAuthenticationState();
   builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();
   ```

### Step 12: Secure the Pages

1. **Secure Specific Pages**:
   - Add the `[Authorize]` attribute to `Photos/Editor.razor` and `Photos/Delete.razor`:

   ```csharp
   @attribute [Authorize]
   ```

2. **Update the Router**:
   - In the `Routes.razor` component of the Server project, include:

   ```csharp
   <Router AppAssembly="typeof(Program).Assembly" AdditionalAssemblies="new[] { typeof(Client._Imports).Assembly }">
       <Found Context="routeData">
           <AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)">
               <NotAuthorized>
                   <RedirectToLogin />
               </NotAuthorized>
           </AuthorizeRouteView>
           <FocusOnNavigate RouteData="routeData" Selector="h1" />
       </Found>
   </Router>
   ```

   - Ensure that `_Imports.razor` includes:

   ```csharp
   @using Microsoft.AspNetCore.Components.Authorization
   ```

3. **Update the Navigation Menu**:
   - Modify `NavMenu.razor` to include the `LoginOrOut` component and conditionally render links based on user authorization:

   ```csharp
   <nav class="navbar navbar-expand-md navbar-dark fixed-top bg-dark">
       <div class="container-fluid">
           <a class="navbar-brand" href="#">Photo Sharing Application</a>
           <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarCollapse" aria-controls="navbarCollapse" aria-expanded="false" aria-label="Toggle navigation">
               <span class="navbar-toggler-icon"></span>
           </button>
           <div class="collapse navbar-collapse" id="navbarCollapse">
               <ul class="navbar-nav me-auto mb-2 mb-md-0">
                   <li class="nav-item">
                       <NavLink class="nav-link" href="" Match="NavLinkMatch.All"> Home </NavLink>
                   </li>
                   <li class="nav-item">
                       <NavLink class="nav-link" href="photos/all"> Photos </NavLink>
                   </li>
                   <AuthorizeView>
                       <Authorized>
                           <li class="nav-item">
                               <NavLink class="nav-link" href="photos/create"> Add Photo </NavLink>
                           </li>
                           <li class="nav-item">
                               <NavLink class="nav-link" href="user-claims"> Claims </NavLink>
                           </li>
                       </Authorized>
                   </AuthorizeView>
               </ul>
               <LogInOrOut />
           </div>
       </div>
   </nav>
   ```

### Step 13: Test the Authentication Flow

1. **Set Up Multiple Startup Projects**:
   - Configure the solution to start the Identity Server, the API, and the Blazor Server by right-clicking on the Solution Explorer and selecting `Set Startup Projects`.

2. **Run the Solution**:
   - Start the solution and navigate to the Blazor Server.
   - Attempt to add a photo. You should be redirected to the Identity Server for authentication.
   - Log in with the test user (e.g., `alice` with password `alice`).
   - You should be redirected back to the Blazor Server and be able to add the photo.

3. **Test Comment Functionality**:
   - Restart the browser and start the solution again.
   - Attempt to add a comment. If everything is configured correctly, the comment should be added successfully after authentication.

### Fun Experiment

1. **Test User Claims**:
   - The `UserClaims.razor` page displays user claims. To experiment with this, add the following directive to the top of the `UserClaims.razor` file:

   ```csharp
   @rendermode InteractiveAuto
   ```

   - This allows you to observe how claims are handled differently on the server and client sides. The server renders the claims with full knowledge, while the client only receives essential information, ensuring secure handling of user data.

### Next Steps

In the next lab, you'll implement resource-based authorization to ensure that only the owners of a photo or comment can edit or delete it.