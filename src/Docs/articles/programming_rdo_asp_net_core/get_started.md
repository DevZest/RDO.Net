# Get Started

Currently RDO.AspNetCore is provided as NuGet package [DevZest.Data.AspNetCore] to support:

* DataSet model binding;
* DataSet validation;
* DataSet tag helpers.

This NuGet package is targeting ASP.Net Core Version 2.2.

## The Startup Class

After adding [DevZest.Data.AspNetCore] reference into your ASP.Net Core 2.2 application, you need to add required services in your `Startup` class, by calling <xref:DevZest.Data.AspNetCore.MvcBuilderExtensions.AddDataSetMvc*> API:

```csharp
public class Startup
{
    ...
    public void ConfigureServices(IServiceCollection services)
    {
        ...
        services.AddMvc()
            .AddDataSetMvc()    // Add DataSet MVC support
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        ...
    }
    ...
}
```

[DevZest.Data.AspNetCore]: https://www.nuget.org/packages/DevZest.Data.AspNetCore/

## The Movies.AspNetCore Sample

The `Movies.AspNetCore` sample, as described in <xref:tutorial_movies_aspnetcore> tutorial, is an ASP.Net Core application that display and manage a database of movies:

![image](/images/tutorial_movies_aspnetcore_run.jpg)

You can follow the step by step instructions in the tutorial to create the sample, or you can open `Tutorial.sln` under `samples\Tutorial` of the source code repo.
