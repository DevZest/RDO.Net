---
uid: data_presentation_the_right_way
---

# Data Presentation, the Right Way

Separation of the graphical user interface from the business logic or back-end logic (the data model), is still a challenge task. Frameworks such as [MVVM](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93viewmodel) introduces a new layer (the view model), trying to handle most if not all of the view's display logic.

Presentation logic is complex - it's responsible for overcoming the gap between computer and human being, which is a big (maybe biggest) challenge in computer science. To encapsulate presentation logic into a separate layer, we need to plan carefully at the very beginning. Unfortunately, existing MVVM implementations, among other similar frameworks, are built as an afterthought.

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

Thanks to [Rich Metadata](xref:orm_data_access_the_right_way#rich-metadata---relational-data-objects), we now have foundations to implement a comprehensive Model-View-Presenter (MVP) to improve the separation of concerns in presentation logic. The following is the architecture of RDO.WPF MVP:

[!include[Welcome to RDO.Net](../_rdo_wpf_mvp_architecture.md)]

Simply derive you data presenter from <xref:DevZest.Data.Presenters.DataPresenter`1> class, and put a <xref:DevZest.Data.Views.DataView> into your view, you got all the above features immediately, without any complex control. The following code:

# [C#](#tab/cs)

[!code-csharp[DetailPresenter](../../../../samples/AdventureWorksLT.WpfApp/SalesOrderWindow.DetailPresenter.cs)]

# [VB.Net](#tab/vb)

[!code-vb[DetailPresenter](../../../../samples.vb/AdventureWorksLT.WpfApp/SalesOrderWindow.DetailPresenter.vb)]

***

Will produce the following data grid UI, with foreign key lookup and paste append from clipboard implemented:
![image](/images/SalesOrderDetailUI.jpg)

In the end, you have ALL of your presentation logic in 100% strongly typed, extremely clean code!

The data presentation described previously has been implemented in Windows Presentation Foundation (WPF) desktop development. Unfortunately, the whole thing cannot be ported to the world of web development, at this moment.

[!include[Web Development, Now and Future](../_web_development_now_and_future.md)]
