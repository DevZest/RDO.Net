using DevZest.Data.CodeAnalysis;
using System;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DevZest.Data.Tools
{
    public static class Icons
    {
        public static readonly BitmapImage Model = LoadImage(nameof(Model));
        public static readonly BitmapImage PrimaryKey = LoadImage(nameof(PrimaryKey));
        public static readonly BitmapImage Key = LoadImage(nameof(Key));
        public static readonly BitmapImage Ref = LoadImage(nameof(Ref));
        public static readonly BitmapImage Column = LoadImage(nameof(Column));
        public static readonly BitmapImage LocalColumn = LoadImage(nameof(LocalColumn));
        public static readonly BitmapImage ColumnList = LoadImage(nameof(ColumnList));
        public static readonly BitmapImage Computation = LoadImage(nameof(Computation));
        public static readonly BitmapImage ChildModel = LoadImage(nameof(ChildModel));
        public static readonly BitmapImage ForeignKey = LoadImage(nameof(ForeignKey));
        public static readonly BitmapImage CheckConstraint = LoadImage(nameof(CheckConstraint));
        public static readonly BitmapImage UniqueConstraint = LoadImage(nameof(UniqueConstraint));
        public static readonly BitmapImage CustomValidator = LoadImage(nameof(CustomValidator));
        public static readonly BitmapImage Index = LoadImage(nameof(Index));
        public static readonly BitmapImage Projection = LoadImage(nameof(Projection));
        public static readonly BitmapImage Folder = LoadImage(nameof(Folder));
        public static readonly BitmapImage FolderOpen = LoadImage(nameof(FolderOpen));
        public static readonly BitmapImage Db = LoadImage(nameof(Db));
        public static readonly BitmapImage Table = LoadImage(nameof(Table));
        public static readonly BitmapImage Relationship = LoadImage(nameof(Relationship));

        private static BitmapImage LoadImage(string iconFileName)
        {
            var assemblyName = typeof(Icons).Assembly.GetName().Name;
            var uriString = string.Format(@"pack://application:,,,/{0};component/Icons/{1}.png", assemblyName, iconFileName);
            var uri = new Uri(uriString);
            return new BitmapImage(uri);
        }

        public static ImageSource GetIcon(this ModelMapper.NodeKind nodeKind, bool isExpanded)
        {
            switch (nodeKind)
            {
                case ModelMapper.NodeKind.Model:
                    return Model;
                case ModelMapper.NodeKind.PrimaryKey:
                    return PrimaryKey;
                case ModelMapper.NodeKind.Key:
                    return Key;
                case ModelMapper.NodeKind.Ref:
                    return Ref;
                case ModelMapper.NodeKind.Column:
                    return Column;
                case ModelMapper.NodeKind.LocalColumn:
                    return LocalColumn;
                case ModelMapper.NodeKind.ColumnList:
                    return ColumnList;
                case ModelMapper.NodeKind.Computation:
                    return Computation;
                case ModelMapper.NodeKind.ChildModel:
                    return ChildModel;
                case ModelMapper.NodeKind.ForeignKey:
                    return ForeignKey;
                case ModelMapper.NodeKind.CheckConstraint:
                    return CheckConstraint;
                case ModelMapper.NodeKind.UniqueConstraint:
                    return UniqueConstraint;
                case ModelMapper.NodeKind.CustomValidator:
                    return CustomValidator;
                case ModelMapper.NodeKind.Index:
                    return Index;
                case ModelMapper.NodeKind.Projection:
                    return Projection;
                case ModelMapper.NodeKind.Folder:
                    return isExpanded ? FolderOpen : Folder;
                default:
                    Debug.Fail(string.Format("Unknown NodeKind: {0}", nodeKind));
                    return null;
            }
        }

        public static ImageSource GetIcon(this DbMapper.NodeKind nodeKind, bool isExpanded)
        {
            switch (nodeKind)
            {
                case DbMapper.NodeKind.Db:
                    return Db;
                case DbMapper.NodeKind.Table:
                    return Table;
                case DbMapper.NodeKind.RelationshipDeclaration:
                case DbMapper.NodeKind.RelationshipImplementation:
                    return Relationship;
                case DbMapper.NodeKind.Folder:
                    return isExpanded ? FolderOpen : Folder;
                default:
                    Debug.Fail(string.Format("Unknown NodeKind: {0}", nodeKind));
                    return null;
            }
        }
    }
}
