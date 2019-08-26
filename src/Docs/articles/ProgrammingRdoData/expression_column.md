---
uid: expression_column
---

# Expression Column

Expression column is a <xref:DevZest.Data.Column`1> derived object with its <xref:DevZest.Data.Column`1.Expression> property returns non null value and its <xref:DevZest.Data.Column`1.IsExpression> returns true. The <xref:DevZest.Data.Column`1.Expression> property, of type <xref:DevZest.Data.Primitives.ColumnExpression`1>, contains an expression tree for an expression column. This expression tree can be evaluated locally, or translated to native SQL expression by database providers.

Concrete column class normally provide members such as static methods, operator overloads and type converters so that you can use to construct expression column. For example, <xref:DevZest.Data._Int32> provides following members:

* Static methods: <xref:DevZest.Data._Int32.Param*> and <xref:DevZest.Data._Int32.Const*>;
* Binary operator overrides: `+`, `-`, `*`, `/`, `%`, `&`, `|`, `^`, `==`, `!=`, `>`, `<`, `>=`, `<=`
* Unary operator overrides: `-`(negates) and `~`(OnesComplement);
* Type conversion, either implicitly or explicitly, to/from other types.

You can write expression using columns in a similar way of using their underlying data types. For example, the code `UnitPrice * (_Decimal.Const(1) - UnitPriceDiscount) * OrderQty).IfNull(_Decimal.Const(0))` produce a expression column to calculate order subtotal as `UnitPrice * (1 - UnitPriceDiscount) * OrderQty`.

Pay attention when writing expression column:

* Understand the difference between value expression created by `Param` (implicit conversion) and `Const` (explicit conversion), which will create <xref:DevZest.Data.Primitives.ParamExpression`1> and <xref:DevZest.Data.Primitives.ConstantExpression`1> respectively. Be aware of possible **SQL injection** if you're using `Const` expression when the value is provided by user.
* Since logical operator override is only allowed by boolean types, <xref:DevZest.Data._Boolean> column type overrides '&' and '|' instead of '&&' and '||'.
* Most column types override '==' and '!=' operator to compare underlying values. If you want to compare two column object reference, use `Object.ReferenceEquals` method instead.
