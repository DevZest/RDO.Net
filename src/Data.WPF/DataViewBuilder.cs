using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    public sealed class DataViewBuilder : IDisposable
    {
        internal DataViewBuilder(DataView view)
        {
            Debug.Assert(view != null);
            _view = view;
        }

        DataView _view;
        public DataView View
        {
            get
            {
                if (_view == null)
                    throw new ObjectDisposedException(GetType().FullName);

                return _view;
            }
        }

        public void Dispose()
        {
            _view = null;
        }

        public GridTemplate Template
        {
            get { return View.Template; }
        }

        public DataViewBuilder AddGridColumn(string width, out int index)
        {
            index = Template.AddGridColumn(width);
            return this;
        }

        public DataViewBuilder AddGridColumns(params string[] widths)
        {
            if (widths == null)
                throw new ArgumentNullException(nameof(widths));

            Template.AddGridColumns(widths);
            return this;
        }

        public DataViewBuilder AddGridRow(string height, out int index)
        {
            index = Template.AddGridRow(height);
            return this;
        }

        public DataViewBuilder AddGridRows(params string[] heights)
        {
            if (heights == null)
                throw new ArgumentNullException(nameof(heights));

            Template.AddGridRows(heights);
            return this;
        }

        public DataViewBuilder WithOrientation(ListOrientation value)
        {
            Template.ListOrientation = value;
            return this;
        }

        public GridRangeConfig Range(int column, int row)
        {
            return new GridRangeConfig(this, View.Template.Range(column, row));
        }

        public GridRangeConfig Range(int left, int top, int right, int bottom)
        {
            return new GridRangeConfig(this, View.Template.Range(left, top, right, bottom));
        }

        public DataViewBuilder WithPinnedLeft(int value)
        {
            Template.PinnedLeft = value;
            return this;
        }

        public DataViewBuilder WithPinnedTop(int value)
        {
            Template.PinnedTop = value;
            return this;
        }

        public DataViewBuilder WithPinnedRight(int value)
        {
            Template.PinnedRight = value;
            return this;
        }

        public DataViewBuilder WithPinnedBottom(int value)
        {
            Template.PinnedBottom = value;
            return this;
        }

        public DataViewBuilder Pin(int left, int top, int right, int bottom)
        {
            Template.PinnedLeft = left;
            Template.PinnedTop = top;
            Template.PinnedRight = right;
            Template.PinnedBottom = bottom;
            return this;
        }

        public DataViewBuilder WithIsVirtualizing(bool value)
        {
            View.InitIsVirtualizing(value);
            return this;
        }

        public DataViewBuilder WithEofVisible(bool value)
        {
            View.InitIsEofVisible(value);
            return this;
        }

        public DataViewBuilder WithEmptySetVisible(bool value)
        {
            View.InitIsEmptySetVisible(value);
            return this;
        }
    }
}
