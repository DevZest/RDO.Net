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

## AppSettings

Change *appsettings.Development.json* to add connection string:

[!code-json[appsetting](../../../samples/Tutorial/Movies.AspNetCore/appsettings.Development.json)]

## Startup.cs

Change *Startup.cs* to inject `Db`:

[!code-csharp[startup](../../../samples/Tutorial/Movies.AspNetCore/Startup.cs)]
