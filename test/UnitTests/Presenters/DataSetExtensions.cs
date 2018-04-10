using DevZest.Data.Views;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;

namespace DevZest.Data.Presenters
{
    internal static class DataSetExtensions
    {
        internal static IReadOnlyList<Column> GetRowMatchColumns(this DataSet dataSet)
        {
            var primaryKey = dataSet.Model.PrimaryKey;
            return primaryKey == null ? null : primaryKey.Select(x => x.Column).ToArray();
        }

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
                templateBuilder.Seal();
            }
            var result = createFunc(template, dataSet);
            result.InitializeElements(null);
            return result;
        }

        private sealed class ConcreteElementManager : ElementManager
        {
            public ConcreteElementManager(Template template, DataSet dataSet, Predicate<DataRow> where = null, IComparer<DataRow> orderBy = null, bool emptyBlockViewList = false)
                : base(null, template, dataSet, dataSet.GetRowMatchColumns(), where, orderBy, emptyBlockViewList)
            {
            }
        }

        internal static ElementManager CreateElementManager(this DataSet dataSet, Action<TemplateBuilder> buildTemplateAction)
        {
            return dataSet.CreateManager(buildTemplateAction, (t, d) => new ConcreteElementManager(t, d));
        }

        internal static ConcreteInputManager CreateInputManager(this DataSet dataSet, Action<TemplateBuilder> buildTemplateAction)
        {
            return dataSet.CreateManager(buildTemplateAction, (t, d) => new ConcreteInputManager(t, d));
        }

        internal static LayoutManager CreateLayoutManager(this DataSet dataSet, Action<TemplateBuilder> buildTemplateAction)
        {
            return dataSet.CreateManager(buildTemplateAction, (t, d) => LayoutManager.Create(null, t, d, d.GetRowMatchColumns()));
        }
    }

    internal sealed class ConcreteInputManager : InputManager
    {
        public ConcreteInputManager(Template template, DataSet dataSet, Predicate<DataRow> where = null, IComparer<DataRow> orderBy = null, bool emptyBlockViewList = false)
            : base(null, template, dataSet, dataSet.GetRowMatchColumns(), where, orderBy, emptyBlockViewList)
        {
        }

        private IReadOnlyList<Scalar> _scalars;
        public ConcreteInputManager WithScalars(IReadOnlyList<Scalar> scalars)
        {
            _scalars = scalars;
            return this;
        }

        Func<IScalarValidationErrors, IScalarValidationErrors> _validator;
        public ConcreteInputManager WithValidator(Func<IScalarValidationErrors, IScalarValidationErrors> validator)
        {
            _validator = validator;
            return this;
        }

        internal override IScalarValidationErrors PerformValidateScalars()
        {
            var result = ScalarValidationErrors.Empty;
            if (_scalars != null)
            {
                for (int i = 0; i < _scalars.Count; i++)
                    result = _scalars[i].Validate(result);
            }
            if (_validator != null)
                result = _validator(result);
            return result;
        }
    }
}
