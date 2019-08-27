# DataSet

The <xref:DevZest.Data.DataSet`1> object is a memory-resident representation of data. You can use it independently as your local data store, or you can use it seamlessly with <xref:DevZest.Data.DbSet`1> and JSON.

## Create DataSet

DataSet can be created in a couple of ways:

| API | Description |
|-----|-------------|
| [DataSet\<T\>.Create](xref:DevZest.Data.DataSet`1.Create*) | Create an empty DataSet. |
| [DataSet\<T\>.ParseJson](xref:DevZest.Data.DataSet`1.ParseJson*) | Create DataSet from JSON. |
| [DbSet\<T\>.ToDataSetAsync](xref:DevZest.Data.DbSet`1.ToDataSetAsync*) | Create DataSet from <xref:DevZest.Data.DbTable`1> or <xref:DevZest.Data.DbQuery`1>. |

## Serialize/Deserialize DataSet

DataSet can be serialized/deserialzed to/from JSON:

| API | Description |
|-----|-------------|
| [DataSet.ToJsonString](xref:DevZest.Data.DataSet.ToJsonString*) | Serialize DataSet into JSON. |
| [DataSet\<T\>.ParseJson](xref:DevZest.Data.DataSet`1.ParseJson*) | Deserialize JSON into DataSet. |

You can provide your own <xref:DevZest.Data.IJsonCustomizer> to customize JSON serialization/deserialization.

## Access Data Values

You can think of <xref:DevZest.Data.DataSet`1> as two-dimensional array for simplicity: it consists of a collection of <xref:DevZest.Data.DataRow> objects horizontally, and collection of <xref:DevZest.Data.Column> objects via [Model.Columns](xref:DevZest.Data.Model.Columns) vertically. Data values are stored in strongly typed <xref:DevZest.Data.Column`1> objects to avoid boxing and un-boxing.

You can access data values via indexer of <xref:DevZest.Data.Column`1> class:

# [C#](#tab/cs)

```cs
DataSet<ProductCategory> result = DataSet<ProductCategory>.Create().AddRows(13);
ProductCategory _ = result._;
_.SuspendIdentity();
_.ProductCategoryID[0] = 1;
_.ProductCategoryID[1] = 2;
_.ProductCategoryID[2] = 3;
_.ProductCategoryID[3] = 4;
_.ProductCategoryID[4] = 5;
_.ProductCategoryID[5] = 6;
_.ProductCategoryID[6] = 7;
_.ProductCategoryID[7] = 8;
_.ProductCategoryID[8] = 9;
_.ProductCategoryID[9] = 10;
_.ProductCategoryID[10] = 11;
_.ProductCategoryID[11] = 12;
_.ProductCategoryID[12] = 13;
_.ParentProductCategoryID[0] = null;
_.ParentProductCategoryID[1] = null;
_.ParentProductCategoryID[2] = 1;
_.ParentProductCategoryID[3] = 1;
_.ParentProductCategoryID[4] = 1;
_.ParentProductCategoryID[5] = 1;
_.ParentProductCategoryID[6] = 2;
_.ParentProductCategoryID[7] = 6;
_.ParentProductCategoryID[8] = 6;
_.ParentProductCategoryID[9] = 6;
_.ParentProductCategoryID[10] = 7;
_.ParentProductCategoryID[11] = 7;
_.ParentProductCategoryID[12] = 7;
_.Name[0] = "Bikes";
_.Name[1] = "Other";
_.Name[2] = "Mountain Bikes";
_.Name[3] = "Road Bikes";
_.Name[4] = "Touring Bikes";
_.Name[5] = "Components";
_.Name[6] = "Clothing";
_.Name[7] = "Handlebars";
_.Name[8] = "Bottom Brackets";
_.Name[9] = "Brakes";
_.Name[10] = "Bib-Shorts";
_.Name[11] = "Caps";
_.Name[12] = "Gloves";
_.RowGuid[0] = new Guid("cfbda25c-df71-47a7-b81b-64ee161aa37c");
_.RowGuid[1] = new Guid("09e91437-ba4f-4b1a-8215-74184fd95db8");
_.RowGuid[2] = new Guid("2d364ade-264a-433c-b092-4fcbf3804e01");
_.RowGuid[3] = new Guid("000310c0-bcc8-42c4-b0c3-45ae611af06b");
_.RowGuid[4] = new Guid("02c5061d-ecdc-4274-b5f1-e91d76bc3f37");
_.RowGuid[5] = new Guid("c657828d-d808-4aba-91a3-af2ce02300e9");
_.RowGuid[6] = new Guid("10a7c342-ca82-48d4-8a38-46a2eb089b74");
_.RowGuid[7] = new Guid("3ef2c725-7135-4c85-9ae6-ae9a3bdd9283");
_.RowGuid[8] = new Guid("a9e54089-8a1e-4cf5-8646-e3801f685934");
_.RowGuid[9] = new Guid("d43ba4a3-ef0d-426b-90eb-4be4547dd30c");
_.RowGuid[10] = new Guid("67b58d2b-5798-4a90-8c6c-5ddacf057171");
_.RowGuid[11] = new Guid("430dd6a8-a755-4b23-bb05-52520107da5f");
_.RowGuid[12] = new Guid("92d5657b-0032-4e49-bad5-41a441a70942");
_.ModifiedDate[0] = Convert.ToDateTime("2002-06-01T00:00:00.000");
_.ModifiedDate[1] = Convert.ToDateTime("2002-06-01T00:00:00.000");
_.ModifiedDate[2] = Convert.ToDateTime("2002-06-01T00:00:00.000");
_.ModifiedDate[3] = Convert.ToDateTime("2002-06-01T00:00:00.000");
_.ModifiedDate[4] = Convert.ToDateTime("2002-06-01T00:00:00.000");
_.ModifiedDate[5] = Convert.ToDateTime("2002-06-01T00:00:00.000");
_.ModifiedDate[6] = Convert.ToDateTime("2002-06-01T00:00:00.000");
_.ModifiedDate[7] = Convert.ToDateTime("2002-06-01T00:00:00.000");
_.ModifiedDate[8] = Convert.ToDateTime("2002-06-01T00:00:00.000");
_.ModifiedDate[9] = Convert.ToDateTime("2002-06-01T00:00:00.000");
_.ModifiedDate[10] = Convert.ToDateTime("2002-06-01T00:00:00.000");
_.ModifiedDate[11] = Convert.ToDateTime("2002-06-01T00:00:00.000");
_.ModifiedDate[12] = Convert.ToDateTime("2002-06-01T00:00:00.000");
_.ResumeIdentity();
```

# [VB.Net](#tab/vb)

```vb
Dim result As DataSet(Of ProductCategory) = DataSet(Of ProductCategory).Create().AddRows(13)
Dim x As ProductCategory = result.Entity
x.SuspendIdentity()
x.ProductCategoryID(0) = 1
x.ProductCategoryID(1) = 2
x.ProductCategoryID(2) = 3
x.ProductCategoryID(3) = 4
x.ProductCategoryID(4) = 5
x.ProductCategoryID(5) = 6
x.ProductCategoryID(6) = 7
x.ProductCategoryID(7) = 8
x.ProductCategoryID(8) = 9
x.ProductCategoryID(9) = 10
x.ProductCategoryID(10) = 11
x.ProductCategoryID(11) = 12
x.ProductCategoryID(12) = 13
x.ParentProductCategoryID(0) = Nothing
x.ParentProductCategoryID(1) = Nothing
x.ParentProductCategoryID(2) = 1
x.ParentProductCategoryID(3) = 1
x.ParentProductCategoryID(4) = 1
x.ParentProductCategoryID(5) = 1
x.ParentProductCategoryID(6) = 2
x.ParentProductCategoryID(7) = 6
x.ParentProductCategoryID(8) = 6
x.ParentProductCategoryID(9) = 6
x.ParentProductCategoryID(10) = 7
x.ParentProductCategoryID(11) = 7
x.ParentProductCategoryID(12) = 7
x.Name(0) = "Bikes"
x.Name(1) = "Other"
x.Name(2) = "Mountain Bikes"
x.Name(3) = "Road Bikes"
x.Name(4) = "Touring Bikes"
x.Name(5) = "Components"
x.Name(6) = "Clothing"
x.Name(7) = "Handlebars"
x.Name(8) = "Bottom Brackets"
x.Name(9) = "Brakes"
x.Name(10) = "Bib-Shorts"
x.Name(11) = "Caps"
x.Name(12) = "Gloves"
x.RowGuid(0) = New Guid("cfbda25c-df71-47a7-b81b-64ee161aa37c")
x.RowGuid(1) = New Guid("09e91437-ba4f-4b1a-8215-74184fd95db8")
x.RowGuid(2) = New Guid("2d364ade-264a-433c-b092-4fcbf3804e01")
x.RowGuid(3) = New Guid("000310c0-bcc8-42c4-b0c3-45ae611af06b")
x.RowGuid(4) = New Guid("02c5061d-ecdc-4274-b5f1-e91d76bc3f37")
x.RowGuid(5) = New Guid("c657828d-d808-4aba-91a3-af2ce02300e9")
x.RowGuid(6) = New Guid("10a7c342-ca82-48d4-8a38-46a2eb089b74")
x.RowGuid(7) = New Guid("3ef2c725-7135-4c85-9ae6-ae9a3bdd9283")
x.RowGuid(8) = New Guid("a9e54089-8a1e-4cf5-8646-e3801f685934")
x.RowGuid(9) = New Guid("d43ba4a3-ef0d-426b-90eb-4be4547dd30c")
x.RowGuid(10) = New Guid("67b58d2b-5798-4a90-8c6c-5ddacf057171")
x.RowGuid(11) = New Guid("430dd6a8-a755-4b23-bb05-52520107da5f")
x.RowGuid(12) = New Guid("92d5657b-0032-4e49-bad5-41a441a70942")
x.ModifiedDate(0) = Convert.ToDateTime("2002-06-01T00:00:00.000")
x.ModifiedDate(1) = Convert.ToDateTime("2002-06-01T00:00:00.000")
x.ModifiedDate(2) = Convert.ToDateTime("2002-06-01T00:00:00.000")
x.ModifiedDate(3) = Convert.ToDateTime("2002-06-01T00:00:00.000")
x.ModifiedDate(4) = Convert.ToDateTime("2002-06-01T00:00:00.000")
x.ModifiedDate(5) = Convert.ToDateTime("2002-06-01T00:00:00.000")
x.ModifiedDate(6) = Convert.ToDateTime("2002-06-01T00:00:00.000")
x.ModifiedDate(7) = Convert.ToDateTime("2002-06-01T00:00:00.000")
x.ModifiedDate(8) = Convert.ToDateTime("2002-06-01T00:00:00.000")
x.ModifiedDate(9) = Convert.ToDateTime("2002-06-01T00:00:00.000")
x.ModifiedDate(10) = Convert.ToDateTime("2002-06-01T00:00:00.000")
x.ModifiedDate(11) = Convert.ToDateTime("2002-06-01T00:00:00.000")
x.ModifiedDate(12) = Convert.ToDateTime("2002-06-01T00:00:00.000")
x.ResumeIdentity()
```

***

You can also use <xref:DevZest.Data.DataRow> object as indexer parameter.
