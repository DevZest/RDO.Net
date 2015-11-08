
using DevZest.Data.Primitives;
using System.Data.SqlTypes;

namespace DevZest.Data.SqlServer
{
    public sealed class SqlXmlModel : Model
    {
        static SqlXmlModel()
        {
            RegisterColumn((SqlXmlModel x) => x.Xml);
        }

        public SqlXmlModel()
        {
        }

        internal void Initialize(SqlXml sourceData, string xPath)
        {
            SourceData = _SqlXml.Param(sourceData).DbExpression;
            XPath = xPath;
        }

        public _SqlXml Xml { get; private set; }

        internal DbExpression SourceData { get; private set; }

        internal string XPath { get; private set; }
    }
}
