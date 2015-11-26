using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PluginCore;

namespace QuickNavigate.Forms
{
    public sealed partial class OpenRecentFileForm : Form
    {
        readonly Settings settings;
        readonly List<string> recentFiles;
        readonly List<string> openedFiles;

        static List<string> GetOpenedFiles(ICollection<string> documents)
        {
            return (from document in PluginBase.MainForm.Documents
                    where documents.Contains(document.FileName)
                    select document.FileName).ToList();
        }

        public OpenRecentFileForm(Settings settings)
        {
            this.settings = settings;
            InitializeComponent();
            Font = PluginBase.Settings.DefaultFont;
            tree.ItemHeight = tree.Font.Height;
            if (settings.RecentFilesSize.Width > MinimumSize.Width) Size = settings.RecentFilesSize;
            recentFiles = PluginBase.MainForm.Settings.PreviousDocuments.Where(File.Exists).ToList();
            openedFiles = GetOpenedFiles(recentFiles);
            RefrestTree();
        }

        void RefrestTree()
        {
            tree.BeginUpdate();
            tree.Items.Clear();
            FillTree();
            if (tree.Items.Count > 0) tree.SelectedIndex = 0;
            tree.EndUpdate();
        }

        void FillTree()
        {
            string separator = Path.PathSeparator.ToString();
            int maxItems = settings.MaxItems;
            bool wholeWord = settings.RecentFilesWholeWord;
            bool matchCase = settings.RecentFilesMatchCase;
            string search = input.Text;
            List<string> matches = openedFiles;
            if (search.Length > 0) matches = SearchUtil.Matches(openedFiles, search, separator, maxItems, wholeWord, matchCase);
            tree.Items.AddRange(matches.ToArray());
            if (matches.Capacity > 0 && settings.EnableItemSpacer) tree.Items.Add(settings.ItemSpacer);
            matches = (from it in recentFiles where !openedFiles.Contains(it) select it).ToList();
            if (search.Length > 0) matches = SearchUtil.Matches(matches, search, separator, maxItems, wholeWord, matchCase);
            tree.Items.AddRange(matches.ToArray());
        }

        void Navigate()
        {
            if (tree.SelectedItem == null) return;
            string file = PluginBase.CurrentProject.GetAbsolutePath((string)tree.SelectedItem);
            ((Form)PluginBase.MainForm).BeginInvoke((MethodInvoker)delegate
            {
                ProjectManager.PluginMain plugin = (ProjectManager.PluginMain)PluginBase.MainForm.FindPlugin("30018864-fadd-1122-b2a5-779832cbbf23");
                plugin.OpenFile(file);
            });
            Close();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"/> that contains the event data. </param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    Close();
                    break;
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

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyPress"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyPressEventArgs"/> that contains the event data. </param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            int keyCode = e.KeyChar;
            e.Handled = keyCode == 12; //Ctrl+L
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"/> that contains the event data. </param>
        protected override void OnFormClosing(FormClosingEventArgs e) => settings.RecentFilesSize = Size;

        void OnInputTextChanged(object sender, EventArgs e) => RefrestTree();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control || e.Shift || tree.SelectedItem == null) return;
            int count = tree.Items.Count - 1;
            if (count <= 1) return;
            int index = tree.SelectedIndex;
            switch (e.KeyCode)
            {
                case Keys.Down:
                    if (index < count) tree.SelectedItem = tree.Items[index + 1];
                    else if (settings.WrapList) tree.SelectedItem = tree.Items[0];
                    break;
                case Keys.Home:
                    tree.SelectedItem = tree.Items[0];
                    break;
                case Keys.Up:
                    if (index > 0) tree.SelectedItem = tree.Items[index - 1];
                    else if (settings.WrapList) tree.SelectedItem = tree.Items[count];
                    break;
                case Keys.End:
                    tree.SelectedItem = tree.Items[count];
                    break;
                default: return;
            }
            e.Handled = true;
        }

        void OnTreeMouseDoubleClick(object sender, MouseEventArgs e) => Navigate();
    }
}