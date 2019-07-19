---
uid: data_presentation_the_right_way
---

# Data Presentation, the Right Way

Separation of the graphical user interface from the business logic or back-end logic (the data model), is still a challenge task. Frameworks such as [MVVM](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93viewmodel) introduces a new layer (the view model), trying to handle most if not all of the view's display logic:

![image](/images/MVVM.jpg)

Presentation logic is complex - it's responsible for overcoming the gap between computer data and human being, which IMO is the biggest challenge in computer science. To encapsulate presentation logic into a separate layer, we need to plan carefully at the very beginning. Unfortunately, existing MVVM implementations, among other similar frameworks, are built as an afterthought.

## The Anti-Patterns

### The POCO Obsession, Again

When model is arbitrary object (POCO), hiding the model from the view is simply not possible. The presentation layer can do little about it, in many cases, it just expose the model object via aggregation, without any value-added.

On the other hand, data model cannot be 100% POCO. For example, `INotifyPropertyChanged` interface is mandatory for most data models, and `IDataErrorInfo` interface is required if you want to bind to custom validation error. Since these interfaces must be implemented by all data models, they must be dead simple.

In the end, your presentation layer can do little with the data model.

### Complex Control

Complex control, such as `DataGrid`, has very complex presentation logics. Since these controls are built without existing presentation layer, these logics are naturally encapsulated into the control itself - the view. This puts the presentation layer into an embarrassed situation: for simple control without complex view state such as `TextBox`, it has little job to do; for complex control such as `DataGrid`, the control has done the job.

In the end, your presentation layer can do little with the view too.

Put it together, if the presentation layer is an afterthought, there is little room left for implementation, especially at abstraction level.

## The Right Way

Thanks to [Rich Metadata](xref:orm_data_access_the_right_way#rich-metadata---relational-data-objects), we now have foundations to build a comprehensive presentation layer into a highly reusable and fully customizable <xref:DevZest.Data.Presenters.DataPresenter> class, mapping <xref:DevZest.Data.DataRow> into <xref:DevZest.Data.Presenters.RowPresenter>, with the following features built out of box:

* Selection, sorting, filtering and hierarchical grouping.
* UI elements life time management and data binding.
* Editing and validation.
* Layout and UI virtualization.

Simply derive you data presenter from <xref:DevZest.Data.Presenters.DataPresenter`1> class, and put a <xref:DevZest.Data.Views.DataView> into your view, you got all the above features immediately, without any complex control. The following code:
[!code-csharp[Db](../../../samples/AdventureWorksLT.WpfApp/SalesOrderWindow.DetailPresenter.cs)]
Will produce the following data grid UI, with foreign key lookup and paste append from clipboard implemented:
![image](/images/SalesOrderDetailUI.jpg)

In the end, you have ALL of your presentation logic in 100% strongly typed, extremely clean code!

## Web Development, Now and Future

The data presentation described previously has been implemented in Windows Presentation Foundation (WPF) desktop development. Unfortunately, the whole thing cannot be ported to the world of web development, at this moment.

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
