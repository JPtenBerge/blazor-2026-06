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
    - Asynchronous JavaScript And XML (2005)
                                  JSON
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

## Coole links

- [Vite die uitlegt waarom HMR zo fijn is](https://vite.dev/guide/why)
- [Benchmark frontend frameworks](https://github.com/krausest/js-framework-benchmark)
  - focust vooral op DOM-aanpassingen, vandaar dat WASM doorgaans niet super uit de bus komt


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