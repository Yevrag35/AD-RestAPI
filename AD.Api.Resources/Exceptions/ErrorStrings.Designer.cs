﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AD.Api.Resources {
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
    public class ErrorStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ErrorStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("AD.Api.Resources.Exceptions.ErrorStrings", typeof(ErrorStrings).Assembly);
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
        ///   Looks up a localized string similar to The struct named &apos;{0}&apos; of type &apos;{1}&apos; is empty but was expected not to be..
        /// </summary>
        public static string EmptyStruct_Full_Format {
            get {
                return ResourceManager.GetString("EmptyStruct_Full_Format", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The provided struct of type &apos;{0}&apos; is empty but was expected not to be..
        /// </summary>
        public static string EmptyStruct_NoName_Message {
            get {
                return ResourceManager.GetString("EmptyStruct_NoName_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The provided struct is empty but was expected not to be..
        /// </summary>
        public static string EmptyStruct_NoName_NoType_Message {
            get {
                return ResourceManager.GetString("EmptyStruct_NoName_NoType_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The struct named &apos;{0}&apos; is empty but was expected not to be..
        /// </summary>
        public static string EmptyStruct_NoType_Message {
            get {
                return ResourceManager.GetString("EmptyStruct_NoType_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The dependency injection method from type &apos;{0}&apos; had an exception occur..
        /// </summary>
        public static string Exception_DI {
            get {
                return ResourceManager.GetString("Exception_DI", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A service was attempted to be registered twice in the specified Dependency Injection container and this was not intended -&gt; &apos;{0}&apos;.
        /// </summary>
        public static string Exception_DuplicatedService {
            get {
                return ResourceManager.GetString("Exception_DuplicatedService", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Registration method must have at least the IServiceCollection parameter type..
        /// </summary>
        public static string Exception_InvalidMethodParameters {
            get {
                return ResourceManager.GetString("Exception_InvalidMethodParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An exception was thrown due to a invalid operation in a AD Api-related library..
        /// </summary>
        public static string Exception_Message_Default {
            get {
                return ResourceManager.GetString("Exception_Message_Default", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to find the specified attribute &quot;{0}&quot;..
        /// </summary>
        public static string Exception_MissingAttr {
            get {
                return ResourceManager.GetString("Exception_MissingAttr", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to find a parameterless constructor for type &apos;{0}&apos; with flags: {1}..
        /// </summary>
        public static string Exception_MissingCtor {
            get {
                return ResourceManager.GetString("Exception_MissingCtor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An exception occurred during a reflection operation but no other specifics were provided..
        /// </summary>
        public static string Exception_Reflection_Default {
            get {
                return ResourceManager.GetString("Exception_Reflection_Default", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A member named &apos;{0}&apos; was not found within type &apos;{1}&apos; using the specified BindingFlags.  The library cannot be loaded..
        /// </summary>
        public static string Exception_Static_Startup {
            get {
                return ResourceManager.GetString("Exception_Static_Startup", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The type &apos;{0}&apos; is not assignable to type &apos;{1}&apos;..
        /// </summary>
        public static string Exception_TypeNotAssignable {
            get {
                return ResourceManager.GetString("Exception_TypeNotAssignable", resourceCulture);
            }
        }
    }
}
