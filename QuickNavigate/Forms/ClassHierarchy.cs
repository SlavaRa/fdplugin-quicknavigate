using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ASCompletion;
using ASCompletion.Context;
using ASCompletion.Model;
using JetBrains.Annotations;
using PluginCore;
using QuickNavigate.Helpers;

namespace QuickNavigate.Forms
{
    public sealed partial class ClassHierarchy : ClassModelExplorerForm
    {
        readonly ClassModel curClass;
        readonly Brush defaultNodeBrush;
        readonly Dictionary<string, List<ClassModel>> extendsToClasses;
        readonly Dictionary<string, TreeNode> typeToNode = new Dictionary<string, TreeNode>();

        [NotNull]
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

        [NotNull]
        static IEnumerable<ClassModel> GetExtends([NotNull] ClassModel theClass)
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
        public ClassHierarchy([NotNull] ClassModel model, [NotNull] Settings settings) : base(settings)
        {
            curClass = model;
            Font = PluginBase.Settings.DefaultFont;
            InitializeComponent();
            if (settings.HierarchyExplorerSize.Width > MinimumSize.Width) Size = settings.HierarchyExplorerSize;
            defaultNodeBrush = new SolidBrush(tree.BackColor);
            extendsToClasses = GetAllProjectExtendsClasses();
            InitializeTree();
            RefreshTree();
        }

        [CanBeNull]
        public TypeNode SelectedNode => tree.SelectedNode as TypeNode;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                defaultNodeBrush?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        void InitializeTree() => tree.ImageList = FormHelper.GetTreeIcons();

        void RefreshTree()
        {
            tree.BeginUpdate();
            tree.Nodes.Clear();
            FillTree();
            tree.ExpandAll();
            tree.EndUpdate();
        }

        void FillTree()
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

        void FillNode([NotNull] TreeNode node)
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

        [CanBeNull]
        TreeNode GetNextEnabledNode()
        {
            TreeNode node = tree.SelectedNode;
            while (node.NextVisibleNode != null)
            {
                node = node.NextVisibleNode;
                if (((ClassHierarchyNode) node).Enabled) return node;
            }
            return null;
        }

        [CanBeNull]
        TreeNode GetPrevEnabledNode()
        {
            TreeNode node = tree.SelectedNode;
            while (node.PrevVisibleNode != null)
            {
                node = node.PrevVisibleNode;
                if (((ClassHierarchyNode) node).Enabled) return node;
            }
            return null;
        }

        [CanBeNull]
        TreeNode GetUpEnabledNode()
        {
            TreeNode result = null;
            TreeNode node = tree.SelectedNode;
            while (node.PrevVisibleNode != null)
            {
                node = node.PrevVisibleNode;
                if (((ClassHierarchyNode) node).Enabled) result = node;
            }
            return result;
        }

        [CanBeNull]
        TreeNode GetLastEnabledNode()
        {
            TreeNode result = null;
            TreeNode node = tree.SelectedNode;
            while (node.NextVisibleNode != null)
            {
                node = node.NextVisibleNode;
                if (((ClassHierarchyNode) node).Enabled) result = node;
            }
            return result;
        }

        protected override void ShowContextMenu()
        {
            if (SelectedNode == null) return;
            ShowContextMenu(new Point(SelectedNode.Bounds.X, SelectedNode.Bounds.Bottom));
        }

        protected override void ShowContextMenu(Point position)
        {
            if (SelectedNode == null) return;
            ContextMenuStrip.Items[2].Enabled = !curClass.Equals(SelectedNode.Model);
            ContextMenuStrip.Items[4].Enabled = File.Exists(SelectedNode.Model.InFile.FileName);
            ContextMenuStrip.Show(tree, position);
        }

        protected override void Navigate()
        {
            if (SelectedNode != null) DialogResult = DialogResult.OK;
        }

        #region Event Handlers

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
                case Keys.Escape:
                    Close();
                    break;
                case Keys.Enter:
                    e.Handled = true;
                    Navigate();
                    break;
                case Keys.Apps:
                    e.Handled = true;
                    ShowContextMenu();
                    break;
            }
        }

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

        void OnTreeNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) => Navigate();

        void OnTreeDrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            ClassHierarchyNode node = (ClassHierarchyNode) e.Node;
            Brush fillBrush = defaultNodeBrush;
            Brush drawBrush = Brushes.Black;
            if (node.Enabled)
            {
                if ((e.State & TreeNodeStates.Selected) > 0)
                {
                    fillBrush = SelectedNodeBrush;
                    drawBrush = Brushes.White;
                }
            }
            else drawBrush = Brushes.DimGray;
            Rectangle bounds = e.Bounds;
            e.Graphics.FillRectangle(fillBrush, bounds.X, bounds.Y, tree.Width - bounds.X, tree.ItemHeight);
            e.Graphics.DrawString(e.Node.Text, e.Node.NodeFont ?? tree.Font, drawBrush, e.Bounds.Left, e.Bounds.Top, StringFormat.GenericDefault);
        }

        void OnInputTextChanged(object sender, EventArgs e)
        {
            if (tree.Nodes.Count == 0) return;
            List<string> matches = SearchUtil.Matches(typeToNode.Keys.ToList(), input.Text, ".", Settings.MaxItems, Settings.HierarchyExplorerWholeWord, Settings.HierarchyExplorerMatchCase);
            bool mathesIsEmpty = matches.Count == 0;
            foreach (KeyValuePair<string, TreeNode> k in typeToNode)
            {
                ((ClassHierarchyNode) k.Value).Enabled = mathesIsEmpty || matches.Contains(k.Key);
            }
            tree.Refresh();
            if (mathesIsEmpty) tree.SelectedNode = typeToNode[curClass.Type];
            else
            {
                matches.Sort();
                tree.SelectedNode = typeToNode[matches[0]];
            }
        }

        void OnInputPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Apps) input.ContextMenu = SelectedNode != null ? InputEmptyContextMenu : null;
        }

        void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            TreeNode node;
            TreeNode enabledNode = null;
            int lastVisibleIndex = tree.VisibleCount - 1;
            switch (e.KeyCode)
            {
                case Keys.Space:
                    e.Handled = true;
                    return;
                case Keys.L:
                    e.Handled = e.Control;
                    return;
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
                    for (int i = 0; i < lastVisibleIndex; i++)
                    {
                        if (node.PrevVisibleNode == null) break;
                        node = node.PrevVisibleNode;
                        if (((ClassHierarchyNode) node).Enabled) enabledNode = node;
                    }
                    if (enabledNode != null) tree.SelectedNode = enabledNode;
                    break;
                case Keys.PageDown:
                    node = tree.SelectedNode;
                    for (int i = 0; i < lastVisibleIndex; i++)
                    {
                        if (node.NextVisibleNode == null) break;
                        node = node.NextVisibleNode;
                        if (((ClassHierarchyNode) node).Enabled) enabledNode = node;
                    }
                    if (enabledNode != null) tree.SelectedNode = enabledNode;
                    break;
                default: return;
            }
            e.Handled = true;
        }

        #endregion
    }
}