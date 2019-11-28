using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views;
using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Tools
{
    public static class BindingFactory
    {
        public static RowBinding<TreeItemView> BindToTreeItemView(this ModelMapper.TreeItem _, Func<RowPresenter, string> nameStringFormat)
        {
            return _.BindToTreeItemView(nameStringFormat, GetIcon);
        }

        public static RowBinding<TreeItemView> BindToTreeItemView(this DbMapper.TreeItem _)
        {
            return _.BindToTreeItemView(null, GetIcon);
        }

        private static RowBinding<TreeItemView> BindToTreeItemView<T>(this T _, Func<RowPresenter, string> nameStringFormat, Func<T, RowPresenter, ImageSource> getIcon)
            where T : ITreeItem
        {
            return new RowBinding<TreeItemView>((v, p) =>
            {
                var treeNode = p.GetValue(_.TreeNode);
                v.Refresh(p.Depth, GetExpanderVisibility(p), getIcon(_, p), GetText(p, treeNode, nameStringFormat), treeNode.IsEnabled);
            });
        }

        private static Visibility GetExpanderVisibility(RowPresenter p)
        {
            if (p.Depth == 0)
                return Visibility.Collapsed;
            else if (p.Children.Count > 0)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }

        private static ImageSource GetIcon(ModelMapper.TreeItem _, RowPresenter p)
        {
            return p.GetValue(_.Node).Kind.GetIcon(p.IsExpanded);
        }

        private static ImageSource GetIcon(DbMapper.TreeItem _, RowPresenter p)
        {
            return p.GetValue(_.Node).Kind.GetIcon(p.IsExpanded);
        }

        private static string GetText(RowPresenter p, TreeNode treeNode, Func<RowPresenter, string> nameStringFormat)
        {
            var name = treeNode.Name;
            if (nameStringFormat == null)
                return name;
            var format = nameStringFormat(p);
            if (format != null)
                return string.Format(format, name);

            return treeNode.IsFolder ? string.Format(CultureInfo.InvariantCulture, "{0} ({1})", name, p.Children.Count) : name;
        }

        public static RowBinding<TextBlock> BindToTextBlock(this LocalColumn<IFieldSymbol> mounter)
        {
            return new RowBinding<TextBlock>((v, p) =>
            {
                v.Text = p.GetValue(mounter)?.Name;
            }).WithInput(new ExplicitTrigger<TextBlock>(), mounter, null);
        }

        public static RowBinding<TextBlock> BindToTextBlock(this LocalColumn<IParameterSymbol> param)
        {
            return new RowBinding<TextBlock>((v, p) =>
            {
                v.Text = p.GetValue(param)?.Name;
            }).WithInput(new ExplicitTrigger<TextBlock>(), param, null);
        }

        public static RowBinding<TextBox> BindToTextBox(this LocalColumn<IPropertySymbol> column)
        {
            return new RowBinding<TextBox>((v, p) =>
            {
                v.Text = p.GetValue(column)?.Name;
            }).WithInput(new ExplicitTrigger<TextBox>(), column, null);
        }

        public static RowBinding<TextBox> BindToTextBox(this LocalColumn<IFieldSymbol> mounter)
        {
            return new RowBinding<TextBox>((v, p) =>
            {
                v.Text = p.GetValue(mounter)?.Name;
            }).WithInput(new ExplicitTrigger<TextBox>(), mounter, null);
        }

        public static T AddBinding<T>(this TemplateBuilder<T> builder, MessageView messageView, Scalar<INamedTypeSymbol> resourceType, Scalar<IPropertySymbol> resourceProperty, Scalar<string> message)
            where T : TemplateBuilder<T>
        {
            messageView.AddBinding(builder, resourceType, resourceProperty, message);
            return (T)builder;
        }

        public static RowBinding<RowHeader> BindToRowHeader(this Model source)
        {
            return new RowBinding<RowHeader>(null);
        }
    }
}
