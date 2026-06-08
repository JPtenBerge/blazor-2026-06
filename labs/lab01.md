# Lab 1: The Blazor FrontEnd

The goal of our first lab is to start building our web application. We won't add any page yet, we just want to create a Solution with a Blazor project.  
Although we're going to try to use as much Static Server Side Rendering as possible, we will still start with a template that ensures the most flexible interactivity mode: Auto.  
This way, should we encounter the limits of Static SSR, we will be ready to switch to any other mode as we see fit.

- Open Visual Studio.
- Create a new project.
- Select `Blazor Web App`. Select `Next`.
- In the `Solution Name` field, type `PhotoSharingApplication`
- In the `Project name` field, type `PhotoSharingApplication` 
- Provide a `Location` for the project, such as `Lab01\Start`. 
- Select `Next`.
- Select `.NET 10.0` as the target framework.
- Select `None` as `Authentication Type`.
- Ensure that `Configure for HTTPS` is checked.
- Select `Auto` as the `Interactive Mode`.
- Select `Per Page / Component` as the `Interactivity Location`.
- Ensure that `Include Sample Pages` is checked.
- Ensure that `Do not use top level statements` is unchecked.
- Ensure that `Enlist in .NET Aspire Orchestration` is unchecked.
- Select `Create`.
- Run it with `F5`.

Notice that the template has 
- A `Home` routable component
  - This is the default page that is shown when you run the application.
- A `Weather` routable component
  - This is a page that simulates fetching data from a service to show it in a table. This page uses Streaming **Static Server Side Rendering**.
- A `Counter` routable component
  - This is a page that shows a counter that can be incremented. This page uses **Interactive Auto Rendering**, in fact it's contained in the `Client` project so that it can be compiled and downloaded as WebAssembly.

In the next lab we will build additional pages to manage the photos in our application.