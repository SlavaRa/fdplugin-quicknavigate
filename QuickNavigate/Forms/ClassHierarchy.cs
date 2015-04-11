using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ASCompletion;
using ASCompletion.Context;
using ASCompletion.Model;
using FlashDevelop;
using PluginCore;
using ScintillaNet;

namespace QuickNavigate.Forms
{
    /// <summary>
    /// </summary>
    public partial class ClassHierarchy : Form
    {
        public event ShowInHandler GotoPositionOrLine;
        public event ShowInHandler ShowInQuickOutline;
        public event ShowInHandler ShowInClassHierarchy;
        public event ShowInHandler ShowInProjectManager;
        public event ShowInHandler ShowInFileExplorer;
        readonly ClassModel curClass;
        readonly Settings settings;
        readonly Brush selectedNodeBrush = new SolidBrush(SystemColors.ControlDarkDark);
        readonly Brush defaultNodeBrush;
        readonly Dictionary<string, List<ClassModel>> extendsToClasses;
        readonly Dictionary<string, TreeNode> typeToNode = new Dictionary<string, TreeNode>();
        readonly ContextMenu inputEmptyContextMenu = new ContextMenu();
        readonly ContextMenuStrip contextMenu = new ContextMenuStrip();

        /// <summary>
        /// </summary>
        /// <returns></returns>
        static Dictionary<string, List<ClassModel>> GetAllProjectExtendsClasses()
        {
            Dictionary<string, List<ClassModel>> result = new Dictionary<string, List<ClassModel>>();
            foreach (PathModel path in ASContext.Context.Classpath)
            {
                path.ForeachFile(aFile =>
                {
                    foreach (ClassModel aClass in aFile.Classes)
                    {
                        string extendsType = aClass.ExtendsType;
                        if (string.IsNullOrEmpty(extendsType)) continue;
                        if (!result.ContainsKey(extendsType)) result[extendsType] = new List<ClassModel>();
                        result[extendsType].Add(aClass);
                    }
                    return true;
                });
            }
            return result;
        }

        /// <summary>
        /// </summary>
        /// <param name="theClass"></param>
        /// <returns></returns>
        static IEnumerable<ClassModel> GetExtends(ClassModel theClass)
        {
            List<ClassModel> result = new List<ClassModel>();
            ClassModel aClass = theClass.Extends;
            while (!aClass.IsVoid())
            {
                result.Add(aClass);
                aClass = aClass.Extends;
            }
            result.Reverse();
            return result;
        }

        /// <summary>
        /// Initializes a new instance of the QuickNavigate.Controls.ClassHierarchy
        /// </summary>
        /// <param name="model"></param>
        /// <param name="settings"></param>
        public ClassHierarchy(ClassModel model, Settings settings)
        {
            curClass = model;
            this.settings = settings;
            Font = PluginBase.Settings.ConsoleFont;
            InitializeComponent();
            if (settings.HierarchyExplorerSize.Width > MinimumSize.Width) Size = settings.HierarchyExplorerSize;
            ((MainForm)PluginBase.MainForm).ThemeControls(this);
            defaultNodeBrush = new SolidBrush(tree.BackColor);
            extendsToClasses = GetAllProjectExtendsClasses();
            CreateContextMenu();
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
        void CreateContextMenu()
        {
            contextMenu.Items.Add("&Goto Position Or Line", PluginBase.MainForm.FindImage("67"), OnGotoPositionOnLine);
            contextMenu.Items.Add("Show in Quick &Outline", PluginBase.MainForm.FindImage("315|16|0|0"), OnShowInQuickOutline);
            contextMenu.Items.Add("Show in &Class Hierarchy", PluginBase.MainForm.FindImage("99|16|0|0"), OnShowInClassHiearachy);
            contextMenu.Items.Add("Show in &Project Manager", PluginBase.MainForm.FindImage("274"), OnShowInProjectManager);
            contextMenu.Items.Add("Show in &File Explorer", PluginBase.MainForm.FindImage("209"), OnShowInFileExplorer);
        }

        /// <summary>
        /// </summary>
        void InitTree()
        {
            ImageList icons = new ImageList() {TransparentColor = Color.Transparent};
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
            typeToNode.Clear();
            if (curClass.IsVoid()) return;
            TreeNode parent = null;
            int icon;
            foreach (ClassModel aClass in GetExtends(curClass))
            {
                icon = PluginUI.GetIcon(aClass.Flags, aClass.Access);
                TreeNode child = new ClassNode(aClass, icon, icon);
                if (parent == null) tree.Nodes.Add(child);
                else parent.Nodes.Add(child);
                typeToNode[aClass.Type] = child;
                parent = child;
            }
            icon = PluginUI.GetIcon(curClass.Flags, curClass.Access);
            TreeNode node = new ClassNode(curClass, icon, icon);
            node.NodeFont = new Font(tree.Font, FontStyle.Underline);
            if (parent == null) tree.Nodes.Add(node);
            else parent.Nodes.Add(node);
            tree.SelectedNode = node;
            typeToNode[curClass.Type] = node;
            FillNode(node);
        }

        /// <summary>
        /// </summary>
        /// <param name="node"></param>
        void FillNode(TreeNode node)
        {
            if (!extendsToClasses.ContainsKey(node.Name)) return;
            foreach (ClassModel aClass in extendsToClasses[node.Name])
            {
                ClassModel extends = aClass.InFile.Context.ResolveType(aClass.ExtendsType, aClass.InFile);
                if (extends.Type == node.Text)
                {
                    int icon = PluginUI.GetIcon(aClass.Flags, aClass.Access);
                    TreeNode child = new ClassNode(aClass, icon, icon);
                    node.Nodes.Add(child);
                    typeToNode[aClass.Type] = child;
                    FillNode(child);
                }
            }
        }

        /// <summary>
        /// </summary>
        void Navigate()
        {
            if (tree.SelectedNode == null) return;
            ClassModel theClass = ((ClassNode)tree.SelectedNode).Model;
            FileModel file = ModelsExplorer.Instance.OpenFile(theClass.InFile.FileName);
            if (file != null)
            {
                theClass = file.GetClassByName(theClass.Name);
                if (!theClass.IsVoid())
                {
                    int line = theClass.LineFrom;
                    ScintillaControl sci = PluginBase.MainForm.CurrentDocument.SciControl;
                    if (sci != null && line > 0 && line < sci.LineCount)
                        sci.GotoLine(line);
                }
            }
            Close();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        TreeNode GetNextEnabledNode()
        {
            TreeNode node = tree.SelectedNode;
            while (node.NextVisibleNode != null)
            {
                node = node.NextVisibleNode;
                if ((node.Tag as string) == "enabled") return node;
            }
            return null;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        TreeNode GetPrevEnabledNode()
        {
            TreeNode node = tree.SelectedNode;
            while (node.PrevVisibleNode != null)
            {
                node = node.PrevVisibleNode;
                if ((node.Tag as string) == "enabled") return node;
            }
            return null;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        TreeNode GetUpEnabledNode()
        {
            TreeNode result = null;
            TreeNode node = tree.SelectedNode;
            while (node.PrevVisibleNode != null)
            {
                node = node.PrevVisibleNode;
                if ((node.Tag as string) == "enabled") result = node;
            }
            return result;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        TreeNode GetLastEnabledNode()
        {
            TreeNode result = null;
            TreeNode node = tree.SelectedNode;
            while (node.NextVisibleNode != null)
            {
                node = node.NextVisibleNode;
                if ((node.Tag as string) == "enabled") result = node;
            }
            return result;
        }

        /// <summary>
        /// Displays the shortcut menu.
        /// </summary>
        void ShowContextMenu()
        {
            ClassNode node = tree.SelectedNode as ClassNode;
            if (node == null) return;
            ShowContextMenu(new Point(node.Bounds.X, node.Bounds.Y + node.Bounds.Height));
        }

        /// <summary>
        /// Displays the shortcut menu.
        /// </summary>
        void ShowContextMenu(Point position)
        {
            ClassNode node = tree.SelectedNode as ClassNode;
            if (node == null) return;
            contextMenu.Items[1].Enabled = !curClass.Equals(node.Model);
            contextMenu.Items[3].Enabled = File.Exists(node.Model.InFile.FileName);
            contextMenu.Show(tree, position);
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
                case Keys.L:
                    if (e.Control)
                    {
                        input.Focus();
                        input.SelectAll();
                    }
                    break;
                case Keys.Apps:
                    e.Handled = true;
                    ShowContextMenu();
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
            e.Handled = keyCode == (int) Keys.Space
                        || keyCode == 12; //Ctrl+L
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"/> that contains the event data. </param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            settings.HierarchyExplorerSize = Size;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnInputTextChanged(object sender, EventArgs e)
        {
            if (tree.Nodes.Count == 0) return;
            List<string> matches = SearchUtil.Matches(new List<string>(typeToNode.Keys), input.Text, ".", settings.MaxItems, settings.HierarchyExplorerWholeWord, settings.HierarchyExplorerMatchCase);
            bool mathesIsEmpty = matches.Count == 0;
            foreach (KeyValuePair<string, TreeNode> k in typeToNode)
            {
                if (mathesIsEmpty) k.Value.Tag = "enabled";
                else k.Value.Tag = matches.Contains(k.Key) ? "enabled" : "disabled";
            }
            tree.Refresh();
            if (mathesIsEmpty)
            {
                tree.SelectedNode = typeToNode[curClass.Type];
                return;
            }
            matches.Sort();
            tree.SelectedNode = typeToNode[matches[0]];
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnInputPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Apps) input.ContextMenu = tree.SelectedNode != null ? inputEmptyContextMenu : null;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control || e.Shift || tree.SelectedNode == null) return;
            TreeNode node;
            TreeNode enabledNode = null;
            int visibleCount = tree.VisibleCount - 1;
            switch (e.KeyCode)
            {
                case Keys.Down:
                    node = GetNextEnabledNode();
                    if (node != null) tree.SelectedNode = node;
                    else if (settings.WrapList)
                    {
                        node = GetUpEnabledNode();
                        if (node != null) tree.SelectedNode = node;
                    }
                    break;
                case Keys.Up:
                    node = GetPrevEnabledNode();
                    if (node != null) tree.SelectedNode = node;
                    else if (settings.WrapList)
                    {
                        node = GetLastEnabledNode();
                        if (node != null) tree.SelectedNode = node;
                    }
                    break;
                case Keys.Home:
                    node = GetUpEnabledNode();
                    if (node != null) tree.SelectedNode = node;
                    break;
                case Keys.End:
                    node = GetLastEnabledNode();
                    if (node != null) tree.SelectedNode = node;
                    break;
                case Keys.PageUp:
                    node = tree.SelectedNode;
                    for (int i = 0; i < visibleCount; i++)
                    {
                        if (node.PrevVisibleNode == null) break;
                        node = node.PrevVisibleNode;
                        if ((node.Tag as string) == "enabled") enabledNode = node;
                    }
                    if (enabledNode != null) tree.SelectedNode = enabledNode;
                    break;
                case Keys.PageDown:
                    node = tree.SelectedNode;
                    for (int i = 0; i < visibleCount; i++)
                    {
                        if (node.NextVisibleNode == null) break;
                        node = node.NextVisibleNode;
                        if ((node.Tag as string) == "enabled") enabledNode = node;
                    }
                    if (enabledNode != null) tree.SelectedNode = enabledNode;
                    break;
                default: return;
            }
            e.Handled = true;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnTreeNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            ClassNode node = e.Node as ClassNode;
            if (node == null) return;
            tree.SelectedNode = node;
            ShowContextMenu(new Point(e.Location.X, node.Bounds.Y + node.Bounds.Height));
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
            string tag = e.Node.Tag as string;
            Brush fillBrush = defaultNodeBrush;
            Brush drawBrush = Brushes.Black;
            if (string.IsNullOrEmpty(tag) || tag == "enabled")
            {
                if ((e.State & TreeNodeStates.Selected) > 0)
                {
                    fillBrush = selectedNodeBrush;
                    drawBrush = Brushes.White;
                }
            }
            else if (tag == "disabled") drawBrush = Brushes.DimGray;
            Rectangle bounds = e.Bounds;
            e.Graphics.FillRectangle(fillBrush, bounds.X, bounds.Y, tree.Width - bounds.X, tree.ItemHeight);
            e.Graphics.DrawString(e.Node.Text, e.Node.NodeFont ?? tree.Font, drawBrush, e.Bounds.Left, e.Bounds.Top, StringFormat.GenericDefault);
        }

        void OnGotoPositionOnLine(object sender, EventArgs e)
        {
            GotoPositionOrLine(this, ((ClassNode)tree.SelectedNode).Model);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnShowInQuickOutline(object sender, EventArgs e)
        {
            ShowInQuickOutline(this, ((ClassNode)tree.SelectedNode).Model);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnShowInClassHiearachy(object sender, EventArgs e)
        {
            ShowInClassHierarchy(this, ((ClassNode)tree.SelectedNode).Model);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnShowInProjectManager(object sender, EventArgs e)
        {
            ShowInProjectManager(this, ((ClassNode)tree.SelectedNode).Model);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnShowInFileExplorer(object sender, EventArgs e)
        {
            ShowInFileExplorer(this, ((ClassNode)tree.SelectedNode).Model);
        }

        #endregion
    }
}