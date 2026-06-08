# Lab 13: Unit Testing Razor Components with bUnit, xUnit, Moq, and AwesomeAssertions

In this lab, you will write unit tests for your Razor components using the `bUnit` testing library alongside `xUnit`, `Moq`, and `AwesomeAssertions`. You'll primarily focus on testing the Razor components, but if time permits, you'll also explore unit testing other classes, such as the WebAPI controller and the client-side `CommentsRepository` that uses `HttpClient`.

### Setting Up the Test Projects

Since your components are spread across two different projects, you will create two separate test projects:

1. **PhotoSharingApplication.UnitTests** - This project will test the components in the `PhotoSharingApplication` project (server-side components).
2. **PhotoSharingApplication.Client.UnitTests** - This project will test the components in the `PhotoSharingApplication.Client` project (client-side components).

#### Step 1: Create the Test Projects

1. **Create two new xUnit Test Projects**:
   - `PhotoSharingApplication.UnitTests`
   - `PhotoSharingApplication.Client.UnitTests`

2. **Modify each project file** to use the Razor SDK by editing the `.csproj` file:

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
```

3. **Add the necessary NuGet packages** to both test projects:

   - `bUnit`
   - `AwesomeAssertions`
   - `Moq`

   Additionally, for the client-side project (`PhotoSharingApplication.Client.UnitTests`), add:

   - `RichardSzalay.MockHttp` (for testing `HttpClient` interactions)

#### Step 2: Add a `_Imports.razor` File

Each test project needs an `_Imports.razor` file to simplify the usage of common namespaces.

1. **Create a new `_Imports.razor` file** in each test project with the following content:

**For `PhotoSharingApplication.UnitTests`:**

```csharp
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.JSInterop
@using Microsoft.Extensions.DependencyInjection
@using AngleSharp.Dom
@using Bunit
@using Bunit.TestDoubles
@using Xunit
@using Moq
@using AwesomeAssertions
@using PhotoSharingApplication.Core.Models
@using PhotoSharingApplication.Core.Interfaces
```

**For `PhotoSharingApplication.Client.UnitTests`:**

```csharp
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.JSInterop
@using Microsoft.Extensions.DependencyInjection
@using AngleSharp.Dom
@using Bunit
@using Bunit.TestDoubles
@using Xunit
@using Moq
@using AwesomeAssertions
@using PhotoSharingApplication.Client.Core.Models
@using PhotoSharingApplication.Client.Core.Interfaces
@using PhotoSharingApplication.Client.Components
```

2. **Add project references** to the corresponding projects:
   - `PhotoSharingApplication.UnitTests` should reference `PhotoSharingApplication`.
   - `PhotoSharingApplication.Client.UnitTests` should reference `PhotoSharingApplication.Client`.

### Unit Testing Server-Side Components

Let's start with testing the `PhotoCard.razor` component in the `PhotoSharingApplication.UnitTests` project.

#### Step 3: Testing the `PhotoCard.razor` Component

Assuming you have a `PhotoCard.razor` component with the following code:

```csharp
@if(Photo is not null){
    <div class="card">
        <img src="@Photo.DataUrl" class="card-img-top" alt="@Photo.Title">
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
                @if (ShowUpdate)
                {
                    <NavLink href="@($"photos/edit/{Photo.Id}")" class="card-link">Update</NavLink>
                }
                @if (ShowDelete)
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

    [Parameter]
    public bool ShowDelete { get; set; } = false;

    private bool ShowFooter => ShowDetails || ShowUpdate || ShowDelete;
}
```

You can test various scenarios:

1. **RendersEmpty_WhenPhotoIsNull**:
   - This test ensures that no markup is rendered when the `Photo` parameter is null.

2. **DoesNotRenderFooter_WhenShowDetails_ShowDelete_And_ShowEdit_AreFalse**:
   - This test ensures that the footer is not rendered when all footer-related flags are false.

3. **RendersFooterWithDetailsLink_WhenShowDetailsIsTrue**:
   - This test ensures that the Details link is rendered when `ShowDetails` is true.

4. **RendersFooterWithDeleteLink_WhenShowDeleteIsTrue**:
   - This test ensures that the Delete link is rendered when `ShowDelete` is true.

5. **RendersFooterWithEditLink_WhenShowUpdateIsTrue**:
   - This test ensures that the Update link is rendered when `ShowUpdate` is true.

Here’s how you might write these tests in a `PhotoCardTests.razor` component:

```csharp
@inherits BunitContext
@using PhotoSharingApplication.Components

<h3>PhotoCardTests</h3>

@code {
    [Fact]
    public void RendersEmpty_WhenPhotoIsNull()
    {
        // Arrange
        Photo photo = null;

        // Act
        var cut = Render(@<PhotoCard Photo="photo" />);

        // Assert
        cut.MarkupMatches("");
    }

    [Fact]
    public void DoesNotRenderFooter_WhenShowDetails_ShowDelete_And_ShowEdit_AreFalse()
    {
        // Arrange
        var photo = new Photo()
        {
            Id = 1,
            Title = "Test Title",
            Description = "Test Description",
            PhotoFile = new byte[] {1,2,3,4,5,6,7,8},
            ImageMimeType = "image/jpeg"
        };

        // Act
        var cut = Render(@<PhotoCard Photo="photo"/>);

        // Assert
        cut.MarkupMatches(@<div class="card">
            <img src="data:image/jpeg;base64,AQIDBAUGBwg=" class="card-img-top" alt="Test Title">
            <div class="card-body">
                <h5 class="card-title">Test Title</h5>
                <p class="card-text">Test Description</p>
                <p class="card-text">1</p>
            </div>
        </div>);
    }

    [Fact]
    public void RendersFooterWithDetailsLink_WhenShowDetailsIsTrue()
    {
        // Arrange
        var photo = new Photo()
        {
            Id = 1,
            Title = "Test Title",
            Description = "Test Description",
            PhotoFile = new byte[] {1, 2, 3, 4, 5, 6, 7, 8},
            ImageMimeType = "image/jpeg"
        };

        // Act
        var cut = Render(@<PhotoCard Photo="photo" ShowDetails />);

        // Assert
        var footer = cut.Find(".card-footer");
        footer.MarkupMatches(@<div class="card-footer">
            <a href="photos/details/1" class="card-link">Details</a>
        </div>);
    }

    [Fact]
    public void RendersFooterWithDeleteLink_WhenShowDeleteIsTrue()
    {
        // Arrange
        var photo = new Photo()
        {
            Id = 1,
            Title = "Test Title",
            Description = "Test Description",
            PhotoFile = new byte[] {1, 2, 3, 4, 5, 6, 7, 8},
            ImageMimeType = "image/jpeg"
        };

        // Act
        var cut = Render(@<PhotoCard Photo="photo" ShowDelete />);

        // Assert
        var footer = cut.Find(".card-footer");
        footer.MarkupMatches(@<div class="card-footer">
            <a href="photos/delete/1" class="card-link">Delete</a>
        </div>);
    }

    [Fact]
    public void RendersFooterWithEditLink_WhenShowUpdateIsTrue()
    {
        // Arrange
        var photo = new Photo()
        {
            Id = 1,
            Title = "Test Title",
            Description = "Test Description",
            PhotoFile = new byte[] {1, 2, 3, 4, 5, 6, 7, 8},
            ImageMimeType = "image/jpeg"
        };

        // Act
        var cut = Render(@<PhotoCard Photo="photo" ShowUpdate />);

        // Assert
        var footer = cut.Find(".card-footer");
        footer.MarkupMatches(@<div class="card-footer">
            <a href="photos/edit/1" class="card-link">Update</a>
        </div>);
    }
}
```

#### Photos/All.razor

If your `Photos/All.razor` component looks like this:

```csharp
@page "/photos/all"
@inject IPhotosRepository PhotosRepository
<h3>All Photos</h3>

<NavLink href="photos/create">Upload new Photo</NavLink>

@if (photos is null)
{
    <p>...Loading...</p>
} else
{
    <div class="row row-cols-1 row-cols-md-3 g-4">
    @foreach (var photo in photos)
    {
        <div class="col">
            <PhotoCard Photo="photo" ShowDelete ShowDetails ShowUpdate />
        </div>
    }
    </div>
}

@code {
    IEnumerable<Photo> photos = default!;

    protected override async Task OnInitializedAsync()
    {
        photos = await PhotosRepository.GetPhotosAsync();
    }
}
```

You can write tests for the `Photos/All.razor` component as follows:

```csharp
@inherits BunitContext
@using PhotoSharingApplication.Components
@using PhotoSharingApplication.Components.Pages.Photos
@using PhotoSharingApplication.Core.Interfaces
@using PhotoSharingApplication.Client.Core.Models;

<h3>AllTests</h3>

@code {
    [Fact]
    public void RendersEmpty_WhenPhotosIsNull()
    {
        // Arrange
        Moq.Mock<IPhotosRepository> mock = new Moq.Mock<IPhotosRepository>();
        mock.Setup(mbox => mbox.GetPhotosAsync()).ReturnsAsync((List<Photo>)null);
        Services.AddSingleton<IPhotosRepository>(mock.Object);
        
        // Act
        var cut = Render(@<All/>);

        // Assert
        cut.MarkupMatches(
            @<text>
        <h3>All Photos</h3>
        <a href="photos/create">Upload new Photo</a>
        <p>...Loading...</p>
            </text>
    );
    }

    [Fact]
    public void RendersOnePhotoCard_ForEachPhoto()
    {
        //Arrange
        Moq.Mock<IPhotosRepository> mock = new Moq.Mock<IPhotosRepository>();
        var returnedList = new List<Photo>() { new Photo() { Id = 1, Title = "p1" }, new Photo() { Id = 2, Title = "p2" }, new Photo() { Id = 3, Title = "p3" } };
        mock.Setup(mbox=>mbox.GetPhotosAsync()).ReturnsAsync(returnedList);
        Services.AddSingleton<IPhotosRepository>(mock.Object);
        
        //Act
        var cut = Render(@<All/>);

        //Assert
        cut.FindComponents<PhotoCard>().Should().HaveCount(3);
    }
}
```

### Photos/Delete.razor

You could test wether Clicking On Submit Navigates To Photos/All, using the BunitNavigationManager:

```csharp
[Fact]
public void ClickingOnSubmit_NavigatesToPhotosAll()
{
    // Arrange
    Photo photo = new Photo() { Id = 1, Title = "p1" };
    Moq.Mock<IPhotosRepository> mock = new Moq.Mock<IPhotosRepository>();
    mock.Setup(mbox => mbox.GetPhotoByIdAsync(1)).ReturnsAsync(photo);
    mock.Setup(mbox => mbox.DeletePhotoAsync(1)).ReturnsAsync(photo);

    Services.AddSingleton<IPhotosRepository>(mock.Object);

    // Act
    var cut = Render(@<Delete Id="1" />);

    // Assert
    cut.Find("form").Submit();

    var navMan = Services.GetRequiredService<BunitNavigationManager>();
    navMan.Uri.Should().Be("http://localhost/photos/all");
}
```

### Photos/Edit.razor

You could test wether the Form Is Correctly Bound To Photo Fields

```csharp
[Fact]
public void FormIsCorrectlyBoundToPhotoFields()
{
    // Arrange
    Photo photo = new Photo() { Id = 1, Title = "p1" };
    Moq.Mock<IPhotosRepository> mock = new Moq.Mock<IPhotosRepository>();
    mock.Setup(mbox => mbox.GetPhotoByIdAsync(1)).ReturnsAsync(photo);

    Services.AddSingleton<IPhotosRepository>(mock.Object);

    var cut = Render(@<Editor Id="1" />);

    // Act
    cut.Find("#photoTitle").Change("p2");
    cut.Find("#photoDescription").Change("d2");

    cut.Find("form").Submit();

    // Assert        
    photo.Title.Should().Be("p2");
    photo.Description.Should().Be("d2");
}
```

### Unit Testing Client-Side Components

Next, let's focus on the `CommentsComponent` in the `PhotoSharingApplication.Client.UnitTests` project.

#### Step 4: Testing the `CommentsComponent.razor`

Testing the `CommentsComponent` could involve the following scenarios:

1. **Persisting and Retrieving State** using `FakePersistentComponentState`.
2. **Rendering of Child Components** such as `CommentComponentSplit`.
3. **Handling of Events** like adding, updating, and deleting comments.

Example of a test for the `CommentsComponent`:

```csharp
@inherits BunitContext
<h3>CommentsComponentTests</h3>

@code {
    [Fact]
    public void PersistsCommentsInPersistentComponentState()
    {
        // Arrange
        List<Comment> comments = new() { new Comment() };

        Mock<ICommentsRepository> mockCommentsRepository = new Mock<ICommentsRepository>();
        Services.AddSingleton(mockCommentsRepository.Object);

        ComponentFactories.AddStub<CommentComponentSplit>();
        var fakeState = this.AddBunitPersistentComponentState();
        
        fakeState.Persist(nameof(comments), comments);

        // Act
        var cut = Render(@<CommentsComponent />);
        
        fakeState.TriggerOnPersisting();

        bool foundState = fakeState.TryTake<List<Comment>>(nameof(comments), out var data);

        // Assert
        foundState.Should().BeTrue();
        data.Should().BeEquivalentTo(comments);
    }

    [Fact]
    public void RendersOneInsert_WhenCommentsIsNull()
    {
        // Arrange
        List<Comment> comments = null;

        Mock<ICommentsRepository> mockCommentsRepository = new Mock<ICommentsRepository>();
        Services.AddSingleton(mockCommentsRepository.Object);

        ComponentFactories.AddStub<CommentComponentSplit>();
        var fakeState = this.AddBunitPersistentComponentState();

        fakeState.Persist(nameof(comments), comments);

        // Act
        var cut = Render(@<CommentsComponent />);

        fakeState.TriggerOnPersisting();

        var stub = cut.FindComponent<Stub<CommentComponentSplit>>();

        // Assert
        stub.Instance.Parameters.Get(x => x.CommentState).Should().Be(CommentStates.Insert);
    }

    [Fact]
    public void InvokesRepositoryWhenStateNotPersisted()
    {
        // Arrange
        List<Comment> comments = new() { new Comment() };

        Mock<ICommentsRepository> mockCommentsRepository = new Mock<ICommentsRepository>();
        mockCommentsRepository.Setup(x => x.GetCommentsForPhotoAsync(It.IsAny<int>())).ReturnsAsync(comments).Verifiable();
        Services.AddSingleton(mockCommentsRepository.Object);

        ComponentFactories.AddStub<CommentComponentSplit>();
        var fakeState = this.AddBunitPersistentComponentState();

        // Act
        var cut = Render(@<CommentsComponent />);

        // Assert
        mockCommentsRepository.Verify(x => x.GetCommentsForPhotoAsync(It.IsAny<int>()), Times.Once);
    }
}
```

### CommentsRepository

If you have time, you can also test the `CommentsRepository` class that uses `HttpClient` to interact with the server. This is not strictly Blazor testing, but it's a good practice to test the repository classes.

Here's an example of how you might test the `CommentsRepository`:

```csharp
using AwesomeAssertions;
using PhotoSharingApplication.Client.Core.Models;
using PhotoSharingApplication.Client.Infrastructure;
using RichardSzalay.MockHttp;
using System.Text.Json;

namespace PhotoSharingApplication.Client.UnitTests.Infrastructure;

public class CommentsRepositoryTests
{
    private readonly HttpClient httpClient;
    private readonly MockHttpMessageHandler mockHttpHandler;
    public CommentsRepositoryTests()
    {
        mockHttpHandler = new MockHttpMessageHandler();
        httpClient = mockHttpHandler.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost");
    }

    [Fact]
    public async Task AddCommentAsync_ReturnsComment()
    {
        // Arrange
        var comment = new Comment { Id = 1, PhotoId = 1, Subject = "Test", Body = "Test" };
        mockHttpHandler.When(HttpMethod.Post, "http://localhost/api/comments").Respond("application/json", JsonSerializer.Serialize(comment));
        var sut = new CommentsRepository(httpClient);

        // Act
        var result = await sut.AddCommentAsync(comment);

        // Assert
        result.Should().BeEquivalentTo(comment);
    }

    [Fact]
    public async Task DeleteCommentAsync_ReturnsNoContent()
    {
        // Arrange
        mockHttpHandler.When(HttpMethod.Delete, "http://localhost/api/comments/1").Respond("application/json", "");
        var sut = new CommentsRepository(httpClient);

        // Act
        await sut.DeleteCommentAsync(1);

        // Assert
        mockHttpHandler.VerifyNoOutstandingRequest();
    }

    [Fact]
    public async Task GetCommentByIdAsync_ReturnsComment()
    {
        // Arrange
        var comment = new Comment { Id = 1, PhotoId = 1, Subject = "Test", Body = "Test" };
        mockHttpHandler.When(HttpMethod.Get, "http://localhost/api/comments/1").Respond("application/json", JsonSerializer.Serialize(comment));
        var sut = new CommentsRepository(httpClient);

        // Act
        var result = await sut.GetCommentByIdAsync(1);

        // Assert
        result.Should().BeEquivalentTo(comment);
    }

    [Fact]
    public async Task GetCommentsForPhotoAsync_ReturnsComments()
    {
        // Arrange
        var comments = new List<Comment>
        {
            new Comment { Id = 1, PhotoId = 1, Subject = "Test", Body = "Test" },
            new Comment { Id = 2, PhotoId = 1, Subject = "Test", Body = "Test" }
        };
        mockHttpHandler.When(HttpMethod.Get, "http://localhost/api/photos/1/comments").Respond("application/json", JsonSerializer.Serialize(comments));
        var sut = new CommentsRepository(httpClient);

        // Act
        var result = await sut.GetCommentsForPhotoAsync(1);

        // Assert
        result.Should().BeEquivalentTo(comments);
    }

    [Fact]
    public async Task UpdateCommentAsync_ReturnsComment()
    {
        // Arrange
        var comment = new Comment { Id = 1, PhotoId = 1, Subject = "Test", Body = "Test" };
        mockHttpHandler.When(HttpMethod.Put, "http://localhost/api/comments/1").Respond("application/json", JsonSerializer.Serialize(comment));
        var sut = new CommentsRepository(httpClient);

        // Act
        var result = await sut.UpdateCommentAsync(comment);

        // Assert
        result.Should().BeEquivalentTo(comment);
    }
}
```

### Summary

In this lab, you:
- Set up two test projects: one for server-side components and another for client-side components.
- Wrote unit tests for server-side components using bUnit and other testing libraries.
- Tested client-side components, focusing on state persistence, event handling, and interaction with mock services.
- Explored advanced testing scenarios, including mocking, dependency injection, and event handling in Razor components.

By the end of this lab, you should be comfortable with writing comprehensive unit tests for your Blazor components, ensuring they function as expected in various scenarios.
In the lab solution you can find more examples of tests for the two projects.