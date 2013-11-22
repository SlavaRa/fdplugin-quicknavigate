using ASCompletion;
using ASCompletion.Context;
using ASCompletion.Model;
using PluginCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace QuickNavigatePlugin
{
    public partial class OpenTypeForm : Form
    {
        private const int MAX_ITEMS = 100;
        
        private readonly List<string> projectTypes = new List<string>();
        private readonly List<string> openedTypes = new List<string>();
        private readonly Dictionary<string, ClassModel> dictionary = new Dictionary<string,ClassModel>();
        private Settings settings;
        private Font nameFont;
        private Font pathFont;
        private IASContext context;

        public OpenTypeForm(Settings settings)
        {
            this.settings = settings;
            InitializeComponent();

            if (settings.TypeFormSize.Width > MinimumSize.Width) Size = settings.TypeFormSize;

            pathFont = new Font(listBox.Font.Name, listBox.Font.Size, FontStyle.Regular);
            nameFont = new Font("Courier New", 10, FontStyle.Regular);

            DetectContext();
            CreateItemsList();
            RefreshListBox();
        }

        private void DetectContext()
        {
            context = ASContext.Context;
            if (PluginBase.CurrentProject != null)
                context = ASContext.GetLanguageContext(PluginBase.CurrentProject.Language);
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
                if (matchedItems.Capacity > 0) matchedItems.Add("-----------------");

                matchedItems.AddRange(SearchUtil.GetMatchedItems(projectTypes, textBox.Text, ".", MAX_ITEMS, wholeWord, matchCase));
            }
            else matchedItems = openedTypes;

            foreach (string item in matchedItems)
            {
                listBox.Items.Add(item);
            }
        }

        private void CreateItemsList()
        {
            projectTypes.Clear();
            openedTypes.Clear();
            dictionary.Clear();

            if (context == null || context.Classpath == null || context.Classpath.Count == 0)
                return;

            foreach (PathModel path in context.Classpath)
            {
                path.ForeachFile(FileModelDelegate);
            }
        }

        private bool FileModelDelegate(FileModel model)
        {
            foreach (ClassModel classModel in model.Classes)
            {
                if (dictionary.ContainsKey(classModel.QualifiedName))
                    continue;

                bool isFileOpened = SearchUtil.IsFileOpened(classModel.InFile.FileName);

                if (isFileOpened) openedTypes.Add(classModel.QualifiedName);
                else projectTypes.Add(classModel.QualifiedName);

                dictionary.Add(classModel.QualifiedName, classModel);
            }
            
            return true;
        }

        private void Navigate()
        {
            if (listBox.SelectedItem == null) return;

            ClassModel classModel = dictionary[listBox.SelectedItem.ToString()];
            FileModel model = ModelsExplorer.Instance.OpenFile(classModel.InFile.FileName);
            if (model != null)
            {
                ClassModel theClass = model.GetClassByName(classModel.Name);
                if (!theClass.IsVoid())
                {
                    int line = theClass.LineFrom;
                    ScintillaNet.ScintillaControl sci = PluginBase.MainForm.CurrentDocument.SciControl;
                    if (sci != null && !theClass.IsVoid() && line > 0 && line < sci.LineCount)
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
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                e.Graphics.FillRectangle(Brushes.LightSkyBlue, e.Bounds);
            else
                e.Graphics.FillRectangle(new SolidBrush(listBox.BackColor), e.Bounds);

            if (e.Index >= 0)
            {
                var fullName = (string)listBox.Items[e.Index];

                int slashIndex = fullName.LastIndexOf('.');
                string path = " " + fullName.Substring(0, slashIndex + 1);
                string name = fullName.Substring(slashIndex + 1);

                int pathSize = (int)e.Graphics.MeasureString(path, pathFont).Width - 4;
                var nameBounds = new Rectangle(e.Bounds.X + pathSize, e.Bounds.Y + 1, e.Bounds.Width - pathSize, e.Bounds.Height + 1);

                e.Graphics.DrawString(path, pathFont, Brushes.Gray, e.Bounds);
                e.Graphics.DrawString(name, nameFont, Brushes.Black, nameBounds);
                e.DrawFocusRectangle();
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