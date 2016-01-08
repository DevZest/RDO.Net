using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    public sealed class DataSetPresenterBuilder : IDisposable
    {
        internal DataSetPresenterBuilder(DataSetPresenter presenter)
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

        public DataSetPresenterBuilder AddGridColumn(string width, out int index)
        {
            index = Template.AddGridColumn(width);
            return this;
        }

        public DataSetPresenterBuilder AddGridColumns(params string[] widths)
        {
            if (widths == null)
                throw new ArgumentNullException(nameof(widths));

            Template.AddGridColumns(widths);
            return this;
        }

        public DataSetPresenterBuilder AddGridRow(string height, out int index)
        {
            index = Template.AddGridRow(height);
            return this;
        }

        public DataSetPresenterBuilder AddGridRows(params string[] heights)
        {
            if (heights == null)
                throw new ArgumentNullException(nameof(heights));

            Template.AddGridRows(heights);
            return this;
        }

        public DataSetPresenterBuilder WithOrientation(GridOrientation value)
        {
            Template.Orientation = value;
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

        public DataSetPresenterBuilder WithPinnedLeft(int value)
        {
            Template.PinnedLeft = value;
            return this;
        }

        public DataSetPresenterBuilder WithPinnedTop(int value)
        {
            Template.PinnedTop = value;
            return this;
        }

        public DataSetPresenterBuilder WithPinnedRight(int value)
        {
            Template.PinnedRight = value;
            return this;
        }

        public DataSetPresenterBuilder WithPinnedBottom(int value)
        {
            Template.PinnedBottom = value;
            return this;
        }

        public DataSetPresenterBuilder Pin(int left, int top, int right, int bottom)
        {
            Template.PinnedLeft = left;
            Template.PinnedTop = top;
            Template.PinnedRight = right;
            Template.PinnedBottom = bottom;
            return this;
        }

        public DataSetPresenterBuilder WithIsVirtualizing(bool value)
        {
            Presenter.InitIsVirtualizing(value);
            return this;
        }

        public DataSetPresenterBuilder WithEofVisible(bool value)
        {
            Presenter.InitIsEofVisible(value);
            return this;
        }

        public DataSetPresenterBuilder WithEmptySetVisible(bool value)
        {
            Presenter.InitIsEmptySetVisible(value);
            return this;
        }
    }
}
