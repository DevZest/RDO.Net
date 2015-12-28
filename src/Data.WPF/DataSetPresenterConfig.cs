using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    public sealed class DataSetPresenterConfig : IDisposable
    {
        internal DataSetPresenterConfig(DataSetPresenter presenter)
        {
            Debug.Assert(presenter != null);
            _presenter = presenter;
        }

        DataSetPresenter _presenter;
        public DataSetPresenter Presenter
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

        public DataSetPresenterConfig AddGridColumn(string width, out int index)
        {
            index = Template.AddGridColumn(width);
            return this;
        }

        public DataSetPresenterConfig AddGridColumns(params string[] widths)
        {
            if (widths == null)
                throw new ArgumentNullException(nameof(widths));

            Template.AddGridColumns(widths);
            return this;
        }

        public DataSetPresenterConfig AddGridRow(string height, out int index)
        {
            index = Template.AddGridRow(height);
            return this;
        }

        public DataSetPresenterConfig AddGridRows(params string[] heights)
        {
            if (heights == null)
                throw new ArgumentNullException(nameof(heights));

            Template.AddGridRows(heights);
            return this;
        }

        public DataSetPresenterConfig WithOrientation(GridOrientation value)
        {
            Template.Orientation = value;
            return this;
        }

        public GridRangeConfig Range()
        {
            return new GridRangeConfig(this, Presenter.Template.Range());
        }

        public GridRangeConfig Range(int column, int row)
        {
            return new GridRangeConfig(this, Presenter.Template.Range(column, row));
        }

        public GridRangeConfig Range(int left, int top, int right, int bottom)
        {
            return new GridRangeConfig(this, Presenter.Template.Range(left, top, right, bottom));
        }

        public DataSetPresenterConfig WithPinnedLeft(int value)
        {
            Template.PinnedLeft = value;
            return this;
        }

        public DataSetPresenterConfig WithPinnedTop(int value)
        {
            Template.PinnedTop = value;
            return this;
        }

        public DataSetPresenterConfig WithPinnedRight(int value)
        {
            Template.PinnedRight = value;
            return this;
        }

        public DataSetPresenterConfig WithPinnedBottom(int value)
        {
            Template.PinnedBottom = value;
            return this;
        }

        public DataSetPresenterConfig Pin(int left, int top, int right, int bottom)
        {
            Template.PinnedLeft = left;
            Template.PinnedTop = top;
            Template.PinnedRight = right;
            Template.PinnedBottom = bottom;
            return this;
        }

        public DataSetPresenterConfig WithIsVirtualizing(bool value)
        {
            Presenter.IsVirtualizing = value;
            return this;
        }

        public DataSetPresenterConfig WithShowsEof(bool value)
        {
            Presenter.ShowsEof = value;
            return this;
        }

        public DataSetPresenterConfig WithShowsEmptyDataRow(bool value)
        {
            Presenter.ShowsEmptyDataRow = value;
            return this;
        }

    }
}
