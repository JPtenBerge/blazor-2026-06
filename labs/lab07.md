# Lab 7: Implementing a Repository with Entity Framework for SQL Server

In this lab, you'll extend your photo-sharing application by integrating Entity Framework to interact with a SQL Server database. You'll replace your in-memory data storage with a database-backed repository, enabling more robust data management.

#### Step 1: Add Required Packages
To start, you'll need to add the following NuGet packages to your project:

1. **Microsoft.EntityFrameworkCore.SqlServer**: This package allows Entity Framework Core to work with SQL Server.
2. **Microsoft.EntityFrameworkCore.Tools**: This package provides tools to create migrations and update the database schema.

To add these packages, open the NuGet Package Manager and install the following:

```shell
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools
```

#### Step 2: Create the `PhotosDbContext` Class
The `PhotosDbContext` class represents the session with your SQL Server database. It will be responsible for managing the entities in your application and applying configurations.

Add the following class to your `PhotoSharingApplication.Infrastructure.Data` namespace:

```csharp
using Microsoft.EntityFrameworkCore;
using PhotoSharingApplication.Core.Models;

namespace PhotoSharingApplication.Infrastructure.Data;

public class PhotosDbContext : DbContext
{
    public PhotosDbContext(DbContextOptions<PhotosDbContext> options) : base(options)
    {
    }

    public DbSet<Photo> Photos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Photo>().Property(p => p.Title).HasMaxLength(100);
        modelBuilder.Entity<Photo>().Property(p => p.Description).HasMaxLength(300);
        modelBuilder.Entity<Photo>().Property(p => p.ImageMimeType).HasMaxLength(100);
    }
}
```

**Explanation**:
- **DbContext**: The `PhotosDbContext` class inherits from `DbContext`, which provides the infrastructure for database interactions.
- **DbSet<Photo> Photos**: This property represents the `Photos` table in the database.
- **OnModelCreating**: Configures entity properties, like maximum lengths for strings.

#### Step 3: Register the DbContext as a Service
To make the `PhotosDbContext` available throughout your application, you need to register it as a service in the `Program.cs` file. Add the following code:

```csharp
builder.Services.AddDbContext<PhotosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PhotosDbContext")));
```

This code registers the `PhotosDbContext` using the connection string defined in `appsettings.json`. It allows Entity Framework to manage the database connection for you.

#### Step 4: Configure the Connection String
Add the following section to your `appsettings.json` file to define the connection string for your SQL Server database:

```json
"ConnectionStrings": {
  "PhotosDbContext": "Server=(localdb)\\mssqllocaldb;Database=BlazorLabs-PhotosContext;Trusted_Connection=True;"
}
```

This connection string points to a local SQL Server instance. Make sure it's configured correctly for your environment.

#### Step 5: Create the Database Schema
Next, you'll create the database schema based on your `PhotosDbContext`. Use the Visual Studio Package Manager Console to run the following commands:

1. **Create a Migration**:
   ```shell
   Add-Migration InitialCreate
   ```

2. **Update the Database**:
   ```shell
   Update-Database
   ```

These commands will create the database and apply the initial schema based on your entity configurations.

#### Step 6: Update the `IPhotosRepository` Interface
Modify your `IPhotosRepository` interface to use asynchronous methods. This will allow your application to perform database operations without blocking the main thread, improving performance and responsiveness.

Replace your existing `IPhotosRepository` with the following:

```csharp
using PhotoSharingApplication.Core.Models;

namespace PhotoSharingApplication.Core.Interfaces;

public interface IPhotosRepository
{
    Task<Photo> AddPhotoAsync(Photo photo);
    Task<Photo?> GetPhotoByIdAsync(int id);
    Task<IEnumerable<Photo>> GetPhotosAsync();
    Task<Photo?> DeletePhotoAsync(int id);
    Task UpdatePhotoAsync(Photo photo);
}
```

**Why Use Async?**:
- Asynchronous methods prevent the application from freezing during long-running operations like database queries, which is especially important in web applications.

#### Step 7: Create the `PhotosEFRepository` Class
Now, create a new class `PhotosEFRepository` that implements the updated `IPhotosRepository` interface using Entity Framework. This class will interact with the SQL Server database.

Add the following code to your `PhotoSharingApplication.Infrastructure.Repositories` namespace:

```csharp
using Microsoft.EntityFrameworkCore;
using PhotoSharingApplication.Core.Interfaces;
using PhotoSharingApplication.Core.Models;
using PhotoSharingApplication.Infrastructure.Data;

namespace PhotoSharingApplication.Infrastructure.Repositories;

public class PhotosEFRepository : IPhotosRepository
{
    private readonly PhotosDbContext context;

    public PhotosEFRepository(PhotosDbContext context)
    {
        this.context = context;
    }

    public async Task<Photo> AddPhotoAsync(Photo photo)
    {
        // The AddPhotoAsync method should add the photo parameter to the Photos property of the DbContext, then save the changes.
        context.Photos.Add(photo);
        await context.SaveChangesAsync();
        return photo;
    }

    public async Task<Photo?> DeletePhotoAsync(int id)
    {
        // The DeletePhotoAsync method should find the photo by its ID, delete it if found, and save the changes.
        Photo? photo = await context.Photos.FirstOrDefaultAsync(p => p.Id == id);
        if (photo is not null)
        {
            context.Photos.Remove(photo);
            await context.SaveChangesAsync();
        }
        return photo;
    }

    public async Task<Photo?> GetPhotoByIdAsync(int id)
    {
        // The GetPhotoByIdAsync method should retrieve a photo by its ID, using a non-tracking query to ensure the context doesn't track changes to this entity.
        return await context.Photos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Photo>> GetPhotosAsync()
    {
        // The GetPhotosAsync method should return all photos from the database.
        return await context.Photos.AsNoTracking().ToListAsync();
    }

    public async Task UpdatePhotoAsync(Photo photo)
    {
        // The UpdatePhotoAsync method should mark the photo entity as modified and save changes to the database.
        context.Entry(photo).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }
}
```

#### Step 8: Register the New Repository
Replace the old `PhotosListRepository` registration in `Program.cs` with your new `PhotosEFRepository`:

```csharp
builder.Services.AddScoped<IPhotosRepository, PhotosEFRepository>();
```

This line registers `PhotosEFRepository` as the service to be used whenever `IPhotosRepository` is requested, enabling dependency injection with your new repository.
You can delete the old `PhotosListRepository` class, since we're not going to use it anymore.


#### Step 9: Update the UI Components

**All.razor**:

Previously, we used the `OnInitialized` method to load the photos synchronously. Since we want to use the new asynchronous methods of the repository, we should switch to `OnInitializedAsync`. This change ensures that the application remains responsive while waiting for the data to load.

```csharp
@code {
    IEnumerable<Photo> photos = default!;

    protected override async Task OnInitializedAsync()
    {
        photos = await PhotosRepository.GetPhotosAsync();
    }
}
```

**Delete.razor**:

Previously, the `OnParametersSet` method was used to retrieve the photo by its ID. Now, we need to switch to the `OnParametersSetAsync` method to use the asynchronous `GetPhotoByIdAsync` method of the repository. This update ensures that the photo data is retrieved without blocking the UI thread.

```csharp
@code {
    [Parameter]
    public int Id { get; set; }

    [SupplyParameterFromForm(FormName = "PhotoDeleteForm")]
    Photo? photo { get; set;}

    protected override async Task OnParametersSetAsync()
    {
        photo ??= await PhotosRepository.GetPhotoByIdAsync(Id);
    }

    async Task DeletePhoto()
    {
        if(photo is not null)
        {
            await PhotosRepository.DeletePhotoAsync(photo.Id);
            NavigationManager.NavigateTo("photos/all");
        }
    }
}
```

**Details.razor**:

The `OnInitialized` method was used to load the photo synchronously. Now, it should be replaced with the `OnInitializedAsync` method, utilizing the `GetPhotoByIdAsync` method to fetch the photo data asynchronously.

```csharp
@code {
    Photo? photo;

    [Parameter]
    public int Id { get; set; }   

    protected override async Task OnInitializedAsync()
    {
        photo = await PhotosRepository.GetPhotoByIdAsync(Id);
    }
}
```

**Editor.razor**:

The `OnParametersSet` method was previously used to set up the `photo` object. This should now be replaced with `OnParametersSetAsync`, which allows us to use the asynchronous methods of the repository. This change ensures that the photo data is correctly loaded or initialized when editing or creating a new photo.

```csharp
@code {
    [Parameter]
    public int? Id { get; set; }

    [SupplyParameterFromForm(FormName = "PhotoForm")]
    Photo? photo { get; set; }

    [SupplyParameterFromForm(FormName = "PhotoForm")]
    FileModel? fileModel { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if(Id.HasValue)


        {
            photo = await PhotosRepository.GetPhotoByIdAsync(Id.Value);
        }
        else
        {
            photo ??= new Photo() { Title = "New Photo" };
        }
        fileModel ??= new FileModel();
    }

    async Task Save()
    {
        if (photo is not null)
        {
            if (fileModel?.File is not null)
            {
                using var memoryStream = new MemoryStream();
                await fileModel.File.OpenReadStream().CopyToAsync(memoryStream);
                photo.PhotoFile = memoryStream.ToArray();
                photo.ImageMimeType = fileModel.File.ContentType;
            }
            if (photo.Id == 0)
            {
                await PhotosRepository.AddPhotoAsync(photo);
            }
            else
            {
                await PhotosRepository.UpdatePhotoAsync(photo);
            }
        }
        NavigationManager.NavigateTo("photos/all");
    }

    class FileModel
    {
        public IFormFile? File { get; set; }
    }
}
```

#### Step 10: Update the Model
Finally, make a small change to your `Photo` model to enforce some data integrity rules:

```csharp
namespace PhotoSharingApplication.Core.Models;

public class Photo
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public byte[]? PhotoFile { get; set; }
    public string? ImageMimeType { get; set; }
    public string? DataUrl => PhotoFile == null ? null : $"data:{ImageMimeType};base64,{Convert.ToBase64String(PhotoFile)}";
}
```

**Explanation**:
- The `required` modifier ensures that a `Title` must always be provided when creating a `Photo` object.

#### Lab Completion:
By the end of this lab, you will have successfully integrated Entity Framework with your Blazor application, using a SQL Server database. Your app will be more scalable and capable of handling real-world data operations asynchronously. This sets the stage for building robust, production-ready applications with .NET.