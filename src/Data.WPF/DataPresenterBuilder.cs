using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    public sealed class DataPresenterBuilder : IDisposable
    {
        internal DataPresenterBuilder(DataPresenter presenter)
        {
            Debug.Assert(presenter != null);
            _presenter = presenter;
        }

        DataPresenter _presenter;
        public DataPresenter Presenter
        {
            get
            {
                if (_presenter == null)
                    throw new ObjectDisposedException(GetType().FullName);

                return _presenter;
            }
        }

        public void Dispose()
        {
            _presenter = null;
        }

        public GridTemplate Template
        {
            get { return Presenter.Template; }
        }

        public DataPresenterBuilder AddGridColumn(string width, out int index)
        {
            index = Template.AddGridColumn(width);
            return this;
        }

        public DataPresenterBuilder AddGridColumns(params string[] widths)
        {
            if (widths == null)
                throw new ArgumentNullException(nameof(widths));

            Template.AddGridColumns(widths);
            return this;
        }

        public DataPresenterBuilder AddGridRow(string height, out int index)
        {
            index = Template.AddGridRow(height);
            return this;
        }

        public DataPresenterBuilder AddGridRows(params string[] heights)
        {
            if (heights == null)
                throw new ArgumentNullException(nameof(heights));

            Template.AddGridRows(heights);
            return this;
        }

        public DataPresenterBuilder WithOrientation(RepeatOrientation value)
        {
            Template.RepeatOrientation = value;
            return this;
        }

        public GridRangeConfig Range(int column, int row)
        {
            return new GridRangeConfig(this, Presenter.Template.Range(column, row));
        }

        public GridRangeConfig Range(int left, int top, int right, int bottom)
        {
            return new GridRangeConfig(this, Presenter.Template.Range(left, top, right, bottom));
        }

        public DataPresenterBuilder WithPinnedLeft(int value)
        {
            Template.PinnedLeft = value;
            return this;
        }

        public DataPresenterBuilder WithPinnedTop(int value)
        {
            Template.PinnedTop = value;
            return this;
        }

        public DataPresenterBuilder WithPinnedRight(int value)
        {
            Template.PinnedRight = value;
            return this;
        }

        public DataPresenterBuilder WithPinnedBottom(int value)
        {
            Template.PinnedBottom = value;
            return this;
        }

        public DataPresenterBuilder Pin(int left, int top, int right, int bottom)
        {
            Template.PinnedLeft = left;
            Template.PinnedTop = top;
            Template.PinnedRight = right;
            Template.PinnedBottom = bottom;
            return this;
        }

        public DataPresenterBuilder WithVirtualizationThreshold(int value)
        {
            Presenter.InitVirtualizationThreshold(value);
            return this;
        }

        public DataPresenterBuilder WithEofVisible(bool value)
        {
            Presenter.InitIsEofVisible(value);
            return this;
        }

        public DataPresenterBuilder WithEmptySetVisible(bool value)
        {
            Presenter.InitIsEmptySetVisible(value);
            return this;
        }

        public DataPresenterBuilder WithRowViewConstructor(Func<RowView> rowViewConstructor)
        {
            if (rowViewConstructor == null)
                throw new ArgumentNullException(nameof(rowViewConstructor));

            Presenter.LayoutManager.InitRowViewConstructor(rowViewConstructor);
            return this;
        }
    }
}
