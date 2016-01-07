using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using PluginCore;

namespace QuickNavigate.Forms
{
    public sealed partial class OpenRecentProjectsForm : Form
    {
        [NotNull]
        readonly Settings settings;

        public OpenRecentProjectsForm([NotNull] Settings settings)
        {
            this.settings = settings;
            Font = PluginBase.Settings.DefaultFont;
            InitializeComponent();
            tree.ItemHeight = tree.Font.Height;
            if (settings.RecentProjectsSize.Width > MinimumSize.Width) Size = settings.RecentProjectsSize;
            RefrestTree();
        }

        public string SelectedItem => tree?.SelectedItem.ToString();

        void RefrestTree()
        {
            tree.BeginUpdate();
            tree.Items.Clear();
            FillTree();
            if (tree.Items.Count > 0) tree.SelectedIndex = 0;
            else open.Enabled = false;
            tree.EndUpdate();
        }

        void FillTree()
        {
            List<string> matches = ProjectManager.PluginMain.Settings.RecentProjects
                        .Where(File.Exists)
                        .ToList();
            if (matches.Count == 0) return;
            string search = input.Text;
            if (search.Length > 0) matches = SearchUtil.Matches(matches, search, Path.PathSeparator.ToString(), settings.MaxItems, settings.RecentProjectsWholeWord, settings.RecentProjectsMatchCase);
            if (matches.Count > 0) tree.Items.AddRange(matches.ToArray());
        }

        void Navigate()
        {
            if (SelectedItem == null) return;
            DialogResult = DialogResult.OK;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    e.Handled = true;
                    Navigate();
                    break;
                case Keys.L:
                    if (e.Control)
                    {
                        input.Focus();
                        input.SelectAll();
                    }
                    break;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            settings.RecentProjectsSize = Size;
        }

        void OnInputTextChanged(object sender, EventArgs e) => RefrestTree();

        void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            int lastIndex = tree.Items.Count - 1;
            int index = tree.SelectedIndex;
            switch (e.KeyCode)
            {
                case Keys.L:
                    e.Handled = e.Control;
                    return;
                case Keys.Down:
                    if (index < lastIndex) tree.SetSelected(index + 1, true);
                    else if (settings.WrapList) tree.SetSelected(0, true);
                    break;
                case Keys.Up:
                    if (index > 0) tree.SetSelected(index - 1, true);
                    else if (settings.WrapList) tree.SetSelected(lastIndex, true);
                    break;
                case Keys.Home:
                    tree.SetSelected(0, true);
                    break;
                case Keys.End:
                    tree.SetSelected(lastIndex, true);
                    break;
                default: return;
            }
            e.Handled = true;
        }

        void OnTreeMouseDoubleClick(object sender, MouseEventArgs e) => Navigate();

        void OnTreeSelectedIndexChanged(object sender, EventArgs e) => open.Enabled = SelectedItem != null;
    }
}