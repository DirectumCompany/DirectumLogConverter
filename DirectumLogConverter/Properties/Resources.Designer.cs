﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DirectumLogConverter.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DirectumLogConverter.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Done! {0:F}s elapsed..
        /// </summary>
        internal static string ConversionDone {
            get {
                return ResourceManager.GetString("ConversionDone", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Converting log from &quot;{0}&quot; to &quot;{1}&quot;....
        /// </summary>
        internal static string ConversionStarted {
            get {
                return ResourceManager.GetString("ConversionStarted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to File &quot;{0}&quot; already exists, overwrite?.
        /// </summary>
        internal static string FileOverwriteConfirmation {
            get {
                return ResourceManager.GetString("FileOverwriteConfirmation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Required argument is missing..
        /// </summary>
        internal static string MissingRequiredOptionError {
            get {
                return ResourceManager.GetString("MissingRequiredOptionError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to One or more of converted files exist, overwrite?.
        /// </summary>
        internal static string MultipleFilesOverwriteConfirmation {
            get {
                return ResourceManager.GetString("MultipleFilesOverwriteConfirmation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Option &quot;{0}&quot; is unknown..
        /// </summary>
        internal static string UnknownOptionError {
            get {
                return ResourceManager.GetString("UnknownOptionError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unrecognized input &quot;{0}&quot;.
        /// </summary>
        internal static string UnrecognizedInput {
            get {
                return ResourceManager.GetString("UnrecognizedInput", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DirectumLogConverter is a tool for converting Directum JSON logs.
        ///
        ///Usage of {0}:
        ///
        ///  {0} [source] [destination]
        ///
        ///  [source] argument is mandatory, [destination] is not, if omitted it will use source file name with postfix &quot;{1}&quot; as destination file name.
        ///
        ///Switches:
        ///
        ///  -c, --csv: Use csv as output format..
        /// </summary>
        internal static string Usage {
            get {
                return ResourceManager.GetString("Usage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} [y/n]: .
        /// </summary>
        internal static string UserConfirmationTemplate {
            get {
                return ResourceManager.GetString("UserConfirmationTemplate", resourceCulture);
            }
        }
    }
}
