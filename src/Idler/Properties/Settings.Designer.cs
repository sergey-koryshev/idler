﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Idler.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.7.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ShiftDB.mdb")]
        public string DataSource {
            get {
                return ((string)(this["DataSource"]));
            }
            set {
                this["DataSource"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.DateTime SelectedDate {
            get {
                return ((global::System.DateTime)(this["SelectedDate"]));
            }
            set {
                this["SelectedDate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:30:00")]
        public global::System.TimeSpan ReminderInterval {
            get {
                return ((global::System.TimeSpan)(this["ReminderInterval"]));
            }
            set {
                this["ReminderInterval"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool IsReminderEnabled {
            get {
                return ((bool)(this["IsReminderEnabled"]));
            }
            set {
                this["IsReminderEnabled"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("100")]
        public double CategoryColumnWidth {
            get {
                return ((double)(this["CategoryColumnWidth"]));
            }
            set {
                this["CategoryColumnWidth"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("20")]
        public double EffortColumnWidth {
            get {
                return ((double)(this["EffortColumnWidth"]));
            }
            set {
                this["EffortColumnWidth"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:05:00")]
        public global::System.TimeSpan AutoBlurInterval {
            get {
                return ((global::System.TimeSpan)(this["AutoBlurInterval"]));
            }
            set {
                this["AutoBlurInterval"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool IsAutoBlurEnabled {
            get {
                return ((bool)(this["IsAutoBlurEnabled"]));
            }
            set {
                this["IsAutoBlurEnabled"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string ExcelTemplate {
            get {
                return ((string)(this["ExcelTemplate"]));
            }
            set {
                this["ExcelTemplate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool IsExcelTemplateUsed {
            get {
                return ((bool)(this["IsExcelTemplateUsed"]));
            }
            set {
                this["IsExcelTemplateUsed"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool SkipWeekends {
            get {
                return ((bool)(this["SkipWeekends"]));
            }
            set {
                this["SkipWeekends"] = value;
            }
        }
    }
}
