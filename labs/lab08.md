# Lab 8: Blazor Components and Interactivity

In this lab, we will focus on the Comments functionality of the PhotoSharing application.  
This time, instead of using different routes, links and Forms like we did for the Photos, we will use a different approach.
We will create a single `CommentsComponent`, to be displayed in the `Photos/Details.razor` page.
The `CommentsComponent` will contain a list of `CommentComponent`.
A `CommentComponent` will render a different component depending on its `CommentState`:
- `View` will render a `CommentViewComponent`
  - A `CommentViewComponent` will render the properties of a Comment, with buttons to switch to `Edit` or `Delete` mode.
- `Insert` will render a `CommentInsertComponent`
  - A `CommentInsertComponent` will render fields to edit the properties of a Comment, with a button to `Add` the Comment.
- `Edit` will render a `CommentEditComponent` 
  - A `CommentEditComponent` will render fields to edit the properties of a Comment, with `Save` and `Cancel` buttons to switch to back to `View` mode.
- `Delete` will render a `CommentDeleteComponent` 
  - A `CommentDeleteComponent` will render a confirmation message and buttons to `Confirm` or `Cancel` the deletion, which will switch back to `View` mode.

The `CommentsComponent` will also have a `CommentComponent` set to `Insert` mode, to add a new Comment.

In order for the components to communicate with each other, we will use properties and events and we will need some *interactivity*.  
In this lab we will start with `InteractiveServer`. In a following lab we will move to `InteractiveWebAssembly` and `InteractiveAuto`.

Since we are going to use the `CommentComponent` in the `Photos/Details.razor` page, we will start by creating the `CommentComponent` and the `CommentsComponent`.

We will proceed step by step, so first we'll use One-Way binding, then move to Two-Way binding.

### Create the `CommentsComponent`

Create a new Razor Component called `CommentsComponent` in the `Components` folder.

### Create the `CommentComponent`

Create a new Razor Component called `CommentComponent` in the `Components` folder.

### Mode switching

The `CommentComponent` will have a `CommentState` property that will be used to switch between the different modes.
Let's create an enumeration for the `CommentStates` property in the `CommentComponent`:

```csharp
public enum CommentStates
{
    View,
    Insert,
    Edit,
    Delete
}
```

The `CommentComponent` will have a `CommentState` property of type `CommentStates`, which should be set by the parent component.

```csharp
[Parameter]
public CommentStates CommentState { get; set; }
```

For now, let's just render a different message depending on the `CommentState`:

```csharp
@switch(CommentState)
{
    case CommentStates.View:
        <p>View Mode</p>
        break;
    case CommentStates.Insert:
        <p>Insert Mode</p>
        break;
    case CommentStates.Edit:
        <p>Edit Mode</p>
        break;
    case CommentStates.Delete:
        <p>Delete Mode</p>
        break;
}
```

### Use it in the `CommentsComponent`

In the `CommentsComponent`, we will create four `CommentComponent` and set the `CommentState` property of each one to a different state, to see if they work as expected.

```csharp
<CommentComponent CommentState="CommentComponent.CommentStates.View" />

<CommentComponent CommentState="CommentComponent.CommentStates.Insert" />

<CommentComponent CommentState="CommentComponent.CommentStates.Edit" />

<CommentComponent CommentState="CommentComponent.CommentStates.Delete" />
```

### CommentState default value

If we want to set a default value for the `CommentState` property, we can do it in the `CommentComponent`:

```csharp
[Parameter]
public CommentStates CommentState { get; set; } = CommentStates.View;
```

This way, if the parent component does not set the `CommentState` property, the `CommentComponent` will default to `View` mode.

```csharp
<CommentComponent />

<CommentComponent CommentState="CommentComponent.CommentStates.Insert" />

<CommentComponent CommentState="CommentComponent.CommentStates.Edit" />

<CommentComponent CommentState="CommentComponent.CommentStates.Delete" />
```

### Use the `CommentComponent` in the `Home.razor` page

Just as a temporary test, we can use the `CommentsComponent` in the `Home.razor` page to see if it works as expected.

```csharp
<CommentsComponent />
```

If you cannot find the `CommentsComponent`, you may need to add a `using` directive to the top of the file or to the `_imports.razor` file.

### Run the application

Run the application and check if the `CommentComponent` is rendering the correct message for each state.

### Switching between states

Now let's say we put buttons directly in the `CommentComponent` to switch between the different states.

```csharp
@switch(CommentState)
{
    case CommentStates.View:
        <p>View Mode</p>
        <button @onclick="OnEditClicked">Edit</button>
        <button @onclick="OnDeleteClicked">Delete</button>
        break;
    case CommentStates.Insert:
        <p>Insert Mode</p>
        <button @onclick="OnAddClicked">Add</button>
        break;
    case CommentStates.Edit:
        <p>Edit Mode</p>
        <button @onclick="OnSaveClicked">Save</button>
        <button @onclick="OnCancelClicked">Cancel</button>
        break;
    case CommentStates.Delete:
        <p>Delete Mode</p>
        <button @onclick="OnDeleteConfirmedClicked">Delete!</button>
        <button @onclick="OnCancelClicked">Cancel</button>
        break;
}
```

What should the buttons event handlers do?
Since the `CommentState` property is a Parameter, we should not modify it directly.
The recommended approach is to make a private copy of the `CommentState` property and modify that copy, without touching the original value of the Parameter.
The `OnParametersSet` is a good place to initialize our own private copy. 


```csharp
private CommentStates commentState;
protected override void OnParametersSet()
{
    commentState = CommentState;
}
```

The event handlers should modify the private copy of the `CommentState` property.

```csharp
async Task OnEditClicked()
{
    commentState = CommentStates.Edit;
}

async Task OnAddClicked()
{
    
}

async Task OnDeleteClicked()
{
    commentState = CommentStates.Delete;
}

async Task OnSaveClicked()
{
    commentState = CommentStates.View;
}

async Task OnCancelClicked()
{
    commentState = CommentStates.View;
}

async Task OnDeleteConfirmedClicked()
{
    commentState = CommentStates.View;
}
```

The `switch` should then use the private copy.

```csharp
@switch(commentState)
{
    case CommentStates.View:
        <p>View Mode</p>
        <button @onclick="OnEditClicked">Edit</button>
        <button @onclick="OnDeleteClicked">Delete</button>
        break;
    case CommentStates.Insert:
        <p>Insert Mode</p>
        <button @onclick="OnAddClicked">Add</button>
        break;
    case CommentStates.Edit:
        <p>Edit Mode</p>
        <button @onclick="OnSaveClicked">Save</button>
        <button @onclick="OnCancelClicked">Cancel</button>
        break;
    case CommentStates.Delete:
        <p>Delete Mode</p>
        <button @onclick="OnDeleteConfirmedClicked">Delete!</button>
        <button @onclick="OnCancelClicked">Cancel</button>
        break;
}
```

Now if you run the application, you will notice that nothing happens. This is because the `CommentsComponent` needs interactivity in order to handle the click events of the buttons, while right now it's using the default Static SSR, which offers no interactivity. So let's make the `CommentsComponent` interactive, specifically starting with `InteractiveServer`.

### Make the `CommentsComponent` interactive

In order to make the `CommentsComponent` interactive, we need to specify its `rendermode` in the `Home` page.

```csharp
<CommentsComponent @rendermode="InteractiveServer" />
```

### Run the application

Run the application and check if the `CommentComponent` is rendering the correct message for each state and if the buttons are working as expected.

### Events

Let's think about the Comment itself.
A Comment has the following properties:
- `Id`
- `PhotoId`
- `Subject`
- `Body`

Let's add the `Comment` class to the `Models` folder.

```csharp
public class Comment
{
    public int Id { get; set; }
    public int PhotoId { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}
```

Now we can add a `Comment` property to the `CommentComponent` that can be set by its parent component:

```csharp
[Parameter]
public Comment Comment { get; set; }
```

Once again, we should make a private copy of the `Comment` property in the `CommentComponent` and set it in the `OnParametersSet` method:

```csharp
private Comment comment;
protected override void OnParametersSet()
{
    commentState = CommentState;
    comment = new Comment() { Id = Comment.Id, PhotoId = Comment.PhotoId, Subject = Comment.Subject, Body = Comment.Body };
}
```

Now let's add a way for the `CommentComponent` to notify the parent component when it's time to Add, Update or Delete a Comment.

We will create three events in the `CommentComponent`:
- `OnAddComment`
- `OnUpdateComment`
- `OnDeleteComment`

```csharp
[Parameter]
public EventCallback<Comment> OnAddComment { get; set; }

[Parameter]
public EventCallback<Comment> OnUpdateComment { get; set; }

[Parameter]
public EventCallback<Comment> OnDeleteComment { get; set; }
```

The `OnAddComment` event will be triggered when the `Add` button is clicked.

```csharp
async Task OnAddClicked()
{
    await OnAddComment.InvokeAsync(comment);
}
```

The `OnUpdateComment` event will be triggered when the `Save` button is clicked.

```csharp
async Task OnSaveClicked()
{
    commentState = CommentStates.View;
    await OnUpdateComment.InvokeAsync(comment);
}
```

The `OnDeleteComment` event will be triggered when the `Delete!` button is clicked.

```csharp
async Task OnDeleteConfirmedClicked()
{
    commentState = CommentStates.View;
    await OnDeleteComment.InvokeAsync(comment);
}
```

### Use the `CommentComponent` in the `CommentsComponent`

Later, in the `CommentsComponent`, we will create a list of `Comment` and set the `Comment` property of each `CommentComponent` to a different `Comment`.
For now, let's just start with two comments, one for `View` mode and one for `Insert` mode.

```csharp
<CommentComponent Comment="comment1" CommentState="CommentComponent.CommentStates.Insert" OnAddComment="AddComment" />

<CommentComponent Comment="comment2" OnUpdateComment="UpdateComment" OnDeleteComment="DeleteComment" />

@code {
    Comment comment1 = new Comment() { Id = 1, PhotoId = 1, Subject = "Subject 1", Body = "Body 1" };
    Comment comment2 = new Comment() { Id = 2, PhotoId = 1, Subject = "Subject 2", Body = "Body 2" };
    

    async Task AddComment(Comment comment)
    {
        // Add comment
        Console.WriteLine($"Add comment {comment.Id}, {comment.Subject}, {comment.Body}");
    }

    async Task UpdateComment(Comment comment)
    {
        // Update comment
        Console.WriteLine($"Update comment {comment.Id}, {comment.Subject}, {comment.Body}");
    }

    async Task DeleteComment(Comment comment)
    {
        // Delete comment
        Console.WriteLine($"Delete comment {comment.Id}, {comment.Subject}, {comment.Body}");
    }
}
```

If you test the application you should see the messages in the console when you click the buttons.

## Input Fields

Now let's add input fields to the `CommentComponent` to allow the user to view and edit the properties of a `Comment`.

```csharp
@switch(commentState)
{
    case CommentStates.View:
        <p>View Mode</p>
        <p>@comment.Subject</p>
        <p>@comment.Body</p>
        <button @onclick="OnEditClicked">Edit</button>
        <button @onclick="OnDeleteClicked">Delete</button>
        break;
    case CommentStates.Insert:
        <p>Insert Mode</p>
        <input @bind="comment.Subject" />
        <input @bind="comment.Body" />
        <button @onclick="OnAddClicked">Add</button>
        break;
    case CommentStates.Edit:
        <p>Edit Mode</p>
        <input @bind="comment.Subject" />
        <input @bind="comment.Body" />
        <button @onclick="OnSaveClicked">Save</button>
        <button @onclick="OnCancelClicked">Cancel</button>
        break;
    case CommentStates.Delete:
        <p>Delete Mode</p>
        <p>@comment.Subject</p>
        <p>@comment.Body</p>
        <button @onclick="OnDeleteConfirmedClicked">Delete!</button>
        <button @onclick="OnCancelClicked">Cancel</button>
        break;
}
```

### Run the application

Run the application and check if the `CommentComponent` is rendering the correct message for each state and if the buttons are working as expected.

### Refactor?

You may argue that the `CommentComponent` is getting too big and that we should split it into smaller components.
On the other hand, splitting it into smaller components would mean having to add yet another level of communication between the components, which would increase the complexity. 
So you should consider the trade-offs and decide what is best for your specific case.
If you decide to split the `CommentComponent` into smaller components, you can do it in the next steps.

We will create 4 new components:
- `CommentViewComponent`
- `CommentEditComponent`
- `CommentDeleteComponent`
- `CommentInsertComponent`

All of them will use Two-Way Binding so that the parent component can set the properties of the `CommentState`.
They will have a `CommentStateChanged` event that will be triggered when the user clicks a button to switch to a different state.

### CommentViewComponent

Let's start by creating a `CommentViewComponent` that will render the properties of a `Comment` and the buttons to switch to `Edit` or `Delete` mode.

Create a new Razor Component called `CommentViewComponent` in the `Components` folder and transfer there the UI and logic of the View state. Also implement the Two-Way Binding property so that the parent component can set the `CommentState` property of the `CommentViewComponent`.

```csharp
@using PhotoSharingApplication.Client.Core.Models
@if (comment is not null)
{
    <p>View Mode</p>
    <p>@comment.Subject</p>
    <p>@comment.Body</p>
    <button @onclick="OnEditClicked">Edit</button>
    <button @onclick="OnDeleteClicked">Delete</button>
}
@code {
    [Parameter]
    public Comment Comment { get; set; }
    private Comment comment;

    [Parameter]
    public CommentStates CommentState { get; set; } = CommentStates.View;
    
    [Parameter]
    public EventCallback<CommentStates> CommentStateChanged { get; set; }

    override protected void OnParametersSet()
    {
        comment = new Comment() { Id = Comment.Id, PhotoId = Comment.PhotoId, Subject = Comment.Subject, Body = Comment.Body };
    }

    async Task OnEditClicked()
    {
        await CommentStateChanged.InvokeAsync(CommentStates.Edit);
    }

    async Task OnDeleteClicked()
    {
        await CommentStateChanged.InvokeAsync(CommentStates.Delete);
    }
}
```

### CommentEditComponent

Create a new Razor Component called `CommentEditComponent` in the `Components` folder and transfer there the UI and logic of the Edit state. Also implement the Two-Way Binding property so that the parent component can set the `CommentState` property of the `CommentEditComponent`.

```csharp
@using PhotoSharingApplication.Client.Core.Models
@if (comment is not null)
{
    <p>Edit Mode</p>
    <input @bind="comment.Subject" />
    <input @bind="comment.Body" />
    <button @onclick="OnSaveClicked">Save</button>
    <button @onclick="OnCancelClicked">Cancel</button>
}
@code {
    [Parameter]
    public CommentStates CommentState { get; set; } = CommentStates.Edit;
    [Parameter]
    public EventCallback<CommentStates> CommentStateChanged { get; set; }

    [Parameter]
    public Comment Comment { get; set; }
    private Comment comment;

    [Parameter]
    public EventCallback<Comment> OnUpdateComment { get; set; }


    protected override void OnParametersSet()
    {
        comment = new Comment() { Id = Comment.Id, PhotoId = Comment.PhotoId, Subject = Comment.Subject, Body = Comment.Body };
    }

    async Task OnSaveClicked()
    {
        await CommentStateChanged.InvokeAsync(CommentStates.View);
        await OnUpdateComment.InvokeAsync(comment);
    }

    async Task OnCancelClicked()
    {
        await CommentStateChanged.InvokeAsync(CommentStates.View);
    }

}
```

### CommentDeleteComponent

Create a new Razor Component called `CommentDeleteComponent` in the `Components` folder and transfer there the UI and logic of the Delete state. Also implement the Two-Way Binding property so that the parent component can set the `CommentState` property of the `CommentDeleteComponent`.

```csharp
@using PhotoSharingApplication.Client.Core.Models
@if (comment is not null)
{
    <p>Delete Mode</p>
    <p>@comment.Subject</p>
    <p>@comment.Body</p>
    <button @onclick="OnDeleteConfirmedClicked">Delete!</button>
    <button @onclick="OnCancelClicked">Cancel</button>
}
@code {
    [Parameter]
    public CommentStates CommentState { get; set; } = CommentStates.Delete;
    [Parameter]
    public EventCallback<CommentStates> CommentStateChanged { get; set; }

    [Parameter]
    public Comment Comment { get; set; }
    private Comment comment;

    [Parameter]
    public EventCallback<Comment> OnDeleteComment { get; set; }

    protected override void OnParametersSet()
    {
        comment = new Comment() { Id = Comment.Id, PhotoId = Comment.PhotoId, Subject = Comment.Subject, Body = Comment.Body };
    }

    async Task OnCancelClicked()
    {
        await CommentStateChanged.InvokeAsync(CommentStates.View);
    }

    async Task OnDeleteConfirmedClicked()
    {
        await CommentStateChanged.InvokeAsync(CommentStates.View);
        await OnDeleteComment.InvokeAsync(comment);
    }
}
```

### CommentInsertComponent

Create a new Razor Component called `CommentInsertComponent` in the `Components` folder and transfer there the UI and logic of the Insert state. Also implement the Two-Way Binding property so that the parent component can set the `CommentState` property of the `CommentInsertComponent`.

```csharp
@using PhotoSharingApplication.Client.Core.Models
@if (comment is not null)
{
    <p>Insert Mode</p>
    <input @bind="comment.Subject" />
    <input @bind="comment.Body" />
    <button @onclick="OnAddClicked">Add</button>
}
@code {
    [Parameter]
    public CommentStates CommentState { get; set; } = CommentStates.Insert;

    [Parameter]
    public EventCallback<CommentStates> CommentStateChanged { get; set; }

    [Parameter]
    public Comment Comment { get; set; }
    private Comment comment;

    [Parameter]
    public EventCallback<Comment> OnAddComment { get; set; }

    protected override void OnParametersSet()
    {
        comment = new Comment() { Id = Comment.Id, PhotoId = Comment.PhotoId, Subject = Comment.Subject, Body = Comment.Body };
    }

    async Task OnAddClicked()
    {
        await CommentStateChanged.InvokeAsync(CommentStates.View);
        await OnAddComment.InvokeAsync(comment);
    }
}
```

### Use the new components in the `CommentComponent`

Now we can use the new components in the `CommentComponent`, simplifying its code.

```csharp
@using PhotoSharingApplication.Client.Core.Models
<h3>CommentComponent</h3>

@if (comment is not null)
{
    @switch (commentState)
    {
        case CommentStates.View:
            <CommentViewComponent Comment="comment" @bind-CommentState="commentState" />
            break;
        case CommentStates.Insert:
            <CommentInsertComponent Comment="comment" @bind-CommentState="commentState" OnAddComment="OnAddComment" />
            break;
        case CommentStates.Edit:
            <CommentEditComponent Comment="comment" @bind-CommentState="commentState" OnUpdateComment="OnUpdateComment" />
            break;
        case CommentStates.Delete:
            <CommentDeleteComponent Comment="comment" @bind-CommentState="commentState" OnDeleteComment="OnDeleteComment" />
            break;
    }
}

@code {
    [Parameter]
    public CommentStates CommentState { get; set; } = CommentStates.View;
    private CommentStates commentState;

    [Parameter]
    public Comment Comment { get; set; }
    private Comment comment;

    [Parameter]
    public EventCallback<Comment> OnAddComment { get; set; }

    [Parameter]
    public EventCallback<Comment> OnDeleteComment { get; set; }

    [Parameter]
    public EventCallback<Comment> OnUpdateComment { get; set; }


    protected override void OnParametersSet()
    {
        commentState = CommentState;
        comment = new Comment() { Id = Comment.Id, PhotoId = Comment.PhotoId, Subject = Comment.Subject, Body = Comment.Body };
    }

}
```

### Further Refactoring and styling

You can further refactor the common code between the components, for example by creating a `CommentBaseComponent` that contains the common code and then inherit from it in the other components.

You can also add some styling to the components to make them look better.

### CommentComponentBase

Create a new class Component called `CommentComponentBase` in the `Components` folder deriving from `ComponentBase` and transfer there the common code of the `CommentViewComponent`, `CommentInsertComponent`, `CommentDeleteComponent` and `CommentUpdateComponent`.

```csharp
using Microsoft.AspNetCore.Components;
using PhotoSharingApplication.Client.Core.Models;

namespace PhotoSharingApplication.Client.Components;

public class CommentComponentBase : ComponentBase
{
    [Parameter]
    public Comment Comment { get; set; }
    protected Comment comment;

    [Parameter]
    public CommentStates CommentState { get; set; } = CommentStates.View;

    [Parameter]
    public EventCallback<CommentStates> CommentStateChanged { get; set; }

    override protected void OnParametersSet()
    {
        comment = new Comment() { Id = Comment.Id, PhotoId = Comment.PhotoId, Subject = Comment.Subject, Body = Comment.Body };
    }

    protected async Task NotifyCommentStateChanged(CommentStates state) => await CommentStateChanged.InvokeAsync(state);
}
```

### CommentViewComponent

```csharp
@using PhotoSharingApplication.Client.Core.Models
@inherits CommentComponentBase
@if (comment is not null)
{
    <div class="card">
        <div class="card-header">
            <h5 class="card-title">Comment</h5>
        </div>
        <div class="card-body">
            <dl>
                <dt>Subject</dt>
                <dd>@comment.Subject</dd>
                <dt>Body</dt>
                <dd>@comment.Body</dd>
            </dl>
        </div>
        <div class="card-footer">
            <button class="btn btn-primary" @onclick="OnEditClicked">Edit</button>
            <button class="btn btn-warning" @onclick="OnDeleteClicked">Delete</button>
        </div>
    </div>
}
@code {
    async Task OnEditClicked()
    {
        await NotifyCommentStateChanged(CommentStates.Edit);
    }

    async Task OnDeleteClicked()
    {
        await NotifyCommentStateChanged(CommentStates.Delete);
    }
}
```

### CommentInsertComponent

```csharp
@using PhotoSharingApplication.Client.Core.Models
@inherits CommentComponentBase
@if (comment is not null)
{
    <div class="card">
        <div class="card-header">
            <h5 class="card-title">Insert Comment</h5>
        </div>
        <div class="card-body">
            <div class="form-group">
                <label for="subject" class="form-control-label">Subject</label>
                <input class="form-control" @bind="comment.Subject" id="subject" name="subject" />
            </div>
            <div class="form-group">
                <label for="body" class="form-control-label">Body</label>
                <textarea class="form-control" @bind="comment.Body" id="body" name="body"></textarea>
            </div>
        </div>
        <div class="card-footer">
            <button class="btn btn-primary" @onclick="OnAddClicked">Add</button>
        </div>
    </div>
}
@code {
    [Parameter]
    public EventCallback<Comment> OnAddComment { get; set; }

    async Task OnAddClicked()
    {
        await NotifyCommentStateChanged(CommentStates.View);
        await OnAddComment.InvokeAsync(comment);
    }
}
```

### CommentEditComponent

```csharp
@using PhotoSharingApplication.Client.Core.Models
@inherits CommentComponentBase
@if (comment is not null)
{
    <div class="card">
        <div class="card-header">
            <h5 class="card-title">Edit Comment</h5>
        </div>
        <div class="card-body">
            <div class="form-group">
                <label for="subject" class="form-control-label">Subject</label>
                <input class="form-control" @bind="comment.Subject" id="subject" name="subject" />
            </div>
            <div class="form-group">
                <label for="body" class="form-control-label">Body</label>
                <textarea class="form-control" @bind="comment.Body" id="body" name="body"></textarea>
            </div>
        </div>
        <div class="card-footer">
            <button class="btn btn-primary" @onclick="OnSaveClicked">Save</button>
            <button class="btn btn-secondary" @onclick="OnCancelClicked">Cancel</button>
        </div>
    </div>
}
@code {
    [Parameter]
    public EventCallback<Comment> OnUpdateComment { get; set; }

    async Task OnSaveClicked()
    {
        await NotifyCommentStateChanged(CommentStates.View);
        await OnUpdateComment.InvokeAsync(comment);
    }

    async Task OnCancelClicked()
    {
        await NotifyCommentStateChanged(CommentStates.View);
    }

}
```

### CommentDeleteComponent

```csharp
@using PhotoSharingApplication.Client.Core.Models
@inherits CommentComponentBase
@if (comment is not null)
{
    <div class="card">
        <div class="card-header">
            <h5 class="card-title text-danger">Delete Comment?</h5>
        </div>
        <div class="card-body">
            <p class="card-text">@comment.Subject</p>
            <p class="card-text">@comment.Body</p>
        </div>
        <div class="card-footer">
            <button @onclick="OnDeleteConfirmedClicked" class="btn btn-danger">Delete!</button>
            <button @onclick="OnCancelClicked" class="btn btn-secondary">Cancel</button>
        </div>
    </div>
}
@code {
    [Parameter]
    public EventCallback<Comment> OnDeleteComment { get; set; }

    async Task OnCancelClicked()
    {
        await NotifyCommentStateChanged(CommentStates.View);
    }

    async Task OnDeleteConfirmedClicked()
    {
        await NotifyCommentStateChanged(CommentStates.View);
        await OnDeleteComment.InvokeAsync(comment);
    }
}
```

In the next lab we're going to take care about the CommentsRepository