---
uid: tutorial_get_started
---

# Tutorial: Get Started

## Overview

This tutorial contains step by step information to demonstrate the basics of RDO.Net. At the end, you'll have a WPF application:

![image](/images/tutorial_movies_wpf_run.jpg)

and an ASP.Net Core Application(C# only):

![image](/images/tutorial_movies_aspnetcore_run.jpg)

that can display and manage a database of movies.

View or download sample code: [C#](https://github.com/DevZest/RDO.Tutorial) | [VB.Net](https://github.com/DevZest/RDO.Tutorial)

## Prerequisites

* Visual Studio 2017 (version >= 15.3) or Visual Studio 2019 (all versions), with *.Net desktop development* and *ASP.NET web development* workloads installed.
* [RDO.Tools](https://marketplace.visualstudio.com/items?itemName=DevZest.Data.Tools) installed and [activated](xref:get_started#rdotools-activation).
* [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms), Version 17.0 or later.

## Create the Solution

1. In Visual Studio, create a blank solution *Tutorial*:

# [VS2017](#tab/vs2017)

![image](/images/tutorial_create_solution_vs2017.jpg)

# [VS2019](#tab/vs2019)

![image](/images/tutorial_create_solution_vs2019.jpg)

***
 
2. Add projects into the solution:

In Visual Studio *Solution Explorer* tool window, right click the solution *Tutorial*, then click "*Add...*"-> "*New Project...*" from the context menu, to add the following projects:

| Project Name        | Project Type                           |
|---------------------|----------------------------------------|
| *Movies*            | Class Libary (.Net Standard)           |
| *Movies.DbDesign*   | Console App (.Net Core)                |
| *Movies.Test*       | MSTest Project (.Net Core)             |
| *Movies.WPF*        | WPF App (.Net Framework)               |
| *Movies.AspNetCore* | ASP.Net Core Web Application (C# only) |

>[!NOTE]
>* It's important to name the projects correctly so the namespaces will match when you copy and paste code.
>* It appears Visual Studio does not support VB.Net for ASP.Net Core Web Application. *Movies.AspNetCore* is only available in C#.

In the end, you will have following projects created in Visual Studio:

# [C#](#tab/cs)

![image](/images/tutorial_projects_cs.jpg)

# [VB.Net](#tab/vb)

![image](/images/tutorial_projects_vb.jpg)

***