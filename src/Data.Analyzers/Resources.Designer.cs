﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DevZest.Data.Analyzers {
    using System;
    using System.Reflection;
    
    
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
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DevZest.Data.Analyzers.Resources", typeof(Resources).GetTypeInfo().Assembly);
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
        ///   Looks up a localized string similar to Duplicate mounter registration for property &apos;{0}&apos;..
        /// </summary>
        internal static string DuplicateMounterRegistration_Message {
            get {
                return ResourceManager.GetString("DuplicateMounterRegistration_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Duplicate mounter registration..
        /// </summary>
        internal static string DuplicateMounterRegistration_Title {
            get {
                return ResourceManager.GetString("DuplicateMounterRegistration_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Local column &apos;{0}&apos; is invalid for RegisterColumn, use RegisterLocalColumn instead..
        /// </summary>
        internal static string InvalidRegisterLocalColumn_Message {
            get {
                return ResourceManager.GetString("InvalidRegisterLocalColumn_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Local column is invalid for RegisterColumn..
        /// </summary>
        internal static string InvalidRegisterLocalColumn_Title {
            get {
                return ResourceManager.GetString("InvalidRegisterLocalColumn_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The getter parameter must be a lambda expression which returns a non-readonly property of current class..
        /// </summary>
        internal static string InvalidRegisterMounterGetterParam_Message {
            get {
                return ResourceManager.GetString("InvalidRegisterMounterGetterParam_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid getter of mounter registration..
        /// </summary>
        internal static string InvalidRegisterMounterGetterParam_Title {
            get {
                return ResourceManager.GetString("InvalidRegisterMounterGetterParam_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mounter registration is only allowed as static field initializer or static constructor statement..
        /// </summary>
        internal static string InvalidRegisterMounterInvocation_Message {
            get {
                return ResourceManager.GetString("InvalidRegisterMounterInvocation_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid invocation of mounter registration..
        /// </summary>
        internal static string InvalidRegisterMounterInvocation_Title {
            get {
                return ResourceManager.GetString("InvalidRegisterMounterInvocation_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Missing registration for model property &apos;{0}&apos;..
        /// </summary>
        internal static string MissingMounterRegistration_Message {
            get {
                return ResourceManager.GetString("MissingMounterRegistration_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Missing registration for model property..
        /// </summary>
        internal static string MissingMounterRegistration_Title {
            get {
                return ResourceManager.GetString("MissingMounterRegistration_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mounter &apos;{0}&apos; of property &apos;{1}&apos; does not conform to naming convention, rename mounter name &apos;{0}&apos; to &apos;{2}&apos;..
        /// </summary>
        internal static string MounterNaming_Message {
            get {
                return ResourceManager.GetString("MounterNaming_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mounter name does not conform to convention..
        /// </summary>
        internal static string MounterNaming_Title {
            get {
                return ResourceManager.GetString("MounterNaming_Title", resourceCulture);
            }
        }
    }
}
