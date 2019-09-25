---
uid: tutorial_movies_aspnetcore
---

# Movies.AspNetCore Project

In this section, we will work on *Movies.AspNetCore* project, a ASP.Net Core Razor Pages application to display and manage a database of movies.

>[!NOTE]
>It appears Visual Studio does not support VB.Net for ASP.Net Core Web Application. *Movies.AspNetCore* is only available in C#.

## Project Setup

Make the following changes to *Movies.AspNetCore* project:

*Step 1*. Add project reference *Movies* to this project.

*Step 2*. Add NuGet package [DevZest.Data.AspNetCore](https://www.nuget.org/packages/DevZest.Data.AspNetCore/) to this project.

*Step 3*. Create subfolder *App_Data* under project root directory. Ignore this folder in source control.

*Step 4*. Add following pre-build event command line to this project, in *Build Events* tab -> *Pre-build event command line* in project property document. To show project property document, right click *Movies.AspNetCore* project in *Solution Explorer* tool window, then click *Properties* context menu item:

```shell
COPY /Y "$(ProjectDir)..\Movies.DbDesign\LocalDb\Movies.mdf" "$(ProjectDir)App_Data"
COPY /Y "$(ProjectDir)..\Movies.DbDesign\LocalDb\Movies_log.ldf" "$(ProjectDir)App_Data"
```

Build project *Movies.AspNetCore*, you should have two files *Movies.mdf* and *Movies_log.ldf* copied to *App_Data* folder.

*Step 5*. Change *appsettings.Development.json* to add connection string:

[!code-json[appsetting](../../../../samples/Tutorial/Movies.AspNetCore/appsettings.Development.json)]

## Configure Services

Change *Startup.cs* to add DataSet MVC support:

[!code-csharp[startup](../../../../samples/Tutorial/Movies.AspNetCore/Startup.cs)]

## Add Tag Helpers

Add tag helpers by changing *Pages/_ViewImports.cshtml* to:

[!code-cshtml[_ViewImports](../../../../samples/Tutorial/Movies.AspNetCore/Pages/_ViewImports.cshtml)]

## Razor Page Index

Change *Pages/Index.cshtml* to:

[!code-cshtml[Index](../../../../samples/Tutorial/Movies.AspNetCore/Pages/Index.cshtml)]

Change *Pages/Index.cshtml.cs* to:

[!code-csharp[Index](../../../../samples/Tutorial/Movies.AspNetCore/Pages/Index.cshtml.cs)]

## Razor Page Create

Add razor page *Pages/Create.cshtml*:

[!code-cshtml[Create](../../../../samples/Tutorial/Movies.AspNetCore/Pages/Create.cshtml)]

Change code behind *Pages/Create.cshtml.cs* to:

[!code-csharp[Create](../../../../samples/Tutorial/Movies.AspNetCore/Pages/Create.cshtml.cs)]

## Razor Page Edit

Add razor page *Pages/Edit.cshtml*:

[!code-cshtml[Edit](../../../../samples/Tutorial/Movies.AspNetCore/Pages/Edit.cshtml)]

Change code behind *Pages/Edit.cshtml.cs* to:

[!code-csharp[Edit](../../../../samples/Tutorial/Movies.AspNetCore/Pages/Edit.cshtml.cs)]

## Razor Page Details

Add razor page *Pages/Details.cshtml*:

[!code-cshtml[Details](../../../../samples/Tutorial/Movies.AspNetCore/Pages/Details.cshtml)]

Change code behind *Pages/Details.cshtml.cs* to:

[!code-csharp[Details](../../../../samples/Tutorial/Movies.AspNetCore/Pages/Details.cshtml.cs)]

## Razor Page Delete

Add razor page *Pages/Delete.cshtml*:

[!code-cshtml[Delete](../../../../samples/Tutorial/Movies.AspNetCore/Pages/Delete.cshtml)]

Change code behind *Pages/Delete.cshtml.cs* to:

[!code-csharp[Delete](../../../../samples/Tutorial/Movies.AspNetCore/Pages/Delete.cshtml.cs)]

## Run

Press F5 to run project *Movies.AspNetCore*. You should have a working ASP.Net Core application to display and manage a database of movies:

![image](/images/tutorial_movies_aspnetcore_run.jpg)
