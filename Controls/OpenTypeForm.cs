using ASCompletion;
using ASCompletion.Context;
using ASCompletion.Model;
using PluginCore;
using PluginCore.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace QuickNavigatePlugin
{
    public partial class OpenTypeForm : Form
    {
        private const int MAX_ITEMS = 100;
        private const string ITEM_SPACER = "-----------------";
        private readonly List<string> projectTypes = new List<string>();
        private readonly List<string> openedTypes = new List<string>();
        private readonly Dictionary<string, ClassModel> dictionary = new Dictionary<string,ClassModel>();
        private readonly Settings settings;

        public OpenTypeForm(Settings settings)
        {
            this.settings = settings;
            Font = PluginBase.Settings.ConsoleFont;
            InitializeComponent();
            if (settings.TypeFormSize.Width > MinimumSize.Width) Size = settings.TypeFormSize;
            (PluginBase.MainForm as FlashDevelop.MainForm).ThemeControls(this);
            CreateItemsList();
            RefreshListBox();
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
            bool wholeWord = settings.TypeFormWholeWord;
            bool matchCase = settings.TypeFormMatchCase;
            List<string> matchedItems;
            if (textBox.Text.Length > 0)
            {
                matchedItems = SearchUtil.GetMatchedItems(openedTypes, textBox.Text, ".", 0, wholeWord, matchCase);
                if (matchedItems.Capacity > 0) matchedItems.Add(ITEM_SPACER);
                matchedItems.AddRange(SearchUtil.GetMatchedItems(projectTypes, textBox.Text, ".", MAX_ITEMS, wholeWord, matchCase));
            }
            else matchedItems = openedTypes;
            listBox.Items.AddRange(matchedItems.ToArray());
        }

        private void CreateItemsList()
        {
            projectTypes.Clear();
            openedTypes.Clear();
            dictionary.Clear();
            IASContext context = ASContext.GetLanguageContext(PluginBase.CurrentProject.Language);
            if (context == null) return;
            foreach (PathModel path in context.Classpath)
            {
                path.ForeachFile(FileModelDelegate);
            }
        }

        private bool FileModelDelegate(FileModel model)
        {
            foreach (ClassModel classModel in model.Classes)
            {
                string qualifiedName = classModel.QualifiedName;
                if (dictionary.ContainsKey(qualifiedName)) continue;
                if (SearchUtil.IsFileOpened(classModel.InFile.FileName)) openedTypes.Add(qualifiedName);
                else projectTypes.Add(qualifiedName);
                dictionary.Add(qualifiedName, classModel);
            }
            return true;
        }

        private void Navigate()
        {
            if (listBox.SelectedItem == null) return;
            string selectedItem = listBox.SelectedItem.ToString();
            if (selectedItem == ITEM_SPACER) return;
            ClassModel classModel = dictionary[selectedItem];
            FileModel model = ModelsExplorer.Instance.OpenFile(classModel.InFile.FileName);
            if (model != null)
            {
                ClassModel theClass = model.GetClassByName(classModel.Name);
                if (!theClass.IsVoid())
                {
                    int line = theClass.LineFrom;
                    ScintillaNet.ScintillaControl sci = PluginBase.MainForm.CurrentDocument.SciControl;
                    if (sci != null && line > 0 && line < sci.LineCount)
                        sci.GotoLine(line);
                }
            }
            Close();
        }

        #region Event Handlers

        private void OpenTypeForm_KeyDown(object sender, KeyEventArgs e)
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
            }
        }

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
            RefreshListBox();
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
                int slashIndex = fullName.LastIndexOf('.');
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

        private void OpenTypeForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            settings.TypeFormSize = Size;
        }

        #endregion

    }
}