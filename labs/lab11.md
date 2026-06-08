# Lab 11: Implementing a RESTful Web Service for Comments

In this lab, you will extend your photo-sharing application by implementing a RESTful Web Service to handle comments. Since your `CommentsComponent` is now rendered and executed client-side using InteractiveWebAssembly, you need a way for the client-side application to communicate with the server to manage comments. This will be accomplished by creating an ASP.NET WebAPI Controller.

### Introduction to the Problem

With the `CommentsComponent` now being rendered client-side, the component needs to send and receive data from the server via HTTP requests. To facilitate this, you'll implement a RESTful Web Service using ASP.NET WebAPI. This will allow the client-side Blazor WebAssembly application to perform CRUD (Create, Read, Update, Delete) operations on comments.

### Step-by-Step Implementation

#### Step 1: Add a `CommentsController` WebAPI Controller

First, you need to create a new WebAPI controller to manage comments.

1. In the `PhotoSharingApplication` project, add a new class named `CommentsController` to the `Controllers` folder.

2. Add the following code to define an empty `CommentsController`:

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhotoSharingApplication.Client.Core.Interfaces;
using PhotoSharingApplication.Client.Core.Models;

namespace PhotoSharingApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        // The controller code will be added in the next steps.
    }
}
```

- **Explanation**:
  - The `[ApiController]` attribute marks this class as an API controller.
  - The `[Route("api/[controller]")]` attribute defines the base route for the controller, which will be `api/comments`.

#### Step 2: Inject the Repository Dependency

Next, you need to inject the `ICommentsRepository` dependency into the controller’s constructor.

1. Modify the `CommentsController` constructor to inject the repository:

```csharp
public class CommentsController(ICommentsRepository _commentsRepository) : ControllerBase
{
    
}
```

- **Explanation**:
  - The `ICommentsRepository` interface is injected via the constructor and stored in a private read-only field `commentsRepository`.

#### Step 3: Implement the `GetComment` Action

Now, implement the first action to retrieve a comment by its ID.

1. Add the following method to the `CommentsController`:

```csharp
[HttpGet("{id:int}")]
public async Task<ActionResult<Comment>> GetComment(int id)
{
    var comment = await _commentsRepository.GetCommentByIdAsync(id);
    return comment is null ? NotFound($"Could not find comment with id {id}") : comment;
}
```

- **Explanation**:
  - The `[HttpGet("{id:int}")]` attribute specifies that this action responds to HTTP GET requests with an `id` parameter in the route. The `:int` is a route constraint that indicates the value must be an integer.
  - The `id` parameter is used to retrieve a comment by its ID from the repository. If the comment is found, it is returned; otherwise, a `404 Not Found` response is returned.

#### Step 4: Implement the `GetCommentsByPhotoId` Action

Next, implement an action to retrieve all comments for a specific photo.

1. Add the following method to the `CommentsController`:

```csharp
[HttpGet("/api/photos/{photoId}/comments")]
public async Task<IEnumerable<Comment>> GetCommentsByPhotoId(int photoId)
{
    return await _commentsRepository.GetCommentsForPhotoAsync(photoId);
}
```

- **Explanation**:
  - This action uses the `[HttpGet("/api/photos/{photoId}/comments")]` attribute to define a custom route that includes the `photoId`.
  - It retrieves all comments associated with the specified `photoId` from the repository.

#### Step 5: Implement the `PutComment` Action

Now, implement an action to update a comment.

1. Add the following method to the `CommentsController`:

```csharp
[HttpPut("{id:int}")]
public async Task<IActionResult> PutComment(int id, Comment comment)
{
    if (id != comment.Id)
    {
        return BadRequest("IDs in URL and body should match");
    }

    await _commentsRepository.UpdateCommentAsync(comment);

    return NoContent();
}
```

- **Explanation**:
  - The `[HttpPut("{id:int}")]` attribute specifies that this action responds to HTTP PUT requests with an `id` parameter in the route.
  - The `id` parameter is matched with the `comment.Id` to ensure consistency. If they don't match, a `400 Bad Request` response is returned.
  - If the update is successful, the action returns a `204 No Content` response.

#### Step 6: Implement the `PostComment` Action

Next, implement an action to create a new comment.

1. Add the following method to the `CommentsController`:

```csharp
[HttpPost]
public async Task<ActionResult<Comment>> PostComment(Comment comment)
{
    await _commentsRepository.AddCommentAsync(comment);

    return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
}
```

- **Explanation**:
  - The `[HttpPost]` attribute specifies that this action responds to HTTP POST requests.
  - The new comment is added using the repository. The action returns a `201 Created` response, along with the URI of the newly created comment.

#### Step 7: Implement the `DeleteComment` Action

Finally, implement an action to delete a comment by its ID.

1. Add the following method to the `CommentsController`:

```csharp
[HttpDelete("{id}")]
public async Task<ActionResult<Comment>> DeleteComment(int id)
{
    var comment = await _commentsRepository.GetCommentByIdAsync(id);
    if (comment is null)
    {
        return NotFound($"Could not find comment with id {id}");
    }

    await _commentsRepository.DeleteCommentAsync(id);

    return comment;
}
```

- **Explanation**:
  - The `[HttpDelete("{id}")]` attribute specifies that this action responds to HTTP DELETE requests with an `id` parameter in the route.
  - If the comment is found, it is deleted, and the deleted comment is returned in the response. If not found, a `404 Not Found` response is returned.

#### Step 8: Register the WebAPI Controllers in `Program.cs`

To make your WebAPI controllers available, you need to update the `Program.cs` file.

1. **Register the controllers** by adding the following lines to `Program.cs`:

```csharp
builder.Services.AddControllers();
app.MapControllers();
```

- **Explanation**:
  - `builder.Services.AddControllers()` registers the controllers in the DI container.
  - `app.MapControllers()` maps the controllers' routes, enabling them to handle HTTP requests.

#### Step 9: Test the Controller

You can now test the WebAPI controller using the Endpoints Explorer in Visual Studio.

1. **Open the Endpoints Explorer** in Visual Studio.
2. **Right-click on the `GET /api/comments/{id}`** and select **Generate Request**.
3. **Click on Debug** to run the request and inspect the response.

#### Step 10: Test All Actions Using an HTTP File

You can test all the actions by creating an `.http` file in Visual Studio and modifying the content to match the schema of the `Comment` object. Below is an example of how the `.http` file might look:

```http
@PhotoSharingApplication_HostAddress = https://localhost:7246

GET {{PhotoSharingApplication_HostAddress}}/api/photos/1/comments

###

GET {{PhotoSharingApplication_HostAddress}}/api/comments/8

###

POST {{PhotoSharingApplication_HostAddress}}/api/comments
Content-Type: application/json

{
"id": 0,
"photoId": 1,
"subject": "New Comment",
"body": "Added through .http file"
}

###

PUT {{PhotoSharingApplication_HostAddress}}/api/comments/8
Content-Type: application/json

{
    "id": 8,
    "photoId": 1,
    "subject": "New Comment",
    "body": "Modified through .http file"
}

###

DELETE {{PhotoSharingApplication_HostAddress}}/api/comments/8

###
```

- **Explanation**:
  - This `.http` file includes requests to test all CRUD operations on the comments resource.
  - Replace the `HostAddress` and `id` values as needed to match your specific setup.

### Conclusion

In this lab, you:
- Implemented a RESTful WebAPI Controller for managing comments.
- Created actions to handle CRUD operations.
- Registered the WebAPI controllers and tested them using Visual Studio's Endpoints Explorer and an `.http` file.

With the WebAPI controller in place, your Blazor WebAssembly client can now interact with the server to manage comments efficiently.