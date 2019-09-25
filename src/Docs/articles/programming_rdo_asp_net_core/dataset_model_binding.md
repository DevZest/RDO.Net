# DataSet Model Binding

If the DataSet in your model is scalar (single-item), you can decorate it with <xref:DevZest.Data.AspNetCore.ScalarAttribute>, as demonstrated in the Movies.AspNetCore sample:

```csharp
public class EditModel : PageModel
{
    ...
    [BindProperty]
    [Scalar]
    public DataSet<Movie> Movie { get; set; }
    ...
}
```

