using PluginCore;
using PluginCore.Helpers;
using ProjectManager.Projects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace QuickNavigatePlugin
{
    public partial class OpenResourceForm : Form
    {
        private const int MAX_ITEMS = 100;
        private const string ITEM_SPACER = "-----------------";
        private readonly List<string> projectFiles = new List<string>();
        private readonly List<string> openedFiles = new List<string>();
        private readonly Settings settings;

        public OpenResourceForm(Settings settings)
        {
            this.settings = settings;
            Font = PluginBase.Settings.ConsoleFont;
            InitializeComponent();
            if (settings.ResourceFormSize.Width > MinimumSize.Width) Size = settings.ResourceFormSize;
            (PluginBase.MainForm as FlashDevelop.MainForm).ThemeControls(this);
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
                if (matchedItems.Capacity > 0) matchedItems.Add(ITEM_SPACER);
                matchedItems.AddRange(SearchUtil.GetMatchedItems(projectFiles, textBox.Text, "\\", MAX_ITEMS, wholeWord, matchCase));
            }
            else matchedItems = openedFiles;
            listBox.Items.AddRange(matchedItems.ToArray());
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
            foreach (string file in GetProjectFiles())
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
            if (!settings.ResourcesCaching || projectFiles.Count == 0)
            {
                projectFiles.Clear();
                foreach (string folder in GetProjectFolders())
                    if (Directory.Exists(folder))
                        projectFiles.AddRange(Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories));
            }
            return projectFiles;
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
            if (project.Language.StartsWith("haxe"))
            {
                folders.AddRange((project as Project).AdditionalPaths);

                string haxePath = Environment.ExpandEnvironmentVariables("%HAXEPATH%");
                if (!string.IsNullOrEmpty(haxePath)) folders.Add(Path.Combine(haxePath, "std"));
            }
            return folders;
        }

        #region Event Handlers

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    if (listBox.SelectedIndex < listBox.Items.Count - 1)
                    {
                        listBox.SelectedIndex++;
                        e.Handled = true;
                    }
                    break;

                case Keys.Up:
                    if (listBox.SelectedIndex > 0)
                    {
                        listBox.SelectedIndex--;
                        e.Handled = true;
                    }
                    break;
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
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            if (selected) e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
            else e.Graphics.FillRectangle(new SolidBrush(listBox.BackColor), e.Bounds);
            if (e.Index >= 0)
            {
                string fullName = (string)listBox.Items[e.Index];
                int slashIndex = fullName.LastIndexOf(Path.DirectorySeparatorChar);
                string path = fullName.Substring(0, slashIndex + 1);
                string name = fullName.Substring(slashIndex + 1);
                int pathSize = DrawHelper.MeasureDisplayStringWidth(e.Graphics, path, e.Font) - 2;
                if (pathSize < 0) pathSize = 0; // No negative padding...
                if (selected)
                {
                    e.Graphics.DrawString(path, e.Font, Brushes.LightGray, e.Bounds.Left, e.Bounds.Top, StringFormat.GenericDefault);
                    e.Graphics.DrawString(name, e.Font, Brushes.White, e.Bounds.Left + pathSize, e.Bounds.Top, StringFormat.GenericDefault);
                }
                else
                {
                    e.Graphics.DrawString(path, e.Font, Brushes.Gray, e.Bounds.Left, e.Bounds.Top, StringFormat.GenericDefault);
                    e.Graphics.DrawString(name, e.Font, Brushes.Black, e.Bounds.Left + pathSize, e.Bounds.Top, StringFormat.GenericDefault);
                }
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
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    Close();
                    break;

                case Keys.Enter:
                    e.Handled = true;
                    Navigate();
                    break;

                case Keys.R:
                    if (e.Control && !worker.IsBusy) LoadFileList();
                    break;
            }
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