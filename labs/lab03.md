# Lab 3:  Forms

Now that we have a Blazor project with our first Photos/All and Photos/Details, we will proceed to create two additional pages that will allow us to add a new photo, modify an existing photo and delete an existing one.

### Creating a Blazor Photo Editor Form

#### Lab Objective:

You will create a Blazor component to allows users to create and edit photo records. The lab involves setting up routes for creating and editing photos, binding data to form elements, and handling form submission. You will learn how to work with Blazor's `EditForm` component, data binding, and parameter passing within a Razor component.

#### Lab Steps:

1. **Set Up Routing for the Component**
   - Start by defining two routes for the Blazor component. These routes will handle both creating a new photo and editing an existing one.
   - Use the `@page` directive to define the following routes at the top of your Razor component:
     ```csharp
     @page "/photos/create"
     @page "/photos/edit/{id:int}"
     ```
   - The first route will be used for creating new photos, and the second route will be used for editing existing photos by their ID.

2. **Create the Basic Structure of the Form**
   - Below the route definitions, add a header for your form:
     ```html
     <h3>Editor</h3>
     ```
   - Next, create an `EditForm` component that binds to a `Photo` object. Set up the form to handle submissions with a `Save` method:
     ```html
     <EditForm Model="@photo" OnSubmit="Save" FormName="PhotoForm">
     ```

3. **Add Form Fields for Photo Properties**
   - Inside the `EditForm`, add form controls for the `Id`, `Title`, and `Description` properties of the `Photo` object:
     - The `Id` field should be hidden, as it will be automatically managed.
     - Use an `InputText` component for the `Title` and an `InputTextArea` component for the `Description`:
       ```html
       <InputNumber @bind-Value="photo.Id" hidden />
       <p>
           <label>
               Title:
               <InputText @bind-Value="photo.Title" />
           </label>
       </p>
       <p>
           <label>
               Description (optional):
               <InputTextArea @bind-Value="photo.Description" />
           </label>
       </p>
       ```
   - Finish the form with a submit button:
     ```html
     <button type="submit">Save</button>
     </EditForm>
     ```

4. **Implement the Code-Behind Logic**
   - Below the form markup, implement the C# logic for handling parameters and saving data:
     - Define the `Id` parameter to handle the photo's unique identifier from the URL:
       ```csharp
       [Parameter]
       public int? Id { get; set; }
       ```
     - Bind the `photo` object to the form using the `SupplyParameterFromForm` attribute:
       ```csharp
       [SupplyParameterFromForm(FormName = "PhotoForm")]
       Photo? photo { get; set; }
       ```
     - Initialize a list of photos for the example. This list simulates a data source:
       ```csharp
       List<Photo> photos;
       protected override void OnInitialized()
       {
           photos = new List<Photo>{
               new Photo { Id = 1, Title = "My Title", Description = "Lorem ipsum dolor sit amen" },
               new Photo { Id = 2, Title = "Another Title", Description = "All work and no play makes Jack a dull boy" },
               new Photo { Id = 3, Title = "Yet another Title", Description = "Some description" }
           };        
       }
       ```
     - Use the `OnParametersSet` method to check if an `Id` was passed in the URL. If an `Id` is present, fetch the corresponding photo. Otherwise, create a new `Photo` object:
       ```csharp
       protected override void OnParametersSet()
       {
           if(Id.HasValue)
           {
               photo ??= photos.FirstOrDefault(p => p.Id == Id);
           }
           else
           {
               photo ??= new Photo();
           }    
       }
       ```
     - Finally, implement the `Save` method to handle form submission. For this lab, simply output the photo details to the console:
       ```csharp
       void Save()
       {
           Console.WriteLine($"Saving photo: {photo.Id} {photo.Title} {photo.Description}");
       }
       ```

5. **Define the `Photo` Class**
   - Define a `Photo` class within the component to represent the photo data structure:
     ```csharp
     class Photo
     {
         public int Id { get; set; }
         public string Title { get; set; }
         public string Description { get; set; }
     }
     ```


### Photo Deletion 

#### Objective:
You’re going to build on your previous work by adding the ability to delete a photo from your list. This is where your app starts to become more dynamic and user-interactive, allowing users to not just create and edit photos, but also remove them entirely. You’ll learn how to set up a delete confirmation, handle user input, and manage the deletion process within Blazor.

#### Prerequisites:
Before starting, make sure you've completed the previous lab where you created the photo editor form. You'll be continuing from that codebase, so have it ready.

#### What You’re Going to Do:
1. **Set Up the Delete Route**
   - First things first, you need to define a new route that will handle photo deletions. This route will take the `id` of the photo you want to delete.
   - At the top of your Razor component, add the following directive:
     ```csharp
     @page "/photos/delete/{id:int}"
     ```
   - This route will trigger the component whenever a user tries to delete a photo, passing the photo's ID as a parameter.

2. **Create the Delete UI**
   - Now, you’re going to create the UI that asks the user if they really want to delete the photo. 
   - Start by adding a header:
     ```html
     <h3>Delete</h3>
     ```
   - Next, add a conditional block that checks if the photo exists. If it doesn't, display a "Photo not found" message:
     ```csharp
     @if (photo is null)
     {
         <p>Photo not found</p>
     } 
     else
     {
     ```
   - If the photo exists, display its details so the user can confirm it's the right one before deleting:
     ```html
         <h3>Are you sure you want to delete this?</h3>
         <article>
             <p>@photo.Id</p>
             <p>@photo.Title</p>
             <p>@photo.Description</p>
         </article>   
     ```
   - Finally, add a delete button wrapped in an `EditForm` to handle the deletion when clicked:
     ```html
         <EditForm FormName="PhotoDeleteForm" Model="photo" OnSubmit="DeletePhoto">
             <button type="submit">Delete</button>
         </EditForm>
     }
     ```

3. **Write the Code to Handle Deletion**
   - Below the markup, you need to write the C# code that will actually delete the photo when the form is submitted.
   - Start by defining the `Id` parameter to capture the photo's ID from the route:
     ```csharp
     [Parameter]
     public int Id { get; set; }
     ```
   - Then, initialize your list of photos in the `OnInitialized` method, just like you did before:
     ```csharp
     List<Photo> photos;

     protected override void OnInitialized()
     {
         photos = new List<Photo>{
             new Photo { Id = 1, Title = "My Title", Description = "Lorem ipsum dolor sit amen" },
             new Photo { Id = 2, Title = "Another Title", Description = "All work and no play makes Jack a dull boy" },
             new Photo { Id = 3, Title = "Yet another Title", Description = "Some description" }
         };
         photo = photos.FirstOrDefault(p => p.Id == Id);
     }
     ```
   - Use the `OnParametersSet` method to retrieve the photo based on the ID passed from the route:
     ```csharp
     protected override void OnParametersSet()
     {
         photo ??= photos.FirstOrDefault(p => p.Id == Id);    
     }
     ```
   - Finally, write the `DeletePhoto` method to remove the photo. For now, simply log the deletion to the console (you'll replace this with actual deletion logic later):
     ```csharp
     void DeletePhoto()
     {
         Console.WriteLine($"Deleting photo: {photo.Id} {photo.Title} {photo.Description}");
     }
     ```

4. **Test It Out**
   - Once you've completed these steps, run your app and navigate to a URL like `/photos/delete/1`.
   - You should see the confirmation screen with the details of the photo. If you click "Delete," it should log the photo's details to the console, indicating it would be deleted.

In the real world, instead of just logging the deletion, you'd remove the photo from the list and perhaps also from a database. But for now, logging to the console is a good first step to make sure everything works as expected.

---

This is a crucial step in building a full CRUD (Create, Read, Update, Delete) application. Once you have this down, you'll be ready to handle more complex data management scenarios in your Blazor apps.



