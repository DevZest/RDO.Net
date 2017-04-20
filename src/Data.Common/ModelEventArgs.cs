using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public class ModelEventArgs : EventArgs
    {
        public ModelEventArgs(Model model)
        {
            Check.NotNull(model, nameof(model));
            _model = model;
        }

        private readonly Model _model;
        public Model Model
        {
            get { return _model; }
        }
    }
}
