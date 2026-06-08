# Lab 9: Implementing and Fixing the Comments Feature in a Blazor Application

In this lab, you continue with the comments feature. You will create a repository to manage comments, use it within the `CommentsComponent`, and then discover and fix an issue related to the DbContext's scope when using the `InteractiveServer` render mode.

#### Step 1: Modify the `PhotosDbContext`

First, you need to modify the `PhotosDbContext` to include a `DbSet` for comments.

1. **Add a new `DbSet` for Comments**:
    ```csharp
    public DbSet<Comment> Comments { get; set; }
    ```

2. **Modify the `OnModelCreating` method** to configure the `Comment` entity:
    ```csharp
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Photo>().Property(p => p.Title).HasMaxLength(100);
        modelBuilder.Entity<Photo>().Property(p => p.Description).HasMaxLength(300);
        modelBuilder.Entity<Photo>().Property(p => p.ImageMimeType).HasMaxLength(100);

        // Comment entity configuration
        modelBuilder.Entity<Comment>().Property(c => c.Subject).HasMaxLength(100);
        modelBuilder.Entity<Comment>().Property(c => c.Body).HasMaxLength(300);
    }
    ```

3. **Create a new migration and update the database** using Visual Studio Tools:
    - Use the Package Manager Console:
        ```shell
        Add-Migration AddComments
        Update-Database
        ```

#### Step 2: Create the Comments Repository

Now, create the repository class that will manage the comments.

1. **Create the `CommentsRepository` class**:

    ```csharp
    using Microsoft.EntityFrameworkCore;
    using PhotoSharingApplication.Client.Core.Interfaces;
    using PhotoSharingApplication.Client.Core.Models;
    using PhotoSharingApplication.Infrastructure.Data;

    namespace PhotoSharingApplication.Infrastructure.Repositories;

    public class CommentsRepository(PhotosDbContext context) : ICommentsRepository
    {
        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            context.Comments.Add(comment);
            await context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment?> GetCommentByIdAsync(int id)
        {
            return await context.Comments.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Comment>> GetCommentsForPhotoAsync(int photoId)
        {
            return await context.Comments.AsNoTracking().Where(c => c.PhotoId == photoId).ToListAsync();
        }

        public async Task DeleteCommentAsync(int id)
        {
            Comment? comment = await context.Comments.FirstOrDefaultAsync(c => c.Id == id);
            if (comment is not null)
            {
                context.Comments.Remove(comment);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Comment> UpdateCommentAsync(Comment comment)
        {
            context.Comments.Update(comment);
            await context.SaveChangesAsync();
            return comment;
        }
    }
    ```

2. **Create the `ICommentsRepository` interface**:

    ```csharp
    using PhotoSharingApplication.Core.Models;

    namespace PhotoSharingApplication.Core.Interfaces;

    public interface ICommentsRepository
    {
        Task<Comment> AddCommentAsync(Comment comment);
        Task DeleteCommentAsync(int id);
        Task<Comment?> GetCommentByIdAsync(int id);
        Task<List<Comment>> GetCommentsForPhotoAsync(int photoId);
        Task<Comment> UpdateCommentAsync(Comment comment);
    }
    ```

3. **Register the `CommentsRepository` service** in the DI container in `Program.cs`:

    ```csharp
    builder.Services.AddScoped<ICommentsRepository, CommentsRepository>();
    ```

#### Step 3: Modify the `CommentsComponent`

Next, modify the `CommentsComponent` to interact with the repository.

1. **Inject the `ICommentsRepository`** into the `CommentsComponent`:

    ```csharp
    @inject ICommentsRepository CommentsRepository
    ```

2. **Add a `PhotoId` parameter** to be passed by the parent:

    ```csharp
    [Parameter]
    public int PhotoId { get; set; }
    ```

3. **Add a property to store a list of comments**:

    ```csharp
    private List<Comment> comments = default!;
    ```

4. **Use the repository in the `OnInitializedAsync` method** to load comments for the photo:

    ```csharp
    protected override async Task OnInitializedAsync()
    {
        newComment = new Comment() { PhotoId = PhotoId };
        comments = await CommentsRepository.GetCommentsForPhotoAsync(PhotoId);
    }
    ```

5. **Implement methods to add, update, and delete comments**:

    ```csharp
    async Task AddComment(Comment comment)
    {
        await CommentsRepository.AddCommentAsync(comment);
        comments = await CommentsRepository.GetCommentsForPhotoAsync(PhotoId);
        newComment = new Comment() { PhotoId = PhotoId };
    }

    async Task UpdateComment(Comment comment)
    {
        await CommentsRepository.UpdateCommentAsync(comment);
        comments = await CommentsRepository.GetCommentsForPhotoAsync(PhotoId);
    }

    async Task DeleteComment(Comment comment)
    {
        await CommentsRepository.DeleteCommentAsync(comment.Id);
        comments = await CommentsRepository.GetCommentsForPhotoAsync(PhotoId);
    }
    ```

6. **Update the UI to render the comments**:

    ```csharp
    <h3>Comments</h3>

    <CommentComponent Comment="newComment" CommentState="CommentStates.Insert" OnAddComment="AddComment" />

    @if (comments is not null)
    {
        @foreach (var comment in comments)
        {
            <CommentComponent Comment="comment" CommentState="CommentStates.View" OnUpdateComment="UpdateComment" OnDeleteComment="DeleteComment" />
        }
    }
    ```

#### Step 4: Update the `Photos/Details.razor` Component

Remove the `CommentsComponent` from the Home page and add it to the `Photos/Details.razor` page.

1. **Update the `Photos/Details.razor`**:

    ```csharp
    <div class="col">
        <PhotoCard Photo="photo" ShowDelete ShowUpdate />
        <CommentsComponent PhotoId="@photo.Id" @rendermode="InteractiveServer" />
    </div>
    ```

#### Step 5: Run the Application and Observe the Error

Run the application and navigate to the details page of a photo. Try adding a new comment, which should work. Then, attempt to edit the comment you just added. You should encounter an error indicating that the comment is already tracked by the `DbContext`.

**What’s Going On?**

- The problem arises because you are using `InteractiveServer` mode, which opens a SignalR circuit. When the `DbContext` is registered with `AddDbContext`, it is added as a *Scoped* service. With SignalR, "Scoped" means that the `DbContext` stays alive for the duration of the connection. Therefore, when you try to update the comment, Entity Framework throws an error because the comment is already tracked by the same `DbContext`.

#### Step 6: Fix the Issue by Using a `DbContextFactory`

To fix the issue, you need to register a `DbContextFactory` instead of a `DbContext`. This will ensure that a fresh instance of the `DbContext` is created each time a repository method is called, avoiding conflicts.

1. **Register the `DbContextFactory` in `Program.cs`**:

    ```csharp
    builder.Services.AddDbContextFactory<PhotosDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("PhotosDbContext")));
    ```

2. **Modify the `CommentsRepository` to use the `DbContextFactory`**:

    ```csharp
    using Microsoft.EntityFrameworkCore;
    using PhotoSharingApplication.Client.Core.Interfaces;
    using PhotoSharingApplication.Client.Core.Models;
    using PhotoSharingApplication.Infrastructure.Data;

    namespace PhotoSharingApplication.Infrastructure.Repositories;

    public class CommentsRepository : ICommentsRepository
    {
        private readonly IDbContextFactory<PhotosDbContext> _contextFactory;

        public CommentsRepository(IDbContextFactory<PhotosDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Comments.Add(comment);
            await context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment?> GetCommentByIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Comments.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Comment>> GetCommentsForPhotoAsync(int photoId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Comments.AsNoTracking().Where(c => c.PhotoId == photoId).ToListAsync();
        }

        public async Task DeleteCommentAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            Comment? comment = await context.Comments.FirstOrDefaultAsync(c => c.Id == id);
            if (comment is not null)
            {
                context.Comments.Remove(comment);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Comment> UpdateCommentAsync(Comment comment)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Comments.Update(comment);
            await context.SaveChangesAsync();
            return comment;
        }
    }
    ```

With these changes, each method in the repository creates a new instance of `PhotosDbContext`, ensuring that no entity tracking conflicts occur when using `InteractiveServer` mode.

### Lab Completion

By completing this lab, you have:
- Implemented a repository pattern to manage comments in the database.
- Discovered and resolved an issue related to the `DbContext` scope when using `InteractiveServer` mode in Blazor.

This lab has taught you how to effectively manage state and data access in a real-world Blazor application, ensuring robustness and reliability in your data layer.