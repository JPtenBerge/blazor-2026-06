# Lab 4: Services and Infrastructure

So far, our pages have been using hardcoded data. We are going to replace this with a service that can provide the data.

We will create a service with methods to 
- get a list of all the photos
- get one photo given its id
- create a given photo
- update a given photo
- delete a photo given its id 

For now, our service will be a prototype that works with a List in memory.
We will replace it with one that can use a DataBase in a later lab.

After having created our service, we will register it in the Dependency Injection container and use it in our components.

### The Core

We are going to start by creating a new folder that will contain the business logic of our application. This folder will contain the interfaces for our services and the entities that we are going to use.

- In the `Solution Explorer`, right click on the `PhotoSharingApplication` project
- Select `Add` -> `New Folder`
- Name the folder `Core`
- In the `Core` folder, add a new `Models` folder
- In the `Models` folder, add a new class `Photo.cs` with the following code:

```cs
public class Photo {
  public int Id { get; set; }
  public string Title { get; set; }
  public string Description { get; set; }
}
```

- In the `Core` folder, add a new `Interfaces` folder
- In the `Interfaces` folder, add a new interface `IPhotoRepository.cs` with the following code:

```cs
using PhotoSharingApplication.Core.Models;

namespace PhotoSharingApplication.Core.Interfaces;

public interface IPhotosRepository
{
    void AddPhoto(Photo newPhoto);
    Photo? GetPhotoById(int id);
    IEnumerable<Photo> GetPhotos();
    void RemovePhoto(int id);
    void UpdatePhoto(Photo photo);
}
```

### The Infrastructure

We are going to create a new folder that will contain the implementation of our service. This folder will be responsible for the data access and will be called `Infrastructure`.

- In the `Solution Explorer`, right click on the `PhotoSharingApplication` project
- Select `Add` -> `New Folder`
- Name the folder `Infrastructure`
- In the `Infrastructure` folder, add a new `Repositories` folder
- In the `Repositories` folder, add a new class `PhotosListRepository.cs` with the following code:

```cs
using PhotoSharingApplication.Core.Interfaces;
using PhotoSharingApplication.Core.Models;

namespace PhotoSharingApplication.Infrastructure.Repositories;

public class PhotosListRepository : IPhotosRepository
{
    private readonly List<Photo> photos = [];

    public PhotosListRepository()
    {
        photos.Add(new Photo { Id = 1, Title = "Me", Description = "Just me" });
        photos.Add(new Photo { Id = 2, Title = "My cat", Description = "Just my cat" });
        photos.Add(new Photo { Id = 3, Title = "My dog", Description = "Just my dog" });
    }

    public IEnumerable<Photo> GetPhotos()
    {
        return photos;
    }

    public Photo? GetPhotoById(int id)
    {
        return photos.FirstOrDefault(p => p.Id == id);
    }

    public void AddPhoto(Photo newPhoto)
    {
        newPhoto.Id = photos.Any() ? photos.Max(p => p.Id) + 1 : 1;
        photos.Add(newPhoto);
    }

    public void RemovePhoto(int id)
    {
        Photo? photo = photos.FirstOrDefault(p => p.Id == id);
        if (photo is not null)
            photos.Remove(photo);
    }

    public void UpdatePhoto(Photo photo)
    {
        var photoToUpdate = photos.FirstOrDefault(p => p.Id == photo.Id);
        if (photoToUpdate is null)
            return;
        photoToUpdate.Title = photo.Title;
        photoToUpdate.Description = photo.Description;
    }

}
```

### Register the service

We need to register our service in the Dependency Injection container. We do this in the `Program.cs` file of the `PhotoSharingApplication` project.

- Open the `Program.cs` file of the `PhotoSharingApplication` project
- Add the following code, before the `var app = builder.Build();` line:

```cs
builder.Services.AddSingleton<IPhotosRepository, PhotosListRepository>();
```

### Use the service

It's time to modify our Razor components to use the service we just created.

- Open the `_Imports.razor` file in the `Components` folder of the `PhotoSharingApplication` project
- Add the following lines:

```cs
@using PhotoSharingApplication.Core.Interfaces
@using PhotoSharingApplication.Core.Models
```

### The All Page

- Open the `All.razor` file in the `Pages/Photos` folder of the `PhotoSharingApplication` project
- After the `@page` directive, add the following code:

```cs
@inject IPhotosRepository PhotosRepository
```

- Replace the `@code` section method with the following:

```cs
@code {
    IEnumerable<Photo> photos;

    protected override void OnInitialized()
    {
        photos = PhotosRepository.GetPhotos();
    }
}
```

You can delete the `Photo` class, since we are now using the one in the `Core/Model` folder.

If you run the application now, you should see the same list of photos as before, but this time the data is coming from the service we created.

### The Details Page

- Open the `Details.razor` file in the `Pages/Photos` folder of the `PhotoSharingApplication` project
- After the `@page` directive, add the following code:

```cs
@inject IPhotosRepository PhotosRepository
```

- Replace the `@code` section method with the following:

```cs
@code {
    Photo? photo;

    [Parameter]
    public int Id { get; set; }   

    protected override void OnInitialized()
    {
        photo = PhotosRepository.GetPhotoById(Id);
    }
}
```

You can delete the `Photo` class, since we are now using the one in the `Core/Model` folder.

If you run the application now, you should see the same behavior as before, but this time the data is coming from the service we created.

### The Editor Page

- Open the `Editor.razor` file in the `Pages/Photos` folder of the `PhotoSharingApplication` project
- After the `@page` directive, add the following code:

```cs
@inject IPhotosRepository PhotosRepository
@inject NavigationManager NavigationManager
```

- Replace the `@code` section method with the following:

```cs
@code {
    [Parameter]
    public int? Id { get; set; }

    [SupplyParameterFromForm(FormName = "PhotoForm")]
    Photo? photo { get; set; }

    protected override void OnParametersSet()
    {
        if(Id.HasValue)
        {
            photo ??= PhotosRepository.GetPhotoById(Id.Value);
        }
        else
        {
            photo ??= new Photo();
        }    
    }

    void Save()
    {
        if(photo.Id == 0)
        {
            PhotosRepository.AddPhoto(photo);
        }
        else
        {
            PhotosRepository.UpdatePhoto(photo);
        }
        NavigationManager.NavigateTo("photos/all");
    }

}
```

You can delete the `Photo` class, since we are now using the one in the `Core/Model` folder.

If you run the application now, you should see the same behavior as before, but this time the data is coming from the service we created. Also, once you add or edit a photo, user is redirected to the `Photos/All` page.

### The Delete Page

- Open the `Delete.razor` file in the `Pages/Photos` folder of the `PhotoSharingApplication` project
- After the `@page` directive, add the following code:

```cs
@inject IPhotosRepository PhotosRepository
@inject NavigationManager NavigationManager
```

- Replace the `@code` section method with the following:

```cs
@code {
    [Parameter]
    public int Id { get; set; }

    [SupplyParameterFromForm(FormName = "PhotoDeleteForm")]
    Photo? photo { get; set;}

    protected override void OnParametersSet()
    {
        photo ??= PhotosRepository.GetPhotoById(Id);
    }

    void DeletePhoto()
    {
        PhotosRepository.RemovePhoto(photo.Id);
        NavigationManager.NavigateTo("photos/all");
    }
}
```

If you run the application now, you should see the same behavior as before, but this time the data is coming from the service we created. Also, once you delete a photo, user is redirected to the `Photos/All` page.



