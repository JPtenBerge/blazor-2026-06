# Lab 18: Custom Validation with FluentValidation in Blazor

In this lab, you'll learn how to create custom validation rules in a Blazor application using the FluentValidation library. Instead of using the default `DataAnnotationValidator`, you'll create your own `PhotoValidator` class to enforce validation rules on the `Photo` model. You'll then integrate this validator into your application, ensuring that the repository methods aren't called if validation errors exist. Finally, you'll build a custom Blazor component to display validation messages, enhancing your understanding of form validation in Blazor.

### Step 1: Install FluentValidation

1. **Add the FluentValidation package to your Blazor server project:**

   Use the NuGet Package Manager or run the following command in the Package Manager Console:

   ```bash
   dotnet add package FluentValidation
   ```

### Step 2: Create the `PhotoValidator` Class

You will create a custom validator for the `Photo` model. This validator will enforce rules such as ensuring that the `Title` field is not empty and does not exceed 100 characters, and that the `Description` field does not exceed 300 characters.

1. **Create the `PhotoValidator` Class:**

   ```csharp
   using FluentValidation;
   using PhotoSharingApplication.Core.Models;

   namespace PhotoSharingApplication.Validators;

   public class PhotoValidator : AbstractValidator<Photo>
   {
       public PhotoValidator()
       {
           RuleFor(photo => photo.Title).NotEmpty().MaximumLength(100);
           RuleFor(photo => photo.Description).MaximumLength(300);
       }
   }
   ```

2. **Register the Validator in the DI Container:**

   Open your `Program.cs` file and add the following line to register the `PhotoValidator` in the dependency injection container:

   ```csharp
   builder.Services.AddScoped<IValidator<Photo>, PhotoValidator>();
   ```

### Step 3: Integrate Validation in the `PhotosService`

Next, you'll modify the `PhotosService` to use the `PhotoValidator` during the add and update operations. If validation fails, the repository methods will not be invoked.

1. **Modify the `PhotosService` Class:**

   Inject the `IValidator<Photo>` into the `PhotosService` constructor and use it to validate the `Photo` object before performing any database operations.

   ```csharp
   using FluentValidation;
   using PhotoSharingApplication.Core.Interfaces;
   using PhotoSharingApplication.Core.Models;

   public class PhotosService : IPhotosService
   {
       private readonly IPhotosRepository _photosRepository;
       private readonly IAuthorizationService _authorizationService;
       private readonly AuthenticationStateProvider _authenticationStateProvider;
       private readonly IValidator<Photo> _photoValidator;

       public PhotosService(IPhotosRepository photosRepository, IAuthorizationService authorizationService, AuthenticationStateProvider authenticationStateProvider, IValidator<Photo> photoValidator)
       {
           _photosRepository = photosRepository;
           _authorizationService = authorizationService;
           _authenticationStateProvider = authenticationStateProvider;
           _photoValidator = photoValidator;
       }

       public async Task<Photo> AddPhotoAsync(Photo photo)
       {
           var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
           photo.Owner = authState?.User?.Identity?.Name ?? throw new UnauthorizedException();

           // Validate the photo
           _photoValidator.ValidateAndThrow(photo);

           return await _photosRepository.AddPhotoAsync(photo);
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
               // Validate the photo
               _photoValidator.ValidateAndThrow(photo);

               await _photosRepository.UpdatePhotoAsync(photo);
           }
       }
       // Other methods omitted for brevity
   }
   ```

### Step 4: Create the `PhotoFluentValidator` Component

Instead of using the built-in `DataAnnotationsValidator`, you will create your own Blazor component to handle validation using FluentValidation.

1. **Create the `PhotoFluentValidator` Component:**

   Add a new class `PhotoFluentValidator.cs` to your Blazor project:

   ```csharp
   using Microsoft.AspNetCore.Components.Forms;
   using Microsoft.AspNetCore.Components;
   using FluentValidation;
   using PhotoSharingApplication.Core.Models;

   namespace PhotoSharingApplication.Components.CustomValidators;

   public class PhotoFluentValidator : ComponentBase {
       private ValidationMessageStore? messageStore;

       [CascadingParameter]
       private EditContext? CurrentEditContext { get; set; }

       [Inject]
       public IValidator<Photo> Validator { get; set; }

       protected override void OnInitialized()
       {
           if (CurrentEditContext is null)
           {
               throw new InvalidOperationException(
                   $"{nameof(PhotoFluentValidator)} requires a cascading " +
                   $"parameter of type {nameof(EditContext)}. " +
                   $"For example, you can use {nameof(PhotoFluentValidator)} " +
                   $"inside an {nameof(EditForm)}.");
           }

           messageStore = new(CurrentEditContext);

           CurrentEditContext.OnValidationRequested += (s, e) => DisplayErrors();
           CurrentEditContext.OnFieldChanged += (s, e) => DisplayErrors();
       }

       public void DisplayErrors()
       {
           if (CurrentEditContext is not null)
           {
               messageStore?.Clear();
               Dictionary<string, List<string>> errors = Validator.Validate(CurrentEditContext.Model as Photo).Errors
                   .GroupBy(e => e.PropertyName)
                   .ToDictionary(
                       g => g.Key,
                       g => g.Select(e => e.ErrorMessage).ToList()
                   );
               foreach (var err in errors)
               {
                   messageStore?.Add(CurrentEditContext.Field(err.Key), err.Value);
               }

               CurrentEditContext.NotifyValidationStateChanged();
           }
       }
   }
   ```

### Step 5: Update the `Editor.razor` Component

Now, you'll integrate the `PhotoFluentValidator` component into the `Editor.razor` component to display validation messages.

1. **Modify `Editor.razor`:**

   Add the `PhotoFluentValidator` component along with the `ValidationSummary` and individual `ValidationMessage` components:

   ```csharp
   <EditForm Model="@photo" OnValidSubmit="Save" FormName="PhotoForm" enctype="multipart/form-data">
       <PhotoFluentValidator />
       <ValidationSummary />
       <div class="mb-3">
           <label for="photoTitle" class="form-label">Title:</label>
           <InputText @bind-Value="photo.Title" class="form-control" id="photoTitle"/>
           <ValidationMessage For="() => photo.Title"/>
       </div>
       <div class="mb-3">
           <label for="photoDescription" class="form-label">Description (optional):</label>
           <InputTextArea @bind-Value="photo.Description" id="photoDescription" class="form-control" />
           <ValidationMessage For="() => photo.Description" />
       </div>
       <button type="submit" class="btn btn-primary">Save</button>
   </EditForm>
   ```

### Step 6: Customize Validation Styles

To enhance the user experience, you can add custom styles to highlight invalid form fields and display error messages.

1. **Add Styles to `app.css`:**

   Add the following styles to your `app.css` file:

   ```css
   @keyframes horizontal-shaking {
       0% {
           transform: translateX(0)
       }

       25% {
           transform: translateX(5px)
       }

       50% {
           transform: translateX(-5px)
       }

       75% {
           transform: translateX(5px)
       }

       100% {
           transform: translateX(0)
       }
   }

   .invalid {
       animation: horizontal-shaking 0.5s;
       animation-iteration-count: 2;
       border: 1px solid var(--bs-danger);
   }

   ul.validation-errors {
       list-style-type: none;
       color: var(--bs-danger);
       font-weight:bold;
       margin-left:0;
       padding-left: 0;
   }

   .validation-message {
       color: var(--bs-danger);
       animation: horizontal-shaking 0.5s;
       animation-iteration-count: 2;
   }
   ```

### Step 7: Test Your Application

Run your application and navigate to the photo editor. Try submitting the form with invalid data to see your custom validation in action. The form should prevent submission and display the appropriate validation messages. If you enter valid data, the form should submit successfully.
