---
uid: tutorial_movies_test
---

# Data Access and Movies.Test Project

In this section, we will add data access code to *Movies* project and add tests to *Movies.Test* project.

## Add Data Access Code

Add class file *Db.Api.cs* (or *Db.Api.vb* if using VB.Net) to project *Movies*:

# [C#](#tab/cs)

[!code-csharp[DbApi](../../../samples/Tutorial/Movies/Db.Api.cs)]

# [VB.Net](#tab/vb)

[!code-vb[DbApi](../../../samples.vb/Tutorial/Movies/Db.Api.vb)]

***

## Movies.Test Project Setup

Make the following changes to *Movies.Test* project:

*Step 1*. Delete *UnitTest1.cs* (or *UnitTest1.vb*) from project.

*Step 2*. Add project reference *Movies.DbDesign* to this project.

*Step 3*. Change default namespace (root namespace in Visual Basic) from `Movies.Test` to `Movies`, by right clicking the project *Movies.Test* in *Solution Explorer* tool window, then click *Properties* in context menu:

# [C#](#tab/cs)

![image](/images/tutorial_movies_test_default_namespace.jpg)

# [VB.Net](#tab/vb)

![image](/images/tutorial_movies_test_root_namespace.jpg)

***

*Step 4*. Add *EmptyDb.mdf* and *EmptyDb_log.ldf* to this project as linking, by right clicking *Movies.Test* project in *Solution Explorer* tool window, then click context menu item "*Add*" -> "Existing Item...", select *All Files(*.*)* from right bottom combobox, then navigate to *LocalDb subfolder* of *Movies.DbDesign* project, then select file *EmptyDb.mdf* and *EmptyDb_log.ldf*:

![image](/images/tutorial_add_emptydb.jpg)

 Click the little dropdown arrow next to the *Add* button and select *Add as Link*, instead of copying the file into the project directory, Visual Studio will create a link to the original:

![image](/images/tutorial_linked_emptydb.jpg)

Select these two files, then right click and select context menu item *Properties*, make sure *Copy to Output Directory* is set to *Copy Always*:

![image](/images/tutorial_copy_always.jpg)

## Add Tests

Add class *DbTests.cs* (or DbTests.vb if you're using VB.Net) to project *Movies.Test*:

# [C#](#tab/cs)

[!code-csharp[DbTests](../../../samples/Tutorial/Movies.Test/DbTests.cs)]

# [VB.Net](#tab/vb)

[!code-vb[DbTests](../../../samples.vb/Tutorial/Movies.Test/DbTests.vb)]

***

Build and run tests in Visual Studio *Test Explorer*, you should have two tests passed.
