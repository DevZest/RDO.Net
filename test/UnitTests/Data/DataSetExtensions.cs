using DevZest.Data;
using DevZest.Windows.Controls;
using DevZest.Windows.Data.Primitives;
using System;
using System.Windows;

namespace DevZest.Windows.Data
{
    internal static class DataSetExtensions
    {
        private sealed class AutoInitRowView : RowView
        {
            protected override Size MeasureOverride(Size constraint)
            {
                return RowPresenter.LayoutManager.Measure(this, constraint);
            }
        }

        private sealed class AutoInitBlockView : BlockView
        {
            protected override Size MeasureOverride(Size constraint)
            {
                return ScrollableManager.Measure(this, constraint);
            }
        }

        private static T CreateManager<T>(this DataSet dataSet, Action<TemplateBuilder> buildTemplateAction, Func<Template, DataSet, T> createFunc)
            where T : ElementManager
        {
            var template = new Template();
            using (var templateBuilder = new TemplateBuilder(template, dataSet.Model))
            {
                buildTemplateAction(templateBuilder);
                templateBuilder.BlockView<AutoInitBlockView>().RowView<AutoInitRowView>();
            }
            var result = createFunc(template, dataSet);
            result.InitializeElements(null);
            return result;
        }

        private sealed class ConcreteElementManager : ElementManager
        {
            public ConcreteElementManager(Template template, DataSet dataSet, Func<Model, Column<bool?>> where = null,
                Func<Model, ColumnSort[]> orderBy = null, bool emptyBlockViewList = false)
                : base(template, dataSet, where, orderBy, emptyBlockViewList)
            {
            }
        }

        internal static ElementManager CreateElementManager(this DataSet dataSet, Action<TemplateBuilder> buildTemplateAction)
        {
            return dataSet.CreateManager(buildTemplateAction, (t, d) => new ConcreteElementManager(t, d));
        }

        private sealed class ConcreteInputManager : InputManager
        {
            public ConcreteInputManager(Template template, DataSet dataSet, Func<Model, Column<bool?>> where = null,
                Func<Model, ColumnSort[]> orderBy = null, bool emptyBlockViewList = false)
                : base(template, dataSet, where, orderBy, emptyBlockViewList)
            {
            }
        }

        internal static InputManager CreateInputManager(this DataSet dataSet, Action<TemplateBuilder> buildTemplateAction)
        {
            return dataSet.CreateManager(buildTemplateAction, (t, d) => new ConcreteInputManager(t, d));
        }

        internal static LayoutManager CreateLayoutManager(this DataSet dataSet, Action<TemplateBuilder> buildTemplateAction)
        {
            return dataSet.CreateManager(buildTemplateAction, (t, d) => LayoutManager.Create(t, d));
        }
    }
}
