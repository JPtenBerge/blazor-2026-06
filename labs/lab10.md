# Lab 10: Rendering the `CommentsComponent` on the Client Side with WebAssembly

In this lab, you'll continue building on your previous work by rendering the `CommentsComponent` on the client side using WebAssembly. You'll encounter some challenges related to dependency injection and state management, which you'll begin to address in this lab. Some issues will be fully resolved in a later lab.

#### Step 1: Change the Render Mode to `InteractiveWebAssembly`

Start by modifying the `Photos/Details.razor` page to render the `CommentsComponent` using the `InteractiveWebAssembly` render mode.

```csharp
<div class="col">
    <PhotoCard Photo="photo" ShowDelete ShowUpdate />
    <CommentsComponent PhotoId="@photo.Id" @rendermode="InteractiveWebAssembly" />
</div>
```

#### Step 2: Move Components to the `Client` Project

To render the `CommentsComponent` on the client side, all related files must be moved to the `Client` project because WebAssembly components are compiled and downloaded to the browser.

1. **Move the following components to the `Client` project**:
   - `CommentsComponent.razor`
   - `CommentComponent.razor`

2. **If you have split your `CommentComponent` into subcomponents, also move the following files**:
   - `CommentComponentBase.cs`
   - `CommentDeleteComponent.razor`
   - `CommentEditComponent.razor`
   - `CommentInsertComponent.razor`
   - `CommentViewComponent.razor`

3. **Move the supporting code that these components reference**:
   - The `CommentStates` enumeration
   - The `Core/Interfaces/ICommentsRepository` interface
   - The `Core/Models/Comment` class

**IMPORTANT**: Do **not** move the `CommentsRepository` implementation, as it is a server-side service that should remain in the `Server` project.

#### Step 3: Run the Application and Observe the Error

After moving the necessary files, run the application and navigate to the Details page. You'll see the `CommentsComponent` rendered initially, but after a moment, an error will appear on the page.

If you open the browser console, you'll see the following error:

```plaintext
Error: One or more errors occurred. (Cannot provide a value for property 'CommentsRepository' on type 'PhotoSharingApplication.Client.Components.CommentsComponent'. There is no registered service of type 'PhotoSharingApplication.Client.Core.Interfaces.ICommentsRepository'.)
```

### Step 4: Understand the Issue

The error indicates that the `CommentsRepository` service is not registered in the DI container for the client-side (WebAssembly) application. This is because the `CommentsComponent` is being rendered using WebAssembly, where the server-side DI container isn't available.

However, you still see the comments rendered initially due to **Prerendering**. The `CommentsComponent` is prerendered on the server, where the `CommentsRepository` service is available, allowing it to render initially.

### Step 5: Create a Client-Side `CommentsRepository`

To fix this issue, you need to register a client-side implementation of the `ICommentsRepository` service. However, since the client doesn't have direct access to the `DbContext`, you will leave the implementation empty for now and complete it later.

1. **Create a new folder** named `Infrastructure/Repositories` in the `Client` project.
2. **Add a new class** named `CommentsRepository` to this folder. The class should implement the `ICommentsRepository` interface.

```csharp
using PhotoSharingApplication.Client.Core.Interfaces;
using PhotoSharingApplication.Client.Core.Models;

namespace PhotoSharingApplication.Client.Infrastructure;

public class CommentsRepository : ICommentsRepository
{
    public Task<Comment> AddCommentAsync(Comment comment)
    {
        throw new NotImplementedException();
    }

    public Task DeleteCommentAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Comment?> GetCommentByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<List<Comment>> GetCommentsForPhotoAsync(int photoId)
    {
        throw new NotImplementedException();
    }

    public Task<Comment> UpdateCommentAsync(Comment comment)
    {
        throw new NotImplementedException();
    }
}
```

3. **Register the `ICommentsRepository` service** in the `Client` project. Open the `Program.cs` file in the `Client` project and add the following line after `var builder = WebAssemblyHostBuilder.CreateDefault(args);`:

```csharp
builder.Services.AddScoped<ICommentsRepository, CommentsRepository>();
```

### Step 6: Run the Application Again

Run the application and go to the Details page. You'll see the `CommentsComponent` rendered, but this time, you'll encounter a different error:

```plaintext
Unhandled exception rendering component: The method or operation is not implemented.
```

This error occurs because the client-side application attempts to rerender the component after the WebAssembly is downloaded, but the repository methods are not implemented yet.

### Step 7: Persist Prerendered State

To avoid the immediate error after WebAssembly initialization, you can persist the state server-side and rehydrate it client-side using the `PersistentComponentState` service.

1. **Inject the `PersistentComponentState` service** in the `CommentsComponent`:

```csharp
@inject PersistentComponentState ApplicationState
```

2. **Declare a variable** of type `PersistingComponentStateSubscription` to hold the subscription to the `Persisting` event:

```csharp
private PersistingComponentStateSubscription persistingSubscription;
```

3. **Override the `OnInitializedAsync` method** to register the `PersistData` method with the `Persisting` event and retrieve the state from the server:

```csharp
protected override async Task OnInitializedAsync()
{
    newComment = new Comment() { PhotoId = PhotoId };
    
    persistingSubscription = ApplicationState.RegisterOnPersisting(PersistData);

    if (!ApplicationState.TryTakeFromJson<List<Comment>>(nameof(comments), out var restored))
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
```

4. **Override the `Dispose` method** to dispose of the subscription:

```csharp
void IDisposable.Dispose()
{
    persistingSubscription.Dispose();
}
```

5. **Implement the `IDisposable` interface** in the component:

```csharp
@implements IDisposable
```

### Step 8: Run the Application

Now, run the application again and go to the Details page. The `CommentsComponent` should be rendered without any errors. The Add, Edit, and Delete functionalities are not yet implemented in the client-side repository, so those will not work, but the component will render and maintain its state between server-side prerendering and client-side rehydration.

### Summary

In this lab, you:
- Rendered the `CommentsComponent` using `InteractiveWebAssembly` and moved related files to the `Client` project.
- Encountered and understood the issue related to the DI container when moving to WebAssembly.
- Created a placeholder client-side `CommentsRepository` to satisfy dependency injection requirements.
- Implemented state persistence using `PersistentComponentState` to prevent errors after WebAssembly initialization.

In the next steps, you will first implement a RESTful service to expose the database access to the client, and then complete the client-side repository methods to fully enable the Add, Edit, and Delete functionalities for comments in WebAssembly mode by using an `HttpClient`.
