using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PluginCore;
using ASCompletion.Context;
using ASCompletion.Model;
using ASCompletion;

namespace QuickNavigatePlugin
{
    public partial class OpenTypeForm : Form
    {
        private const int MAX_ITEMS = 100;
        
        private readonly List<string> projectTypes = new List<string>();
        private readonly List<string> openedTypes = new List<string>();
        private Font nameFont;
        private Font pathFont;
        private PluginMain plugin;
        private readonly Dictionary<string, ClassModel> dictionary = new Dictionary<string,ClassModel>();
        private IASContext context;

        public OpenTypeForm(PluginMain plugin)
        {
            this.plugin = plugin;
            InitializeComponent();

            if ((plugin.Settings as Settings).TypeFormSize.Width > MinimumSize.Width)
                Size = (plugin.Settings as Settings).TypeFormSize;

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

        private void listBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                e.Graphics.FillRectangle(Brushes.LightSkyBlue, e.Bounds);
            else
                e.Graphics.FillRectangle(new SolidBrush(listBox.BackColor), e.Bounds);

            if (e.Index >= 0)
            {
                var fullName = (string) listBox.Items[e.Index];

                int slashIndex = fullName.LastIndexOf('.');
                string path = " " + fullName.Substring(0, slashIndex + 1);
                string name = fullName.Substring(slashIndex + 1);

                int pathSize = (int) e.Graphics.MeasureString(path, pathFont).Width - 4;
                var nameBounds = new Rectangle(e.Bounds.X + pathSize, e.Bounds.Y + 1, e.Bounds.Width - pathSize, e.Bounds.Height + 1);

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
                matchedItems = SearchUtil.getMatchedItems(openedTypes, textBox.Text, ".", 0);
                if (matchedItems.Capacity > 0)
                    matchedItems.Add("-----------------");

                matchedItems.AddRange(SearchUtil.getMatchedItems(projectTypes, textBox.Text, ".", MAX_ITEMS));
            }
            else matchedItems = openedTypes;

            foreach (string item in matchedItems)
                listBox.Items.Add(item);
        }

        private void CreateItemsList()
        {
            projectTypes.Clear();
            openedTypes.Clear();
            dictionary.Clear();

            if (context == null || context.Classpath == null || context.Classpath.Count == 0)
                return;

            foreach (PathModel path in context.Classpath)
                path.ForeachFile(fileModelDelegate);
        }

        private bool fileModelDelegate(FileModel model)
        {
            foreach (ClassModel classModel in model.Classes)
            {
                if (!dictionary.ContainsKey(classModel.QualifiedName))
                {
                    bool isFileOpened = plugin.isFileOpened(classModel.InFile.FileName);

                    if (isFileOpened)
                        openedTypes.Add(classModel.QualifiedName);
                    else
                        projectTypes.Add(classModel.QualifiedName);

                    dictionary.Add(classModel.QualifiedName, classModel);
                }
            }
            
            return true;
        }


        private void Navigate()
        {
            if (listBox.SelectedItem == null)
                return;

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

        private void OpenTypeForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
            else if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                Navigate();
            }
        }

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

        private void OpenTypeForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            (plugin.Settings as Settings).TypeFormSize = Size;
        }
    }
}