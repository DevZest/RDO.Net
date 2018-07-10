using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Analyzers.CSharp
{
    [TestClass]
    public class MounterRegistrationCodeFixTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MounterRegistrationAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new MounterRegistrationCodeFixProvider();
        }

        [TestMethod]
        public void RegisterLocalColumn_no_static_constructor()
        {
            var test = @"
using DevZest.Data;

class SimpleModel : Model
{
    public LocalColumn<int> Column1 { get; private set; }
}";
            var expected = @"
using DevZest.Data;

class SimpleModel : Model
{
    static SimpleModel()
    {
        RegisterLocalColumn((SimpleModel _) => _.Column1);
    }

    public LocalColumn<int> Column1 { get; private set; }
}";
            VerifyCSharpFix(test, expected);
        }

        [TestMethod]
        public void RegisterLocalColumn_empty_static_constructor()
        {
            var test = @"
using DevZest.Data;

class SimpleModel : Model
{
    static SimpleModel()
    {
    }

    public LocalColumn<int> Column1 { get; private set; }
}";
            var expected = @"
using DevZest.Data;

class SimpleModel : Model
{
    static SimpleModel()
    {
        RegisterLocalColumn((SimpleModel _) => _.Column1);
    }

    public LocalColumn<int> Column1 { get; private set; }
}";
            VerifyCSharpFix(test, expected);
        }

        [TestMethod]
        public void RegisterColumn()
        {
            var test = @"
using DevZest.Data;

class SimpleModel : Model
{
    public _Int32 Column1 { get; private set; }
}";
            var expected = @"
using DevZest.Data;

class SimpleModel : Model
{
    protected static readonly Mounter<_Int32> _Column1 = RegisterColumn((SimpleModel _) => _.Column1);

    public _Int32 Column1 { get; private set; }
}";
            VerifyCSharpFix(test, expected);
        }

        [TestMethod]
        public void RegisterColumn_existing_mounter()
        {
            var test = @"
using DevZest.Data;

class SimpleModel : Model
{
    protected static readonly Mounter<_Int32> _Column1 = RegisterColumn((SimpleModel _) => _.Column1);

    public _Int32 Column1 { get; private set; }

    public _Int32 Column2 { get; private set; }
}";

            var expected = @"
using DevZest.Data;

class SimpleModel : Model
{
    protected static readonly Mounter<_Int32> _Column1 = RegisterColumn((SimpleModel _) => _.Column1);
    protected static readonly Mounter<_Int32> _Column2 = RegisterColumn((SimpleModel _) => _.Column2);

    public _Int32 Column1 { get; private set; }

    public _Int32 Column2 { get; private set; }
}";
            VerifyCSharpFix(test, expected);
        }

        [TestMethod]
        public void RegisterColumn_abstract_generic()
        {
            var test = @"
using DevZest.Data;

abstract class SimpleModelBase<T> : Model<T>
    where T : PrimaryKey
{
    public _Int32 Column1 { get; private set; }
}";
            var expected = @"
using DevZest.Data;

abstract class SimpleModelBase<T> : Model<T>
    where T : PrimaryKey
{
    protected static readonly Mounter<_Int32> _Column1 = RegisterColumn((SimpleModelBase<T> _) => _.Column1);

    public _Int32 Column1 { get; private set; }
}";
            VerifyCSharpFix(test, expected);
        }

        [TestMethod]
        public void RegisterChildModel_abstract_generic_parent_model()
        {
            var test = @"
using DevZest.Data;

class ChildModel : Model
{
}

abstract class SimpleModelBase<T> : Model<T>
    where T : PrimaryKey
{
    public ChildModel Child { get; private set; }
}";
            var expected = @"
using DevZest.Data;

class ChildModel : Model
{
}

abstract class SimpleModelBase<T> : Model<T>
    where T : PrimaryKey
{
    static SimpleModelBase()
    {
        RegisterChildModel((SimpleModelBase<T> _) => _.Child);
    }

    public ChildModel Child { get; private set; }
}";
            VerifyCSharpFix(test, expected);
        }

        [TestMethod]
        public void RegisterChildModel_recursive_child()
        {
            var test = @"
using DevZest.Data;

public class RecursiveModel : Model<RecursiveModel.PK>
{
    public sealed class PK : PrimaryKey
    {
        public PK(_Int32 id)
            : base(id)
        {
        }

        public _Int32 ID
        {
            get { return GetColumn<_Int32>(0); }
        }
    }

    protected sealed override PK CreatePrimaryKey()
    {
        return new PK(ID);
    }

    public static readonly Mounter<_Int32> _ID = RegisterColumn((RecursiveModel _) => _.ID);
    public static readonly Mounter<_Int32> _ParentID = RegisterColumn((RecursiveModel _) => _.ParentID);

    public _Int32 ID { get; private set; }

    public _Int32 ParentID { get; private set; }

    public RecursiveModel Child { get; private set; }

    private PK _fk_Parent;
    public PK FK_Parent
    {
        get { return _fk_Parent ?? (_fk_Parent = new PK(ParentID)); }
    }
}";
            var expected = @"
using DevZest.Data;

public class RecursiveModel : Model<RecursiveModel.PK>
{
    public sealed class PK : PrimaryKey
    {
        public PK(_Int32 id)
            : base(id)
        {
        }

        public _Int32 ID
        {
            get { return GetColumn<_Int32>(0); }
        }
    }

    protected sealed override PK CreatePrimaryKey()
    {
        return new PK(ID);
    }

    public static readonly Mounter<_Int32> _ID = RegisterColumn((RecursiveModel _) => _.ID);
    public static readonly Mounter<_Int32> _ParentID = RegisterColumn((RecursiveModel _) => _.ParentID);

    static RecursiveModel()
    {
        RegisterChildModel((RecursiveModel _) => _.Child, (_) => _.FK_Parent);
    }

    public _Int32 ID { get; private set; }

    public _Int32 ParentID { get; private set; }

    public RecursiveModel Child { get; private set; }

    private PK _fk_Parent;
    public PK FK_Parent
    {
        get { return _fk_Parent ?? (_fk_Parent = new PK(ParentID)); }
    }
}";
            VerifyCSharpFix(test, expected);
        }
    }
}
