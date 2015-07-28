using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ASCompletion;
using ASCompletion.Context;
using ASCompletion.Model;

namespace QuickNavigate.Forms
{
    /// <summary>
    /// </summary>
    public partial class ClassHierarchy : ClassModelExplorerForm
    {
        readonly ClassModel curClass;
        readonly Brush defaultNodeBrush;
        readonly Dictionary<string, List<ClassModel>> extendsToClasses;
        readonly Dictionary<string, TreeNode> typeToNode = new Dictionary<string, TreeNode>();
        
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
        public ClassHierarchy(ClassModel model, Settings settings) : base(settings)
        {
            curClass = model;
            InitializeComponent();
            if (settings.HierarchyExplorerSize.Width > MinimumSize.Width) Size = settings.HierarchyExplorerSize;
            defaultNodeBrush = new SolidBrush(tree.BackColor);
            extendsToClasses = GetAllProjectExtendsClasses();
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
            typeToNode.Clear();
            if (curClass.IsVoid()) return;
            TreeNode parent = null;
            int icon;
            foreach (ClassModel aClass in GetExtends(curClass))
            {
                icon = PluginUI.GetIcon(aClass.Flags, aClass.Access);
                TreeNode child = new ClassHierarchyNode(aClass, icon, icon);
                if (parent == null) tree.Nodes.Add(child);
                else parent.Nodes.Add(child);
                typeToNode[aClass.Type] = child;
                parent = child;
            }
            icon = PluginUI.GetIcon(curClass.Flags, curClass.Access);
            TreeNode node = new ClassHierarchyNode(curClass, icon, icon);
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
                if (extends.Type != node.Text) continue;
                int icon = PluginUI.GetIcon(aClass.Flags, aClass.Access);
                TreeNode child = new ClassHierarchyNode(aClass, icon, icon);
                node.Nodes.Add(child);
                typeToNode[aClass.Type] = child;
                FillNode(child);
            }
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
            ContextMenuStrip.Items[2].Enabled = !curClass.Equals(node.Model);
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
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
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
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyPress"/> event.
        /// </summary>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            int keyCode = e.KeyChar;
            e.Handled = keyCode == (int) Keys.Space
                        || keyCode == 12; //Ctrl+L
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"/> event.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Settings.HierarchyExplorerSize = Size;
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
        void OnInputTextChanged(object sender, EventArgs e)
        {
            if (tree.Nodes.Count == 0) return;
            List<string> matches = SearchUtil.Matches(new List<string>(typeToNode.Keys), input.Text, ".", Settings.MaxItems, Settings.HierarchyExplorerWholeWord, Settings.HierarchyExplorerMatchCase);
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
            if (e.KeyCode == Keys.Apps) input.ContextMenu = tree.SelectedNode != null ? InputEmptyContextMenu : null;
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
                    else if (Settings.WrapList)
                    {
                        node = GetUpEnabledNode();
                        if (node != null) tree.SelectedNode = node;
                    }
                    break;
                case Keys.Up:
                    node = GetPrevEnabledNode();
                    if (node != null) tree.SelectedNode = node;
                    else if (Settings.WrapList)
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
        void OnTreeDrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            string tag = e.Node.Tag as string;
            Brush fillBrush = defaultNodeBrush;
            Brush drawBrush = Brushes.Black;
            if (string.IsNullOrEmpty(tag) || tag == "enabled")
            {
                if ((e.State & TreeNodeStates.Selected) > 0)
                {
                    fillBrush = SelectedNodeBrush;
                    drawBrush = Brushes.White;
                }
            }
            else if (tag == "disabled") drawBrush = Brushes.DimGray;
            Rectangle bounds = e.Bounds;
            e.Graphics.FillRectangle(fillBrush, bounds.X, bounds.Y, tree.Width - bounds.X, tree.ItemHeight);
            e.Graphics.DrawString(e.Node.Text, e.Node.NodeFont ?? tree.Font, drawBrush, e.Bounds.Left, e.Bounds.Top, StringFormat.GenericDefault);
        }

        #endregion
    }
}