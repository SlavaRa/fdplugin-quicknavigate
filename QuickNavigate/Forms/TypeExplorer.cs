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
using QuickNavigate.Collections;

namespace QuickNavigate.Forms
{
    /// <summary>
    /// </summary>
    public partial class TypeExplorer : ClassModelExplorerForm
    {
        readonly List<string> projectTypes = new List<string>();
        readonly List<string> openedTypes = new List<string>();
        static readonly Dictionary<string, ClassModel> TypeToClassModel = new Dictionary<string, ClassModel>();
        readonly Brush defaultNodeBrush;

        /// <summary>
        /// Initializes a new instance of the QuickNavigate.Controls.TypeExplorer
        /// </summary>
        /// <param name="settings"></param>
        public TypeExplorer(Settings settings) : base(settings)
        {
            InitializeComponent();
            if (settings.TypeFormSize.Width > MinimumSize.Width) Size = settings.TypeFormSize;
            searchingInExternalClasspaths.Checked = settings.SearchExternalClassPath;
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
                defaultNodeBrush?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// </summary>
        void CreateItemsList()
        {
            projectTypes.Clear();
            openedTypes.Clear();
            TypeToClassModel.Clear();
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
        /// <returns>true</returns>
        bool FileModelDelegate(FileModel model)
        {
            foreach (ClassModel aClass in model.Classes)
            {
                string type = aClass.Type;
                if (TypeToClassModel.ContainsKey(type)) continue;
                if (IsFileOpened(aClass.InFile.FileName)) openedTypes.Add(type);
                else projectTypes.Add(type);
                TypeToClassModel.Add(type, aClass);
            }
            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        static bool IsFileOpened(string fileName) => PluginBase.MainForm.Documents.Any(doc => doc.FileName == fileName);

        /// <summary>
        /// </summary>
        protected override void InitTree()
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
        protected override void RefreshTree()
        {
            tree.BeginUpdate();
            tree.Nodes.Clear();
            FillTree();
            tree.ExpandAll();
            tree.EndUpdate();
        }

        /// <summary>
        /// </summary>
        protected override void FillTree()
        {
            string search = input.Text.Trim();
            if (string.IsNullOrEmpty(search) && openedTypes.Count > 0) tree.Nodes.AddRange(CreateNodes(openedTypes, string.Empty).ToArray());
            else
            {   
                bool wholeWord = Settings.TypeFormWholeWord;
                bool matchCase = Settings.TypeFormMatchCase;
                var matches = SearchUtil.Matches(openedTypes, search, ".", 0, wholeWord, matchCase);
                if (matches.Count > 0) tree.Nodes.AddRange(CreateNodes(matches, search).ToArray());
                if (Settings.EnableItemSpacer && matches.Capacity > 0) tree.Nodes.Add(Settings.ItemSpacer);
                matches = SearchUtil.Matches(projectTypes, search, ".", Settings.MaxItems, wholeWord, matchCase);
                if (matches.Count > 0) tree.Nodes.AddRange(CreateNodes(matches, search).ToArray());
            }
            if (tree.Nodes.Count > 0) tree.SelectedNode = tree.Nodes[0];
        }

        /// <summary>
        /// </summary>
        /// <param name="matches"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        static IEnumerable<TypeNode> CreateNodes(IEnumerable<string> matches, string search) => SortNodes(matches.Select(CreateNode), search);

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static TypeNode CreateNode(string type)
        {
            ClassModel aClass = TypeToClassModel[type];
            return new TypeNode(aClass, PluginUI.GetIcon(aClass.Flags, aClass.Access));
        }

        /// <summary>
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        static IEnumerable<TypeNode> SortNodes(IEnumerable<TypeNode> nodes, string search)
        {
            search = search.ToLower();
            List<TypeNode> nodes0 = new List<TypeNode>();
            List<TypeNode> nodes1 = new List<TypeNode>();
            List<TypeNode> nodes2 = new List<TypeNode>();
            foreach (TypeNode node in nodes)
            {
                string name = node.Name.ToLower();
                if (name == search) nodes0.Add(node);
                else if (name.StartsWith(search)) nodes1.Add(node);
                else nodes2.Add(node);
            }
            nodes0.Sort(TypeExplorerNodeComparer.Package);
            nodes1.Sort(TypeExplorerNodeComparer.NameIgnoreCase);
            nodes2.Sort(TypeExplorerNodeComparer.NamePackageIgnoreCase);
            return nodes0.Concat(nodes1).Concat(nodes2);
        }

        /// <summary>
        /// Displays the shortcut menu.
        /// </summary>
        protected override void ShowContextMenu()
        {
            TypeNode node = tree.SelectedNode as TypeNode;
            if (node == null) return;
            ShowContextMenu(new Point(node.Bounds.X, node.Bounds.Y + node.Bounds.Height));
        }

        /// <summary>
        /// Displays the shortcut menu.
        /// </summary>
        protected override void ShowContextMenu(Point position)
        {
            TypeNode node = tree.SelectedNode as TypeNode;
            if (node == null) return;
            ContextMenuStrip.Items[4].Enabled = File.Exists(node.Model.InFile.FileName);
            ContextMenuStrip.Show(tree, position);
        }

        protected override void Navigate()
        {
            TypeNode node = tree.SelectedNode as TypeNode;
            if (node == null) return;
            base.Navigate(node);
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
                default:
                    base.OnKeyDown(e);
                    break;
            }
        }
        
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"/> that contains the event data. </param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Settings.TypeFormSize = Size;
            Settings.SearchExternalClassPath = searchingInExternalClasspaths.Checked;
        }

        protected override void OnTreeNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TypeNode node = e.Node as TypeNode;
            if (node == null) return;
            tree.SelectedNode = node;
            base.OnTreeNodeMouseClick(sender, e);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnInputTextChanged(object sender, EventArgs e) => RefreshTree();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnInputPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Apps) input.ContextMenu = tree.SelectedNode != null ? InputEmptyContextMenu : null;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Shift) return;
            TreeNode node;
            int visibleCount = tree.VisibleCount - 1;
            switch (e.KeyCode)
            {
                case Keys.Space:
                    e.Handled = true;
                    return;
                case Keys.E:
                case Keys.L:
                    e.Handled = e.Control;
                    return;
                case Keys.Down:
                    if (tree.SelectedNode.NextVisibleNode != null) tree.SelectedNode = tree.SelectedNode.NextVisibleNode;
                    else if (Settings.WrapList) tree.SelectedNode = tree.Nodes[0];
                    break;
                case Keys.Up:
                    if (tree.SelectedNode.PrevVisibleNode != null) tree.SelectedNode = tree.SelectedNode.PrevVisibleNode;
                    else if (Settings.WrapList)
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
        void OnTreeDrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            Brush fillBrush = defaultNodeBrush;
            Brush textBrush = Brushes.Black;
            Brush moduleBrush = Brushes.DimGray;
            if ((e.State & TreeNodeStates.Selected) > 0)
            {
                fillBrush = SelectedNodeBrush;
                textBrush = Brushes.White;
                moduleBrush = Brushes.LightGray;
            }
            Rectangle bounds = e.Bounds;
            string text = e.Node.Text;
            float x = text == Settings.ItemSpacer ? 0 : bounds.X;
            float itemWidth = tree.Width - x;
            Graphics graphics = e.Graphics;
            graphics.FillRectangle(fillBrush, x, bounds.Y, itemWidth, tree.ItemHeight);
            Font font = tree.Font;
            graphics.DrawString(text, font, textBrush, x, bounds.Top, StringFormat.GenericDefault);
            TypeNode node = e.Node as TypeNode;
            if (node == null) return;
            if (!string.IsNullOrEmpty(node.In))
            {
                x += graphics.MeasureString(text, font).Width;
                graphics.DrawString($"({node.In})", font, moduleBrush, x, bounds.Top, StringFormat.GenericDefault);
            }
            x = itemWidth;
            string module = node.Module;
            if (!string.IsNullOrEmpty(module))
            {
                x -= graphics.MeasureString(module, font).Width;
                graphics.DrawString(module, font, moduleBrush, x, bounds.Y, StringFormat.GenericDefault);
            }
            if (node.IsPrivate)
            {
                font = new Font(font, FontStyle.Underline);
                x -= graphics.MeasureString("(private)", font).Width;
                graphics.DrawString("(private)", font, moduleBrush, x, bounds.Y, StringFormat.GenericTypographic);
            }
        }

        #endregion
    }
}