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

namespace QuickNavigate.Forms
{
    public sealed partial class ClassHierarchyForm : ClassModelExplorerForm
    {
        readonly ClassModel curClass;
        readonly Dictionary<string, List<ClassModel>> extendsToClasses;
        readonly Dictionary<string, TreeNode> typeToNode = new Dictionary<string, TreeNode>();

        [NotNull]
        static Dictionary<string, List<ClassModel>> GetAllProjectExtendsClasses()
        {
            var result = new Dictionary<string, List<ClassModel>>();
            foreach (var path in ASContext.Context.Classpath)
            {
                path.ForeachFile(aFile =>
                {
                    foreach (var aClass in aFile.Classes)
                    {
                        var extendsType = aClass.ExtendsType;
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
            var result = new List<ClassModel>();
            var aClass = theClass.Extends;
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
        public ClassHierarchyForm([NotNull] ClassModel model, [NotNull] Settings settings) : base(settings)
        {
            curClass = model;
            Font = PluginBase.Settings.DefaultFont;
            InitializeComponent();
            if (settings.HierarchyExplorerSize.Width > MinimumSize.Width) Size = settings.HierarchyExplorerSize;
            extendsToClasses = GetAllProjectExtendsClasses();
            InitializeTree();
            InitializeTheme();
            RefreshTree();
        }

        [CanBeNull]
        public TypeNode SelectedNode => tree.SelectedNode as TypeNode;

        void InitializeTree()
        {
            tree.ImageList = ASContext.Panel.TreeIcons;
            tree.ItemHeight = tree.ImageList.ImageSize.Height;
        }

        void InitializeTheme()
        {
            input.BackColor = PluginBase.MainForm.GetThemeColor("TextBox.BackColor", SystemColors.Window);
            input.ForeColor = PluginBase.MainForm.GetThemeColor("TextBox.ForeColor", SystemColors.WindowText);
            tree.BackColor = PluginBase.MainForm.GetThemeColor("TreeView.BackColor", SystemColors.Window);
            tree.ForeColor = PluginBase.MainForm.GetThemeColor("TreeView.ForeColor", SystemColors.WindowText);
            BackColor = PluginBase.MainForm.GetThemeColor("TreeView.BackColor", SystemColors.Window);
            ForeColor = PluginBase.MainForm.GetThemeColor("TreeView.ForeColor", SystemColors.WindowText);
        }

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
            foreach (var aClass in GetExtends(curClass))
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
            foreach (var aClass in extendsToClasses[node.Name])
            {
                var extends = aClass.InFile.Context.ResolveType(aClass.ExtendsType, aClass.InFile);
                if (extends.Type != node.Text) continue;
                var icon = PluginUI.GetIcon(aClass.Flags, aClass.Access);
                TreeNode child = new ClassHierarchyNode(aClass, icon, icon);
                node.Nodes.Add(child);
                typeToNode[aClass.Type] = child;
                FillNode(child);
            }
        }

        [CanBeNull]
        TreeNode GetNextEnabledNode()
        {
            var node = tree.SelectedNode;
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
            var node = tree.SelectedNode;
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
            var node = tree.SelectedNode;
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
            var node = tree.SelectedNode;
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
            var node = e.Node as TypeNode;
            if (node == null) return;
            tree.SelectedNode = node;
            base.OnTreeNodeMouseClick(sender, e);
        }

        void OnTreeNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) => Navigate();

        void OnTreeDrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            var node = (ClassHierarchyNode) e.Node;
            var fillBrush = PluginBase.MainForm.GetThemeColor("TreeView.BackColor", SystemColors.Window);
            var textBrush = PluginBase.MainForm.GetThemeColor("TreeView.ForeColor", SystemColors.WindowText);
            if (node.Enabled)
            {
                if ((e.State & TreeNodeStates.Selected) > 0)
                {
                    fillBrush = PluginBase.MainForm.GetThemeColor("TreeView.Highlight", SystemColors.Highlight);
                    textBrush = PluginBase.MainForm.GetThemeColor("TreeView.HighlightText", SystemColors.HighlightText);
                }
            }
            else textBrush = Color.DimGray;
            var bounds = e.Bounds;
            e.Graphics.FillRectangle(new SolidBrush(fillBrush), bounds.X, bounds.Y, tree.Width - bounds.X, tree.ItemHeight);
            e.Graphics.DrawString(e.Node.Text, e.Node.NodeFont ?? tree.Font, new SolidBrush(textBrush), e.Bounds.Left, e.Bounds.Top, StringFormat.GenericDefault);
        }

        void OnInputTextChanged(object sender, EventArgs e)
        {
            if (tree.Nodes.Count == 0) return;
            var matches = SearchUtil.FindAll(typeToNode.Keys.ToList(), input.Text);
            var mathesIsEmpty = matches.Count == 0;
            foreach (var k in typeToNode)
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
            var lastVisibleIndex = tree.VisibleCount - 1;
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
                    for (var i = 0; i < lastVisibleIndex; i++)
                    {
                        if (node.PrevVisibleNode == null) break;
                        node = node.PrevVisibleNode;
                        if (((ClassHierarchyNode) node).Enabled) enabledNode = node;
                    }
                    if (enabledNode != null) tree.SelectedNode = enabledNode;
                    break;
                case Keys.PageDown:
                    node = tree.SelectedNode;
                    for (var i = 0; i < lastVisibleIndex; i++)
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