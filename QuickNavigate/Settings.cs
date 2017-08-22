// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.ComponentModel;
using System.Drawing;

namespace QuickNavigate
{
    [Serializable]
    public class Settings
    {
        bool ctrlClickEnabled = true;

        [Category("General")]
        [DisplayName("Enable navigation by Ctrl+Click")]
        [Description("Go to declaration by Ctrl+Click on the word")]
        [DefaultValue(true)]
        public bool CtrlClickEnabled
        {
            get => ctrlClickEnabled;
            set => ctrlClickEnabled = value;
        }

        bool enableItemSpacer = true;

        [Category("General")]
        [DisplayName("Enable item spacer")]
        [DefaultValue(true)]
        public bool EnableItemSpacer
        {
            get => enableItemSpacer;
            set => enableItemSpacer = value;
        }

        string itemSpacer = "—————————————————————————————————————————————————————————————";

        [Category("General")]
        [DisplayName("Item spacer")]
        [DefaultValue("—————————————————————————————————————————————————————————————")]
        public string ItemSpacer
        {
            get => itemSpacer;
            set => itemSpacer = value;
        }

        int maxItems = 100;

        [Category("General")]
        [DisplayName("Max items")]
        [DefaultValue(100)]
        public int MaxItems
        {
            get => maxItems;
            set => maxItems = value;
        }

        bool typeExplorerSearchExternalClassPath = true;

        [Browsable(false)]
        public bool TypeExplorerSearchExternalClassPath
        {
            get => typeExplorerSearchExternalClassPath;
            set => typeExplorerSearchExternalClassPath = value;
        }

        [Browsable(false)] public Size TypeExplorerSize { get; set; }
        [Browsable(false)] public Size QuickOutlineSize { get; set; }
        [Browsable(false)] public Size HierarchyExplorerSize { get; set; }
        [Browsable(false)] public Size RecentFilesSize { get; set; }
        [Browsable(false)] public Size RecentProjectsSize { get; set; }
    }
}