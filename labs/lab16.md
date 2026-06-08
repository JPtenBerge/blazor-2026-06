# Lab 16: Resource Based Authorization - Monolithic Application

In this lab students are going to make sure that only owners of a photo may update and delete their photos. This is an example of resource based authorization.

# IMPORTANT: This lab assumes that you have a monolithic application with a Blazor project with the Frontend, the Backend and the Identity provider all in one. If you have a distributed application with Duende and Oidc, skip this lab and follow lab 16_oidc instead.

## Steps

### Photo Owner

1. Add a new property to the `Photo` class to store the owner of the photo. This property should be of type `string` and should be named `Owner`.
2. Add a migration and update the database schema.

```bash
Add-Migration -Context PhotoSharingApplication.Infrastructure.Data.PhotosDbContext PhotoCommentOwner
Update-Database -Context PhotoSharingApplication.Infrastructure.Data.PhotosDbContext
```

### Policies

1. Add a new policy to `Program.cs` that checks if the user is the owner of the photo.

```csharp
builder.Services.AddAuthorizationBuilder()
.AddPolicy("PhotoOwner", policy => policy.Requirements.Add(new PhotoOwnerRequirement()));
builder.Services.AddSingleton<IAuthorizationHandler, PhotoOwnerHandler>();
```

2. Add a new class `PhotoOwnerRequirement` that implements `IAuthorizationRequirement`.

```csharp
public class PhotoOwnerRequirement : IAuthorizationRequirement
{
}
```

3. Add a new class `PhotoOwnerHandler` that implements `IAuthorizationHandler`.

```csharp
public class PhotoOwnerHandler : AuthorizationHandler<PhotoOwnerRequirement, Photo>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PhotoOwnerRequirement requirement, Photo resource)
    {
        if (context.User.Identity.Name == resource.Owner)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

### Blazor

### Photos/All.razor

- Add an `AuthorizeView` component to the `Photos/All.razor` component to show the `Create New` link only to authenticated users. 
- Add a `NotAuthorized` section to show a link to the login page.

```csharp
<AuthorizeView>
    <Authorized>
        <NavLink href="photos/create">Create New</NavLink>
    </Authorized>
    <NotAuthorized>
        <NavLink class="nav-link" href="Account/Login">
            <span class="bi bi-person-badge-nav-menu" aria-hidden="true"></span> Login to add a Photo
        </NavLink>
    </NotAuthorized>
</AuthorizeView>
```

### Photos/Delete.razor

We already added the `[Authorize]` attribute to the `Photos/Delete.razor` component, meaning we are sure that only authenticated users can access this page. What we still need to check is whether the Authenticated User is the owner of the photo.  
In order to do that, we will call the AuthorizationService, passing the Authenticated User, the Photo and the name of the Policy to check.  
If the user is not the owner of the photo, we will redirect the user to the `Account/AccessDenied` page.

To do that, we need to 
- Inject the `IAuthorizationService` in the `Photos/Delete.razor` component
- Inject the `NavigationManager` in the `Photos/Delete.razor` component
- Get the Authenticated User from the `AuthenticationStateProvider` in the `Photos/Delete.razor` component

1. Inject the `IAuthorizationService` in the `Photos/Delete.razor` component

```csharp
@inject IAuthorizationService AuthorizationService
```

2. Add a new property to store the `AuthenticationStateTask` of type `Task<AuthenticationState>` in the `Photos/Delete.razor` component. This property will be used to get the Authenticated User and it is available as `CascadingParameter`.

```csharp
[CascadingParameter]
public Task<AuthenticationState> AuthenticationStateTask { get; set; }
```

3. In the OnParametersSetAsync method, get the Authenticated User and check if the user is the owner of the photo. If not, redirect the user to the `Account/AccessDenied` page.

```csharp
protected override async Task OnParametersSetAsync()
{
    photo ??= await PhotosRepository.GetPhotoByIdAsync(Id);
    if (AuthenticationStateTask is not null)
    {
        var authState = await AuthenticationStateTask;
        var authorizationResult = await AuthorizationService.AuthorizeAsync(authState.User, photo, PhotoSharingPolicies.PhotoOwnerPolicy);
        if (!authorizationResult.Succeeded)
        {
            NavigationManager.NavigateTo("Account/AccessDenied");
        }
    }
}
```

### Photos/Editor.razor

In this page, we should make sure that only the owner of the photo can update it.
The steps are very similar to the ones we did in the `Photos/Delete.razor` component.
If instead a User is trying to add a new Photo, we should add their name in the Owner property of the new Photo.

1. Inject the `IAuthorizationService` in the `Photos/Editor.razor` component

```csharp
@inject IAuthorizationService AuthorizationService
```

2. Add a new property to store the `AuthenticationStateTask` of type `Task<AuthenticationState>` in the `Photos/Editor.razor` component. This property will be used to get the Authenticated User and it is available as `CascadingParameter`.

```csharp
[CascadingParameter]
public Task<AuthenticationState> AuthenticationStateTask { get; set; }
```

3. In the OnParametersSetAsync method, 
  - if the route has an Id then get the Authenticated User and check if the user is the owner of the photo, redirecting the user to the `Account/AccessDenied` page if they are not.
  - if the route does not have an Id, get the Authenticated User and add their name in the Owner property of the new Photo.

```csharp
protected override async Task OnParametersSetAsync()
{
    var authState = await AuthenticationStateTask;
    if(Id.HasValue)
    {
        photo ??= await PhotosRepository.GetPhotoByIdAsync(Id.Value);
        var authorizationResult = await AuthorizationService.AuthorizeAsync(authState.User, photo, PhotoSharingPolicies.PhotoOwnerPolicy);
        if (!authorizationResult.Succeeded)
        {
            NavigationManager.NavigateTo("Account/AccessDenied");
        }
    }
    else
    {
        photo ??= new Photo() { Title = "New Photo", Owner = authState.User.Identity.Name };
    }
    fileModel ??= new FileModel();
}    
```

### PhotoCard.razor

In the `PhotoCard.razor` component, we should show the `Edit` and `Delete` buttons only to the owner of the photo.
Once again, we need the AuthenticationState and the AuthorizationService to get the User and check the Policy.

1. Inject the `IAuthorizationService` in the `PhotoCard.razor` component

```csharp
@inject IAuthorizationService AuthorizationService
```

2. Add a new property to store the `AuthenticationStateTask` of type `Task<AuthenticationState>` in the `PhotoCard.razor` component. This property will be used to get the Authenticated User and it is available as `CascadingParameter`.

```csharp
[CascadingParameter]
public Task<AuthenticationState> AuthenticationStateTask { get; set; }
```

3. Create private properties to store the visibility of the `Update` and `Delete` buttons.

```csharp
private bool _showUpdate;
private bool _showDelete;
```

4. In the OnParametersSetAsync method, get the Authenticated User and check if the user is the owner of the photo. 

```csharp
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
```

5. Update the `ShowFooter` property to depend on the new private internal state

```csharp
private bool ShowFooter => ShowDetails || _showUpdate || _showDelete;
```

6. Update the `PhotoCard.razor` component to show the `Edit` and `Delete` buttons only to the owner of the photo.

```csharp
@if (_showUpdate)
{
    <NavLink href="@($"photos/edit/{Photo.Id}")" class="card-link">Update</NavLink>
}
@if (_showDelete)
{
    <NavLink href="@($"photos/delete/{Photo.Id}")" class="card-link">Delete</NavLink>
}
```

### Refactor

1. Create a new class `PhotoSharingPolicies` to store the name of the policies.

```csharp
public static class PhotoSharingPolicies
{
    public const string PhotoOwnerPolicy = "PhotoOwner";
}
```

2. Create an Extension Method to add the `PhotoOwner` policy to the `AuthorizationOptions`.

```csharp
public static class PhotoSharingPoliciesExtensions {
    public static IServiceCollection AddPhotoSharingPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder().AddPolicy(PhotoSharingPolicies.PhotoOwnerPolicy, policy => policy.Requirements.Add(new PhotoOwnerRequirement()));
        services.AddSingleton<IAuthorizationHandler, PhotoOwnerHandler>();
        return services;
    }
}
```

3. Update the `Program.cs` to use the new Extension Method.

- Replace

```csharp
builder.Services.AddAuthorizationBuilder()
.AddPolicy("PhotoOwner", policy => policy.Requirements.Add(new PhotoOwnerRequirement()));
builder.Services.AddSingleton<IAuthorizationHandler, PhotoOwnerHandler>();
```

- With

```csharp
builder.Services.AddPhotoSharingPolicies();
```

In the `Photos/Delete.razor` and `Photos/Editor.razor` components, replace the `"PhotoOwnerPolicy"` string with the `PhotoSharingPolicies.PhotoOwnerPolicy` constant.

```csharp
var authorizationResult = await AuthorizationService.AuthorizeAsync(authState.User, photo, PhotoSharingPolicies.PhotoOwnerPolicy);
```

### Test

1. Run the application and and navigate to `Photos/all`. 
  - You should see the `Create New` link only if you are authenticated.
  - If you are not authenticated, you should see a link to the login page.
  - If you are authenticated, you should see the `Edit` and `Delete` buttons only for the photos that you are the owner of.
2. Try to access the `Photos/Delete` page of a photo that you are not the owner of, writing the address on the browser address bar. You should be redirected to the `Account/AccessDenied` page.
3. Try to access the `Photos/Editor` page of a photo that you are not the owner of, writing the address on the browser address bar. You should be redirected to the `Account/AccessDenied` page.
4. Try to access the `Photos/Delete` page of a photo that you are the owner of. You should be able to access the page.
5. Try to access the `Photos/Editor` page of a photo that you are the owner of. You should be able to access the page.

### Bonus

Instead of having the Razor components access the Repository directly, we could create a new service that would be responsible for the authorization checks before invoking the Repository. This way, we would have a cleaner separation of concerns.
The Razor component should still be responsible for the User Interface and the navigation in case of failure, but the authorization checks during the actual Update and Delete should be done by the service.

1. Create a new service `PhotoService` that will be responsible for the authorization checks.

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
            await _photosRepository.UpdatePhotoAsync(photo);
        }
    }
}
```

- Create the Interface `IPhotosService`

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
}
```

- Register the Service in the `Program.cs`

```csharp
builder.Services.AddScoped<IPhotosService, PhotosService>();
```

- Use the `IPhotosService` in the `Photos/All.razor`, `Photos\Details.razor,` `Photos/Editor.razor` and `Photos/Delete.razor` components.

# Optional: Protect the API

This part isn't strictly related to Blazor, but it's a good practice to protect the API as well.

- Add an Author property to the Comment class and update the database schema by creating and applying a migration.
- Create the Policies, Handlers and Extensions as we did for the Blazor application.
- Create a Service that will be responsible for the authorization checks in the API.
- Use the `IAuthorizationService` to check if the user is the owner of the comment before updating or deleting it.
  - Since the Service will run server side, we can inject the HttpContextAccessor to get the User.
- Use the Service in the CommentsController to check if the user is the owner of the comment before updating or deleting it.

### Comment class

```csharp
namespace PhotoSharingApplication.Client.Core.Models;

public class Comment
{
    public int Id { get; set; }
    public int PhotoId { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public string? Author { get; set; }
}
```

- Add Migration and Update Database

```bash
Add-Migration -Context PhotoSharingApplication.Infrastructure.Data.PhotosDbContext CommentAuthor
Update-Database -Context PhotoSharingApplication.Infrastructure.Data.PhotosDbContext
```

### Policies

```csharp
using Microsoft.AspNetCore.Authorization;

namespace PhotoSharingApplication.Auth;

public class CommentOwnerRequirement : IAuthorizationRequirement
{
}
```

```csharp
using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Client.Core.Models;

namespace PhotoSharingApplication.Auth;

public class CommentOwnerHandler : AuthorizationHandler<CommentOwnerRequirement, Comment>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CommentOwnerRequirement requirement, Comment resource)
    {
        System.Security.Claims.ClaimsPrincipal user = context.User;
        if (user.Identity.IsAuthenticated && user.Identity.Name == resource.Author)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

### Extensions

```csharp
public static class PhotoSharingPolicies
{
    public const string PhotoOwnerPolicy = "PhotoOwner";
    public const string CommentOwnerPolicy = "CommentOwner";
}
```

```csharp
public static class PhotoSharingPoliciesExtensions {
    public static AuthorizationBuilder RequireCommentOwner(this AuthorizationBuilder builder)
    {
        builder.AddPolicy(PhotoSharingPolicies.CommentOwnerPolicy, policy => { 
            policy.Requirements.Add(new CommentOwnerRequirement());
            policy.RequireAuthenticatedUser();
        });
        return builder;
    }

    public static AuthorizationBuilder RequirePhotoOwner(this AuthorizationBuilder builder)
    {
        builder.AddPolicy(PhotoSharingPolicies.PhotoOwnerPolicy, policy =>
        {
            policy.Requirements.Add(new PhotoOwnerRequirement());
            policy.RequireAuthenticatedUser();
        });
        return builder;
    }

    public static IServiceCollection AddPhotoSharingPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder().RequireCommentOwner().RequirePhotoOwner();

        services.AddSingleton<IAuthorizationHandler, CommentOwnerHandler>();
        services.AddSingleton<IAuthorizationHandler, PhotoOwnerHandler>();
        return services;
    }
}
```

### Service

```csharp
using Microsoft.AspNetCore.Authorization;
using PhotoSharingApplication.Auth;
using PhotoSharingApplication.Client.Core.Interfaces;
using PhotoSharingApplication.Client.Core.Models;
using PhotoSharingApplication.Core.Interfaces;

namespace PhotoSharingApplication.Core.Services;

public class CommentsService(ICommentsRepository commentsRepository, IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService) : ICommentsService
{
    public async Task<Comment?> GetCommentByIdAsync(int id)
    {
        return await commentsRepository.GetCommentByIdAsync(id);
    }
    public async Task<List<Comment>> GetCommentsForPhotoAsync(int photoId)
    {
        return await commentsRepository.GetCommentsForPhotoAsync(photoId);
    }
    public async Task DeleteCommentAsync(int id)
    {
        Comment comment = await commentsRepository.GetCommentByIdAsync(id) ?? throw new Exception("Comment not found");
        AuthorizationResult result = await authorizationService.AuthorizeAsync(httpContextAccessor.HttpContext.User, comment, "CommentOwner");
        if (!result.Succeeded)
        {
            throw new UnauthorizedException();
        }
        await commentsRepository.DeleteCommentAsync(id);
    }
    public async Task<Comment> AddCommentAsync(Comment comment)
    {
        comment.Author = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? throw new UnauthorizedException();
        return await commentsRepository.AddCommentAsync(comment);
    }
    public async Task UpdateCommentAsync(Comment comment)
    {
        AuthorizationResult result = await authorizationService.AuthorizeAsync(httpContextAccessor.HttpContext.User, comment, "CommentOwner");
        if (!result.Succeeded)
        {
            throw new UnauthorizedException();
        }
        await commentsRepository.UpdateCommentAsync(comment);
    }
}
```

The interface

```csharp
using PhotoSharingApplication.Client.Core.Models;

namespace PhotoSharingApplication.Core.Interfaces;

public interface ICommentsService
{
    Task<Comment> AddCommentAsync(Comment comment);
    Task DeleteCommentAsync(int id);
    Task<Comment?> GetCommentByIdAsync(int id);
    Task<List<Comment>> GetCommentsForPhotoAsync(int photoId);
    Task UpdateCommentAsync(Comment comment);
}
```

Register the Service in the `Program.cs`

```csharp
builder.Services.AddScoped<ICommentsService, CommentsService>();
```

### Controller

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhotoSharingApplication.Auth;
using PhotoSharingApplication.Client.Core.Models;
using PhotoSharingApplication.Core.Interfaces;

namespace PhotoSharingApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController(ICommentsService _commentsService) : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> GetComment(int id)
        {
            Comment? comment = await _commentsService.GetCommentByIdAsync(id);

            if (comment is null)
            {
                return NotFound();
            }

            return comment;
        }

        [HttpGet("/api/photos/{photoId}/comments")]
        public async Task<IEnumerable<Comment>> GetCommentsByPhotoId(int photoId)
        {
            return await _commentsService.GetCommentsForPhotoAsync(photoId);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutComment(int id, Comment comment)
        {
            if (id != comment.Id)
            {
                return BadRequest();
            }

            try
            {
                await _commentsService.UpdateCommentAsync(comment);

                return NoContent();
            } catch (UnauthorizedException)
            {
                return Unauthorized();
            }
        }

        [HttpPost]
        public async Task<ActionResult<Comment>> PostComment(Comment comment)
        {
            try
            {
                await _commentsService.AddCommentAsync(comment);
                return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
            } catch (UnauthorizedException)
            {
                return Unauthorized();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Comment>> DeleteComment(int id)
        {
            var comment = await _commentsService.GetCommentByIdAsync(id);
            if (comment is null)
            {
                return NotFound();
            }
            try
            {
                await _commentsService.DeleteCommentAsync(id);
                return comment;
            } catch (UnauthorizedException)
            {
                return Unauthorized();
            }
        }
    }
}
```

### Test

1. Run the application and navigate to `Photos/details`. 
    - If you are not logged on and you try to add a comment, you should see an error message in the console
    - If you are logged on and you try to add a comment, you should be able to add it

### Client-side

- The Blazor application should be updated to use the API to manage the comments. In particular, it should handle the `Unauthorized` response from the API to show the user an error message instead of crashing.

### The CommentsRepository

```csharp
using PhotoSharingApplication.Client.Core.Interfaces;
using PhotoSharingApplication.Client.Core.Models;
using System.Net.Http.Json;

namespace PhotoSharingApplication.Client.Infrastructure;

public class CommentsRepository(HttpClient httpClient) : ICommentsRepository
{
    public async Task<Comment> AddCommentAsync(Comment comment)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/comments", comment);
        if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new Exception("Unauthorized");
        }
        if(response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            //we should read the ProblemDetails from the response and throw a more specific exception
            throw new Exception("Bad Request");
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to add comment");
        }
        return await response.Content.ReadFromJsonAsync<Comment>();
    }

    public async Task DeleteCommentAsync(int id)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"/api/comments/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new Exception("Unauthorized");
        }
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            //we should read the ProblemDetails from the response and throw a more specific exception
            throw new Exception("Bad Request");
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to delete comment");
        }
    }

    public async Task<Comment?> GetCommentByIdAsync(int id)
    {
        HttpResponseMessage response = await httpClient.GetAsync($"/api/comments/{id}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null; // Comment with the specified ID was not found
        }

        response.EnsureSuccessStatusCode(); // Ensure the response is successful

        return await response.Content.ReadFromJsonAsync<Comment>();
    }

    public async Task<List<Comment>> GetCommentsForPhotoAsync(int photoId)
    {
        return await httpClient.GetFromJsonAsync<List<Comment>>($"/api/photos/{photoId}/comments");
    }

    public async Task<Comment> UpdateCommentAsync(Comment comment)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"/api/comments/{comment.Id}", comment);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new Exception("Unauthorized");
        }
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            //we should read the ProblemDetails from the response and throw a more specific exception
            throw new Exception("Bad Request");
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to update comment");
        }
        return comment;
    }
}
```

### The Comments.razor component

```csharp
@implements IDisposable
@inject PersistentComponentState ApplicationState
@inject ICommentsRepository CommentsRepository

<h3>Comments</h3>


<CommentComponentSplit Comment="newComment" CommentState="CommentStates.Insert" OnAddComment="AddComment" />

@if (comments is not null)
{
    @foreach (var comment in comments)
    {
        <CommentComponentSplit Comment="comment" CommentState="CommentStates.View" OnUpdateComment="UpdateComment" OnDeleteComment="DeleteComment" />
    }
}

<p style="position:fixed;bottom:0;" hidden="@errorHidden" class="alert alert-danger" role="alert">@message</p>

@code {
    [Parameter]
    public int PhotoId { get; set; }

    private Comment newComment = default!;

    private List<Comment> comments = default!;
    private PersistingComponentStateSubscription persistingSubscription;

    private string message = string.Empty;
    bool errorHidden = true;

    override protected async Task OnInitializedAsync()
    {
        newComment = new Comment() { PhotoId = PhotoId };

        persistingSubscription = ApplicationState.RegisterOnPersisting(PersistData);

        if (!ApplicationState.TryTakeFromJson <List<Comment>> (nameof(comments), out var restored))
        {
            comments = await CommentsRepository.GetCommentsForPhotoAsync(PhotoId);
        }
        else
        {
            comments = restored!;
        }
    }

    private Task PersistData()
    {
        ApplicationState.PersistAsJson(nameof(comments), comments);
        return Task.CompletedTask;
    }

    void IDisposable.Dispose()
    {
        persistingSubscription.Dispose();
    }

    async Task AddComment(Comment comment)
    {
        // Add comment
        Console.WriteLine($"Add comment {comment.Id}, {comment.Subject}, {comment.Body}");
        try
        {
            await CommentsRepository.AddCommentAsync(comment);
            comments = await CommentsRepository.GetCommentsForPhotoAsync(PhotoId);
            newComment = new Comment() { PhotoId = PhotoId };
            message = string.Empty;
            errorHidden = true;
        }
        catch (Exception ex)
        {
            message = $"Error adding comment: {ex.Message}";
            errorHidden = false;
        }
    }

    async Task UpdateComment(Comment comment)
    {
        // Update comment
        Console.WriteLine($"Update comment {comment.Id}, {comment.Subject}, {comment.Body}");
        try{
            await CommentsRepository.UpdateCommentAsync(comment);
            comments = await CommentsRepository.GetCommentsForPhotoAsync(PhotoId);
            errorHidden = true;
        }
        catch (Exception ex)
        {
            message = $"Error updating comment: {ex.Message}";
            errorHidden = false;
        }
    }

    async Task DeleteComment(Comment comment)
    {
        // Delete comment
        Console.WriteLine($"Delete comment {comment.Id}, {comment.Subject}, {comment.Body}");
        try{
            await CommentsRepository.DeleteCommentAsync(comment.Id);
            comments = await CommentsRepository.GetCommentsForPhotoAsync(PhotoId);
            errorHidden = true;
        }
        catch (Exception ex)
        {
            message = $"Error deleting comment: {ex.Message}";
            errorHidden = false;
        }
    }
}
```

### The CommentComponentBase.cs component

```csharp
override protected void OnParametersSet()
{
    comment = Comment is null ? null : new Comment() { Id = Comment.Id, PhotoId = Comment.PhotoId, Subject = Comment.Subject, Body = Comment.Body, Author = Comment.Author };
}
```

### The CommentComponent.razor component

```csharp
protected override void OnParametersSet()
{
    commentState = CommentState;
    comment = Comment is null ? null : new Comment() { Id = Comment.Id, PhotoId = Comment.PhotoId, Subject = Comment.Subject, Body = Comment.Body, Author = Comment.Author };
}
```

### Test

1. Run the application and navigate to `Photos/details`. 
    - If you are not logged on and you try to add a comment, you should see an error message at the bottom of the page
    - If you are logged on and you try to add a comment, you should be able to add it
    - If you are logged on and you try to update a comment that you are not the owner of, you should see an error message at the bottom of the page
    - If you are logged on and you try to delete a comment that you are not the owner of, you should see an error message at the bottom of the page
    - If you are logged on and you try to update a comment that you are the owner of, you should be able to update it
    - If you are logged on and you try to delete a comment that you are the owner of, you should be able to delete it
