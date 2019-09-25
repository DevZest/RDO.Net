## Web Development, Now and Future

Web development is further divided into [server-side](https://en.wikipedia.org/wiki/Server-side_scripting) and [client-side](https://en.wikipedia.org/wiki/Dynamic_web_page#Client-side_scripting) development. ASP.Net Core is the answer from Microsoft to web development:

| Name | Description |
|------------|-------------|
| [Razor Pages](https://docs.microsoft.com/en-us/aspnet/core/razor-pages/) | Server-side page-focused framework. |
| [MVC](https://docs.microsoft.com/en-us/aspnet/core/mvc/) | Server-side traditional Model-View-Controller framework. |
| [Blazor](https://docs.microsoft.com/en-us/aspnet/core/blazor/) | Client-side web UI framework with .NET |
| SPA | Client-side frameworks support: [Angular](https://docs.microsoft.com/en-us/aspnet/core/client-side/spa/angular), [React](https://docs.microsoft.com/en-us/aspnet/core/client-side/spa/react), [React with Redux](https://docs.microsoft.com/en-us/aspnet/core/client-side/spa/react-with-redux), [JavaScript Services](https://docs.microsoft.com/en-us/aspnet/core/client-side/spa-services) |

Razor Pages and MVC are very similar in terms of data presentation. The key difference between Razor pages and MVC is that the model and controller code is also included within the Razor Page itself[[1]]. They are perfect for simple pages that are read-only or do basic data input. Since web application is stateless and there is no UI on the server, the complex UI logic cannot and should not be handled on the server.

Blazor, on the other hand, relies on [WebAssembly](https://webassembly.org/) (abbreviated wasm), with .Net runtime hosted in the browser. Despite it's still in beta phase, the real challenge is DOM: DOM is not designed for interactive UI, so the complex UI logic cannot be handled, or at least elegantly, either.

At this moment, RDO.Net fully supports Razor Pages server-side development, with data binding, validation and tag helpers, provided as NuGet package [DevZest.Data.AspNetCore](https://www.nuget.org/packages/DevZest.Data.AspNetCore/).

In the future, when WebAssembly becomes mature and popular, with an additional layer of non-DOM based (canvas based, for example) simple UI components, the data presentation described previously can be ported to client-side web development.

[1]: https://hackernoon.com/asp-net-core-razor-pages-vs-mvc-which-will-create-better-web-apps-in-2018-bd137ae0acaa