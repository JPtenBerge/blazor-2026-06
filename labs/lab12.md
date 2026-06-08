# Lab 12: Completing the Client-Side `CommentsRepository` Using `HttpClient`

In this lab, you will complete the implementation of the `CommentsRepository` in the `Client` project of your photo-sharing application. The `CommentsRepository` will use an `HttpClient` to interact with the RESTful Web Service you created in the previous lab. This will allow your Blazor WebAssembly application to perform CRUD (Create, Read, Update, Delete) operations on comments by communicating with the server-side API.

### Step-by-Step Implementation

#### Step 1: Inject `HttpClient` into the `CommentsRepository`

Before implementing the methods, you need to inject the `HttpClient` dependency into the `CommentsRepository` constructor. This will enable your repository to make HTTP requests to the server.

1. **Modify the constructor of the `CommentsRepository` class** to accept an `HttpClient` parameter:

```csharp
using PhotoSharingApplication.Client.Core.Interfaces;
using PhotoSharingApplication.Client.Core.Models;
using System.Net.Http.Json;

namespace PhotoSharingApplication.Client.Infrastructure;

public class CommentsRepository(HttpClient _httpClient) : ICommentsRepository
{

    // The methods will be implemented in the next steps.
}
```

- **Explanation**:
  - The `HttpClient` is injected via the constructor and stored in a private read-only field `_httpClient`.
  - This allows you to use `_httpClient` within your repository methods to send HTTP requests to the server.

#### Step 2: Implement the `AddCommentAsync` Method

Now that `HttpClient` is available in your repository, you can start implementing the methods, beginning with the method that will allow you to add a new comment to the server.

1. **Add the following code to the `CommentsRepository`**:

```csharp
public async Task<Comment> AddCommentAsync(Comment comment)
{
    HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/comments", comment);
    return await response.Content.ReadFromJsonAsync<Comment>();
}
```

- **Explanation**:
  - The `AddCommentAsync` method sends a POST request to the server with the new comment serialized as JSON.
  - The response is then deserialized into a `Comment` object and returned.

#### Step 3: Implement the `DeleteCommentAsync` Method

Next, implement the method to delete a comment by its ID.

1. **Add the following code to the `CommentsRepository`**:

```csharp
public async Task DeleteCommentAsync(int id)
{
    await _httpClient.DeleteAsync($"/api/comments/{id}");
}
```

- **Explanation**:
  - The `DeleteCommentAsync` method sends a DELETE request to the server to remove the comment with the specified ID.
  - Since no response body is expected, this method doesn't return any value.

#### Step 4: Implement the `GetCommentByIdAsync` Method

Now, implement the method to retrieve a specific comment by its ID.

1. **Add the following code to the `CommentsRepository`**:

```csharp
public async Task<Comment?> GetCommentByIdAsync(int id)
{
    return await _httpClient.GetFromJsonAsync<Comment>($"/api/comments/{id}");
}
```

- **Explanation**:
  - The `GetCommentByIdAsync` method sends a GET request to retrieve the comment with the specified ID.
  - The server's JSON response is deserialized into a `Comment` object and returned.

#### Step 5: Implement the `GetCommentsForPhotoAsync` Method

Next, implement the method to retrieve all comments associated with a specific photo.

1. **Add the following code to the `CommentsRepository`**:

```csharp
public async Task<List<Comment>> GetCommentsForPhotoAsync(int photoId)
{
    return await _httpClient.GetFromJsonAsync<List<Comment>>($"/api/photos/{photoId}/comments");
}
```

- **Explanation**:
  - The `GetCommentsForPhotoAsync` method sends a GET request to retrieve all comments for the specified photo.
  - The response is deserialized into a `List<Comment>` and returned.

#### Step 6: Implement the `UpdateCommentAsync` Method

Finally, implement the method to update an existing comment.

1. **Add the following code to the `CommentsRepository`**:

```csharp
public async Task<Comment> UpdateCommentAsync(Comment comment)
{
    HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"/api/comments/{comment.Id}", comment);
    return comment;
}
```

- **Explanation**:
  - The `UpdateCommentAsync` method sends a PUT request with the updated comment data.
  - The method returns the comment object, indicating that the update was successful.

#### Step 7: Register `HttpClient` in the `Program.cs` File

To enable the `CommentsRepository` to use `HttpClient` for sending requests, you need to register `HttpClient` as a service in the `Program.cs` file of the `Client` project.

1. **Open the `Program.cs` file** in the `Client` project.

2. **Add the following line** to register `HttpClient`:

```csharp
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
```

- **Explanation**:
  - This line registers `HttpClient` as a scoped service, setting its `BaseAddress` to the base URI of the application. This allows the `CommentsRepository` to use `HttpClient` to communicate with the server.

### Summary

In this lab, you:
- Injected `HttpClient` into the `CommentsRepository` to enable HTTP communication.
- Implemented each method in the `CommentsRepository` to perform CRUD operations on comments using `HttpClient`.
- Registered `HttpClient` in the DI container to make it available for your repository.

With these changes, your Blazor WebAssembly application can now interact with the server-side RESTful WebAPI to manage comments, providing a complete client-server integration.