﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AD.Api {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("AD.Api.Resources.Strings", typeof(Strings).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CN=.
        /// </summary>
        public static string CN_Prefix {
            get {
                return ResourceManager.GetString("CN_Prefix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to application/json.
        /// </summary>
        public static string ContentType_Json {
            get {
                return ResourceManager.GetString("ContentType_Json", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to isCriticalSystemObject.
        /// </summary>
        public static string CriticalSystemObject {
            get {
                return ResourceManager.GetString("CriticalSystemObject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to .
        /// </summary>
        public static string DefaultHostIfEmpty {
            get {
                return ResourceManager.GetString("DefaultHostIfEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to defaultNamingContext.
        /// </summary>
        public static string DefaultNamingContext {
            get {
                return ResourceManager.GetString("DefaultNamingContext", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to distinguishedname.
        /// </summary>
        public static string DistinguishedName {
            get {
                return ResourceManager.GetString("DistinguishedName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ChangePassword.
        /// </summary>
        public static string Invoke_ChangePassword {
            get {
                return ResourceManager.GetString("Invoke_ChangePassword", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SetPassword.
        /// </summary>
        public static string Invoke_PasswordSet {
            get {
                return ResourceManager.GetString("Invoke_PasswordSet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;WKGUID=.
        /// </summary>
        public static string LDAP_Format_WKO {
            get {
                return ResourceManager.GetString("LDAP_Format_WKO", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Everyone.
        /// </summary>
        public static string NTAccount_Everyone {
            get {
                return ResourceManager.GetString("NTAccount_Everyone", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to OU=.
        /// </summary>
        public static string OU_Prefix {
            get {
                return ResourceManager.GetString("OU_Prefix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to computer.
        /// </summary>
        public static string Schema_Computer {
            get {
                return ResourceManager.GetString("Schema_Computer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to group.
        /// </summary>
        public static string Schema_Group {
            get {
                return ResourceManager.GetString("Schema_Group", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to user.
        /// </summary>
        public static string Schema_User {
            get {
                return ResourceManager.GetString("Schema_User", resourceCulture);
            }
        }
    }
}
