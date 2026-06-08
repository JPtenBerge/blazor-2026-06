# Lab 14: Implementing Authentication and Simple Authorization

In this lab, you will add authentication and simple authorization to your existing Blazor Web App by integrating Identity Framework with individual user accounts. This will ensure that only authenticated users can create, edit, and delete photos. You'll create a new project with Individual Accounts authentication, then copy the necessary files and code into your existing lab project. This process will enable your existing project to use the Identity Framework for authentication and authorization.

# IMPORTANT: The next lab is an alternative to this one, since it's still about Authentication and Authorization, but implementing a distributed architecture instead of a monolith. If you want to try both this lab AND the next one, make sure to back up your project before proceeding.

### Step 1: Create a New Project with Individual Accounts Authentication

1. **Create a New Blazor Web App Project**:
   - **Project Name**: Use the same name as your existing project (e.g., `PhotoSharingApplication`).
   - **Authentication Type**: Select **Individual Accounts**.
   - **Interactivity**: Set to **Auto**.
   - **Interactivity Location**: Set to **Per page / component**.

2. **Copy the `<UserSecretsId>`**:
   - Open the `.csproj` file of the new project.
   - Copy the `<UserSecretsId>` line.
   - Paste it into the `.csproj` file of your existing project.
   - Add the following package references to your existing project's `.csproj` file:

   ```xml
   <PackageReference Include="Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore" Version="8.0.8" />
   <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.8" />
   ```

### Step 2: Transfer Identity and Authentication Files to Your Existing Project

You will now transfer all the necessary Identity and authentication-related files from the new project to your existing lab project.

1. **Copy the `Components/Account` Folder**:
   - Copy the entire `Components/Account` folder from the new project to the corresponding location in your existing project.

2. **Replace the Navigation Menu (`NavMenu.razor`)**:
   - Replace the existing `NavMenu.razor` file in your old project with the one from the new project:

   ```csharp
   @implements IDisposable

   @inject NavigationManager NavigationManager

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
               </ul>
               <AuthorizeView>
                   <Authorized>
                       <ul class="navbar-nav mb-2 mb-md-0">
                           <li class="nav-item">
                               <NavLink class="nav-link" href="Account/Manage">
                                   @context.User.Identity?.Name
                               </NavLink>
                           </li>
                           <li class="nav-item">
                               <form action="Account/Logout" method="post">
                                   <AntiforgeryToken />
                                   <input type="hidden" name="ReturnUrl" value="@currentUrl" />
                                   <button type="submit" class="nav-link">
                                       Logout
                                   </button>
                               </form>
                           </li>
                       </ul>
                   </Authorized>
                   <NotAuthorized>
                       <ul class="navbar-nav mb-2 mb-md-0">
                           <li class="nav-item">
                               <NavLink class="nav-link" href="Account/Register">
                                   Register
                               </NavLink>
                           </li>
                           <li class="nav-item">
                               <NavLink class="nav-link" href="Account/Login">
                                   Login
                               </NavLink>
                           </li>
                       </ul>
                   </NotAuthorized>
               </AuthorizeView>
           </div>
       </div>
   </nav>

   @code {
       private string? currentUrl;

       protected override void OnInitialized()
       {
           currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
           NavigationManager.LocationChanged += OnLocationChanged;
       }

       private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
       {
           currentUrl = NavigationManager.ToBaseRelativePath(e.Location);
           StateHasChanged();
       }

       public void Dispose()
       {
           NavigationManager.LocationChanged -= OnLocationChanged;
       }
   }
   ```
3. **Modify the Routes.razor file**:
    - Replace the existing `Routes.razor` file in your old project with the one from the new project:
    
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
4. **Copy the `Data` Folder**:
   - Copy the entire `Data` folder from the new project into your existing project.

5. **Modify the `_Imports.razor`**:
   - Copy the following line from the `_Imports.razor` file of the new project to your existing project:

   ```csharp
   @using Microsoft.AspNetCore.Components.Authorization
   ```

### Step 3: Update Configuration and Services in Your Existing Project

#### Modify `appsettings.json`

1. **Copy the `DefaultConnection` connection string** from the new project’s `appsettings.json` file and modify it as needed in your existing project:

   ```json
   "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BlazorLabs-IdentityContext;Trusted_Connection=True;MultipleActiveResultSets=true"
   ```

#### Update `Program.cs`

1. **Add necessary usings**:

   ```csharp
   using Microsoft.AspNetCore.Components.Authorization;
   using Microsoft.AspNetCore.Identity;
   using PhotoSharingApplication.Components.Account;
   using PhotoSharingApplication.Data;
   ```

2. **Register Identity services** before the `var app = builder.Build();` line in your existing project:

   ```csharp
   builder.Services.AddCascadingAuthenticationState();
   builder.Services.AddScoped<IdentityUserAccessor>();
   builder.Services.AddScoped<IdentityRedirectManager>();
   builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

   builder.Services.AddAuthentication(options =>
   {
       options.DefaultScheme = IdentityConstants.ApplicationScheme;
       options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
   })
       .AddIdentityCookies();

   var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
   builder.Services.AddDbContext<ApplicationDbContext>(options =>
       options.UseSqlServer(connectionString));
   builder.Services.AddDatabaseDeveloperPageExceptionFilter();

   builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
       .AddEntityFrameworkStores<ApplicationDbContext>()
       .AddSignInManager()
       .AddDefaultTokenProviders();

   builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
   ```

3. **Enable migrations** after `app.UseWebAssemblyDebugging();`:

   ```csharp
   app.UseMigrationsEndPoint();
   ```

4. **Map additional Identity endpoints** before `app.Run();`:

   ```csharp
   // Add additional endpoints required by the Identity /Account Razor components.
   app.MapAdditionalIdentityEndpoints();
   ```

### Step 4: Customize Client-Side Authentication in Your Existing Project

1. **Update the Project File**:
   - Add the following package reference in the existing client project:

   ```xml
   <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.Authentication" Version="8.0.8" />
   ```

2. **Copy Additional Components**:
   - From the new project, copy the following files into your existing client project:
     - `Pages/Auth.razor`
     - `PersistentAuthenticationStateProvider.cs`
     - `RedirectToLogin.razor`
     - `UserInfo.cs`

3. **Update `_Imports.razor`**:
   - Ensure the following line is present:

   ```csharp
   @using Microsoft.AspNetCore.Components.Authorization
   ```

4. **Update `Program.cs`** in the existing client project:

   ```csharp
   using Microsoft.AspNetCore.Components.Authorization;
   ```

   - Register authorization services:

   ```csharp
   builder.Services.AddAuthorizationCore();
   builder.Services.AddCascadingAuthenticationState();
   builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();
   ```

### Step 5: Test the Authentication Setup in Your Existing Project

1. **Run the Application**:
   - Start the application and navigate to `/auth`. You should be redirected to the `/account/login` page.

2. **Register a New User**:
   - Go to `/Account/Register` and create a new user. If you encounter a migration error, run the migrations and refresh the page.

3. **Login**:
   - Go to `/Account/Login` and log in with the user you just created.

4. **Manage User Information**:
   - Go to `/Account/Manage` to view and manage user information.

5. **Test Authentication**:
   - Go to `/auth` again. This time, you should be authenticated and able to view the content.

6. **Logout**:
   - Go to `/Account/Logout` and log out of the application.

### Step 6: Secure the Photos/Editor and Photos/Delete Pages in Your Existing Project

1. **Authorize Access**:
   - Ensure that only authenticated users can access the `Photos/Editor` and `Photos/Delete` pages in your existing project. Use the `[Authorize]` attribute in the Razor components to enforce this.

```csharp
@using Microsoft.AspNetCore.Authorization
@attribute [Authorize]
```

```csharp
@using Microsoft.AspNetCore.Authorization
@attribute [Authorize]
```

2. **Test the Authorization**:
   - Run the application and try to access the `Photos/Editor` and `Photos/Delete` pages. You should be redirected to the login page.
   - Log in and try to access the pages again. You should be able to access them now.

### Summary

In this lab, you:
- Created a new project to generate the necessary Identity, authentication, and authorization files.
- Transferred those files and code into your existing lab project.
- Integrated Identity Framework with your existing Blazor Web App using Individual Accounts.
- Secured specific pages, ensuring only authenticated users can access the `Photos/Editor` and `Photos/Delete` pages.
- Tested the full authentication flow, including registration, login, user management, and logout.

In the next lab, you'll implement resource-based authorization to ensure that only the owners of a photo can edit or delete it.