using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PluginCore;
using System.IO;

namespace QuickNavigatePlugin
{
    public partial class OpenResourceForm : Form
    {
        private const int MAX_ITEMS = 100;

        private readonly List<string> projectFiles = new List<string>();
        private readonly List<string> openedFiles = new List<string>();
        private Settings settings;
        private Font nameFont;
        private Font pathFont;

        public OpenResourceForm(Settings settings)
        {
            this.settings = settings;
            InitializeComponent();

            if (settings.ResourceFormSize.Width > MinimumSize.Width) Size = settings.ResourceFormSize;

            pathFont = new Font(listBox.Font.Name, listBox.Font.Size, FontStyle.Regular);
            nameFont = new Font("Courier New", 10, FontStyle.Regular);

            refreshButton.Image = PluginBase.MainForm.FindImage("66");
            new ToolTip().SetToolTip(refreshButton, "Ctrl+R");

            LoadFileList();
        }

        private void RefreshListBox()
        {
            listBox.BeginUpdate();
            listBox.Items.Clear();
            FillListBox();

            if (listBox.Items.Count > 0) listBox.SelectedIndex = 0;

            listBox.EndUpdate();
        }

        private void FillListBox()
        {
            bool wholeWord = settings.ResourceFormWholeWord;
            bool matchCase = settings.ResourceFormMatchCase;

            List<string> matchedItems;

            if (textBox.Text.Length > 0)
            {
                matchedItems = SearchUtil.GetMatchedItems(openedFiles, textBox.Text, "\\", 0, wholeWord, matchCase);
                if (matchedItems.Capacity > 0) matchedItems.Add("-----------------");

                matchedItems.AddRange(SearchUtil.GetMatchedItems(projectFiles, textBox.Text, "\\", MAX_ITEMS, wholeWord, matchCase));
            }
            else matchedItems = openedFiles;

            foreach (string item in matchedItems)
            {
                listBox.Items.Add(item);
            }
        }

        private void LoadFileList()
        {
            openedFiles.Clear();
            projectFiles.Clear();
            ShowMessage("Reading project files...");
            worker.RunWorkerAsync();
        }

        private void RebuildJob()
        {
            IProject project = PluginBase.CurrentProject;
            List<string> allFiles = GetProjectFiles();

            foreach (string file in allFiles)
            {
                if (IsFileHidden(file)) continue;

                if (SearchUtil.IsFileOpened(file)) openedFiles.Add(project.GetRelativePath(file));
                else projectFiles.Add(project.GetRelativePath(file));
            }
        }

        private bool IsFileHidden(string file)
        {
            string path = Path.GetDirectoryName(file);
            string name = Path.GetFileName(file);
            return path.Contains(".svn") || path.Contains(".cvs") || path.Contains(".git") || name.Substring(0, 1) == ".";
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

        /// <summary>
        /// 
        /// </summary>
        public List<string> GetProjectFiles()
        {
            if (!settings.ResourcesCaching || projectFiles.Count == 0) reloadProjectFiles();
            return projectFiles;
        }

        /// <summary>
        /// 
        /// </summary>
        private void reloadProjectFiles()
        {
            projectFiles.Clear();

            List<string> folders = GetProjectFolders();
            foreach (string folder in folders)
                if (Directory.Exists(folder))
                    projectFiles.AddRange(Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories));
        }

        /// <summary>
        /// 
        /// </summary>
        public List<string> GetProjectFolders()
        {
            List<string> folders = new List<string>();
            IProject project = PluginBase.CurrentProject;
            if (project == null) return folders;

            string projectFolder = Path.GetDirectoryName(project.ProjectPath);
            folders.Add(projectFolder);

            if (!settings.SearchExternalClassPath) return folders;

            foreach (string path in project.SourcePaths)
            {
                if (Path.IsPathRooted(path)) folders.Add(path);
                else
                {
                    string folder = Path.GetFullPath(Path.Combine(projectFolder, path));
                    if (!folder.StartsWith(projectFolder)) folders.Add(folder);
                }
            }

            return folders;
        }

        #region eventHandlers

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
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

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            if (!worker.IsBusy) RefreshListBox();
        }

        private void ListBox_DoubleClick(object sender, EventArgs e)
        {
            Navigate();
        }

        private void ListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                e.Graphics.FillRectangle(Brushes.LightSkyBlue, e.Bounds);
            else
                e.Graphics.FillRectangle(new SolidBrush(listBox.BackColor), e.Bounds);

            if (e.Index >= 0)
            {
                var fullName = (string)listBox.Items[e.Index];

                int slashIndex = fullName.LastIndexOf('\\');
                string path = " " + fullName.Substring(0, slashIndex + 1);
                string name = fullName.Substring(slashIndex + 1);

                int pathSize = (int)e.Graphics.MeasureString(path, pathFont).Width - 2;
                var nameBounds = new Rectangle(e.Bounds.X + pathSize, e.Bounds.Y, e.Bounds.Width - pathSize, e.Bounds.Height);

                e.Graphics.DrawString(path, pathFont, Brushes.Gray, e.Bounds);
                e.Graphics.DrawString(name, nameFont, Brushes.Black, nameBounds);
                e.DrawFocusRectangle();
            }
        }

        private void ListBox_Resize(object sender, EventArgs e)
        {
            listBox.Refresh();
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            if (!worker.IsBusy) LoadFileList();
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
                LoadFileList();
        }

        private void OpenResourceForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            settings.ResourceFormSize = Size;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            RebuildJob();
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RefreshListBox();
        }
        
        #endregion
    }
}