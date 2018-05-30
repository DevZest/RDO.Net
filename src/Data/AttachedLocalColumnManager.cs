using DevZest.Data;
using System;
using System.Runtime.CompilerServices;

namespace DevZest.Samples.AdventureWorksLT
{
    public abstract class AttachedLocalColumnManager<TModel, TDataType>
        where TModel : Model
    {
        private ConditionalWeakTable<TModel, EventHandler<EventArgs>> _modelInitializers = new ConditionalWeakTable<TModel, EventHandler<EventArgs>>(); 
        private ConditionalWeakTable<TModel, Column<TDataType>> _modelColumns = new ConditionalWeakTable<TModel, Column<TDataType>>();

        public Column<TDataType> GetAttachedColumn(TModel _)
        {
            Column<TDataType> result;
            if (_modelColumns.TryGetValue(_, out result))
                return result;
            else
                return null;
        }

        private void Initialize(TModel _)
        {
            _.VerifyNotNull(nameof(_));
            if (_.IsInitialized)
                throw new InvalidOperationException(DiagnosticMessages.VerifyDesignMode);

            EventHandler<EventArgs> eventHandler;
            if (_modelInitializers.TryGetValue(_, out eventHandler))
                return;
            else
                eventHandler = (sender, e) =>
                {
                    _.Initializing -= eventHandler;
                    var dataSetContainer = _.DataSetContainer;
                    if (dataSetContainer != null)
                        _modelColumns.Add(_, CreateLocalColumn(dataSetContainer, _));
                };
            _.Initializing += eventHandler;
        }

        public Action<TModel> Initializer
        {
            get { return Initialize; }
        }

        protected abstract Column<TDataType> CreateLocalColumn(DataSetContainer dataSetContainer, TModel _);
    }
}
