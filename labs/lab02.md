# Lab 2: Your First Routable Components

The goal of this lab is to create a new page that displays a list of photos and one to display the details of one photo.  
We will use a simple `List` to store the data.  

## The All Photos Page

We're going to need:
- a new Razor component
- a new class to represent a Photo


## Razor Component

- In the `Solution Explorer`, right click the `Pages` folder and select `Add` -> `New Folder`
  - Name the new Folder `Photos`
- In the `Solution Explorer`, right click the `Pages/Photos` folder and select `Add` -> `Razor Component`
  - Name the file `All.razor`

If you start the application now and navigate to `/photos/all` you'll see an HTTP ERROR 404 (not found).
This is because we did not register the component with the route.

Open the `Pages/Photos/All.razor` file in Visual Studio and insert this code on the first line:

```c#
@page "/photos/all"
```

Start your application and navigate to `http://localhost:{your port}/photos/all`. You should now see the content of your page (which is not much, but we'll fix this soon).

## What is a *Razor component*?

Let's read some [documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/?view=aspnetcore-8.0):

> A component is a self-contained portion of user interface (UI) with processing logic to enable dynamic behavior. 

Read the [Component Classes](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/?view=aspnetcore-8.0#component-classes) chapter on the docs to understand how a component works.

As a recap: each component contains
- UI
- Logic

The UI is mostly HTML, but there can also be tags that *look like* HTML but they're actually nested components.
What you can also find are bits and pieces of C#, maybe because you want to loop through your data in order to build some table rows, or you want to conditionally render a button only under a certain condition and so on.

The logic can be contained in a `code` section or in a separate .cs file (more on this later). In the logic you can have data and behavior, in the form of properties and methods.

## The Photos/All Component

Our goal is to display a list of photos.  
In future labs we will take care of 
- the UI by using Bootstrap
- the data by creating a Db and using Entity Framework Core

For the time being we will display a simple list retrieved from memory.

Let's add some data and use it to dynamically render the UI.

According to the [docs](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/?view=aspnetcore-8.0#markup): 

> Component members are used in rendering logic using C# expressions that start with the @ symbol. For example, a C# field is rendered by prefixing @ to the field name. 

So we need:
- a component member
- a C# expression that start with @

We write a component member as a variable or property in the `code` section. We write the C# expression starting with the @ in the HTML.

Let's change the Photos/All.razor component like this:

```cs
@page "/photos/all"

<h3>All Photos</h3>

<p>@photoTitle</p>

@code {
    string photoTitle = "My title";
}
```

If you run the application and navigate to `photos/all` you should see the paragraph with *My Title* in it.

Now of course we want to show something more than just a  title, so let's create a new data type for a `Photo` and use that instead of a simple string:

```cs
@page "/photos/all"

<h3>All Photos</h3>

<p>@photo.Id</p>
<p>@photo.Title</p>
<p>@photo.Description</p>

@code {
    Photo photo;

    class Photo {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
```

Right now, running the application would result in an exception because our photo is `null`. 
Let's correct the UI to handle this problem, by testing if the photo is null and conditionally render a loading message if it is. For this we will use an [if Razor control structure](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-8.0#control-structures):

```cs
@page "/photos/all"

<h3>AllPhotos</h3>

@if (photo is null)
{
    <p>...Loading...</p>
}
else
{
    <p>@photo.Id</p>
    <p>@photo.Title</p>
    <p>@photo.Description</p>
}
```

We need to create a new `Photo` instance and we can do it in one of the [lifecycle](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/lifecycle?view=aspnetcore-8.0) methods of our component:  

```cs
protected override void OnInitialized()
{
    photo = new Photo { Id = 1, Title = "My  Title", Description = "Lorem ipsum dolor sit amen" };
}
```

Running the application now should show the data we have.

We are expecting more than one picture, so let's change the code to have a `List` and let's loop through the list to build multiple UI elements by using a [foreach Razor control structure](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-8.0#looping-for-foreach-while-and-do-while).

The final code will look like this:

```cs
@page "/photos/all"

<h3>All Photos</h3>

@if (photos is null)
{
    <p>...Loading...</p>
}
else
{
    @foreach (var photo in photos)
    {
<article>
    <p>@photo.Id</p>
    <p>@photo.Title</p>
    <p>@photo.Description</p>
</article>
    }
}

@code {
    List<Photo> photos;

    protected override void OnInitialized()
    {
        photos = new List<Photo>{
            new Photo { Id = 1, Title = "My Title", Description = "Lorem ipsum dolor sit amen" },
            new Photo { Id = 2, Title = "Another Title", Description = "All work and no play makes Jack a dull boy" },
            new Photo { Id = 3, Title = "Yet another Title", Description = "Some description" }
        };
    }

    class Photo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
```

Save your file. 
Go to the browser and verify that the page now contains three articles with the details of our photos.
Do not worry about the style. We will fix it in a later lab.

---

## The Details Page

The details page should show all the information about one particular photo, which means we need to know which photo, first. What we can do is to append the unique id of the photo to the url as a parameter. That way the details page can retrieve it from there. 

We will make use of [Route Parameters](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-8.0#route-parameters)

Add the new `Details.razor` page to the `Pages/Photos` folder of the `PhotoSharingApplication` project, then insert the following line:

```
@page "/photos/details/{id:int}"
```

This means that an address such as `/photos/details/123` will be mapped by the routing engine to an `id` parameter equal to `123`

In the `@code` section, add the following parameter:

```cs
@code{
  [Parameter]
  public int Id { get; set; }
}
```

It's important that the name matches the one we used in the route, although  not case sensitive.

The rest is very similar to the `Photos/All.razor`: we have a `List` of `Photo` but this time we filter the Photo whose Id is equal to the one retrieved in the route. 

```cs
@page "/photos/details/{id:int}"

@if (photo is null)
{
    <p>Photo not found</p>
} 
else
{
    <h3>Details of Photo @photo.Id</h3>

    <article>
        <p>@photo.Id</p>
        <p>@photo.Title</p>
        <p>@photo.Description</p>
    </article>   
}

@code {
    List<Photo> photos;

    Photo? photo;

    [Parameter]
    public int Id { get; set; }   

    protected override void OnInitialized()
    {
        photos = new List<Photo>{
            new Photo { Id = 1, Title = "My Title", Description = "Lorem ipsum dolor sit amen" },
            new Photo { Id = 2, Title = "Another Title", Description = "All work and no play makes Jack a dull boy" },
            new Photo { Id = 3, Title = "Yet another Title", Description = "Some description" }
        };
        photo = photos.FirstOrDefault(p => p.Id == Id);
    }

    class Photo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
```

Save and check that the details view updates correctly when you enter an address such as `photos/details/1` and `photos/details/2`.

