using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;

namespace DevZest.Data.CodeAnalysis
{
    [TestClass]
    public partial class ModelMapperTests
    {
        [TestMethod]
        public void ModelMapper_Refresh_CS()
        {
            var src = string.Empty;
            var document = src.CreateDocument();
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            Assert.IsNull(mapper);

            src =
@"using DevZest.Data;

namespace Test
{
    public class MyModel : Model
    {
    }
}
";

            document = src.CreateDocument();
            mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            Assert.IsNotNull(mapper);
            Assert.AreEqual(document, mapper.Document);
        }

        [TestMethod]
        public void ModelMapper_Refresh_VB()
        {
            var src = string.Empty;
            var document = src.CreateDocument(LanguageNames.VisualBasic);
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            Assert.IsNull(mapper);

            src =
@"Imports DevZest.Data

Namespace Test
    Public Class MyModel
        Inherits Model
    End Class
End Namespace
";

            document = src.CreateDocument(LanguageNames.VisualBasic);
            mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            Assert.IsNotNull(mapper);
            Assert.AreEqual(document, mapper.Document);
        }

        [TestMethod]
        public void ModelMapper_CreatePrimaryKeyEntries_CS()
        {
            var src =
@"using DevZest.Data;

namespace Test
{
    public class MyModel : Model
    {
        public static readonly Mounter<_Int32> _ID = RegisterColumn((MyModel _) => _.ID);

        public _Int32 ID { get; private set; }
    }
}
";

            var document = src.CreateDocument();
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            Assert.IsTrue(mapper.CanAddPrimaryKey);

            var entries = mapper.CreatePrimaryKeyEntries();
            var _ = entries._;
            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("id", _.ConstructorParamName[0]);
            Assert.AreEqual("_ID", _.Mounter[0].Name);
        }

        [TestMethod]
        public void ModelMapper_CreatePrimaryKeyEntries_VB()
        {
            var src =
@"Imports DevZest.Data

Namespace Test
    Public Class MyModel
        Inherits Model

        Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As MyModel) x.ID)

        Private m_ID As _Int32
        Public Property ID As _Int32
            Get
                Return m_ID
            End Get
            Private Set
                m_ID = Value
            End Set
        End Property
    End Class
End Namespace
";

            var document = src.CreateDocument(LanguageNames.VisualBasic);
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            Assert.IsTrue(mapper.CanAddPrimaryKey);

            var entries = mapper.CreatePrimaryKeyEntries();
            var _ = entries._;
            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("id", _.ConstructorParamName[0]);
            Assert.AreEqual("_ID", _.Mounter[0].Name);
        }

        [TestMethod]
        public void ModelMapper_AddPrimaryKey_CS()
        {
            var src =
@"using DevZest.Data;

namespace Test
{
    public class MyModel : Model
    {
        public static readonly Mounter<_Int32> _ID = RegisterColumn((MyModel _) => _.ID);

        public _Int32 ID { get; private set; }
    }
}
";

            var document = src.CreateDocument();
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            Assert.IsTrue(mapper.CanAddPrimaryKey);

            var entries = mapper.CreatePrimaryKeyEntries();
            var _ = entries._;
            document = mapper.AddPrimaryKey("PK", entries, "Key", "Ref");
            var actual = document.GetTextAsync().Result.ToString();
            var expected =
@"using DevZest.Data;

namespace Test
{
    public class MyModel : Model<MyModel.PK>
    {
        public sealed class PK : CandidateKey
        {
            public PK(_Int32 id) : base(id)
            {
            }
        }

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(ID);
        }

        public class Key : Key<PK>
        {
            static Key()
            {
                Register((Key _) => _.ID, _ID);
            }

            protected sealed override PK CreatePrimaryKey()
            {
                return new PK(ID);
            }

            public _Int32 ID { get; private set; }
        }

        public class Ref : Ref<PK>
        {
            static Ref()
            {
                Register((Ref _) => _.ID, _ID);
            }

            protected sealed override PK CreateForeignKey()
            {
                return new PK(ID);
            }

            public _Int32 ID { get; private set; }
        }

        public static readonly Mounter<_Int32> _ID = RegisterColumn((MyModel _) => _.ID);

        public _Int32 ID { get; private set; }
    }
}
";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ModelMapper_AddPrimaryKey_VB()
        {
            var src =
@"Imports DevZest.Data

Namespace Test
    Public Class MyModel
        Inherits Model

        Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As MyModel) x.ID)

        Private m_ID As _Int32
        Public Property ID As _Int32
            Get
                Return m_ID
            End Get
            Private Set
                m_ID = Value
            End Set
        End Property
    End Class
End Namespace
";

            var document = src.CreateDocument(LanguageNames.VisualBasic);
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            Assert.IsTrue(mapper.CanAddPrimaryKey);

            var entries = mapper.CreatePrimaryKeyEntries();
            var _ = entries._;
            document = mapper.AddPrimaryKey("PK", entries, "Key", "Ref");
            var actual = document.GetTextAsync().Result.ToString();
            var expected =
@"Imports DevZest.Data

Namespace Test
    Public Class MyModel
        Inherits Model(Of MyModel.PK)

        Public NotInheritable Class PK
            Inherits CandidateKey

            Public Sub New(id As _Int32)
                MyBase.New(id)
            End Sub
        End Class

        Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
            Return New PK(ID)
        End Function

        Public Class Key
            Inherits Key(Of PK)

            Shared Sub New()
                Register(Function(x As Key) x.ID, _ID)
            End Sub

            Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
                Return New PK(ID)
            End Function

            Private m_ID As _Int32

            Public Property ID As _Int32
                Get
                    Return m_ID
                End Get
                Private Set
                    m_ID = Value
                End Set
            End Property
        End Class

        Public Class Ref
            Inherits Ref(Of PK)

            Shared Sub New()
                Register(Function(x As Ref) x.ID, _ID)
            End Sub

            Protected NotOverridable Overrides Function CreateForeignKey() As PK
                Return New PK(ID)
            End Function

            Private m_ID As _Int32

            Public Property ID As _Int32
                Get
                    Return m_ID
                End Get
                Private Set
                    m_ID = Value
                End Set
            End Property
        End Class

        Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As MyModel) x.ID)

        Private m_ID As _Int32
        Public Property ID As _Int32
            Get
                Return m_ID
            End Get
            Private Set
                m_ID = Value
            End Set
        End Property
    End Class
End Namespace
";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ModelMapper_GetPrimaryKeyEntries_CS()
        {
            var src =
@"using DevZest.Data;

namespace Test
{
    public class MyModel : Model<MyModel.PK>
    {
        public sealed class PK : CandidateKey
        {
            public PK(_Int32 id) : base(id)
            {
            }
        }

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(ID);
        }

        public static readonly Mounter<_Int32> _ID = RegisterColumn((MyModel _) => _.ID);

        public _Int32 ID { get; private set; }
    }
}
";

            var document = src.CreateDocument();
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));

            var entries = mapper.GetPrimaryKeyEntries();
            Assert.AreEqual(1, entries.Count);
            var _ = entries._;
            Assert.AreEqual(mapper.ModelType.GetMembers("ID").Single(), _.Column[0]);
            Assert.AreEqual("id", _.ConstructorParamName[0]);
        }

        [TestMethod]
        public void ModelMapper_GetPrimaryKeyEntries_VB()
        {
            var src =
@"Imports DevZest.Data

Namespace Test
    Public Class MyModel
        Inherits Model(Of PK)

        Public NotInheritable Class PK
            Inherits CandidateKey

            Public Sub New(id As _Int32)
                MyBase.New(id)
            End Sub
        End Class

        Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
            Return New PK(ID)
        End Function

        Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As MyModel) x.ID)

        Private m_ID As _Int32
        Public Property ID As _Int32
            Get
                Return m_ID
            End Get
            Private Set
                m_ID = Value
            End Set
        End Property
    End Class
End Namespace
";

            var document = src.CreateDocument(LanguageNames.VisualBasic);
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));

            var entries = mapper.GetPrimaryKeyEntries();
            Assert.AreEqual(1, entries.Count);
            var _ = entries._;
            Assert.AreEqual(mapper.ModelType.GetMembers("ID").Single(), _.Column[0]);
            Assert.AreEqual("id", _.ConstructorParamName[0]);
        }

        [TestMethod]
        public void ModelMapper_AddKey_CS()
        {
            var src =
@"using DevZest.Data;

namespace Test
{
    public class MyModel : Model<MyModel.PK>
    {
        public sealed class PK : CandidateKey
        {
            public PK(_Int32 id) : base(id)
            {
            }
        }

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(ID);
        }

        public static readonly Mounter<_Int32> _ID = RegisterColumn((MyModel _) => _.ID);

        public _Int32 ID { get; private set; }
    }
}
";

            var document = src.CreateDocument();
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            document = mapper.AddKey("Key", mapper.GetPrimaryKeyEntries());
            var actual = document.GetTextAsync().Result.ToString();
            var expected =
@"using DevZest.Data;

namespace Test
{
    public class MyModel : Model<MyModel.PK>
    {
        public sealed class PK : CandidateKey
        {
            public PK(_Int32 id) : base(id)
            {
            }
        }

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(ID);
        }

        public class Key : Key<PK>
        {
            static Key()
            {
                Register((Key _) => _.ID, _ID);
            }

            protected sealed override PK CreatePrimaryKey()
            {
                return new PK(ID);
            }

            public _Int32 ID { get; private set; }
        }

        public static readonly Mounter<_Int32> _ID = RegisterColumn((MyModel _) => _.ID);

        public _Int32 ID { get; private set; }
    }
}
";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ModelMapper_AddRef_CS()
        {
            var src =
@"using DevZest.Data;

namespace Test
{
    public class MyModel : Model<MyModel.PK>
    {
        public sealed class PK : CandidateKey
        {
            public PK(_Int32 id) : base(id)
            {
            }
        }

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(ID);
        }

        public class Key : Key<PK>
        {
            static Key()
            {
                Register((Key _) => _.ID, _ID);
            }

            protected sealed override PK CreatePrimaryKey()
            {
                return new PK(ID);
            }

            public _Int32 ID { get; private set; }
        }

        public static readonly Mounter<_Int32> _ID = RegisterColumn((MyModel _) => _.ID);

        public _Int32 ID { get; private set; }
    }
}
";

            var document = src.CreateDocument();
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            document = mapper.AddRef("Ref", mapper.GetPrimaryKeyEntries());
            var actual = document.GetTextAsync().Result.ToString();
            var expected =
@"using DevZest.Data;

namespace Test
{
    public class MyModel : Model<MyModel.PK>
    {
        public sealed class PK : CandidateKey
        {
            public PK(_Int32 id) : base(id)
            {
            }
        }

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(ID);
        }

        public class Key : Key<PK>
        {
            static Key()
            {
                Register((Key _) => _.ID, _ID);
            }

            protected sealed override PK CreatePrimaryKey()
            {
                return new PK(ID);
            }

            public _Int32 ID { get; private set; }
        }

        public class Ref : Ref<PK>
        {
            static Ref()
            {
                Register((Ref _) => _.ID, _ID);
            }

            protected sealed override PK CreateForeignKey()
            {
                return new PK(ID);
            }

            public _Int32 ID { get; private set; }
        }

        public static readonly Mounter<_Int32> _ID = RegisterColumn((MyModel _) => _.ID);

        public _Int32 ID { get; private set; }
    }
}
";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ModelMapper_AddKey_VB()
        {
            var src =
@"Imports DevZest.Data

Namespace Test
    Public Class MyModel
        Inherits Model(Of PK)

        Public NotInheritable Class PK
            Inherits CandidateKey

            Public Sub New(id As _Int32)
                MyBase.New(id)
            End Sub
        End Class

        Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
            Return New PK(ID)
        End Function

        Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As MyModel) x.ID)

        Private m_ID As _Int32
        Public Property ID As _Int32
            Get
                Return m_ID
            End Get
            Private Set
                m_ID = Value
            End Set
        End Property
    End Class
End Namespace
";

            var document = src.CreateDocument(LanguageNames.VisualBasic);
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            document = mapper.AddKey("Key", mapper.GetPrimaryKeyEntries());
            var actual = document.GetTextAsync().Result.ToString();
            var expected =
@"Imports DevZest.Data

Namespace Test
    Public Class MyModel
        Inherits Model(Of PK)

        Public NotInheritable Class PK
            Inherits CandidateKey

            Public Sub New(id As _Int32)
                MyBase.New(id)
            End Sub
        End Class

        Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
            Return New PK(ID)
        End Function

        Public Class Key
            Inherits Key(Of PK)

            Shared Sub New()
                Register(Function(x As Key) x.ID, _ID)
            End Sub

            Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
                Return New PK(ID)
            End Function

            Private m_ID As _Int32

            Public Property ID As _Int32
                Get
                    Return m_ID
                End Get
                Private Set
                    m_ID = Value
                End Set
            End Property
        End Class

        Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As MyModel) x.ID)

        Private m_ID As _Int32
        Public Property ID As _Int32
            Get
                Return m_ID
            End Get
            Private Set
                m_ID = Value
            End Set
        End Property
    End Class
End Namespace
";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ModelMapper_AddRef_VB()
        {
            var src =
@"Imports DevZest.Data

Namespace Test
    Public Class MyModel
        Inherits Model(Of PK)

        Public NotInheritable Class PK
            Inherits CandidateKey

            Public Sub New(id As _Int32)
                MyBase.New(id)
            End Sub
        End Class

        Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
            Return New PK(ID)
        End Function

        Public Class Key
            Inherits Key(Of PK)

            Shared Sub New()
                Register(Function(x As Key) x.ID, _ID)
            End Sub

            Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
                Return New PK(ID)
            End Function

            Private m_ID As _Int32

            Public Property ID As _Int32
                Get
                    Return m_ID
                End Get
                Private Set
                    m_ID = Value
                End Set
            End Property
        End Class

        Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As MyModel) x.ID)

        Private m_ID As _Int32
        Public Property ID As _Int32
            Get
                Return m_ID
            End Get
            Private Set
                m_ID = Value
            End Set
        End Property
    End Class
End Namespace
";

            var document = src.CreateDocument(LanguageNames.VisualBasic);
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            document = mapper.AddRef("Ref", mapper.GetPrimaryKeyEntries());
            var actual = document.GetTextAsync().Result.ToString();
            var expected =
@"Imports DevZest.Data

Namespace Test
    Public Class MyModel
        Inherits Model(Of PK)

        Public NotInheritable Class PK
            Inherits CandidateKey

            Public Sub New(id As _Int32)
                MyBase.New(id)
            End Sub
        End Class

        Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
            Return New PK(ID)
        End Function

        Public Class Key
            Inherits Key(Of PK)

            Shared Sub New()
                Register(Function(x As Key) x.ID, _ID)
            End Sub

            Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
                Return New PK(ID)
            End Function

            Private m_ID As _Int32

            Public Property ID As _Int32
                Get
                    Return m_ID
                End Get
                Private Set
                    m_ID = Value
                End Set
            End Property
        End Class

        Public Class Ref
            Inherits Ref(Of PK)

            Shared Sub New()
                Register(Function(x As Ref) x.ID, _ID)
            End Sub

            Protected NotOverridable Overrides Function CreateForeignKey() As PK
                Return New PK(ID)
            End Function

            Private m_ID As _Int32

            Public Property ID As _Int32
                Get
                    Return m_ID
                End Get
                Private Set
                    m_ID = Value
                End Set
            End Property
        End Class

        Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As MyModel) x.ID)

        Private m_ID As _Int32
        Public Property ID As _Int32
            Get
                Return m_ID
            End Get
            Private Set
                m_ID = Value
            End Set
        End Property
    End Class
End Namespace
";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ModelMapper_AddProjection_CS()
        {
            var src =
@"using DevZest.Data;

namespace Test
{
    public class MyModel : Model<MyModel.PK>
    {
        public sealed class PK : CandidateKey
        {
            public PK(_Int32 id) : base(id)
            {
            }
        }

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(ID);
        }

        public static readonly Mounter<_Int32> _ID = RegisterColumn((MyModel _) => _.ID);
        public static readonly Mounter<_String> _Name = RegisterColumn((MyModel _) => _.Name);

        public _Int32 ID { get; private set; }

        public _String Name { get; private set; }
    }
}
";

            var document = src.CreateDocument();
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            var entries = mapper.CreateProjectionEntries();
            entries.RemoveAt(0);
            document = mapper.AddProjection("Lookup", entries);
            var actual = document.GetTextAsync().Result.ToString();
            var expected =
@"using DevZest.Data;

namespace Test
{
    public class MyModel : Model<MyModel.PK>
    {
        public sealed class PK : CandidateKey
        {
            public PK(_Int32 id) : base(id)
            {
            }
        }

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(ID);
        }

        public class Lookup : Projection
        {
            static Lookup()
            {
                Register((Lookup _) => _.Name, _Name);
            }

            public _String Name { get; private set; }
        }

        public static readonly Mounter<_Int32> _ID = RegisterColumn((MyModel _) => _.ID);
        public static readonly Mounter<_String> _Name = RegisterColumn((MyModel _) => _.Name);

        public _Int32 ID { get; private set; }

        public _String Name { get; private set; }
    }
}
";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ModelMapper_AddProjection_VB()
        {
            var src =
@"Imports DevZest.Data

Namespace Test
    Public Class MyModel
        Inherits Model(Of PK)

        Public NotInheritable Class PK
            Inherits CandidateKey

            Public Sub New(id As _Int32)
                MyBase.New(id)
            End Sub
        End Class

        Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
            Return New PK(ID)
        End Function

        Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As MyModel) x.ID)
        Public Shared ReadOnly _Name As Mounter(Of _String) = RegisterColumn(Function(x As MyModel) x.Name)

        Private m_ID As _Int32
        Public Property ID As _Int32
            Get
                Return m_ID
            End Get
            Private Set
                m_ID = Value
            End Set
        End Property

        Private m_Name As _String
        Public Property Name As _String
            Get
                Return m_Name
            End Get
            Private Set
                m_Name = Value
            End Set
        End Property
    End Class
End Namespace
";

            var document = src.CreateDocument(LanguageNames.VisualBasic);
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            var entries = mapper.CreateProjectionEntries();
            entries.RemoveAt(0);
            document = mapper.AddProjection("Lookup", entries);
            var actual = document.GetTextAsync().Result.ToString();
            var expected =
@"Imports DevZest.Data

Namespace Test
    Public Class MyModel
        Inherits Model(Of PK)

        Public NotInheritable Class PK
            Inherits CandidateKey

            Public Sub New(id As _Int32)
                MyBase.New(id)
            End Sub
        End Class

        Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
            Return New PK(ID)
        End Function

        Public Class Lookup
            Inherits Projection

            Shared Sub New()
                Register(Function(x As Lookup) x.Name, _Name)
            End Sub

            Private m_Name As _String

            Public Property Name As _String
                Get
                    Return m_Name
                End Get
                Private Set
                    m_Name = Value
                End Set
            End Property
        End Class

        Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As MyModel) x.ID)
        Public Shared ReadOnly _Name As Mounter(Of _String) = RegisterColumn(Function(x As MyModel) x.Name)

        Private m_ID As _Int32
        Public Property ID As _Int32
            Get
                Return m_ID
            End Get
            Private Set
                m_ID = Value
            End Set
        End Property

        Private m_Name As _String
        Public Property Name As _String
            Get
                Return m_Name
            End Get
            Private Set
                m_Name = Value
            End Set
        End Property
    End Class
End Namespace
";
            Assert.AreEqual(expected, actual);
        }
    }
}
