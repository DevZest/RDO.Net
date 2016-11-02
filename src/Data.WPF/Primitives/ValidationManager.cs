namespace DevZest.Data.Windows.Primitives
{
    internal abstract class ValidationManager : ElementManager
    {
        protected ValidationManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy, bool emptyBlockViewList)
            : base(template, dataSet, where, orderBy, emptyBlockViewList)
        {
        }
    }
}
