# DataSet Model Binding

RDO.AspNetCore supports model binding to DataSet. It registers a default <xref:DevZest.Data.AspNetCore.Primitives.DataSetModelBinder`1> to perform model binding operations, via <xref:DevZest.Data.AspNetCore.Primitives.DataSetModelBinderProvider>.

## Scalar DataSet

Normally the DataSet represents a collection of data rows. By default the model binder expects collection of input data item with an index for each of them. For example, here’s an example of a form that submits three movies:

```html
<form method="post" action="/Home/Create">

    <input type="text" name="[0].ID" value="1" />
    <input type="text" name="[0].Title" value="When Harry Met Sally" />
    <input type="text" name="[0].ReleaseDate" value="1989-2-12" />
    <input type="text" name="[0].Genre" value="Romantic Comedy" />

    <input type="text" name="[1].ID" value="2" />
    <input type="text" name="[1].Title" value=""Ghostbusters" />
    <input type="text" name="[1].ReleaseDate" value="1984-3-13" />
    <input type="text" name="[1].Genre" value="Comedy" />

    <input type="text" name="[2].ID" value="3" />
    <input type="text" name="[2].Title" value=""Ghostbusters2" />
    <input type="text" name="[2].ReleaseDate" value="1986-2-23" />
    <input type="text" name="[2].Genre" value="Comedy" />

    <input type="submit" />
</form>
```

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

Then the form that submits single movie will be:

```html
<form method="post" action="/Home/Create">

    <input type="text" name="ID" value="1" />
    <input type="text" name="Title" value="When Harry Met Sally" />
    <input type="text" name="ReleaseDate" value="1989-2-12" />
    <input type="text" name="Genre" value="Romantic Comedy" />

    <input type="submit" />
</form>
```

## Custom DataSet Model Binder

You can also write your own custom DataSet model binder class derived from the default <xref:DevZest.Data.AspNetCore.Primitives.DataSetModelBinder`1>. Particularly, you can provide your own <xref:DevZest.Data.AspNetCore.IColumnValueConverter> implementation, and wire it with the column in your [DataSetModelBinder\<T\>.CreateDataSet](xref:DevZest.Data.AspNetCore.Primitives.DataSetModelBinder`1.CreateDataSet*) override.

Sorry, no example provided here.
