using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ASCompletion;
using ASCompletion.Context;
using ASCompletion.Model;
using PluginCore;

namespace QuickNavigate.Controls
{
    public partial class TypeExplorer : Form
    {
        private readonly List<string> projectTypes = new List<string>();
        private readonly List<string> openedTypes = new List<string>();
        private readonly Dictionary<string, ClassModel> typeToClassModel = new Dictionary<string, ClassModel>();
        private readonly Settings settings;
        private readonly Brush selectedNodeBrush = new SolidBrush(SystemColors.ControlDarkDark);
        private readonly Brush defaultNodeBrush;
        private readonly IComparer<string> comparer = new SmartTypeComparer();

        /// <summary>
        /// Initializes a new instance of the QuickNavigate.Controls.TypeExplorer
        /// </summary>
        /// <param name="settings"></param>
        public TypeExplorer(Settings settings)
        {
            this.settings = settings;
            Font = PluginBase.Settings.ConsoleFont;
            InitializeComponent();
            if (settings.TypeFormSize.Width > MinimumSize.Width) Size = settings.TypeFormSize;
            (PluginBase.MainForm as FlashDevelop.MainForm).ThemeControls(this);
            defaultNodeBrush = new SolidBrush(tree.BackColor);
            CreateItemsList();
            InitTree();
            RefreshTree();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                selectedNodeBrush.Dispose();
                if (defaultNodeBrush != null) defaultNodeBrush.Dispose();
                if (components != null) components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void CreateItemsList()
        {
            IASContext context = ASContext.GetLanguageContext(PluginBase.CurrentProject.Language);
            if (context == null) return;
            string projectFolder = Path.GetDirectoryName(PluginBase.CurrentProject.ProjectPath);
            bool onlyProjectTypes = !settings.SearchExternalClassPath;
            foreach (PathModel classpath in context.Classpath)
            {
                if (onlyProjectTypes)
                {
                    string path = classpath.Path;
                    if (!Path.IsPathRooted(classpath.Path)) path = Path.GetFullPath(Path.Combine(projectFolder, classpath.Path));
                    if (!path.StartsWith(projectFolder)) continue;
                }
                classpath.ForeachFile(FileModelDelegate);
            }
        }

        private bool FileModelDelegate(FileModel model)
        {
            foreach (ClassModel aClass in model.Classes)
            {
                string type = aClass.Type;
                if (typeToClassModel.ContainsKey(type)) continue;
                if (IsFileOpened(aClass.InFile.FileName)) openedTypes.Add(type);
                else projectTypes.Add(type);
                typeToClassModel.Add(type, aClass);
            }
            return true;
        }

        private bool IsFileOpened(string fileName)
        {
            return PluginBase.MainForm.Documents.Any(doc => doc.FileName == fileName);
        }

        private void InitTree()
        {
            ImageList icons = new ImageList {TransparentColor = Color.Transparent};
            icons.Images.AddRange(new Image[] {
                new Bitmap(PluginUI.GetStream("FilePlain.png")),
                new Bitmap(PluginUI.GetStream("FolderClosed.png")),
                new Bitmap(PluginUI.GetStream("FolderOpen.png")),
                new Bitmap(PluginUI.GetStream("CheckAS.png")),
                new Bitmap(PluginUI.GetStream("QuickBuild.png")),
                new Bitmap(PluginUI.GetStream("Package.png")),
                new Bitmap(PluginUI.GetStream("Interface.png")),
                new Bitmap(PluginUI.GetStream("Intrinsic.png")),
                new Bitmap(PluginUI.GetStream("Class.png")),
                new Bitmap(PluginUI.GetStream("Variable.png")),
                new Bitmap(PluginUI.GetStream("VariableProtected.png")),
                new Bitmap(PluginUI.GetStream("VariablePrivate.png")),
                new Bitmap(PluginUI.GetStream("VariableStatic.png")),
                new Bitmap(PluginUI.GetStream("VariableStaticProtected.png")),
                new Bitmap(PluginUI.GetStream("VariableStaticPrivate.png")),
                new Bitmap(PluginUI.GetStream("Const.png")),
                new Bitmap(PluginUI.GetStream("ConstProtected.png")),
                new Bitmap(PluginUI.GetStream("ConstPrivate.png")),
                new Bitmap(PluginUI.GetStream("Const.png")),
                new Bitmap(PluginUI.GetStream("ConstProtected.png")),
                new Bitmap(PluginUI.GetStream("ConstPrivate.png")),
                new Bitmap(PluginUI.GetStream("Method.png")),
                new Bitmap(PluginUI.GetStream("MethodProtected.png")),
                new Bitmap(PluginUI.GetStream("MethodPrivate.png")),
                new Bitmap(PluginUI.GetStream("MethodStatic.png")),
                new Bitmap(PluginUI.GetStream("MethodStaticProtected.png")),
                new Bitmap(PluginUI.GetStream("MethodStaticPrivate.png")),
                new Bitmap(PluginUI.GetStream("Property.png")),
                new Bitmap(PluginUI.GetStream("PropertyProtected.png")),
                new Bitmap(PluginUI.GetStream("PropertyPrivate.png")),
                new Bitmap(PluginUI.GetStream("PropertyStatic.png")),
                new Bitmap(PluginUI.GetStream("PropertyStaticProtected.png")),
                new Bitmap(PluginUI.GetStream("PropertyStaticPrivate.png")),
                new Bitmap(PluginUI.GetStream("Template.png")),
                new Bitmap(PluginUI.GetStream("Declaration.png"))
            });
            tree.ImageList = icons;
        }

        private void RefreshTree()
        {
            tree.BeginUpdate();
            tree.Nodes.Clear();
            FillTree();
            tree.ExpandAll();
            tree.EndUpdate();
        }

        private void FillTree()
        {
            List<string> matches;
            string search = input.Text.Trim();
            if (string.IsNullOrEmpty(search)) matches = openedTypes;
            else
            {   
                bool wholeWord = settings.TypeFormWholeWord;
                bool matchCase = settings.TypeFormMatchCase;
                ((SmartTypeComparer)comparer).Setup(search, !matchCase);
                matches = SearchUtil.Matches(openedTypes, search, ".", 0, wholeWord, matchCase);
                matches.Sort(comparer);
                if (settings.EnableItemSpacer && matches.Capacity > 0) matches.Add(settings.ItemSpacer);
                List<string> tmpMathes = SearchUtil.Matches(projectTypes, search, ".", settings.MaxItems, wholeWord, matchCase);
                tmpMathes.Sort(comparer);
                matches.AddRange(tmpMathes);
            }
            if (matches.Count == 0) return;
            foreach(string m in matches) 
            {
                ClassModel aClass = typeToClassModel[m];
                int icon = PluginUI.GetIcon(aClass.Flags, aClass.Access);
                tree.Nodes.Add(new TreeNode(){Text = m, ImageIndex = icon, SelectedImageIndex = icon});
            }
            tree.SelectedNode = tree.Nodes[0];
        }

        private void Navigate()
        {
            if (tree.SelectedNode == null) return;
            string selectedItem = tree.SelectedNode.Text;
            if (selectedItem == settings.ItemSpacer) return;
            ClassModel aClass = typeToClassModel[selectedItem];
            FileModel model = ModelsExplorer.Instance.OpenFile(aClass.InFile.FileName);
            if (model != null)
            {
                aClass = model.GetClassByName(aClass.Name);
                if (!aClass.IsVoid())
                {
                    int line = aClass.LineFrom;
                    ScintillaNet.ScintillaControl sci = PluginBase.MainForm.CurrentDocument.SciControl;
                    if (sci != null && line > 0 && line < sci.LineCount)
                        sci.GotoLine(line);
                }
            }
            Close();
        }

        #region Event Handlers

        private void OnKeyDown(object sender, KeyEventArgs e)
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

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            settings.TypeFormSize = Size;
        }

        private void OnInputTextChanged(object sender, EventArgs e)
        {
            RefreshTree();
        }

        private void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control || e.Shift || tree.SelectedNode == null) return;
            TreeNode node;
            int visibleCount = tree.VisibleCount - 1;
            switch (e.KeyCode)
            {
                case Keys.Down:
                    if (tree.SelectedNode.NextVisibleNode != null) tree.SelectedNode = tree.SelectedNode.NextVisibleNode;
                    else if (settings.WrapList) tree.SelectedNode = tree.Nodes[0];
                    break;
                case Keys.Up:
                    if (tree.SelectedNode.PrevVisibleNode != null) tree.SelectedNode = tree.SelectedNode.PrevVisibleNode;
                    else if (settings.WrapList)
                    {
                        node = tree.SelectedNode;
                        while (node.NextVisibleNode != null) node = node.NextVisibleNode;
                        tree.SelectedNode = node;
                    }
                    break;
                case Keys.Home:
                    tree.SelectedNode = tree.Nodes[0];
                    break;
                case Keys.End:
                    node = tree.SelectedNode;
                    while (node.NextVisibleNode != null) node = node.NextVisibleNode;
                    tree.SelectedNode = node;
                    break;
                case Keys.PageUp:
                    node = tree.SelectedNode;
                    for (int i = 0; i < visibleCount; i++)
                    {
                        if (node.PrevVisibleNode == null) break;
                        node = node.PrevVisibleNode;
                    }
                    tree.SelectedNode = node;
                    break;
                case Keys.PageDown:
                    node = tree.SelectedNode;
                    for (int i = 0; i < visibleCount; i++)
                    {
                        if (node.NextVisibleNode == null) break;
                        node = node.NextVisibleNode;
                    }
                    tree.SelectedNode = node;
                    break;
                default: return;
            }
            e.Handled = true;
        }

        private void OnInputKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (int)Keys.Space) e.Handled = true;
        }

        private void OnTreeNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Navigate();
        }

        private void OnTreeDrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            Brush fillBrush = defaultNodeBrush;
            Brush drawBrush = Brushes.Black;
            if ((e.State & TreeNodeStates.Selected) > 0)
            {
                fillBrush = selectedNodeBrush;
                drawBrush = Brushes.White;
            }
            Rectangle bounds = e.Bounds;
            e.Graphics.FillRectangle(fillBrush, bounds.X, bounds.Y, tree.Width - bounds.X, tree.ItemHeight);
            e.Graphics.DrawString(e.Node.Text, tree.Font, drawBrush, e.Bounds.Left, e.Bounds.Top, StringFormat.GenericDefault);
        }

        #endregion
    }

    class SmartTypeComparer : IComparer<string>
    {
        private string search;
        private bool noCase;

        public void Setup(string search, bool noCase)
        {
            if (noCase && !string.IsNullOrEmpty(search)) search = search.ToLower();
            this.search = search;
            this.noCase = noCase;
        }

        public int Compare(string x, string y)
        {
            int cmp = GetPkgLength(x).CompareTo(GetPkgLength(y)) * 100;
            x = GetName(x);
            y = GetName(y);
            int priority = GetPriority(y).CompareTo(GetPriority(x));
            if (priority != 0) return cmp + priority;
            return cmp + StringComparer.Ordinal.Compare(x, y);
        }

        private static int GetPkgLength(string type)
        {
            return type.LastIndexOf('.');
        }

        private static string GetName(string type)
        {
            int i = type.LastIndexOf('.');
            return i == -1 ? type : type.Substring(i + 1);
        }

        private int GetPriority(string name)
        {
            if (noCase) name = name.ToLower();
            if (name == search) return 100;
            return name.StartsWith(search) ? 90 : 0;
        }
    }
}