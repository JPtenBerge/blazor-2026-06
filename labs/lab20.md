# Lab 20: Interoperability between Blazor and JavaScript

This lab will focus on Blazor JavaScript Interoperability.
We are going to learn how to invoke JavaScript functions from Blazor and how to call .NET methods from JavaScript functions in ASP.NET Core Blazor.  

We are going to build a simple `MapComponent` Razor component that will show a map with a marker and popup on the coordinates passed to the component.  
The component will have three parameters: `Latitude`, `Longitude`, and `Description`.
`Latitude` and `Longitude` will be the coordinates of the marker, while `Description` will be the text of the popup.
The component will also intercept the click on the map and change its `Latitude` and `Longitude` properties accordingly.

The first part of this lab will be about invoking JavaScript functions from Blazor. We will use this to show a map on the `PhotoCard.razor` component, setting its coordinates to the `Latitude` and `Longitude` of the `Photo` (properties that we don't have yet but that we're going to add).
The second part will be about calling .NET methods from JavaScript functions. We will use this to intercept the click on the map so that the Blazor component is notified and can change its own `Latitude` and `Longitude` properties.

## Part 1: Invoking JavaScript functions from Blazor

Our goal for this part of the lab is to add a map on the `PhotoCard.razor`, showing a marker on the coordinates where the picture was taken.  
We're going to use [Leaflet](https://leafletjs.com/), an open-source JavaScript library for mobile-friendly interactive maps.  
Since Leaflet is a JavaScript library, we need to learn not only how to use Leaflet itself, but also how to invoke JavaScript from Blazor.

Let's start with a simple example, just to understand the steps to interoperate with JavaScript.  
After we are sure that we have everything setup, we'll include Leaflet.  
For now, we'll start by adding a new `MapComponent.razor` component.  We want the component to interact with JavaScript, so we'll need to render the component with a `@rendermode=InteractiveAuto` directive. That's why we will put our component in the `Components` folder of the `Client` project.

### MapComponent.razor

- In your `Client` project, under the `Components` folder, add a `MapComponent.razor` razor component
- In your `Client` project, under the `Components` folder, add a `MapComponent.razor.js` JavaScript file

In the JavaScript file, call the `alert` function:

```js
alert(`here's your map! (well, not yet but we're working on it)`);
```

In the `Home.razor` page of your `Server` project, add the `MapComponent`:

```csharp
<MapComponent @rendermode="InteractiveAuto" />
```

If you run your project now, you'll see the component on your home page, but the alert message doesn't show up. This is because we haven't referenced the JavaScript file in the `App.razor` file of the `Server` project.

- In the `App.razor` file of your `Server` project, modify the `body` to reference the JavaScript file, after the `blazor.web.js` file:

```csharp
<body>
    <Routes />
    <script src="_framework/blazor.web.js"></script>
    <script src="Components/MapComponent.razor.js"></script>
</body>
```

Now, if you run the application, you should see the alert message popping up.

### .NET to JavaScript

Let's put a button in the `MapComponent.razor` and invoke a JavaScript function when the button is clicked.

- In the `MapComponent.razor` component, add a button and handle the `click` event by invoking a `ShowMap` method

```html

<button @onclick="ShowMap">Show Map</button>

@code {
    public async Task ShowMap() {
        await JSRuntime.InvokeVoidAsync("showMap");
    }
}
```
Don't forget to inject the `IJSRuntime` in the component:

```csharp
@using Microsoft.JSInterop
@inject IJSRuntime JSRuntime
```

- In the `MapComponent.razor.js` file, add a `showMap` function that invokes the `alert` function

```js
function showMap() {
    alert(`here's your map! (well, not yet but we're working on it)`);
}
```

If you run the application now, you should see the alert message popping up when you click the button.

Our function is polluting the global namespace. We should avoid that. We can use a [JavaScript module](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Modules) to isolate our function.

- In the `MapComponent.razor.js` file, modify the `showMap` function to be a module

```js
export function showMap() {
    alert(`here's your map! (well, not yet but we're working on it)`);
}
```

- Modify the `MapComponent.razor` component to load the module

```csharp
private IJSObjectReference? module;

protected async override Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        module = await JSRuntime.InvokeAsync<IJSObjectReference>("import",
            "./Components/MapComponent.razor.js");
    }
}
```

- Use the module to invoke the exported function

```csharp
public async Task ShowMap() {
    await module.InvokeVoidAsync("showMap");
}
```

- Dispose of the module when the component is disposed

```csharp
public async ValueTask DisposeAsync()
{
    if (module is not null)
    {
        await module.DisposeAsync();
    }
}
```

- Don't forget to implement the `IAsyncDisposable` interface

```csharp
@implements IAsyncDisposable
```

If you run the application now, you should still see the alert message popping up when you click the button. If you don't, you may need to empty the cache of your browser and reload the page.

Also, verify that the component works as expected when you navigate to another page and then back to the home page, which should close the signalR circuit and run the code in the browser instead.

Now that we load the module from the razor component, we can remove the script tag from the `App.razor` file.

### Leaflet

Now that we know how to invoke JavaScript functions from Blazor, let's include Leaflet in our project.

- Include Leaflet CSS file in the head section of `App.razor` in the `Server` project:

```html
 <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"
     integrity="sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY="
     crossorigin=""/>
```

- Include Leaflet JavaScript file after Leaflet’s CSS and before the `MapComponent.razor.js` file in the `App.razor` file of the `Server` project:

```html
 <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"
     integrity="sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo="
     crossorigin=""></script>
```

- Put a `div` element with a certain `id` in the `MapComponent.razor` component:

```html
 <div id="map"></div>
```

Make sure the map container has a defined height, for example by setting it in a new `MapComponent.razor.css`:

```css
#map { 
    width: 100%;
    aspect-ratio: 4/3;
}
```

Now you’re ready to initialize the map and do some stuff with it.

- In the `MapComponent.razor.js` file, modify the `showMap` function to create a map in the `map` div, add tiles of your choice, and then add a marker with some text in a popup:

```js
const map = L.map('map').setView([51.505, -0.09], 13);

L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
}).addTo(map);

var marker = L.marker([51.5, -0.09]).addTo(map);

marker.bindPopup('A pretty CSS3 popup.<br> Easily customizable.').openPopup();
```

If you run the application now, you should see the map showing up, with the marker and popup, as soon as you click the button.

On the next step, we want to pass the coordinates and the title of the photo to the `MapComponent`. We will do this by adding a `Latitude`, `Longitude` and `Description` parameter to the `MapComponent` and passing the `Photo.Latitude` and `Photo.Longitude` to the component from the `Photos/Details.razor` page.

Let's go step by step.  
First, let's modify our JavaScript function to accept parameters. We'll pass the coordinates of Amsterdam for now.

- In the `MapComponent.razor.js` file, modify the `showMap` function to accept three parameters `lat`, `lon`, and `msg`:

```js
export function showMap(lat, lon, msg) {
    const map = L.map('map').setView([lat, lon], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map);

    var marker = L.marker([lat, lon]).addTo(map);

    marker.bindPopup(msg).openPopup();
}
```

- In the `MapComponent.razor` component add three `Parameter`s: `Latitude`, `Longitude`, and `Description`

```csharp
[Parameter]
public double Latitude { get; set; }

[Parameter]
public double Longitude { get; set; }

[Parameter]
public string Description { get; set; }
```

- Modify the `ShowMap` method to send the arguments to the JavaScript function

```csharp
async Task ShowMap()
{
    await module.InvokeVoidAsync("showMap", Latitude, Longitude, Description);
}
```

- In the `Home.razor` page, pass the coordinates and name of Amsterdam:

 `Photo.Latitude`, `Photo.Longitude`, and `Photo.Title` to the `MapComponent`:

```csharp
<MapComponent @rendermode="InteractiveAuto" Latitude="52.3676" Longitude="4.9041" Description="Amsterdam" />
```

Running the application now and clicking the button should show the map with the marker and popup on Amsterdam.

Now let's modify our `Photo` model to include the `Latitude` and `Longitude` properties.

Open the `Photo.cs` class in your `Core/Models` folder of your `Server` project and add the `Latitude` and `Longitude` properties:

```csharp
public double Latitude { get; set; }
public double Longitude { get; set; }
```

Now, let's add a migration and update the database.

- In the `Package Manager Console`, add a new migration:

```shell
Add-Migration PhotoLatLon -Context PhotoSharingApplication.Infrastructure.Data.PhotosDbContext
```

- Update the database:

```shell
Update-Database PhotoLatLon -Context PhotoSharingApplication.Infrastructure.Data.PhotosDbContext
```

Right now all the photos in the database have `0,0` as values for `Latitude` and `Longitude`. You can change these values in the database to see the marker in different positions. Remember that `Latitude` should be between -90 and 90, while `Longitude` should be between -180 and 180.

Let's make sure that the `PhotoCard.razor` component is showing the `Latitude` and `Longitude` of the photo.

- In the `PhotoCard.razor` located in the `Components` folder of the `Server` project
  - Add the `MapComponent` and pass the `Latitude` and `Longitude` properties of the `Photo` parameter:

```csharp
<div class="card-body">
    <h5 class="card-title">@Photo.Title</h5>
    <p class="card-text">@Photo.Description</p>
    <p class="card-text">@Photo.Id</p>
    <MapComponent Latitude="@Photo.Latitude" Longitude="@Photo.Longitude" Description="@Photo.Title" @rendermode="InteractiveAuto" />
</div>
```

If you run the application now and go to the `Photos` page, you should see the map showing up with the marker and popup on the coordinates of the photo after clicking on a button.
Clicking on a second button gives an error though. This is because we now have multiple instances of the `MapComponent` on the page, and they all try to use the same `id` for the map container. We need to make sure that each instance has a unique `id`.
Let's add a new `Id` property for the `MapComponent`, where we will pass the `Id` of the `Photo`.

- In the `MapComponent.razor` component, add an `Id` parameter:

```csharp
[Parameter]
public int Id { get; set; }
```

- Modify the `MapComponent.razor.js` file to use the `Id` parameter to set the `id` of the map container:

```js
export function showMap(id, lat, lon, msg) {
    const map = L.map(`map-${id}`).setView([lat, lon], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map);

    var marker = L.marker([lat, lon]).addTo(map);

    marker.bindPopup(msg).openPopup();
}
```

- Modify the `MapComponent.razor` component to pass the `Id` to the JavaScript function:

```csharp
async Task ShowMap()
{
    await module.InvokeVoidAsync("showMap", Id, Latitude, Longitude, Description);
}
```

- In the `MapComponent.razor` component, modify the `div` element to use the `Id` parameter:

```csharp
<div id="@($"map-{Id}")"></div>
```

- In the `MapComponent.razor.css` file, modify the `#map` selector to use a class instead of an id:

```css
.map { 
    width: 100%;
    aspect-ratio: 4/3;
}
```

- In the `MapComponent.razor` component, modify the `div` element to use the `map` class:

```csharp
<div class="map" id="@($"map-{Id}")"></div>
```

- In the `PhotoCard.razor` component, pass the `Id` of the `Photo` to the `MapComponent`:

```csharp
<MapComponent Id="@Photo.Id" Latitude="@Photo.Latitude" Longitude="@Photo.Longitude" Description="@Photo.Title" @rendermode="InteractiveAuto" />
```

If you run the application now and go to the `Photos` page, you should see the map showing up with the marker and popup on the coordinates of the photo after clicking on a button.
A photo with `0,0`, should show the marker on the Equator, at the Greenwich parallel.

You can remove the component from the Home Page.

Also, instead of showing the map only when the user clicks on the button, let's just remove the button and show the map directly after render.

```csharp
@using Microsoft.JSInterop
@implements IAsyncDisposable
@inject IJSRuntime JSRuntime

<div id="@($"map-{Id}")" class="map"></div>

@code {
    private IJSObjectReference? module;

    [Parameter]
    public int Id { get; set; }

    [Parameter]
    public double Latitude { get; set; }

    [Parameter]
    public double Longitude { get; set; }

    [Parameter]
    public string Description { get; set; }

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            module = await JSRuntime.InvokeAsync<IJSObjectReference>("import",
                "./Components/MapComponent.razor.js");
            await module.InvokeVoidAsync("showMap", Id, Latitude, Longitude, Description);
        }
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (module is not null)
        {
            await module.DisposeAsync();
        }
    }
}
```

If you run the application now and go to the `Photos` page, you should see the map showing up with the marker and popup on the coordinates of the photo.

## Part 2: Calling .NET methods from JavaScript functions

In this part of the lab, we are going to intercept the click on the map and change the `Latitude` and `Longitude` properties of the `MapComponent`.

We are going to use the `DotNetObjectReference` class to pass a reference to the `MapComponent` to the `showMap` JavaScript function.

```csharp
protected async override Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        module = await JSRuntime.InvokeAsync<IJSObjectReference>("import",
            "./Components/MapComponent.razor.js");

        DotNetObjectReference<MapComponent> objRef = DotNetObjectReference.Create(this);
        await module.InvokeVoidAsync("showMap", objRef, Id, Latitude, Longitude, Description);
    }
}
```

In the `MapComponent.razor.js` file, modify the `showMap` function to accept a reference to the `MapComponent`:

```js
export function showMap(dotNetHelper, id, lat, lon, msg) {
    const map = L.map(`map-${id}`).setView([lat, lon], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map);

    var marker = L.marker([lat, lon]).addTo(map);

    marker.bindPopup(msg).openPopup();

    map.on('click', function(e) {
        dotNetHelper.invokeMethodAsync('UpdateCoordinates', e.latlng.lat, e.latlng.lng);
    });
}
```

In the `MapComponent.razor` component, add a `UpdateCoordinates` method that will be called when the map is clicked:

```csharp
[JSInvokable]
public async Task UpdateCoordinates(double lat, double lon)
{
    Latitude = lat;
    Longitude = lon;
    StateHasChanged();
}
```

The StateHasChanged method is called to notify the component that the state has changed and that it should re-render.
Add the coordinates to the html:

```csharp
<div id="@($"map-{Id}")" class="map"></div>
Lat: @(String.Format("{0:N2}", Latitude)) - Lon: @(String.Format("{0:N2}", Longitude))
```

Let's transform the `Latitude` and `Longitude` properties into private fields and add a backing field for each property, then add two events to notify the parent component when the coordinates change:

```csharp
[Parameter]
public double Latitude { get; set; }
[Parameter]
public EventCallback<double> LatitudeChanged { get; set; }
private double _latitude;

[Parameter]
public double Longitude { get; set; }
[Parameter]
public EventCallback<double> LongitudeChanged { get; set; }
private double _longitude;

[JSInvokable]
public async ValueTask UpdateCoordinates(double latitude, double longitude)
{
    _longitude = longitude;
    _latitude = latitude;
    await LongitudeChanged.InvokeAsync(longitude);
    await LatitudeChanged.InvokeAsync(latitude);
    StateHasChanged();
}
```

Use the backing fields in the html:

```csharp
Lat: @(String.Format("{0:N2}", _latitude)) - Lon: @(String.Format("{0:N2}", _longitude))
```

This way, a parent component can listen to the `LatitudeChanged` and `LongitudeChanged` events and update the `Photo` object accordingly, should it whish to do so.

Remember, right now our `PhotoEditor` component is not listening to these events because it's rendered statically server side with Static SSR. You can add this functionality as an exercise, but if you chose to switch to Auto interactivity, you will have to move the component on the `Client` project, which will require to implement the client side communication to the server, which means the server will have to provide a WebApi and the client will have to implement a Repository that uses an HttpClient.