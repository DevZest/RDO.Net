﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DevZest.Data.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Json {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Json() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DevZest.Data.Resources.Json", typeof(Json).Assembly);
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
        ///   Looks up a localized string similar to [
        ///   {
        ///      &quot;ProductCategoryID&quot; : 0,
        ///      &quot;ParentProductCategoryID&quot; : null,
        ///      &quot;Name&quot; : &quot;Bikes&quot;,
        ///      &quot;RowGuid&quot; : &quot;cfbda25c-df71-47a7-b81b-64ee161aa37c&quot;,
        ///      &quot;ModifiedDate&quot; : &quot;2002-06-01T00:00:00.000&quot;,
        ///      &quot;SubCategories&quot; : [
        ///         {
        ///            &quot;ProductCategoryID&quot; : -2,
        ///            &quot;Name&quot; : &quot;Mountain Bikes&quot;,
        ///            &quot;RowGuid&quot; : &quot;2d364ade-264a-433c-b092-4fcbf3804e01&quot;,
        ///            &quot;ModifiedDate&quot; : &quot;2002-06-01T00:00:00.000&quot;,
        ///            &quot;SubCategories&quot; : []
        ///         },
        ///         [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string MultiLevelProductCategory {
            get {
                return ResourceManager.GetString("MultiLevelProductCategory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [
        ///   {
        ///      &quot;Column&quot; : &quot;CustomerID&quot;,
        ///      &quot;Direction&quot; : &quot;Ascending&quot;
        ///   },
        ///   {
        ///      &quot;Column&quot; : &quot;SalesOrderID&quot;,
        ///      &quot;Direction&quot; : &quot;Descending&quot;
        ///   }
        ///].
        /// </summary>
        internal static string OrderByJson_ToJson_Parse {
            get {
                return ResourceManager.GetString("OrderByJson_ToJson_Parse", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [
        ///   {
        ///      &quot;ProductCategoryID&quot; : 1,
        ///      &quot;ParentProductCategoryID&quot; : null,
        ///      &quot;Name&quot; : &quot;Bikes&quot;,
        ///      &quot;RowGuid&quot; : &quot;cfbda25c-df71-47a7-b81b-64ee161aa37c&quot;,
        ///      &quot;ModifiedDate&quot; : &quot;2002-06-01T00:00:00.000&quot;,
        ///      &quot;SubCategories&quot; : [
        ///         {
        ///            &quot;ProductCategoryID&quot; : 5,
        ///            &quot;Name&quot; : &quot;Mountain Bikes&quot;,
        ///            &quot;RowGuid&quot; : &quot;2d364ade-264a-433c-b092-4fcbf3804e01&quot;,
        ///            &quot;ModifiedDate&quot; : &quot;2002-06-01T00:00:00.000&quot;,
        ///            &quot;SubCategories&quot; : []
        ///         },
        ///          [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ProductCategories {
            get {
                return ResourceManager.GetString("ProductCategories", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [
        ///   {
        ///      &quot;ProductCategoryID&quot; : 1,
        ///      &quot;ParentProductCategoryID&quot; : null,
        ///      &quot;Name&quot; : &quot;Bikes&quot;,
        ///      &quot;RowGuid&quot; : &quot;CFBDA25C-DF71-47A7-B81B-64EE161AA37C&quot;,
        ///      &quot;ModifiedDate&quot; : &quot;2002-06-01T00:00:00.000&quot;,
        ///      &quot;SubCategories&quot; : []
        ///   },
        ///   {
        ///      &quot;ProductCategoryID&quot; : 2,
        ///      &quot;ParentProductCategoryID&quot; : null,
        ///      &quot;Name&quot; : &quot;Other&quot;,
        ///      &quot;RowGuid&quot; : &quot;C657828D-D808-4ABA-91A3-AF2CE02300E9&quot;,
        ///      &quot;ModifiedDate&quot; : &quot;2002-06-01T00:00:00.000&quot;,
        ///      &quot;SubCategories&quot; : []
        ///   }
        ///].
        /// </summary>
        internal static string ProductCategoriesLevel1 {
            get {
                return ResourceManager.GetString("ProductCategoriesLevel1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [
        ///   {
        ///      &quot;SalesOrderID&quot; : 71774,
        ///      &quot;RevisionNumber&quot; : 2,
        ///      &quot;OrderDate&quot; : &quot;2008-06-01T00:00:00.000&quot;,
        ///      &quot;DueDate&quot; : &quot;2008-06-13T00:00:00.000&quot;,
        ///      &quot;ShipDate&quot; : &quot;2008-06-08T00:00:00.000&quot;,
        ///      &quot;Status&quot; : 5,
        ///      &quot;OnlineOrderFlag&quot; : false,
        ///      &quot;SalesOrderNumber&quot; : &quot;SO71774&quot;,
        ///      &quot;PurchaseOrderNumber&quot; : &quot;PO348186287&quot;,
        ///      &quot;AccountNumber&quot; : &quot;10-4020-000609&quot;,
        ///      &quot;CustomerID&quot; : 29847,
        ///      &quot;ShipToAddressID&quot; : 1092,
        ///      &quot;BillToAddressID&quot; : 1092,
        ///      &quot;ShipMethod&quot; : &quot;CA [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Sales_Order_71774 {
            get {
                return ResourceManager.GetString("Sales_Order_71774", resourceCulture);
            }
        }
    }
}
