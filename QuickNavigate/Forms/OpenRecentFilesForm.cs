using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using PluginCore;
using QuickNavigate.Helpers;
using static System.Windows.Forms.ListBox;

namespace QuickNavigate.Forms
{
    public sealed partial class OpenRecentFilesForm : Form
    {
        [NotNull]
        readonly Settings settings;

        [NotNull]
        readonly List<string> recentFiles;

        [NotNull]
        readonly List<string> openedFiles;

        public OpenRecentFilesForm([NotNull] Settings settings)
        {
            this.settings = settings;
            InitializeComponent();
            Font = PluginBase.Settings.DefaultFont;
            tree.ItemHeight = tree.Font.Height;
            if (settings.RecentFilesSize.Width > MinimumSize.Width) Size = settings.RecentFilesSize;
            recentFiles = PluginBase.MainForm.Settings.PreviousDocuments.Where(File.Exists).ToList();
            openedFiles = FormHelper.FilterOpenedFiles(recentFiles);
            recentFiles.RemoveAll(openedFiles.Contains);
            RefreshTree();
        }

        int selectedIndex;

        [NotNull]
        public List<string> SelectedItems
        {
            get
            {
                List<string> result = (from object item in tree.SelectedItems
                                       where item.ToString() != settings.ItemSpacer
                                       select item.ToString()).ToList();
                return result;
            }
        }

        void RefreshTree()
        {
            tree.BeginUpdate();
            tree.Items.Clear();
            FillTree();
            if (tree.Items.Count > 0)
            {
                selectedIndex = 0;
                tree.SelectedIndex = 0;
            }
            else open.Enabled = false;
            tree.EndUpdate();
        }

        void FillTree()
        {
            string separator = Path.PathSeparator.ToString();
            int maxItems = settings.MaxItems;
            bool wholeWord = settings.RecentFilesWholeWord;
            bool matchCase = settings.RecentFilesMatchCase;
            string search = input.Text;
            if (openedFiles.Count > 0)
            {
                List<string> matches = openedFiles;
                if (search.Length > 0) matches = SearchUtil.Matches(openedFiles, search, separator, maxItems, wholeWord, matchCase);
                if (matches.Count > 0)
                {
                    tree.Items.AddRange(matches.ToArray());
                    if (settings.EnableItemSpacer) tree.Items.Add(settings.ItemSpacer);
                }
            }
            if (recentFiles.Count > 0)
            {
                List<string> matches = recentFiles;
                if (search.Length > 0) matches = SearchUtil.Matches(matches, search, separator, maxItems, wholeWord, matchCase);
                if (matches.Count > 0) tree.Items.AddRange(matches.ToArray());
            }
        }

        void Navigate()
        {
            if (SelectedItems.Count == 0) return;
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
        
        protected override void OnFormClosing(FormClosingEventArgs e) => settings.RecentFilesSize = Size;

        void OnInputTextChanged(object sender, EventArgs e) => RefreshTree();

        void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            int prevSelectedIndex = selectedIndex;
            int lastIndex = tree.Items.Count - 1;
            switch (e.KeyCode)
            {
                case Keys.L:
                    e.Handled = e.Control;
                    return;
                case Keys.Down:
                    if (selectedIndex < lastIndex) ++selectedIndex;
                    else if (settings.WrapList) selectedIndex = 0;
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                    break;
                case Keys.Up:
                    if (selectedIndex > 0) --selectedIndex;
                    else if (settings.WrapList) selectedIndex = lastIndex;
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                    break;
                case Keys.Home:
                    selectedIndex = 0;
                    break;
                case Keys.End:
                    selectedIndex = lastIndex;
                    break;
                default: return;
            }
            SelectedIndexCollection selectedIndices = tree.SelectedIndices;
            if (e.Shift)
            {
                if (selectedIndices.Contains(selectedIndex) && selectedIndices.Count > 1)
                    tree.SetSelected(prevSelectedIndex, false);
                else
                {
                    int index = selectedIndex;
                    int delta = selectedIndex - prevSelectedIndex;
                    int length = Math.Abs(delta);
                    for (int i = 0; i < length; i++)
                    {
                        tree.SetSelected(index, !selectedIndices.Contains(index));
                        if (delta > 0)
                        {
                            if (index == lastIndex) index = 0;
                            else ++index;
                        }
                        else
                        {
                            if (index == 0) index = lastIndex;
                            else --index;
                        }
                    }
                }
            }
            else
            {
                selectedIndices.Clear();
                tree.SelectedItems.Clear();
                tree.SetSelected(selectedIndex, true);
            }
            e.Handled = true;
        }

        void OnTreeMouseDoubleClick(object sender, MouseEventArgs e) => Navigate();

        void OnTreeSelectedIndexChanged(object sender, EventArgs e) => open.Enabled = SelectedItems.Count > 0;
    }
}