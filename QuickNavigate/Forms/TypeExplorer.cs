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
using QuickNavigate.Collections;
using QuickNavigate.Helpers;

namespace QuickNavigate.Forms
{
    public sealed partial class TypeExplorer : ClassModelExplorerForm
    {
        readonly List<string> closedTypes = new List<string>();
        readonly List<string> openedTypes = new List<string>();
        static readonly Dictionary<string, ClassModel> TypeToClassModel = new Dictionary<string, ClassModel>();

        [CanBeNull]
        readonly Brush defaultNodeBrush;

        /// <summary>
        /// Initializes a new instance of the QuickNavigate.Controls.TypeExplorer
        /// </summary>
        /// <param name="settings"></param>
        public TypeExplorer(Settings settings) : base(settings)
        {
            Font = PluginBase.Settings.DefaultFont;
            InitializeComponent();
            if (settings.TypeExplorerSize.Width > MinimumSize.Width) Size = settings.TypeExplorerSize;
            searchingInExternalClasspaths.Checked = settings.TypeExplorerSearchExternalClassPath;
            defaultNodeBrush = new SolidBrush(tree.BackColor);
            CreateItemsList();
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

        void CreateItemsList()
        {
            closedTypes.Clear();
            openedTypes.Clear();
            TypeToClassModel.Clear();
            var context = ASContext.GetLanguageContext(PluginBase.CurrentProject.Language);
            if (context == null) return;
            var projectFolder = Path.GetDirectoryName(PluginBase.CurrentProject.ProjectPath);
            var onlyProjectTypes = !searchingInExternalClasspaths.Checked;
            foreach (var classpath in context.Classpath)
            {
                if (onlyProjectTypes)
                {
                    var path = classpath.Path;
                    if (!Path.IsPathRooted(classpath.Path)) path = Path.GetFullPath(Path.Combine(projectFolder, classpath.Path));
                    if (!path.StartsWith(projectFolder)) continue;
                }
                classpath.ForeachFile(model =>
                {
                    foreach (var aClass in model.Classes)
                    {
                        var type = aClass.Type;
                        if (TypeToClassModel.ContainsKey(type)) continue;
                        if (FormHelper.IsFileOpened(aClass.InFile.FileName)) openedTypes.Add(type);
                        else closedTypes.Add(type);
                        TypeToClassModel.Add(type, aClass);
                    }
                    return true;
                });
            }
        }

        void InitializeTree()
        {
            tree.ImageList = ASContext.Panel.TreeIcons;
            tree.ItemHeight = tree.ImageList.ImageSize.Height;
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
            var search = input.Text.Trim();
            var openedCount = openedTypes.Count;
            if (search.Length == 0)
            {
                if (openedCount > 0) tree.Nodes.AddRange(CreateNodes(openedTypes, string.Empty).ToArray());
            }
            else
            {
                var maxItems = Settings.MaxItems;
                var openedMatches = openedCount > 0 ? SearchUtil.Matches(openedTypes, search) : new List<string>();
                var closedMatches = new List<string>();
                if (maxItems > 0)
                {
                    if (openedMatches.Count >= maxItems) openedMatches = openedMatches.GetRange(0, maxItems);
                    maxItems -= openedMatches.Count;
                    if (maxItems > 0)
                    {
                        closedMatches = SearchUtil.Matches(closedTypes, search);
                        if (closedMatches.Count >= maxItems) closedMatches = closedMatches.GetRange(0, maxItems);
                    }
                }
                else closedMatches = SearchUtil.Matches(closedTypes, search);
                var hasOpenedMatches = openedMatches.Count > 0;
                var hasClosedMatches = closedMatches.Count > 0;
                if (hasOpenedMatches) tree.Nodes.AddRange(CreateNodes(openedMatches, search).ToArray());
                if (Settings.EnableItemSpacer && hasOpenedMatches && hasClosedMatches)
                    tree.Nodes.Add(Settings.ItemSpacer);
                if (hasClosedMatches) tree.Nodes.AddRange(CreateNodes(closedMatches, search).ToArray());
            }
            if (tree.Nodes.Count > 0) tree.SelectedNode = tree.Nodes[0];
        }

        [NotNull]
        static IEnumerable<TypeNode> CreateNodes([NotNull] IEnumerable<string> matches, [NotNull] string search)
        {
            var nodes = matches.Select(CreateNode);
            return SortNodes(nodes, search);
        }

        [NotNull]
        static TypeNode CreateNode(string type)
        {
            var aClass = TypeToClassModel[type];
            return new TypeNode(aClass, PluginUI.GetIcon(aClass.Flags, aClass.Access));
        }

        [NotNull]
        static IEnumerable<TypeNode> SortNodes(IEnumerable<TypeNode> nodes, string search)
        {
            search = search.ToLower();
            var nodes0 = new List<TypeNode>();
            var nodes1 = new List<TypeNode>();
            var nodes2 = new List<TypeNode>();
            foreach (var node in nodes)
            {
                var name = node.Name.ToLower();
                if (name == search) nodes0.Add(node);
                else if (name.StartsWith(search)) nodes1.Add(node);
                else nodes2.Add(node);
            }
            nodes0.Sort(TypeExplorerNodeComparer.Package);
            nodes1.Sort(TypeExplorerNodeComparer.NameIgnoreCase);
            nodes2.Sort(TypeExplorerNodeComparer.NamePackageIgnoreCase);
            return nodes0.Concat(nodes1).Concat(nodes2);
        }

        protected override void ShowContextMenu()
        {
            if (SelectedNode == null) return;
            ShowContextMenu(new Point(SelectedNode.Bounds.X, SelectedNode.Bounds.Bottom));
        }

        protected override void ShowContextMenu(Point position)
        {
            if (SelectedNode == null) return;
            ContextMenuStrip.Items[4].Enabled = File.Exists(SelectedNode.Model.InFile.FileName);
            ContextMenuStrip.Show(tree, position);
        }

        protected override void Navigate()
        {
            if (SelectedNode == null) return;
            DialogResult = DialogResult.OK;
        }

        #region Event Handlers

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
            Settings.TypeExplorerSize = Size;
            Settings.TypeExplorerSearchExternalClassPath = searchingInExternalClasspaths.Checked;
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
            var fillBrush = defaultNodeBrush;
            var textBrush = Brushes.Black;
            var moduleBrush = Brushes.DimGray;
            if ((e.State & TreeNodeStates.Selected) > 0)
            {
                fillBrush = SelectedNodeBrush;
                textBrush = Brushes.White;
                moduleBrush = Brushes.LightGray;
            }
            var bounds = e.Bounds;
            var text = e.Node.Text;
            float x = text == Settings.ItemSpacer ? 0 : bounds.X;
            var itemWidth = tree.Width - x;
            var graphics = e.Graphics;
            graphics.FillRectangle(fillBrush, x, bounds.Y, itemWidth, tree.ItemHeight);
            var font = tree.Font;
            graphics.DrawString(text, font, textBrush, x, bounds.Top, StringFormat.GenericDefault);
            var node = e.Node as TypeNode;
            if (node == null) return;
            if (!string.IsNullOrEmpty(node.In))
            {
                x += graphics.MeasureString(text, font).Width;
                graphics.DrawString($"({node.In})", font, moduleBrush, x, bounds.Top, StringFormat.GenericDefault);
            }
            x = itemWidth;
            var module = node.Module;
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

        void OnInputTextChanged(object sender, EventArgs e) => RefreshTree();

        void OnInputPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Apps) input.ContextMenu = SelectedNode != null ? InputEmptyContextMenu : null;
        }

        void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Shift) return;
            TreeNode node;
            var visibleCount = tree.VisibleCount - 1;
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
                    for (var i = 0; i < visibleCount; i++)
                    {
                        if (node.PrevVisibleNode == null) break;
                        node = node.PrevVisibleNode;
                    }
                    tree.SelectedNode = node;
                    break;
                case Keys.PageDown:
                    node = tree.SelectedNode;
                    for (var i = 0; i < visibleCount; i++)
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

        void OnSearchingModeCheckStateChanged(object sender, EventArgs e)
        {
            CreateItemsList();
            RefreshTree();
        }

        #endregion
    }
}