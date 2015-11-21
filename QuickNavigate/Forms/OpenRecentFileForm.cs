using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PluginCore;

namespace QuickNavigate.Forms
{
    public partial class OpenRecentFileForm : Form
    {
        readonly Settings settings;

        public OpenRecentFileForm(Settings settings)
        {
            this.settings = settings;
            InitializeComponent();
            if (settings.RecentFilesSize.Width > MinimumSize.Width) Size = settings.RecentFilesSize;
            Font = PluginBase.Settings.DefaultFont;
            tree.ItemHeight = tree.Font.Height;
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
            List<string> matches = PluginBase.MainForm.Settings.PreviousDocuments;
            string search = input.Text;
            if (search.Length > 0) matches = SearchUtil.Matches(matches, search, Path.PathSeparator.ToString(), settings.MaxItems, settings.RecentFilesWholeWord, settings.RecentFilesMatchCase);
            foreach (string file in matches.Where(File.Exists))
                tree.Items.Add(file);
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
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            settings.RecentFilesSize = new Size(Size.Width, Size.Height);
        }

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