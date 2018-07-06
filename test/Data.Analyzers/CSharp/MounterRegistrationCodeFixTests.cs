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
    }
}
