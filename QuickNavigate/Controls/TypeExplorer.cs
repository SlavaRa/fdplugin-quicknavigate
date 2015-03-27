using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ASCompletion;
using ASCompletion.Context;
using ASCompletion.Model;
using FlashDevelop;
using PluginCore;
using ScintillaNet;

namespace QuickNavigate.Controls
{
    /// <summary>
    /// </summary>
    /// <param name="model"></param>
    public delegate void ShowInHandler(Form sender, ClassModel model);

    /// <summary>
    /// </summary>
    public partial class TypeExplorer : Form
    {
        public event ShowInHandler ShowInQuickOutline;
        public event ShowInHandler ShowInClassHierarchy;
        public event ShowInHandler ShowInProjectManager;
        public event ShowInHandler ShowInFileExplorer;
        readonly List<string> projectTypes = new List<string>();
        readonly List<string> openedTypes = new List<string>();
        readonly Dictionary<string, ClassModel> typeToClassModel = new Dictionary<string, ClassModel>();
        readonly Settings settings;
        readonly Brush selectedNodeBrush = new SolidBrush(SystemColors.ControlDarkDark);
        readonly Brush defaultNodeBrush;
        readonly IComparer<string> comparer = new SmartTypeComparer();

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
            searchingInExternalClasspaths.Checked = settings.SearchExternalClassPath;
            ((MainForm)PluginBase.MainForm).ThemeControls(this);
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

        /// <summary>
        /// </summary>
        void CreateItemsList()
        {
            projectTypes.Clear();
            openedTypes.Clear();
            typeToClassModel.Clear();
            IASContext context = ASContext.GetLanguageContext(PluginBase.CurrentProject.Language);
            if (context == null) return;
            string projectFolder = Path.GetDirectoryName(PluginBase.CurrentProject.ProjectPath);
            bool onlyProjectTypes = !searchingInExternalClasspaths.Checked;
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

        /// <summary>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool FileModelDelegate(FileModel model)
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

        /// <summary>
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        static bool IsFileOpened(string fileName)
        {
            return PluginBase.MainForm.Documents.Any(doc => doc.FileName == fileName);
        }

        /// <summary>
        /// </summary>
        void InitTree()
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

        /// <summary>
        /// </summary>
        void RefreshTree()
        {
            tree.BeginUpdate();
            tree.Nodes.Clear();
            FillTree();
            tree.ExpandAll();
            tree.EndUpdate();
        }

        /// <summary>
        /// </summary>
        void FillTree()
        {
            List<string> matches;
            string search = input.Text.Trim();
            string itemSpacer = settings.ItemSpacer;
            if (string.IsNullOrEmpty(search)) matches = openedTypes;
            else
            {   
                bool wholeWord = settings.TypeFormWholeWord;
                bool matchCase = settings.TypeFormMatchCase;
                ((SmartTypeComparer)comparer).Setup(search, !matchCase);
                matches = SearchUtil.Matches(openedTypes, search, ".", 0, wholeWord, matchCase);
                matches.Sort(comparer);
                if (settings.EnableItemSpacer && matches.Capacity > 0) matches.Add(itemSpacer);
                List<string> tmpMathes = SearchUtil.Matches(projectTypes, search, ".", settings.MaxItems, wholeWord, matchCase);
                tmpMathes.Sort(comparer);
                matches.AddRange(tmpMathes);
            }
            if (matches.Count == 0) return;
            foreach (string m in matches)
            {
                if (m == itemSpacer) tree.Nodes.Add(new TreeNode(itemSpacer));
                else
                {
                    ClassModel aClass = typeToClassModel[m];
                    int icon = PluginUI.GetIcon(aClass.Flags, aClass.Access);
                    tree.Nodes.Add(new TypeNode(aClass) { Text = m, ImageIndex = icon, SelectedImageIndex = icon });
                }
            }
            tree.SelectedNode = tree.Nodes[0];
        }

        /// <summary>
        /// </summary>
        void Navigate()
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
                    ScintillaControl sci = PluginBase.MainForm.CurrentDocument.SciControl;
                    if (sci != null && line > 0 && line < sci.LineCount)
                        sci.GotoLine(line);
                }
            }
            Close();
        }

        /// <summary>
        /// Displays the shortcut menu.
        /// </summary>
        void ShowContextMenu()
        {
            TreeNode node = tree.SelectedNode;
            if (node == null || node.Text == settings.ItemSpacer) return;
            if (tree.ContextMenu == null) tree.ContextMenu = new ContextMenu();
            tree.ContextMenu.MenuItems.Clear();
            tree.ContextMenu.MenuItems.Add("Show in Quick &Outline", OnShowInQuickOutline);
            tree.ContextMenu.MenuItems.Add("Show in &Class Hierarchy", OnShowInClassHiearachy);
            tree.ContextMenu.MenuItems.Add("Show in &Project Manager", OnShowInProjectManager);
            if (File.Exists(((TypeNode)node).Model.InFile.FileName)) tree.ContextMenu.MenuItems.Add("Show in &File Explorer", OnShowInFileExplorer);
            tree.ContextMenu.Show(tree, new Point(node.Bounds.X, node.Bounds.Y + node.Bounds.Height));
        }

        #region Event Handlers

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
                case Keys.E:
                    if (e.Control) searchingInExternalClasspaths.Checked = !searchingInExternalClasspaths.Checked;
                    break;
                case Keys.L:
                    if (e.Control)
                    {
                        input.Focus();
                        input.SelectAll();
                    }
                    break;
               case Keys.Apps:
                    ShowContextMenu();
                    e.Handled = true;
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
            e.Handled = keyCode == (int)Keys.Space
                     || keyCode == 5  //Ctrl+E
                     || keyCode == 12;//Ctrl+L
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"/> that contains the event data. </param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            settings.TypeFormSize = Size;
            settings.SearchExternalClassPath = searchingInExternalClasspaths.Checked;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnInputTextChanged(object sender, EventArgs e)
        {
            RefreshTree();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnInputKeyDown(object sender, KeyEventArgs e)
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

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnSearchingModeCheckStateChanged(object sender, EventArgs e)
        {
            CreateItemsList();
            RefreshTree();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnTreeNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Navigate();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnTreeDrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            Brush fillBrush = defaultNodeBrush;
            Brush drawBrush = Brushes.Black;
            if ((e.State & TreeNodeStates.Selected) > 0)
            {
                fillBrush = selectedNodeBrush;
                drawBrush = Brushes.White;
            }
            Rectangle bounds = e.Bounds;
            string text = e.Node.Text;
            int x = text == settings.ItemSpacer ? 0 : bounds.X;
            e.Graphics.FillRectangle(fillBrush, x, bounds.Y, tree.Width - x, tree.ItemHeight);
            e.Graphics.DrawString(text, tree.Font, drawBrush, x, e.Bounds.Top, StringFormat.GenericDefault);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnShowInQuickOutline(object sender, EventArgs e)
        {
            ShowInQuickOutline(this, ((TypeNode)tree.SelectedNode).Model);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnShowInClassHiearachy(object sender, EventArgs e)
        {
            ShowInClassHierarchy(this, ((TypeNode)tree.SelectedNode).Model);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnShowInProjectManager(object sender, EventArgs e)
        {
            ShowInProjectManager(this, ((TypeNode)tree.SelectedNode).Model);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnShowInFileExplorer(object sender, EventArgs e)
        {
            ShowInFileExplorer(this, ((TypeNode)tree.SelectedNode).Model);
        }

        #endregion
    }

    /// <summary>
    /// </summary>
    class TypeNode : TreeNode
    {
        public ClassModel Model;

        public TypeNode(ClassModel model)
        {
            Model = model;
        }
    }

    /// <summary>
    /// </summary>
    class SmartTypeComparer : IComparer<string>
    {
        string search;
        bool noCase;

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

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static int GetPkgLength(string type)
        {
            return type.LastIndexOf('.');
        }

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static string GetName(string type)
        {
            int i = type.LastIndexOf('.');
            return i == -1 ? type : type.Substring(i + 1);
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        int GetPriority(string name)
        {
            if (noCase) name = name.ToLower();
            if (name == search) return 100;
            return name.StartsWith(search) ? 90 : 0;
        }
    }
}