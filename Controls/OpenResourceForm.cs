using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PluginCore;

namespace QuickNavigatePlugin
{
    public partial class OpenResourceForm : Form
    {
        private const int MAX_ITEMS = 100;

        private List<string> projectFiles;
        private List<string> openedFiles;
        private Font nameFont;
        private Font pathFont;
        private PluginMain plugin;
        private string defaultMessage;

        public OpenResourceForm(PluginMain plugin)
        {
            this.plugin = plugin;
            InitializeComponent();

            if ((plugin.Settings as Settings).ResourceFormSize.Width > MinimumSize.Width)
                Size = (plugin.Settings as Settings).ResourceFormSize;

            pathFont = new Font(listBox.Font.Name, listBox.Font.Size, FontStyle.Regular);
            nameFont = new Font("Courier New", 10, FontStyle.Regular);

            defaultMessage = messageLabel.Text;
            refreshButton.Image = PluginBase.MainForm.FindImage("66");
            new ToolTip().SetToolTip(refreshButton, "Ctrl+R");

            LoadFileList();
        }

        private void listBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                e.Graphics.FillRectangle(Brushes.LightSkyBlue, e.Bounds);
            else
                e.Graphics.FillRectangle(new SolidBrush(listBox.BackColor), e.Bounds);

            if (e.Index >= 0)
            {
                var fullName = (string) listBox.Items[e.Index];

                int slashIndex = fullName.LastIndexOf('\\');
                string path = " " + fullName.Substring(0, slashIndex + 1);
                string name = fullName.Substring(slashIndex + 1);

                int pathSize = (int) e.Graphics.MeasureString(path, pathFont).Width - 2;
                var nameBounds = new Rectangle(e.Bounds.X + pathSize, e.Bounds.Y, e.Bounds.Width - pathSize, e.Bounds.Height);

                e.Graphics.DrawString(path, pathFont, Brushes.Gray, e.Bounds);
                e.Graphics.DrawString(name, nameFont, Brushes.Black, nameBounds);
                e.DrawFocusRectangle();
            }
        }

        private void RefreshListBox()
        {
            listBox.BeginUpdate();
            listBox.Items.Clear();
            fillListBox();
            if (listBox.Items.Count > 0)
                listBox.SelectedIndex = 0;

            listBox.EndUpdate();
        }

        private void fillListBox()
        {
            List<string> matchedItems;

            if (textBox.Text.Length > 0)
            {
                matchedItems = SearchUtil.getMatchedItems(openedFiles, textBox.Text, "\\", 0);
                if (matchedItems.Capacity > 0)
                    matchedItems.Add("-----------------");

                matchedItems.AddRange(SearchUtil.getMatchedItems(projectFiles, textBox.Text, "\\", MAX_ITEMS));
            }
            else matchedItems = openedFiles;

            foreach (string item in matchedItems)
                listBox.Items.Add(item);
        }

        private void LoadFileList()
        {
            openedFiles = new List<string>();
            projectFiles = new List<string>();
            ShowMessage("Reading project files...");
            worker.RunWorkerAsync();
        }

        private void RebuildJob()
        {
            List<string> allFiles = plugin.GetProjectFiles();

            foreach (string file in allFiles)
            {
                if (isFileHidden(file))
                    continue;

                if (plugin.isFileOpened(file))
                    openedFiles.Add(PluginBase.CurrentProject.GetRelativePath(file));
                else
                    projectFiles.Add(PluginBase.CurrentProject.GetRelativePath(file));
            }
        }

        private bool isFileHidden(String file)
        {
            string path = System.IO.Path.GetDirectoryName(file);
            string name = System.IO.Path.GetFileName(file);
            return path.Contains(".svn")
                || path.Contains(".cvs")
                || path.Contains(".git")
                || name.Substring(0, 1) == ".";
        }

        private void Navigate()
        {
            if (listBox.SelectedItem != null)
            {
                string file = PluginBase.CurrentProject.GetAbsolutePath((string)listBox.SelectedItem);
                PluginBase.MainForm.OpenEditableDocument(file);
                Close();
            }
        }
        
        private void ShowMessage(string text)
        {
            listBox.Items.Clear();
            listBox.Items.Add(text);
        }
        
        #region eventHandlers

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && listBox.SelectedIndex < listBox.Items.Count - 1)
            {
                listBox.SelectedIndex++;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up && listBox.SelectedIndex > 0)
            {
                listBox.SelectedIndex--;
                e.Handled = true;
            }
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            if (!worker.IsBusy)
                RefreshListBox();
        }

        private void listBox_DoubleClick(object sender, EventArgs e)
        {
            Navigate();
        }

        private void listBox_Resize(object sender, EventArgs e)
        {
            listBox.Refresh();
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            if (!worker.IsBusy)
            {
                plugin.invalidateCache();
                LoadFileList();
            }
        }

        private void OpenResourceForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
            else if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                Navigate();
            }
            else if (e.Control && e.KeyCode == Keys.R && !worker.IsBusy)
            {
                plugin.invalidateCache();
                LoadFileList();
            }
        }

        private void OpenResourceForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            (plugin.Settings as Settings).ResourceFormSize = Size;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            RebuildJob();
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RefreshListBox();
        }
        
        #endregion
    }
}