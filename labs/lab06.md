# Lab 6: Styling the UI with CSS and Component Libraries

Blazor has many ways to style your application. You can use your own CSS, existing CSS frameworks, or component libraries.  
In this lab, we will use [Bootstrap](https://getbootstrap.com/) and our own CSS to style our application, but feel free to take a different path if you prefer. 

**Be aware:** some Component Libraries may not work as expected while using Static SSR and may force you to switch to some interactivity, which may result in an increased complexity when we start working with Entity Framework.

Open the `App.razor` component.

You'll see a link to a `bootstrap.min.css` file:

```html
<link href="css/bootstrap/bootstrap.min.css" rel="stylesheet" />
```

This means that we can already use it to give a better look to our UI.  

**Warning**: Chances are that the version of Bootstrap included in the template is not the latest one, so you might want to update it.

If you want to use Bootstrap, you can find the documentation [here](https://getbootstrap.com/docs/5.3/getting-started/introduction/).

Let's start from the beginning.

Our Root Component, registered in `Program.cs`, is the `App` component. This component is the entry point of our application and is responsible for rendering the content of the application.  

The `App` component contains references to 
- bootstrap.min.css
- app.css
- PhotoSharingApplication.styles.css

The `bootstrap.min.css` file is the CSS file for the Bootstrap framework. You can replace it with an updated version if you want.

The `app.css` file is a custom CSS file that you can use to style your application. You can add your custom styles here. This file is located in the `wwwroot/css` folder.  
The default template already fills it with some styles, but you can remove them if you want. I'm going to remove everything except the following:

```css
body {
    min-height: 75rem;
    padding-top: 4.5rem;
}

.blazor-error-boundary {
    background: url(data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNTYiIGhlaWdodD0iNDkiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyIgeG1sbnM6eGxpbms9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsiIG92ZXJmbG93PSJoaWRkZW4iPjxkZWZzPjxjbGlwUGF0aCBpZD0iY2xpcDAiPjxyZWN0IHg9IjIzNSIgeT0iNTEiIHdpZHRoPSI1NiIgaGVpZ2h0PSI0OSIvPjwvY2xpcFBhdGg+PC9kZWZzPjxnIGNsaXAtcGF0aD0idXJsKCNjbGlwMCkiIHRyYW5zZm9ybT0idHJhbnNsYXRlKC0yMzUgLTUxKSI+PHBhdGggZD0iTTI2My41MDYgNTFDMjY0LjcxNyA1MSAyNjUuODEzIDUxLjQ4MzcgMjY2LjYwNiA1Mi4yNjU4TDI2Ny4wNTIgNTIuNzk4NyAyNjcuNTM5IDUzLjYyODMgMjkwLjE4NSA5Mi4xODMxIDI5MC41NDUgOTIuNzk1IDI5MC42NTYgOTIuOTk2QzI5MC44NzcgOTMuNTEzIDI5MSA5NC4wODE1IDI5MSA5NC42NzgyIDI5MSA5Ny4wNjUxIDI4OS4wMzggOTkgMjg2LjYxNyA5OUwyNDAuMzgzIDk5QzIzNy45NjMgOTkgMjM2IDk3LjA2NTEgMjM2IDk0LjY3ODIgMjM2IDk0LjM3OTkgMjM2LjAzMSA5NC4wODg2IDIzNi4wODkgOTMuODA3MkwyMzYuMzM4IDkzLjAxNjIgMjM2Ljg1OCA5Mi4xMzE0IDI1OS40NzMgNTMuNjI5NCAyNTkuOTYxIDUyLjc5ODUgMjYwLjQwNyA1Mi4yNjU4QzI2MS4yIDUxLjQ4MzcgMjYyLjI5NiA1MSAyNjMuNTA2IDUxWk0yNjMuNTg2IDY2LjAxODNDMjYwLjczNyA2Ni4wMTgzIDI1OS4zMTMgNjcuMTI0NSAyNTkuMzEzIDY5LjMzNyAyNTkuMzEzIDY5LjYxMDIgMjU5LjMzMiA2OS44NjA4IDI1OS4zNzEgNzAuMDg4N0wyNjEuNzk1IDg0LjAxNjEgMjY1LjM4IDg0LjAxNjEgMjY3LjgyMSA2OS43NDc1QzI2Ny44NiA2OS43MzA5IDI2Ny44NzkgNjkuNTg3NyAyNjcuODc5IDY5LjMxNzkgMjY3Ljg3OSA2Ny4xMTgyIDI2Ni40NDggNjYuMDE4MyAyNjMuNTg2IDY2LjAxODNaTTI2My41NzYgODYuMDU0N0MyNjEuMDQ5IDg2LjA1NDcgMjU5Ljc4NiA4Ny4zMDA1IDI1OS43ODYgODkuNzkyMSAyNTkuNzg2IDkyLjI4MzcgMjYxLjA0OSA5My41Mjk1IDI2My41NzYgOTMuNTI5NSAyNjYuMTE2IDkzLjUyOTUgMjY3LjM4NyA5Mi4yODM3IDI2Ny4zODcgODkuNzkyMSAyNjcuMzg3IDg3LjMwMDUgMjY2LjExNiA4Ni4wNTQ3IDI2My41NzYgODYuMDU0N1oiIGZpbGw9IiNGRkU1MDAiIGZpbGwtcnVsZT0iZXZlbm9kZCIvPjwvZz48L3N2Zz4=) no-repeat 1rem/1.8rem, #b32121;
    padding: 1rem 1rem 1rem 3.7rem;
    color: white;
}

    .blazor-error-boundary::after {
        content: "An error has occurred."
    }

```

The `PhotoSharingApplication.styles.css` file is the result of the CSS bundling process. It contains all the *.razor.css content bundled together. 

The `App` component calls the `Routes` component, which is responsible for rendering the content of the application based on the URL.  
The `Routes` component renders the content of the application based on the URL, but it encloses the rendered component in a `DefaultLayout`, set to `MainLayout.razor`.

The `MainLayout.razor` component is the default layout for the application. It is located in the `Components/Shared` folder.

### MainLayout.razor

I am going to modify it so that it renders a navigation bar at the top, then the content of the application, enclosed in a bootstrap [container](https://getbootstrap.com/docs/5.3/layout/containers/).

```cs
@inherits LayoutComponentBase

<NavMenu />

<main class="container">
    <div class="p-5 rounded">
        @Body
    </div>
</main>

<div id="blazor-error-ui">
    An unhandled error has occurred.
    <a href="" class="reload">Reload</a>
    <a class="dismiss">🗙</a>
</div>
```

### MainLayout.razor.css

The MainLayout component of the default template comes with its own CSS file, `MainLayout.razor.css`.  

I'm going to remove everything except the following:

```css
#blazor-error-ui {
    background: lightyellow;
    bottom: 0;
    box-shadow: 0 -1px 2px rgba(0, 0, 0, 0.2);
    display: none;
    left: 0;
    padding: 0.6rem 1.25rem 0.7rem 1.25rem;
    position: fixed;
    width: 100%;
    z-index: 1000;
}

    #blazor-error-ui .dismiss {
        cursor: pointer;
        position: absolute;
        right: 0.75rem;
        top: 0.5rem;
    }
```

### NavMenu.razor

The `NavMenu` component will be responsible for rendering the [navigation bar](https://getbootstrap.com/docs/5.3/components/navbar/) at the top of the application.

```html
<nav class="navbar navbar-expand-md navbar-dark fixed-top bg-dark">
    <div class="container-fluid">
        <a class="navbar-brand" href="#">Photo Sharing Application</a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarCollapse" aria-controls="navbarCollapse" aria-expanded="false" aria-label="Toggle navigation">
            <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navbarCollapse">
            <ul class="navbar-nav me-auto mb-2 mb-md-0">
                <li class="nav-item">
                    <NavLink class="nav-link" href="" Match="NavLinkMatch.All"> Home </NavLink>
                </li>
                <li class="nav-item">
                    <NavLink class="nav-link" href="photos/all"> Photos </NavLink>
                </li>
            </ul>
        </div>
    </div>
</nav>
```

### Navmenu.razor.css

I'm going to remove everything from its css


## All.razor

Let's transform our `Pages/Photos/All.razor` to use a [Card](https://getbootstrap.com/docs/5.3/components/card/) for each Photo.  
I want a maximum of three cards on each row, so I'm going to use the [Bootstrap Grid System](https://getbootstrap.com/docs/5.3/layout/grid/).

```cs
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
            <div class="card">
                <img src="@photo.DataUrl" class="card-img-top" alt="@photo.Title">
                <div class="card-body">
                    <h5 class="card-title">@photo.Title</h5>    
                    <p class="card-text">@photo.Description</p>
                    <p class="card-text"><small>@photo.Id</small></p>
                </div>
                <div class="card-footer">
                    <NavLink href="@($"photos/details/{photo.Id}")" class="card-link">Details</NavLink>
                    <NavLink href="@($"photos/edit/{photo.Id}")" class="card-link">Update</NavLink>
                    <NavLink href="@($"photos/delete/{photo.Id}")" class="card-link">Delete</NavLink>
                </div>
            </div>
        </div>
    }
    </div>
}
```

### Details.razor

The Details page is very similar to the All page, but it will show the details of a single photo.

```cs
@page "/photos/details/{id:int}"
@inject IPhotosRepository PhotosRepository

@if (photo is null)
{
    <p>Photo not found</p>
} 
else
{
    <h3>Details of Photo @photo.Id</h3>

    <div class="col">
        <div class="card">
            <img src="@photo.DataUrl" class="card-img-top" alt="@photo.Title">
            <div class="card-body">
                <h5 class="card-title">@photo.Title</h5>
                <p class="card-text">@photo.Description</p>
                <p class="card-text"><small>@photo.Id</small></p>
            </div>
            <div class="card-footer">
                <NavLink href="@($"photos/edit/{photo.Id}")" class="card-link">Update</NavLink>
                <NavLink href="@($"photos/delete/{photo.Id}")" class="card-link">Delete</NavLink>
            </div>
        </div>
    </div>
}
```

### Delete.razor

Same goes for the Delete page.

```cs
@page "/photos/delete/{id:int}"
@inject IPhotosRepository PhotosRepository
@inject NavigationManager NavigationManager

<h3>Delete</h3>

@if (photo is null)
{
    <p>Photo not found</p>
} 
else
{
    <h3 class="text-danger">Are you sure you want to delete this?</h3>
    <div class="col">
        <div class="card">
            <img src="@photo.DataUrl" class="card-img-top" alt="@photo.Title">
            <div class="card-body">
                <h5 class="card-title">@photo.Title</h5>
                <p class="card-text">@photo.Description</p>
                <p class="card-text"><small>@photo.Id</small></p>
            </div>
            <EditForm FormName="PhotoDeleteForm" Model="photo" OnSubmit="DeletePhoto">
                <button type="submit" class="btn btn-danger">Delete</button>
            </EditForm>
        </div>
    </div>
}
```

### Editor.razor

I am going to style the Editor page as well, using the [Bootstrap Form](https://getbootstrap.com/docs/5.3/forms/overview/).

```cs
@page "/photos/create"
@page "/photos/edit/{id:int}"
@inject IPhotosRepository PhotosRepository
@inject NavigationManager NavigationManager

<h3>Editor</h3>

@if(photo is not null){

    <EditForm Model="@photo" OnSubmit="Save" FormName="PhotoForm" enctype="multipart/form-data">
        <InputNumber @bind-Value="photo.Id" hidden />
        <div class="mb-3">
            <label for="photoTitle" class="form-label">Title:</label>
            <InputText @bind-Value="photo.Title" class="form-control" id="photoTitle"/>
        </div>
        <div class="mb-3">
            <label for="photoDescription" class="form-label">Description (optional):</label>
            <InputTextArea @bind-Value="photo.Description" id="photoDescription" class="form-control" />
        </div>
        <div class="mb-3">
            <label for="fileModelFile" class="form-label">File:</label>
            <InputFile name="fileModel.File" class="form-control" id="fileModelFile" />
        </div>
        <button type="submit" class="btn btn-primary">Save</button>
    </EditForm>
    <img src="@photo.DataUrl" alt="@photo.Title" class="img-fluid rounded mx-auto d-block" />
}
```

Our styling is complete.  

If you are bothered by the fact that we have repeated the same code for the cards in the All, Details, and Delete pages, you can create a new component that will render the card for you. 
* You can create a new component in the `Components` folder, called `PhotoCard.razor`.
* You can move the card code from the All, Details, and Delete pages to the `PhotoCard.razor` component.
* You can then use the `PhotoCard` component in the All, Details, and Delete pages.
* You can use a parameter to pass the photo to the `PhotoCard` component.
* You can use parameters to indicate what buttons should be displayed in the footer.


