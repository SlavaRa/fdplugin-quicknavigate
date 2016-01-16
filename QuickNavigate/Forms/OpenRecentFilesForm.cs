﻿using System;
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
            Font = PluginBase.Settings.DefaultFont;
            InitializeComponent();
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
                var result = (from object item in tree.SelectedItems
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
            var search = input.Text;
            if (search.Length == 0) return;
            var separator = Path.PathSeparator;
            search = search.Replace('\\', separator).Replace('/', separator);
            if (openedFiles.Count > 0)
            {
                var matches = openedFiles;
                if (search.Length > 0) matches = SearchUtil.Matches(openedFiles, search);
                if (matches.Count > 0)
                {
                    tree.Items.AddRange(matches.ToArray());
                    if (settings.EnableItemSpacer) tree.Items.Add(settings.ItemSpacer);
                }
            }
            if (recentFiles.Count > 0)
            {
                var matches = recentFiles;
                if (search.Length > 0) matches = SearchUtil.Matches(matches, search);
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
            var prevSelectedIndex = selectedIndex;
            var lastIndex = tree.Items.Count - 1;
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
            var selectedIndices = tree.SelectedIndices;
            if (e.Shift)
            {
                if (selectedIndices.Contains(selectedIndex) && selectedIndices.Count > 1)
                    tree.SetSelected(prevSelectedIndex, false);
                else
                {
                    var index = selectedIndex;
                    var delta = selectedIndex - prevSelectedIndex;
                    var length = Math.Abs(delta);
                    for (var i = 0; i < length; i++)
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