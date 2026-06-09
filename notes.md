# Notes

## Paradigmas der webdevelopment

- Classic webapp / Multi Page Application  (MPA)
  - elke keer een hele nieuwe pagina
    - elk klikje om te navigeren
  - Flash Of Unstyled Content
  - libs/frameworks: ASP.NET WebForms/MVC/Core Spring PHP Laravel WordPress Blazor
  - wanneer: minder interactie, minder snel feedback hoeven geven, meer lightweight
    - minder complexiteit
  - niet hip
- Server-side rendering (SSR)
  - complementair aan de SPA
  - allereerste pagina wordt gerenderd door de server
    - terwijl de gebruiker de UI verkend, worden de interactieve dingetjes op de achtergrond opgestuurd
  - libs/frameworks: Next.js (React) Nuxt.js (Vue) @angular/ssr SvelteKit SolidStart QwikCity  ASP.NET Core
  - lijkt de default
  - veel meer complexiteit
- Static Site Generation (SSG)
  - heul veul `.html`-bestandjes genereren
    - tijdens het builden (vanuit je pipeline)
    - `/product/haarborstel-4275483.html`
  - Wehkamp bol.com   /product/437382
  - libs/frameworks: mkdocs Docusaurus 11ty HUGO Next.js @angular/ssr Nuxt.js
- Single Page Application (SPA)
  - 1 pagina, een stukje wordt ververst
    - Asynchronous JavaScript And XML (2005) (overal wordt allang JSON gebruikt, maar AJAJ klinkt minder tof)
  - zo min mogelijk full page refreshes
  - libs/frameworks: Angular React Vue Svelte Blazor Solid Inferno Qwik
  - wanneer: responsiveness gebruiksvriendelijkheid   hoog niveau aan interactie
    - meer complexiteit
  - hip
  - bundle.js 200kb 2MB

## Blazor

- uitgekomen in 2019
- van Microsoft!
- grote namen:
  - Steve Sanderson
    - ooit bekend van knockout.js
  - Daniel Roth - productmanager
- programmeren in C#
  - dit is de hoofdreden waarom teams/organisaties voor Blazor kiezen.
- content projection doet het heel mooi


## Blazor-edities

- Blazor Static SSR - 2025
  - MPA
- Blazor Server
  - SPA
  - WebSocket die continu open is
    - UI state
    - SignalR: Microsoft's laagje om reconnecten
  - ieder klikje/tikje is een berichtje over de websocket
  - al je code draait op de SERVER
  - nadeel: dode UI als connectie wegvalt
- Blazor WebAssembly
  - SPA
  - al je code draait op de client (in de browser)
  - React Vue Angular Svelte
  - data verkrijgen via REST APIs
  - SSR "prerendering"
  - deze functioneert offline als hij eenmaal geopend is
    - m.u.v. REST APIs die je nog moet aanroepen
  - Browser API "WebAssembly"
    - assembly-achtige taal IN DE BROWSER
    - compilers   jouwtaal => webassembly compileren
    - C# => WebAssembly - "Hello world Blazor WASM" 7MB
  - nadeel: bundle size. 

## Nadelen Blazor

- bij WASM: initiele page load
  - is alleen de allerallereerste keer, daarna weten service workers het uit hun cache te halen
  - compressie helpt: gzip .gz / brotli .br
- bij Server: die connectie moet in stand blijven
- performance bij heel veel DOM-updates
- DX - Developer Experience
  - Blazor heeft geen HMR
  - watcher is meh.
  moderne frontendstacks gebruiken module bundlers om code te compileren en te verversen in de browser.
  - Vite
    - Hot Module Replacement
  - Rspack
- Tailwind / concurrenten integreert niet lekker
  - je kan een post-build command opnemen met een npm command. 
- minor dingetje: `ElementReference` is een struct
  ```razor
  <input @ref="JouwInput">
  public ElementReference JouwInput { get; set; }
  ```
  Ik heb wel eens een mock willen maken hiervan, `new Mock<ElementReference>()`, maar dat kan dus niet want `struct`, vergelijkbaar met dat je niet een `new Mock<int>` of `new Mock<bool>` kan doen.
- errors binnen Razor-template verwijzen naar `.razor.g.cs`

## IDE

- Visual Studio Enterprise
  - $250/maand - $3000/jaar
- RIder
  - $600/jaar

## Verhouding Razor/Blazor

- Razor is de view engine
  - alle syntax  `.razor`
- Blazor gebruikt de Razor view engine
- ASP.NET MVC
  - 2008

## Formvalidatie

- Blazor ondersteunt natuurlijk .NET's ingebouwde data annotations: `[Required]` `[RegularExpression("...")]`
  - zijn wel beperkt in de validaties die je kunt doen
    - geen conditionele validatie
    - beperkt tot "simpele" types, dus geen complexe properties als subclasses of `List<SubClass>`
  - lastig te unittesten
    - met reflection metadata uitlezen, niet heel fantastisch
- FluentValidation


## GET vs POST

- GET is om op te halen
- POST is om toe te voegen
- POST veiliger?
  - een beetje
- GET worden parameters via URL verzonden
  - /Pagina?name=JP&age=39
- POST
  - /Pagina
  - BODY  name=JP&age=39&...

## Dependency injection

- ergens injecten op plekken waar je 'm nodig hebt
- memory management
- worden automatisch geinjecteerd
- handig bij unittesten - mocking!

>high cohesion, low coupling

```cs
ILogger logger;
public void ComplexAlgorithm()
{
  logger.Log("Calculating tail..."); // het maakt dit algoritme niet uit waar er naar toe wordt gelogd - console, file, db
}
```

Blazor Static SSR lifetimes:
- addtransient
  - altijd een nieuwe
- addscoped
  - HTTP/WebSocket/...-request
- addsingleton
  - altijd - tot applicatie sluit/crasht

Blazor WebAssembly lifetimes:
- addtransient
  - altijd een nieuwe
- addscoped
  - [Client-side doesn't currently have a concept of DI scopes. Scoped-registered services behave like Singleton services.](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/dependency-injection?view=aspnetcore-10.0#service-lifetime)
- addsingleton
  - altijd - tot applicatie sluit/crasht


Styling

- Bootstrap
  - Grid system  col-lg-4    grid flex
- Material Design
  - MatBlazor
  - MudBlazor
- Overig
  - AntDesign
  - MIcrosoft Fluent UI - "het ziet er wel erg Outlook-erig uit"

## UI library integreren

Kijk eerst of de UI library waar je interesse in hebt, ondersteund wordt voor jouw Blazoruitvoering. Traditioneel is Blazor interactief met Server/WebAssembly, Static SSR wordt nog niet door alles ondersteund. Zie [Awesome Blazor](https://github.com/AdrienTorris/awesome-blazor?tab=readme-ov-file#libraries--extensions) voor lijst van UI libraries.

Daarna is het integreren meestal ongeveer deze stappen:

- NuGet package toevoegen
- Program dependency injection regelen
- `App.razor`/`Host.razor` toevoegen van `<script>` `<link>`
- `MainLayout.razor` globale UI componenten opnemen, `<Theme />`, `<Snackbar />`
- globale `@using` zodat je de componenten kunt aanspreken zonder prefix:
  ```razor
	<MudBlazor.Component.MudButton />
	<MudButton />  
  ```
- klaar voor gebruik!

## Coole links

- [Vite die uitlegt waarom HMR zo fijn is](https://vite.dev/guide/why)
- [Benchmark frontend frameworks](https://github.com/krausest/js-framework-benchmark)
  - focust vooral op DOM-aanpassingen, vandaar dat WASM doorgaans niet super uit de bus komt
- [Dapper, alternatief voor EF Core](https://github.com/DapperLib/Dapper)
- [Turkey test](https://www.moserware.com/2008/02/does-your-code-pass-turkey-test.html), over het hoofdletterongevoelig vergelijken van strings