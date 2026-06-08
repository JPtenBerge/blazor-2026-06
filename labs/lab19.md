# Lab 19: Implementing a "Favorite" Feature with Local Storage in Blazor

In this lab, you'll implement a "Favorite" feature in your Blazor application, where users can "Like" photos. The liked photos will be stored in the browser's `localStorage`, and users can navigate to a "Favorites" page to view their liked photos. You'll build a new component to manage this functionality and learn how to use Blazor's interactivity to manage state preservation in the browser.

### Step 1: Create the `AddToFavoriteComponent`

First, you'll create a new component that allows users to add a photo to their favorites list, which will be stored in `localStorage`.

1. **Add `AddToFavoriteComponent.razor` to the Client Project:**

   Create a new Razor component named `AddToFavoriteComponent.razor` in the `Client` project. This component will handle the logic for storing the photo ID in `localStorage`.

   ```csharp
   @using System.Text.Json
   @inject IJSRuntime JSRuntime

   <button @onclick="AddToLocalStorage" class="btn btn-link">Like</button>

   @code {
       [Parameter]
       public int Id { get; set; }

       public async Task AddToLocalStorage()
       {
           SortedSet<int> favoritesIds;
           string favoritesLocalStorage = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "favorites");
           if (String.IsNullOrEmpty(favoritesLocalStorage))
           {
               favoritesIds = new SortedSet<int>();
           }
           else
           {
               favoritesIds = JsonSerializer.Deserialize<SortedSet<int>>(favoritesLocalStorage);
           }
           favoritesIds.Add(Id);
           await JSRuntime.InvokeVoidAsync("localStorage.setItem", "favorites", JsonSerializer.Serialize(favoritesIds));
       }
   }
   ```

### Step 2: Update the `PhotoCard.razor` Component

Next, you'll modify the `PhotoCard` component to include the new `AddToFavoriteComponent` when `ShowAddToFavorite` is true, rendering it interactively.

1. **Modify `PhotoCard.razor`:**

   Add a new `ShowAddToFavorite` parameter to the `PhotoCard` component and conditionally render the `AddToFavoriteComponent`.

   ```csharp
   @if (ShowFooter)
   {
       <div class="card-footer">
           @if (ShowDetails)
           {
               <NavLink href="@($"photos/details/{Photo.Id}")" class="card-link">Details</NavLink>
           }
           @if (ShowAddToFavorite)
           {
               <AddToFavoriteComponent Id="@Photo.Id" @rendermode="InteractiveAuto" />
           }
           @if (_showUpdate)
           {
               <NavLink href="@($"photos/edit/{Photo.Id}")" class="card-link">Update</NavLink>
           }
           @if (_showDelete)
           {
               <NavLink href="@($"photos/delete/{Photo.Id}")" class="card-link">Delete</NavLink>
           }
       </div>
   }

   @code {
       [Parameter]
       public bool ShowAddToFavorite { get; set; } = false;

       private bool ShowFooter => ShowDetails || ShowAddToFavorite || _showUpdate || _showDelete;
   }
   ```

### Step 3: Update the `Photos/All.razor` Component

Now you'll modify the `Photos/All.razor` component to display the "Like" button by setting `ShowAddToFavorite` to true when rendering the `PhotoCard`.

1. **Modify `Photos/All.razor`:**

   Update the `PhotoCard` rendering in `Photos/All.razor`:

   ```csharp
   <PhotoCard Photo="photo" ShowDelete ShowDetails ShowUpdate ShowAddToFavorite />
   ```

### Step 4: Add a Navigation Link for the Favorites Page

Next, you'll add a link to the navigation menu that allows users to access their favorites.

1. **Modify `NavMenu.razor`:**

   Add a new navigation link to the `NavMenu.razor` component:

   ```csharp
   <li class="nav-item">
       <NavLink class="nav-link" href="photos/favorites"> Favorites </NavLink>
   </li>
   ```

### Step 5: Create the `Favorites.razor` Component

Finally, you'll create a `Favorites.razor` component that retrieves and displays the list of favorite photos from `localStorage`.

1. **Create `Favorites.razor`:**

   Add a new Razor component named `Favorites.razor` to the `Client` project. This component will handle displaying the user's favorite photos.

   ```csharp
   @page "/photos/favorites"
   @using System.Text.Json
   @inject IJSRuntime JSRuntime

   @rendermode InteractiveAuto

   <h3>Favorite Photos</h3>

   @if (favorites is null || favorites.Count == 0)
   {
       <p>You did not like any photo on this device, yet.</p>
   }
   else
   {
       <div class="row row-cols-1 row-cols-md-3 g-4">
           @foreach (var id in favorites)
           {
               <div class="col">
                   <a href="@($"photos/details/{id}")">
                       <h5>View Details Of Photo @id</h5>
                       <img src="@($"photoimage/{id}")" class="card-img-top" alt="@id">
                   </a>
               </div>
           }
       </div>
   }

   @code {
       SortedSet<int> favorites = default!;

       protected override async Task OnAfterRenderAsync(bool firstRender)
       {
           if (firstRender)
           {
               SortedSet<int> favoritesIds;
               string favoritesLocalStorage = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "favorites");
               if (String.IsNullOrEmpty(favoritesLocalStorage))
               {
                   favorites = new SortedSet<int>();
               }
               else
               {
                   favorites = JsonSerializer.Deserialize<SortedSet<int>>(favoritesLocalStorage);
               }
               StateHasChanged();
           }
       }
   }
   ```

### Step 6: Test Your Application

Run your application and test the new "Favorite" feature. Navigate to the photos page, click the "Like" button on a few photos, then navigate to the "Favorites" page to see your liked photos displayed. Ensure that the photos are correctly stored in `localStorage` and that the Favorites page correctly retrieves and displays them.

### Summary

In this lab, you learned how to use Blazor's interactive rendering and JavaScript interoperability to implement a state-preserving feature using `localStorage`. You created a `Like` button to store photo IDs and a Favorites page to display those liked photos. This feature not only demonstrates state management in Blazor but also highlights the power of integrating JavaScript features with Blazor components. In the next lab, you'll continue to explore more advanced Blazor features.