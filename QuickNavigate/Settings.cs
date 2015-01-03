using System;
using System.ComponentModel;
using System.Drawing;

namespace QuickNavigate
{
    [Serializable]
    public class Settings
    {
        #region General

        [Category("General")]
        [DisplayName("Enable navigation by Ctrl+Click")]
        [Description("Go to declaration by Ctrl+Click on the word")]
        [DefaultValue(true)]
        public bool CtrlClickEnabled { get;set; }

        [Category("General")]
        [DisplayName("Search in external classpath")]
        [Description("Enable searching files in external classpath")]
        [DefaultValue(true)]
        public bool SearchExternalClassPath { get;set; }

        #endregion

        #region Forms

        [Category("Forms")]
        [DisplayName("Enable item spacer")]
        [DefaultValue(true)]
        public bool EnableItemSpacer { get; set; }

        [Category("Forms")]
        [DisplayName("Item spacer")]
        [DefaultValue("-----------------")]
        public string ItemSpacer { get; set; }

        [Category("Forms")]
        [DisplayName("Max items")]
        [DefaultValue(100)]
        public int MaxItems { get; set; }

        [Category("Forms")]
        [DisplayName("Wrap list")]
        [DefaultValue(false)]
        public bool WrapList { get; set; }

        #endregion

        #region Type Explorer

        [Browsable(false)]
        [Category("Type Explorer")]
        public Size TypeFormSize { get; set; }

        [Browsable(true)]
        [Category("Type Explorer")]
        [DisplayName("Whole word")]
        [DefaultValue(false)]
        public bool TypeFormWholeWord { get; set; }

        [Browsable(true)]
        [Category("Type Explorer")]
        [DisplayName("Match case")]
        [DefaultValue(false)]
        public bool TypeFormMatchCase { get; set; }

        #endregion

        #region Outline Form

        [Browsable(false)]
        [Category("Outline Form")]
        public Size OutlineFormSize { get;set; }

        [Browsable(true)]
        [Category("Outline Form")]
        [DisplayName("Whole word")]
        [DefaultValue(false)]
        public bool OutlineFormWholeWord { get;set; }

        [Browsable(true)]
        [Category("Outline Form")]
        [DisplayName("Match case")]
        [DefaultValue(false)]
        public bool OutlineFormMatchCase { get;set; }

        #endregion

        #region Hierarchy Explorer

        [Browsable(false)]
        [Category("Hierarchy Explorer")]
        public Size HierarchyExplorerSize;

        [Browsable(true)]
        [Category("Hierarchy Explorer")]
        [DisplayName("Whole word")]
        [DefaultValue(false)]
        public bool HierarchyExplorerWholeWord { get; set; }

        [Browsable(true)]
        [Category("Hierarchy Explorer")]
        [DisplayName("Match case")]
        [DefaultValue(false)]
        public bool HierarchyExplorerMatchCase { get; set; }

        #endregion
    }
}