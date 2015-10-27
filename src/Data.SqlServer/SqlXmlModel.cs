
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

        internal void Initialize(_SqlXml source, string xPath)
        {
            Source = source;
            XPath = xPath;
        }

        public _SqlXml Xml { get; private set; }

        internal _SqlXml Source { get; private set; }

        internal string XPath { get; private set; }
    }
}
