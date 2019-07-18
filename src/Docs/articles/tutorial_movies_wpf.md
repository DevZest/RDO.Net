---
uid: tutorial_movies_wpf
---

# Movies.WPF Project

In this section, we will work on *Movies.WPF* project, a WPF application to display and manage a database of movies.

## Project Setup

Make the following changes to *Movies.WPF* project:

*Step 1*. Add project reference *Movies* to this project.

*Step 2*. Add NuGet package [DevZest.Data.WPF](https://www.nuget.org/packages/DevZest.Data.WPF/) to this project.

*Step 3*. Add the following assembly level attribute to *Properties/AssemblyInfo.cs* (or *My Project/AssemblyInfo.vb* if you're using VB.Net):

# [C#](#tab/cs)

```csharp
using DevZest.Data.Presenters;
using Movies.WPF;
...
[assembly: ResourceIdRelativeTo(typeof(App))]
```

# [VB.Net](#tab/vb)

```vb
Imports DevZest.Data.Presenters
...
<Assembly: ResourceIdRelativeTo(GetType(Application))>
```

***

>[!Note]
>If you cannot find *AssemblyInfo.vb* in *Solution Explorer*, make sure you have enabled "Show all files" on the toolbar of the *Solution Explorer*.

*Step 4*. Add *Movies.mdf* and *Movies_log.ldf* to this project as linking, by right clicking *Movies.WPF* project in *Solution Explorer* tool window, then click context menu item "*Add*" -> "Existing Item...", select *All Files(*.*)* from right bottom combobox, then navigate to *LocalDb subfolder* of *Movies.DbDesign* project, select file *Movies.mdf* and *Movies_log.ldf*:

![image](/images/tutorial_add_moviesdb.jpg)

 Click the little dropdown arrow next to the *Add* button and select *Add as Link*, instead of copying the file into the project directory, Visual Studio will create a link to the original:

![image](/images/tutorial_linked_moviesdb.jpg)

Select these two files, then right click and select context menu item *Properties*, make sure *Copy to Output Directory* is set to *Copy Always*:

![image](/images/tutorial_copy_always.jpg)

## App/Application

Change *App.xaml.cs*(*Application.xaml.vb* if you're using VB.Net) to:

# [C#](#tab/cs)

[!code-csharp[App](../../../samples/Tutorial/Movies.WPF/App.xaml.cs)]

# [VB.Net](#tab/vb)

[!code-vb[Application](../../../samples.vb/Tutorial/Movies.WPF/Application.xaml.vb)]

***

## MovieDetailWindow

Add class file *MovieDetailWindow.Presenter.cs* (*MovieDetailWindow.Presenter.vb* if you're using VB.Net):

# [C#](#tab/cs)

[!code-csharp[MovieDetailWindowPresenter](../../../samples/Tutorial/Movies.WPF/MovieDetailWindow.Presenter.cs)]

# [VB.Net](#tab/vb)

[!code-vb[MainWindowPresenter](../../../samples.vb/Tutorial/Movies.WPF/MovieDetailWindow.Presenter.vb)]

***

***

Add window *MovieDetailWindow.xaml* to project. Change *MovieDetailWindow.xaml* to:

# [C#](#tab/cs)

[!code-xaml[MovieDetailWindow](../../../samples/Tutorial/Movies.WPF/MovieDetailWindow.xaml)]

# [VB.Net](#tab/vb)

[!code-xaml[MainWindow](../../../samples.vb/Tutorial/Movies.WPF/MovieDetailWindow.xaml)]

***

***

Change code behind *MovieDetailWindow.xaml.cs* (*MovieDetailWindow.xaml.vb* if you're using VB.Net) to:

# [C#](#tab/cs)

[!code-csharp[MovieDetailWindow](../../../samples/Tutorial/Movies.WPF/MovieDetailWindow.xaml.cs)]

# [VB.Net](#tab/vb)

[!code-vb[MovieDetailWindow](../../../samples.vb/Tutorial/Movies.WPF/MovieDetailWindow.xaml.vb)]

***

## MainWindow

Add class file *MainWindow.Presenter.cs* (*MainWindow.Presenter.vb* if you're using VB.Net):

# [C#](#tab/cs)

[!code-csharp[MainWindowPresenter](../../../samples/Tutorial/Movies.WPF/MainWindow.Presenter.cs)]

# [VB.Net](#tab/vb)

[!code-vb[MainWindowPresenter](../../../samples.vb/Tutorial/Movies.WPF/MainWindow.Presenter.vb)]

***

***

Add resource dictionary *MainWindow.Styles.xaml*:

# [C#](#tab/cs)

[!code-xaml[MainWindowStyles](../../../samples/Tutorial/Movies.WPF/MainWindow.Styles.xaml)]

# [VB.Net](#tab/vb)

[!code-xaml[MainWindowStyles](../../../samples.vb/Tutorial/Movies.WPF/MainWindow.Styles.xaml)]

***

***

Change *MainWindow.xaml* to:

# [C#](#tab/cs)

[!code-xaml[MainWindow](../../../samples/Tutorial/Movies.WPF/MainWindow.xaml)]

# [VB.Net](#tab/vb)

[!code-xaml[MainWindow](../../../samples.vb/Tutorial/Movies.WPF/MainWindow.xaml)]

***

***

Change code behind *MainWindow.xaml.cs* (*MainWindow.xaml.vb* if you're using VB.Net) to:

# [C#](#tab/cs)

[!code-csharp[MainWindow](../../../samples/Tutorial/Movies.WPF/MainWindow.xaml.cs)]

# [VB.Net](#tab/vb)

[!code-vb[MainWindow](../../../samples.vb/Tutorial/Movies.WPF/MainWindow.xaml.vb)]

***

## Run

Press F5 to run project *Movies.WPF*. You should have a working WPF application to display and manage a database of movies:

![image](/images/tutorial_movies_wpf_run.jpg)
