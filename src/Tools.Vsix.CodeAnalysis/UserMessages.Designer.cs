﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DevZest.Data.CodeAnalysis {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class UserMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal UserMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DevZest.Data.CodeAnalysis.UserMessages", typeof(UserMessages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid identifier.
        /// </summary>
        internal static string CodeMapper_InvalidIdentifier {
            get {
                return ResourceManager.GetString("CodeMapper_InvalidIdentifier", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Duplicate column is not allowed..
        /// </summary>
        internal static string ColumnEntry_DuplicateColumn {
            get {
                return ResourceManager.GetString("ColumnEntry_DuplicateColumn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Column.
        /// </summary>
        internal static string Display_ColumnEntry_Column {
            get {
                return ResourceManager.GetString("Display_ColumnEntry_Column", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Parameter.
        /// </summary>
        internal static string Display_ForeignKeyEntry_Parameter {
            get {
                return ResourceManager.GetString("Display_ForeignKeyEntry_Parameter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Direction.
        /// </summary>
        internal static string Display_IndexEntry_SortDirection {
            get {
                return ResourceManager.GetString("Display_IndexEntry_SortDirection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Param.
        /// </summary>
        internal static string Display_PrimaryKeyEntry_ConstructorParamName {
            get {
                return ResourceManager.GetString("Display_PrimaryKeyEntry_ConstructorParamName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sort.
        /// </summary>
        internal static string Display_PrimaryKeyEntry_Sort {
            get {
                return ResourceManager.GetString("Display_PrimaryKeyEntry_Sort", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mounter.
        /// </summary>
        internal static string Display_ProjectionEntry_Mounter {
            get {
                return ResourceManager.GetString("Display_ProjectionEntry_Mounter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Computation.
        /// </summary>
        internal static string FolderName_Computation {
            get {
                return ResourceManager.GetString("FolderName_Computation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Constraint.
        /// </summary>
        internal static string FolderName_Constraint {
            get {
                return ResourceManager.GetString("FolderName_Constraint", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Index.
        /// </summary>
        internal static string FolderName_Index {
            get {
                return ResourceManager.GetString("FolderName_Index", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Projection.
        /// </summary>
        internal static string FolderName_Projection {
            get {
                return ResourceManager.GetString("FolderName_Projection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Relationship.
        /// </summary>
        internal static string FolderName_Relationship {
            get {
                return ResourceManager.GetString("FolderName_Relationship", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Validator.
        /// </summary>
        internal static string FolderName_Validator {
            get {
                return ResourceManager.GetString("FolderName_Validator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid parameter name..
        /// </summary>
        internal static string PrimaryKeyEntry_InvalidParamName {
            get {
                return ResourceManager.GetString("PrimaryKeyEntry_InvalidParamName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The parameter name should be Camel Case of the column name..
        /// </summary>
        internal static string PrimaryKeyEntry_MismatchParamName {
            get {
                return ResourceManager.GetString("PrimaryKeyEntry_MismatchParamName", resourceCulture);
            }
        }
    }
}
