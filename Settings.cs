using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace QuickNavigatePlugin
{
    [Serializable]
    public class Settings
    {
        private const Boolean DEFAULT_CTRL_CLICK_ENABLED = true;
        private const Boolean DEFAULT_SEARCH_EXTERNAL_CLASSPATH = true;
        private const Boolean DEFAULT_RESOURCE_CACHING = false;
        private const String CATEGORY = "Shortcuts";
        
        private Boolean ctrlClickEnabled = DEFAULT_CTRL_CLICK_ENABLED;
        private Boolean searchExternalClassPath = DEFAULT_SEARCH_EXTERNAL_CLASSPATH;
        private Boolean resourcesCaching = DEFAULT_RESOURCE_CACHING;
        private Size resourceFormSize;
        private Size typeFormSize;
        private Size outlineFormSize;

        [Browsable(false)]
        public Size OutlineFormSize
        {
            get { return outlineFormSize; }
            set { outlineFormSize = value; }
        }

        [Browsable(false)]
        public Size ResourceFormSize
        {
            get { return resourceFormSize; }
            set { resourceFormSize = value; }
        }

        [Browsable(false)]
        public Size TypeFormSize
        {
            get { return typeFormSize; }
            set { typeFormSize = value; }
        }

        [DisplayName("Enable navigation by Ctrl+Click")]
        [Description("Go to declaration by Ctrl+Click on the word")]
        [DefaultValue(DEFAULT_SEARCH_EXTERNAL_CLASSPATH)]
        public Boolean CtrlClickEnabled
        {
            get { return ctrlClickEnabled; }
            set { ctrlClickEnabled = value; }
        }
        
        [DisplayName("Search in external classpath")]
        [Description("Enable searching files in external classpath")]
        [DefaultValue(DEFAULT_CTRL_CLICK_ENABLED)]
        public Boolean SearchExternalClassPath
        {
            get { return searchExternalClassPath; }
            set { searchExternalClassPath = value; }
        }
        
        [DisplayName("Cache resources")]
        [Description("Do not read resources each time dialog is opening.")]
        [DefaultValue(DEFAULT_RESOURCE_CACHING)]
        public Boolean ResourcesCaching
        {
            get { return resourcesCaching; }
            set { resourcesCaching = value; }
        }
    }
}