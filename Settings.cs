using System;
using System.ComponentModel;
using System.Drawing;

namespace QuickNavigatePlugin
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

        [Category("General")]
        [DisplayName("Cache resources")]
        [Description("Do not read resources each time dialog is opening")]
        [DefaultValue(false)]
        public bool ResourcesCaching { get;set; }

        #endregion

        public const bool HIGHLIGHT_REFERENCES = false;

        [Category("Highlight")]
        [DisplayName("Highlight references to symbol under cursor")]
        [DefaultValue(HIGHLIGHT_REFERENCES)]
        public bool HighlightReferences { get; set; }

        public const int HIGHLIGHT_UPDATE_INTERVAL = 500;

        [Category("Highlight")]
        [DisplayName("Update interval")]
        [DefaultValue(HIGHLIGHT_UPDATE_INTERVAL)]
        public int HighlightUpdateInterval { get; set; }

        #region Highlight

        #endregion

        #region Resource Form

        [Browsable(false)]
        [Category("Resource Form")]
        public Size ResourceFormSize { get;set; }

        [Browsable(true)]
        [Category("Resource Form")]
        [DisplayName("Whole word")]
        [DefaultValue(false)]
        public bool ResourceFormWholeWord { get; set; }

        [Browsable(true)]
        [Category("Resource Form")]
        [DisplayName("Match case")]
        [DefaultValue(false)]
        public bool ResourceFormMatchCase { get; set; }

        #endregion

        #region Type Form

        [Browsable(false)]
        [Category("Type Form")]
        public Size TypeFormSize { get; set; }

        [Browsable(true)]
        [Category("Type Form")]
        [DisplayName("Whole word")]
        [DefaultValue(false)]
        public bool TypeFormWholeWord { get; set; }

        [Browsable(true)]
        [Category("Type Form")]
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

    }
}