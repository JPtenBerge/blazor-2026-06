# Lab 5: Adding Photo Upload Functionality to Your Blazor App

### Lab Objective:
In this lab, you're going to take your Blazor photo editor to the next level by adding the ability to upload an actual photo file. Up until now, you've been able to create and edit the photo's title and description, but now it’s time to let users upload the image itself. You’ll modify both your `Photo` class and your Razor component to handle file uploads.

### What You’re Going to Do:

1. **Modify the Photo Class**
   - First things first, you need to tweak your `Photo` class to store the image data. Right now, your `Photo` class only has `Id`, `Title`, and `Description`. You’re going to add properties for the file's content and its MIME type.
   - Update your `Photo` class like this:
     ```csharp
     public class Photo
     {
         public int Id { get; set; }
         public string Title { get; set; }
         public string Description { get; set; }
         public byte[] PhotoFile { get; set; }
         public string ImageMimeType { get; set; }
         public string? DataUrl => PhotoFile == null ? null : $"data:{ImageMimeType};base64,{Convert.ToBase64String(PhotoFile)}";
     }
     ```
   - This gives you the ability to store the file's binary data (`PhotoFile`) and its MIME type (`ImageMimeType`). The `DataUrl` property is a handy way to display the image right in your app later on.

2. **Update the Razor Component**
   - Now, let’s modify your Razor component to handle file uploads. You’ll add an `InputFile` component to the form and wire it up to the `Photo` object.
   - Update the form markup in your component to include the file input:
     ```html
     <p>
         <label>
             File:
             <InputFile name="fileModel.File" />
         </label>
     </p>
     ```
   - Add `enctype="multipart/form-data"` to the `EditForm`. This is important because it tells the form to send the file data properly when the user submits it.

3. **Handle the File Upload in Code**
   - To actually capture and store the file data, you'll need to make some changes in the `@code` block of your Razor component.
   - Start by adding a `FileModel` class to hold the uploaded file:
     ```csharp
     class FileModel
     {
         public IFormFile File { get; set; }
     }
     ```
   - Then, update your `OnParametersSet` method to initialize the `fileModel` so that it's ready to accept file data:
     ```csharp
     fileModel ??= new FileModel();
     ```
   - Next, modify the `Save` method to process the file upload. You'll read the file into a `MemoryStream`, convert it to a byte array, and store it in the `Photo` object:
     ```csharp
     async Task Save()
     {
         if(fileModel.File is not null)
         {
             using var memoryStream = new MemoryStream();
             await fileModel.File.OpenReadStream().CopyToAsync(memoryStream);
             photo.PhotoFile = memoryStream.ToArray();
             photo.ImageMimeType = fileModel.File.ContentType;
         }
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
     ```
4. **Update the Repository**
   - Finally, you need to make sure your repository can handle the new `PhotoFile` and `ImageMimeType` properties. You’ll need to update the `UpdatePhoto` method to include these new fields.
    - In your `PhotosRepository` class, modify the `UpdatePhoto` method to save the new properties:
      ```csharp
      public void UpdatePhoto(Photo photo) {
        var photoToUpdate = photos.FirstOrDefault(p => p.Id == photo.Id);
        if (photoToUpdate is null)
            return;
        photoToUpdate.Title = photo.Title;
        photoToUpdate.Description = photo.Description;
        photoToUpdate.PhotoFile = photo.PhotoFile;
        photoToUpdate.ImageMimeType = photo.ImageMimeType;
      }
      ```
    - This ensures that when you update a photo, the new file data is saved along with the title and description.
4. **Test the Upload Functionality**
   - Once you’ve made these changes, run your app and go to the photo editor page. You should now see an option to upload a photo file.
   - Try uploading a file, then check if it’s stored properly by viewing the photo or by checking the console output if you've added any debug logging.

## Showing the picture

Now that you’ve got the photo upload functionality working, it’s time to display those photos in your app. You’ll use the `DataUrl` property you added to your `Photo` class to render the images in the UI.

#### What You’re Going to Do:

1. **Understanding the DataUrl Property**
   - Before you start, let's quickly recap what the `DataUrl` property in your `Photo` class does:
     ```csharp
     public string? DataUrl => PhotoFile == null ? null : $"data:{ImageMimeType};base64,{Convert.ToBase64String(PhotoFile)}";
     ```
   - This property converts the photo's binary data into a base64 string that can be used directly in the `src` attribute of an `<img>` tag. This is how you’ll display the photo in the browser.

2. **Add the Image to the Editor Page**
   - Let’s start by adding the image preview to the photo editor page. This will allow users to see the photo they are editing.
   - In your `Photos/Editor.razor`, add the following line of code where you want the image to be displayed:
     ```html
     <img src="@photo.DataUrl" alt="@photo.Title" />
     ```
   - You can place this right under the form fields, so the user sees the image while editing an existing picture.

3. **Show the Image on the Details Page**
   - The details page is another great place to display the image. Here, users will see the full photo with its title and description.
   - In `Photos/Details.razor`, add the same `<img>` tag wherever you want the image to appear:
     ```html
     <img src="@photo.DataUrl" alt="@photo.Title" />
     ```
   - This will ensure that whenever someone views the details of a photo, the image is shown prominently.

4. **Display the Image in the Photo List (All Photos)**
   - If you have a page that lists all photos, it’s a good idea to show a thumbnail of each photo there. This gives users a quick visual overview.
   - In `Photos/All.razor`, you can add the `<img>` tag inside a loop that displays all photos:
     ```cs
     @foreach (var photo in photos)
     {
         <div>
             <img src="@photo.DataUrl" alt="@photo.Title" style="max-width: 150px;" />
             <p>@photo.Title</p>
         </div>
     }
     ```
   - We will limit the size of the image using CSS to create a neat grid of thumbnails at a later time.

5. **Add the Image to the Delete Confirmation Page**
   - Lastly, when someone is about to delete a photo, it’s helpful to show the image they’re about to remove. This prevents accidental deletions.
   - In `Photos/Delete.razor`, place the image inside the confirmation dialog:
     ```html
     <h3>Are you sure you want to delete this?</h3>
     <article>
         <img src="@photo.DataUrl" alt="@photo.Title" />
         <p>@photo.Title</p>
         <p>@photo.Description</p>
     </article>
     ```

6. **Test It Out**
   - After you’ve added the image displays, run your app and navigate through the pages. You should now see the uploaded photos appearing in all the places you added the `<img>` tag.
   - Check that the images render correctly and that they match the titles and descriptions you entered.


## The NavLink component

The very last thing for this lab is to sprinkle some [navigation links](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-8.0#navlink-component) here and there, so that the user can go to the different pages more easily, without having to write the url by hand.

Let's start with the `AllPhotos.razor`.

We will insert a link to view the details, update and delete for each product (passing the specific id to the route) and one link to create a new product.

The last one is actually the easiest, because it's a static address:

```html
<NavLink href="photos/create">Upload new Photo</NavLink>
```

The links for details, update and delete need to be constructed using an [Explicit Razor Expression](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-8.0#explicit-razor-expressions), like this:

```html
<div>
  <NavLink href="@($"photos/details/{photo.Id}")">Details</NavLink>
  <NavLink href="@($"photos/edit/{photo.Id}")">Update</NavLink>
  <NavLink href="@($"photos/delete/{photo.Id}")">Delete</NavLink>
</div>
```

Save and verify that each photo in the `/photos/all` page now links to its own edit, update and delete page.
