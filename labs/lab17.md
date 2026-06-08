# Lab 17: Performance Optimization with Asynchronous Image Loading And Distributed Cache

In this lab, you will optimize the performance of your Blazor application by separating the metadata and image data of photos. This will allow faster initial page loads by downloading photo metadata first and asynchronously fetching images afterward. This approach will also leverage browser caching and parallel download capabilities, improving the perceived performance of your application.

### Step 1: Modify the Photo Model

To begin, you will split the `Photo` model into two classes: `Photo` and `PhotoImage`. The `PhotoImage` class will contain the binary data and MIME type of the image, while the `Photo` class will contain other metadata like `Title`, `Description`, etc.

1. **Create the `PhotoImage` Class:**

   Add the following code to define the `PhotoImage` class:

   ```csharp
   namespace PhotoSharingApplication.Core.Models;

   public class PhotoImage
   {
       public int Id { get; set; }
       public byte[]? PhotoFile { get; set; } = [];
       public string? ImageMimeType { get; set; }
   }
   ```

2. **Modify the `Photo` Class:**

   Update the `Photo` class to remove the `PhotoFile` and `ImageMimeType` properties. Replace them with a `PhotoImage` property:

   ```csharp
   using PhotoSharingApplication.Client.Core.Models;

   namespace PhotoSharingApplication.Core.Models;

   public class Photo
   {
       public int Id { get; set; }
       public required string Title { get; set; }
       public string? Description { get; set; }

       public string? DataUrl => PhotoImage?.PhotoFile == null ? null : $"data:{PhotoImage.ImageMimeType};base64,{Convert.ToBase64String(PhotoImage.PhotoFile)}";
       public string Owner { get; set; }
       public PhotoImage? PhotoImage { get; set; } = new PhotoImage();

       public List<Comment> Comments { get; set; } = [];
   }
   ```

### Step 2: Configure the Database Context

You need to map the `Photo` and `PhotoImage` classes to the same database table while establishing a one-to-one relationship between them.

1. **Update `PhotosDbContext`:**

   Modify the `PhotosDbContext` class as follows:

   ```csharp
   using Microsoft.EntityFrameworkCore;
   using PhotoSharingApplication.Client.Core.Models;
   using PhotoSharingApplication.Core.Models;

   namespace PhotoSharingApplication.Infrastructure.Data;

   public class PhotosDbContext : DbContext
   {
       public PhotosDbContext(DbContextOptions<PhotosDbContext> options) : base(options)
       {
           
       }

       public DbSet<Photo> Photos { get; set; }
       public DbSet<Comment> Comments { get; set; }
       public DbSet<PhotoImage> PhotoImages { get; set; }

       protected override void OnModelCreating(ModelBuilder modelBuilder)
       {
           modelBuilder.Entity<Photo>().Property(p => p.Title).HasMaxLength(100);
           modelBuilder.Entity<Photo>().Property(p => p.Description).HasMaxLength(300);

           modelBuilder.Entity<Photo>().HasOne(p => p.PhotoImage)
                                       .WithOne()
                                       .HasForeignKey<PhotoImage>(p => p.Id);

           modelBuilder.Entity<PhotoImage>().Property(p => p.ImageMimeType).HasMaxLength(100);
           modelBuilder.Entity<PhotoImage>().ToTable("Photos");

           modelBuilder.Entity<Comment>().Property(c => c.Subject).HasMaxLength(100);
           modelBuilder.Entity<Comment>().Property(c => c.Body).HasMaxLength(300);
       }
   }
   ```

### Step 3: Update the Repository and Service Interfaces

Next, you need to modify the `IPhotosRepository` and `IPhotosService` interfaces to include methods that fetch the `Photo` and `PhotoImage` separately.

1. **Update `IPhotosRepository`:**

   Modify the `IPhotosRepository` interface:

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

       Task<Photo?> GetPhotoWithImageByIdAsync(int id);
       Task<PhotoImage?> GetPhotoImageByIdAsync(int id);
   }
   ```

2. **Update `IPhotosService`:**

   Modify the `IPhotosService` interface:

   ```csharp
   using PhotoSharingApplication.Core.Models;

   namespace PhotoSharingApplication.Core.Interfaces;

   public interface IPhotosService
   {
       Task<Photo> AddPhotoAsync(Photo photo);
       Task DeletePhotoAsync(int id);
       Task<Photo?> GetPhotoByIdAsync(int id);
       Task<IEnumerable<Photo>> GetPhotosAsync();
       Task UpdatePhotoAsync(Photo photo);

       Task<Photo?> GetPhotoWithImageByIdAsync(int id);
       Task<PhotoImage?> GetPhotoImageByIdAsync(int id);
   }
   ```

### Step 4: Implement the Updated Repository

Now, you will implement the methods in the `PhotosEFRepository` class to handle the new `PhotoImage` entity.

1. **Update `PhotosEFRepository`:**

   Modify the repository to include methods for fetching `PhotoImage`:

   ```csharp
   using Microsoft.EntityFrameworkCore;
   using PhotoSharingApplication.Core.Interfaces;
   using PhotoSharingApplication.Core.Models;
   using PhotoSharingApplication.Infrastructure.Data;

   namespace PhotoSharingApplication.Infrastructure.Repositories;

   public class PhotosEFRepository(PhotosDbContext context) : IPhotosRepository
   {
       public async Task<Photo> AddPhotoAsync(Photo photo)
       {
           context.Photos.Add(photo);
           await context.SaveChangesAsync();
           return photo;
       }

       public async Task<Photo?> DeletePhotoAsync(int id)
       {
           Photo? photo = context.Photos.Include(p => p.PhotoImage).FirstOrDefault(p => p.Id == id);
           if (photo is not null)
           {
               context.Photos.Remove(photo);
               await context.SaveChangesAsync();
           }
           return photo;
       }

       public async Task<Photo?> GetPhotoByIdAsync(int id)
       {
           return await context.Photos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
       }

       public async Task<PhotoImage?> GetPhotoImageByIdAsync(int id)
       {
           return await context.PhotoImages.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
       }

       public async Task<IEnumerable<Photo>> GetPhotosAsync()
       {
           return await context.Photos.AsNoTracking().ToListAsync();
       }

       public async Task<Photo?> GetPhotoWithImageByIdAsync(int id)
       {
           return await context.Photos.Include(p => p.PhotoImage).AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
       }

       public async Task UpdatePhotoAsync(Photo photo)
       {
           context.Photos.Update(photo);
           await context.SaveChangesAsync();   
       }
   }
   ```

### Step 5: Implement the Updated Service

Implement the corresponding methods in the `PhotosService` class.

1. **Update `PhotosService`:**

   Modify the service to handle the `PhotoImage`:

   ```csharp
   using Microsoft.AspNetCore.Authorization;
   using Microsoft.AspNetCore.Components.Authorization;
   using PhotoSharingApplication.Auth;
   using PhotoSharingApplication.Core.Interfaces;
   using PhotoSharingApplication.Core.Models;

   namespace PhotoSharingApplication.Core.Services;

   public class PhotosService : IPhotosService
   {
       private readonly IPhotosRepository _photosRepository;
       private readonly IAuthorizationService _authorizationService;
       private readonly AuthenticationStateProvider _authenticationStateProvider;

       public PhotosService(IPhotosRepository photosRepository, IAuthorizationService authorizationService, AuthenticationStateProvider authenticationStateProvider)
       {
           _photosRepository = photosRepository;
           _authorizationService = authorizationService;
           _authenticationStateProvider = authenticationStateProvider;
       }

       public async Task<Photo> AddPhotoAsync(Photo photo)
       {
           var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
           photo.Owner = authState?.User?.Identity?.Name ?? throw new UnauthorizedException();
           return await _photosRepository.AddPhotoAsync(photo);
       }

       public Task<Photo?> GetPhotoByIdAsync(int id)
       {
           return _photosRepository.GetPhotoByIdAsync(id);
       }

       public async Task<IEnumerable<Photo>> GetPhotosAsync()
       {
           return await _photosRepository.GetPhotosAsync();
       }

       public async Task DeletePhotoAsync(int id)
       {
           Photo photo = await _photosRepository.GetPhotoByIdAsync(id) ?? throw new Exception("Photo not found");
           var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
           var User = authState?.User ?? throw new UnauthorizedException();
           AuthorizationResult result = await _authorizationService.AuthorizeAsync(User, photo, "PhotoOwner");
           if (!result.Succeeded)
           {
               throw new UnauthorizedException();
           }
           await _photosRepository.DeletePhotoAsync(id);
       }

       public async Task UpdatePhotoAsync(Photo photo)
       {
           var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
           var User = authState?.User ?? throw new UnauthorizedException();
           AuthorizationResult result = await _authorizationService.AuthorizeAsync(User, photo, "PhotoOwner");
           if (!result.Succeeded)
           {
               throw new UnauthorizedException();
           }
           Photo? photoToUpdate = await _photosRepository.GetPhotoByIdAsync(photo.Id);
           if (photoToUpdate is not null)
           {
               await _photosRepository.Update

PhotoAsync(photo);
           }
       }

       public async Task<Photo?> GetPhotoWithImageByIdAsync(int id)
       {
           return await _photosRepository.GetPhotoWithImageByIdAsync(id);
       }

       public async Task<PhotoImage?> GetPhotoImageByIdAsync(int id)
       {
           return await _photosRepository.GetPhotoImageByIdAsync(id);
       }
   }
   ```

### Step 6: Implement a Minimal API to Serve Images

To serve images separately from metadata, you'll create a Minimal API endpoint that returns the image file based on the photo ID.

1. **Add the Minimal API in `Program.cs`:**

   Add the following code to `Program.cs`:

   ```csharp
   app.Map("photoimage/{id}", async (int id, [FromServices] IPhotosService _photosService) => { 
       PhotoImage? photoImage = await _photosService.GetPhotoImageByIdAsync(id);
       if (photoImage is null)
       {
           return Results.NotFound();
       }
       return Results.File(photoImage.PhotoFile ?? [], photoImage.ImageMimeType);
   });
   ```

### Step 7: Update the Razor Components

Finally, you'll update the Razor components to reflect the new structure of the `Photo` model and utilize the new Minimal API for serving images.

1. **Update `Editor.razor`:**

   Modify the `Editor.razor` component to use the new model structure:

   ```csharp
   @page "/photos/create"
   @page "/photos/edit/{id:int}"
   @inject IPhotosService PhotosService
   @inject NavigationManager NavigationManager
   @inject IAuthorizationService AuthorizationService

   @using Microsoft.AspNetCore.Authorization
   @using PhotoSharingApplication.Auth

   @attribute [Authorize]

   <h3>Editor</h3>

   @if(photo is not null){

       <EditForm Model="@photo" OnSubmit="Save" FormName="PhotoForm" enctype="multipart/form-data">
           <InputNumber @bind-Value="photo.Id" hidden />
           <InputText @bind-Value="photo.Owner" hidden />
           <div class="mb-3">
               <label for="photoTitle" class="form-label">Title:</label>
               <InputText @bind-Value="photo.Title" class="form-control" id="photoTitle"/>
           </div>
           <div class="mb-3">
               <label for="photoDescription" class="form-label">Description (optional):</label>
               <InputTextArea @bind-Value="photo.Description" id="photoDescription" class="form-control" />
           </div>
           <div class="mb-3">
               <label for="fileModelFile" class="form-label">File:</label>
               <InputFile name="fileModel.File" class="form-control" id="fileModelFile" />
           </div>
           <button type="submit" class="btn btn-primary">Save</button>
       </EditForm>
       <img src="@photo.DataUrl" alt="@photo.Title" class="img-fluid rounded mx-auto d-block" />
   }

   @code {
       [Parameter]
       public int? Id { get; set; }

       [SupplyParameterFromForm(FormName = "PhotoForm")]
       Photo? photo { get; set; }

       [SupplyParameterFromForm(FormName = "PhotoForm")]
       FileModel? fileModel { get; set; }

       [CascadingParameter]
       public Task<AuthenticationState> AuthenticationStateTask { get; set; }

       protected override async Task OnParametersSetAsync()
       {
           var authState = await AuthenticationStateTask;
           if(Id.HasValue)
           {
               photo ??= await PhotosService.GetPhotoWithImageByIdAsync(Id.Value);
               var authorizationResult = await AuthorizationService.AuthorizeAsync(authState.User, photo, PhotoSharingPolicies.PhotoOwnerPolicy);
               if (!authorizationResult.Succeeded)
               {
                   NavigationManager.NavigateTo("Account/AccessDenied");
               }
           }
           else
           {
               photo ??= new Photo() { Title = "New Photo", Owner = authState.User.Identity.Name, PhotoImage =  new PhotoImage() };
           }
           fileModel ??= new FileModel();
       }

       async Task Save()
       {
           try{
               if (photo is not null)
               {
                   if (fileModel?.File is not null)
                   {
                       using var memoryStream = new MemoryStream();
                       await fileModel.File.OpenReadStream().CopyToAsync(memoryStream);
                       photo.PhotoImage = new PhotoImage();
                       photo.PhotoImage.PhotoFile = memoryStream.ToArray();
                       photo.PhotoImage.ImageMimeType = fileModel.File.ContentType;
                   }
                   if (photo.Id == 0)
                   {
                       await PhotosService.AddPhotoAsync(photo);
                   }
                   else
                   {
                       await PhotosService.UpdatePhotoAsync(photo);
                   }
               }
               NavigationManager.NavigateTo("photos/all");
           } catch (UnauthorizedException ex)
           {
               NavigationManager.NavigateTo("Account/AccessDenied");
           }
       }

       class FileModel
       {
           public IFormFile? File { get; set; }
       }
   }
   ```

2. **Update `PhotoCard.razor`:**

   Modify the `PhotoCard.razor` component to use the Minimal API for image loading:

   ```csharp
   @using Microsoft.AspNetCore.Authorization
   @using PhotoSharingApplication.Auth
   @inject IAuthorizationService AuthorizationService

   @if(Photo is not null){
       <div class="card">
           <img src="@($"photoimage/{Photo.Id}")" class="card-img-top" alt="@Photo.Title">
           
           <div class="card-body">
               <h5 class="card-title">@Photo.Title</h5>
               <p class="card-text">@Photo.Description</p>
               <p class="card-text">@Photo.Id</p>
           </div>
           @if (ShowFooter)
           {
               <div class="card-footer">
                   @if (ShowDetails)
                   {
                       <NavLink href="@($"photos/details/{Photo.Id}")" class="card-link">Details</NavLink>
                   }
                   @if (_showUpdate)
                   {
                       <NavLink href="@($"photos/edit/{Photo.Id}")" class="card-link">Update</NavLink>
                   }
                   @if (_showDelete)
                   {
                       <NavLink href="@($"photos/delete/{Photo.Id}")" class="card-link">Delete</NavLink>
                   }
               </div>
           }
       </div>
   }
   @code {
       [Parameter]
       public Photo Photo { get; set; } = default!;

       [Parameter]
       public bool ShowDetails { get; set; } = false;
       
       [Parameter]
       public bool ShowUpdate { get; set; } = false;
       private bool _showUpdate;

       [Parameter]
       public bool ShowDelete { get; set; } = false;
       private bool _showDelete;

       private bool ShowFooter => ShowDetails || _showUpdate || _showDelete;

       [CascadingParameter]
       public Task<AuthenticationState> AuthenticationStateTask { get; set; }

       protected override async Task OnParametersSetAsync()
       {
           if (AuthenticationStateTask is not null)
           {
               var authState = await AuthenticationStateTask;
               var authorizationResult = await AuthorizationService.AuthorizeAsync(authState.User, Photo, PhotoSharingPolicies.PhotoOwnerPolicy);
               _showDelete = authorizationResult.Succeeded && ShowDelete;
               _showUpdate = authorizationResult.Succeeded && ShowDelete;
           }
       }
   }
   ```

### Step 8: Test Your Application

Run your application and navigate to the page that lists all photos. You should notice an improvement in performance, with the metadata being displayed first and images loading asynchronously.

In the next lab, you'll explore more advanced performance optimizations and caching strategies to further enhance your application's responsiveness and scalability.

# Part 2: Caching Strategies

## Introduction

In this part of the lab, you will a caching strategy to improve the performance and scalability of your Blazor application. You will learn how to leverage distributed caching to store and retrieve data efficiently.

### Objectives

By the end of this part, you will be able to:

- Implement a caching strategy using distributed caching in a Blazor application.
- Use the `IDistributedCache` interface to interact with the distributed cache.
- Configure and use a distributed cache provider in a Blazor application.

### Steps

1. Add the `IDistributedCache` Service
2. Implement Caching in the `PhotosService`
3. Configure the Distributed Cache Provider
4. Test the Application

## Step 1: Add the `IDistributedCache` Service

To begin, you will add the `IDistributedCache` service to the `PhotosService` class to cache photo metadata and images.

1. **Add the `IDistributedCache` Service:**

   Modify the `PhotosService` class to include the `IDistributedCache` service:

   ```csharp
   using Microsoft.Extensions.Caching.Distributed;
   using PhotoSharingApplication.Core.Models;

   namespace PhotoSharingApplication.Core.Services;

   public class PhotosService : IPhotosService
   {
       private readonly IPhotosRepository _photosRepository;
       private readonly IAuthorizationService _authorizationService;
       private readonly AuthenticationStateProvider _authenticationStateProvider;
       private readonly IDistributedCache _cache;

       public PhotosService(IPhotosRepository photosRepository, IAuthorizationService authorizationService, AuthenticationStateProvider authenticationStateProvider, IDistributedCache cache)
       {
           _photosRepository = photosRepository;
           _authorizationService = authorizationService;
           _authenticationStateProvider = authenticationStateProvider;
           _cache = cache;
       }

       // Existing methods
   }
   ```

## Step 2: Implement Caching in the `PhotosService`

Next, you will implement a caching strategy in the `PhotosService` class to store and retrieve photo images using the distributed cache.

1. **Implement Caching in the `PhotosService`:**

   Modify the `GetPhotoImageByIdAsync` method in the `PhotosService` class to cache photo images using the distributed cache:

   ```csharp
   public async Task<PhotoImage?> GetPhotoImageByIdAsync(int id)
   {
       string cacheKey = $"PhotoImage-{id}";
       byte[]? cachedImage = await _cache.GetAsync(cacheKey);
       if (cachedImage is not null)
       {
           return new PhotoImage { Id = id, PhotoFile = cachedImage };
       }

       PhotoImage? photoImage = await _photosRepository.GetPhotoImageByIdAsync(id);
       if (photoImage is not null)
       {
           await _cache.SetAsync(cacheKey, photoImage.PhotoFile, new DistributedCacheEntryOptions
           {
               AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
           });
       }

       return photoImage;
   }
   ```

## Step 3: Configure the Distributed Cache Provider

Now, you will configure the distributed cache provider in the `Program.cs` file to use the `MemoryCache` provider.

1. **Configure the Distributed Cache Provider:**

   Add the following code to configure the distributed cache provider in the `Program.cs` file:

   ```csharp
   builder.Services.AddDistributedMemoryCache();
   ```

## Step 4: Test the Application

- Run your application and navigate to the page that lists all photos. 
- Check the logs on the server console: you should see all the queries to get the images from the database.
- Navigate back to home and then back to the photos page.
- Check the logs on the server console: you should see that the images are not being retrieved from the database anymore.



