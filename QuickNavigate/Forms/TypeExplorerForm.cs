// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ASCompletion.Context;
using ASCompletion.Model;
using JetBrains.Annotations;
using PluginCore;
using ProjectManager.Projects;
using QuickNavigate.Helpers;

namespace QuickNavigate.Forms
{
    public sealed partial class TypeExplorerForm : QuickForm
    {
        [NotNull] readonly List<string> closedTypes = new List<string>();
        [NotNull] readonly List<string> openedTypes = new List<string>();
        [NotNull] static readonly Dictionary<string, ClassModel> TypeToClassModel = new Dictionary<string, ClassModel>();
        [NotNull] readonly List<Button> filters = new List<Button>();
        [NotNull] readonly Dictionary<Keys, Button> keysToFilter = new Dictionary<Keys, Button>();
        [NotNull] readonly Dictionary<Button, string> filterToEnabledTip = new Dictionary<Button, string>();
        [NotNull] readonly Dictionary<Button, string> filterToDisabledTip = new Dictionary<Button, string>();
        [NotNull] readonly Dictionary<FlagType, Button> flagToFilter = new Dictionary<FlagType, Button>();
        [NotNull] readonly Timer timer = new Timer();
        int filesCount;

        /// <summary>
        /// Initializes a new instance of the QuickNavigate.Controls.TypeExplorer
        /// </summary>
        /// <param name="settings"></param>
        public TypeExplorerForm([NotNull] Settings settings) : base(settings)
        {
            InitializeComponent();
            CreateItemsList();
            InitializeTree();
            InitializeTheme();
            RefreshTree();
        }

        [CanBeNull] ToolTip filterToolTip;
        [CanBeNull] Button currentFilter;

        [CanBeNull]
        Button CurrentFilter
        {
            get { return currentFilter; }
            set
            {
                if (currentFilter != null)
                {
                    currentFilter.FlatStyle = FlatStyle.Popup;
                    currentFilter.BackColor = tree.BackColor;
                }
                if (value == CurrentFilter) currentFilter = null;
                else
                {
                    currentFilter = value;
                    if (currentFilter != null)
                    {
                        currentFilter.FlatStyle = FlatStyle.Standard;
                        currentFilter.BackColor = SystemColors.ControlDarkDark;
                    }
                }
                RefreshTree();
                if (value != null && filterToolTip != null) RefreshFilterTip(value);
                input.Select();
            }
        }

        public override TreeNode SelectedNode => tree.SelectedNode;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                timer.Dispose();
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
            var search = input.Text.Trim();
            search = FormHelper.Transcriptor(search);
            ClassNode selectedNode = null;
            if (search.Length > 1 && search.Contains('.') && tree.Nodes.Count > 0)
            {
                var node = SelectedNode as ClassNode ?? (ClassNode) tree.TopNode;
                var parts = search.Split('.');
                if (node.Name.Equals(parts[parts.Length - 2], StringComparison.OrdinalIgnoreCase))
                {
                    selectedNode = node;
                    search = parts.Last();
                }
            }
            tree.BeginUpdate();
            tree.Nodes.Clear();
            if (selectedNode == null)
            {
                if (search.Length == 0) FillNodes(tree.Nodes);
                else FillNodes(tree.Nodes, search);
                tree.SelectedNode = tree.TopNode;
            }
            else
            {
                selectedNode.Nodes.Clear();
                tree.Nodes.Add(selectedNode);
                FillNodes(selectedNode.Nodes, selectedNode.Model, search);
                tree.SelectedNode = selectedNode.FirstNode ?? selectedNode;
            }
            tree.ExpandAll();
            tree.EndUpdate();
        }

        void FillNodes([NotNull] TreeNodeCollection nodes)
        {
            var types = FilterTypes(openedTypes.ToList());
            if (types.Count > 0) nodes.AddRange(types.Select(it => (ClassNode) NodeFactory.CreateTreeNode(TypeToClassModel[it])).ToArray());
        }

        void FillNodes([NotNull] TreeNodeCollection nodes, [NotNull] string search)
        {
            var openedTypes = FilterTypes(this.openedTypes.ToList());
            var closedTypes = FilterTypes(this.closedTypes.ToList());
            var maxItems = Settings.MaxItems;
            if (openedTypes.Count > 0) openedTypes = SearchUtil.FindAll(openedTypes, search);
            TreeNode[] openedNodes;
            TreeNode[] closedNodes;
            if (maxItems > 0)
            {
                openedNodes = CreateClassNodes(search, openedTypes, maxItems);
                maxItems -= openedTypes.Count;
                closedNodes = maxItems > 0 ? CreateClassNodes(search, SearchUtil.FindAll(closedTypes, search), maxItems) : new TreeNode[0];
            }
            else
            {
                openedNodes = CreateClassNodes(search, openedTypes);
                closedNodes = CreateClassNodes(search, SearchUtil.FindAll(closedTypes, search));
            }
            var hasOpenedMatches = openedNodes.Length > 0;
            var hasClosedMatches = closedNodes.Length > 0;
            if (hasOpenedMatches) nodes.AddRange(openedNodes);
            if (Settings.EnableItemSpacer && hasOpenedMatches && hasClosedMatches) nodes.Add(Settings.ItemSpacer);
            if (hasClosedMatches) nodes.AddRange(closedNodes);
        }

        [NotNull]
        static TreeNode[] CreateClassNodes([NotNull] string search, [NotNull] IEnumerable<string> sources)
        {
            return sources
                .Select(it => TypeToClassModel[it])
                .SortModels(search)
                .Select(NodeFactory.CreateTreeNode)
                .ToArray();
        }

        [NotNull]
        static TreeNode[] CreateClassNodes([NotNull] string search, [NotNull] IEnumerable<string> sources, int count)
        {
            return sources
                .Select(it => TypeToClassModel[it])
                .SortModels(search)
                .Take(count)
                .Select(NodeFactory.CreateTreeNode)
                .ToArray();
        }

        static void FillNodes([NotNull] TreeNodeCollection nodes, [NotNull] ClassModel inClass, [NotNull] string search)
        {
            var inFile = inClass.InFile;
            var isHaxe = inFile.haXe;
            var items = SearchUtil.FindAll(inClass.Members.Items, search);
            foreach (var it in items)
            {
                nodes.Add(NodeFactory.CreateTreeNode(inFile, isHaxe, it));
            }
        }

        [NotNull]
        List<string> FilterTypes([NotNull] List<string> list)
        {
            if (CurrentFilter == null) return list;
            var flags = (FlagType) CurrentFilter.Tag;
            list.RemoveAll(it => (TypeToClassModel[it].Flags & flags) == 0);
            return list;
        }

        protected override void ShowContextMenu()
        {
            if (SelectedNode == null) return;
            ShowContextMenu(new Point(SelectedNode.Bounds.X, SelectedNode.Bounds.Bottom));
        }

        protected override void ShowContextMenu(Point position)
        {
            if (!(SelectedNode is ClassNode)) return;
            ContextMenuStrip.Items.Clear();
            var classModel = ((ClassNode) SelectedNode).Model;
            var flags = classModel.Flags;
            var fileName = classModel.InFile.FileName;
            if ((flags & FlagType.Class) > 0
                && (flags & FlagType.Interface) == 0
                && (classModel.Access & Visibility.Public) > 0
                && !((Project)PluginBase.CurrentProject).IsDocumentClass(fileName))
            {
                ContextMenuStrip.Items.Add(QuickContextMenuItem.SetDocumentClassMenuItem);
                ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }
            ContextMenuStrip.Items.Add(QuickContextMenuItem.GotoPositionOrLineMenuItem);
            ContextMenuStrip.Items.Add(QuickContextMenuItem.ShowInClassHierarchyMenuItem);
            ContextMenuStrip.Items.Add(QuickContextMenuItem.ShowInProjectManagerMenuItem);
            if (File.Exists(fileName)) ContextMenuStrip.Items.Add(QuickContextMenuItem.ShowInFileExplorerMenuItem);
            ContextMenuStrip.Show(tree, position);
        }

        protected override void Navigate()
        {
            if (SelectedNode == null) return;
            DialogResult = DialogResult.OK;
        }

        internal void AddFilter(QuickFilter filter) => AddFilter(filter.ImageIndex, filter.Flag, filter.Shortcut, filter.EnabledTip, filter.DisabledTip);

        public void AddFilter(int imageIndex, FlagType flag, Keys shortcut, string enabledTip, string disabledTip)
        {
            if (flagToFilter.ContainsKey(flag)) return;
            var button = new Button
            {
                Anchor = AnchorStyles.Bottom,
                FlatStyle = FlatStyle.Popup,
                ImageList = tree.ImageList,
                ImageIndex = imageIndex,
                Size = new Size(24, 24),
                Tag = flag,
                UseVisualStyleBackColor = true
            };
            button.MouseClick += OnFilterMouseClick;
            button.MouseLeave += OnFilterMouseLeave;
            button.MouseHover += OnFilterMouseHover;
            flagToFilter[flag] = button;
            filterToEnabledTip[button] = enabledTip;
            filterToDisabledTip[button] = disabledTip;
            keysToFilter[shortcut] = button;
            filters.Add(button);
            filters.ForEach(Controls.Remove);
            const int spacing = 6;
            var width = filters[0].Width * filters.Count + spacing * (filters.Count - 1);
            var x = tree.Location.X + (tree.Width - width) / 2;
            for (var i = 0; i < filters.Count; i++)
            {
                button = filters[i];
                button.Location = new Point(x + (button.Size.Width + spacing) * i, tree.Bottom + spacing);
                button.TabIndex = tree.TabIndex + i;
                Controls.Add(button);
            }
        }

        void RefreshFilterTip([NotNull] Button filter)
        {
            var text = filter == CurrentFilter ? filterToDisabledTip[filter] : filterToEnabledTip[filter];
            if (filterToolTip == null) filterToolTip = new ToolTip();
            filterToolTip.Show(text, filter, filter.Width, filter.Height);
        }

        #region Event Handlers

        protected override void OnKeyDown(KeyEventArgs e)
        {
            var keyCode = e.KeyCode;
            if (keysToFilter.ContainsKey(keyCode) && e.Alt)
            {
                CurrentFilter = keysToFilter[keyCode];
                return;
            }
            switch (keyCode)
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

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (Settings != null)
            {
                if (Settings.TypeExplorerSize.Width > MinimumSize.Width) Size = Settings.TypeExplorerSize;
                searchingInExternalClasspaths.Checked = Settings.TypeExplorerSearchExternalClassPath;
            }
            CenterToParent();
            timer.Interval = Math.Max(PluginBase.MainForm.Settings.DisplayDelay, 100);
            timer.Tick += OnTimerTick;
            timer.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            timer.Stop();
            Settings.TypeExplorerSize = Size;
            Settings.TypeExplorerSearchExternalClassPath = searchingInExternalClasspaths.Checked;
        }

        protected override void OnTreeNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var node = e.Node as ClassNode;
            if (node == null) return;
            tree.SelectedNode = node;
            base.OnTreeNodeMouseClick(sender, e);
        }

        void OnTreeNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) => Navigate();

        void OnTreeDrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            var fillBrush = PluginBase.MainForm.GetThemeColor("TreeView.BackColor", SystemColors.Window);
            var textBrush = PluginBase.MainForm.GetThemeColor("TreeView.ForeColor", SystemColors.WindowText);
            var moduleBrush = Brushes.DimGray;
            if ((e.State & TreeNodeStates.Selected) > 0)
            {
                fillBrush = PluginBase.MainForm.GetThemeColor("TreeView.Highlight", SystemColors.Highlight);
                textBrush = PluginBase.MainForm.GetThemeColor("TreeView.HighlightText", SystemColors.HighlightText);
                moduleBrush = Brushes.LightGray;
            }
            var bounds = e.Bounds;
            var text = e.Node.Text;
            float x = text == Settings.ItemSpacer ? 0 : bounds.X;
            var itemWidth = tree.Width - x;
            var graphics = e.Graphics;
            graphics.FillRectangle(new SolidBrush(fillBrush), x, bounds.Y, itemWidth, tree.ItemHeight);
            var font = tree.Font;
            graphics.DrawString(text, font, new SolidBrush(textBrush), x, bounds.Top, StringFormat.GenericDefault);
            var node = e.Node as ClassNode;
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
            if (e.KeyCode == Keys.Apps) input.ContextMenu = SelectedNode != null ? FormHelper.EmptyContextMenu : null;
        }

        void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            if (tree.Nodes.Count == 0) return;
            var keyCode = e.KeyCode;
            if (e.Control && keyCode == Keys.Right && SelectedNode is ClassNode)
            {
                e.Handled = true;
                input.Text = ((ClassNode) SelectedNode).Model.Name;
                input.SelectionStart = input.TextLength;
                return;
            }
            if (keysToFilter.ContainsKey(keyCode))
            {
                e.Handled = e.Alt;
                return;
            }
            if (e.Shift) return;
            TreeNode node;
            var visibleCount = tree.VisibleCount - 1;
            switch (keyCode)
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
                    else if (PluginBase.MainForm.Settings.WrapList) tree.SelectedNode = tree.Nodes[0];
                    break;
                case Keys.Up:
                    if (tree.SelectedNode.PrevVisibleNode != null) tree.SelectedNode = tree.SelectedNode.PrevVisibleNode;
                    else if (PluginBase.MainForm.Settings.WrapList)
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

        void OnFilterMouseHover(object sender, EventArgs e) => RefreshFilterTip((Button)sender);

        void OnFilterMouseLeave(object sender, EventArgs e)
        {
            if (filterToolTip == null) return;
            filterToolTip.Hide((Button)sender);
            filterToolTip = null;
        }

        void OnFilterMouseClick(object sender, EventArgs e) => CurrentFilter = filters.First(sender.Equals);

        void OnTimerTick(object sender, EventArgs e)
        {
            var context = ASContext.GetLanguageContext(PluginBase.CurrentProject.Language);
            if (context == null || SelectedNode != null || context.Classpath.Count == openedTypes.Count + closedTypes.Count) return;
            var filesCount = context.Classpath.Sum(it => it.FilesCount);
            if (filesCount == this.filesCount) return;
            this.filesCount = filesCount;
            CreateItemsList();
            RefreshTree();
        }

        #endregion
    }

    internal static class ListExtensions
    {
        static readonly List<ClassModel> Nodes0 = new List<ClassModel>();
        static readonly List<ClassModel> Nodes1 = new List<ClassModel>();
        static readonly List<ClassModel> Nodes2 = new List<ClassModel>();

        public static List<ClassModel> SortModels(this IEnumerable<ClassModel> list, string search)
        {
            Nodes0.Clear();
            Nodes1.Clear();
            Nodes2.Clear();
            search = search.ToLower();
            foreach (var it in list)
            {
                var name = it.Name.ToLower();
                if (name == search) Nodes0.Add(it);
                else if (name.StartsWith(search)) Nodes1.Add(it);
                else Nodes2.Add(it);
            }
            Nodes0.Sort(ClassModelComparers.Package);
            Nodes1.Sort(ClassModelComparers.Name);
            Nodes2.Sort(ClassModelComparers.NamePackage);
            Nodes0.AddRange(Nodes1);
            Nodes0.AddRange(Nodes2);
            return Nodes0;
        }
    }

    internal static class ClassModelComparers
    {
        internal static IComparer<ClassModel> Name = new NameComparer();
        internal static IComparer<ClassModel> Package = new PackageComparer();
        internal static IComparer<ClassModel> NamePackage = new NamePackageComparer();

        public class NameComparer : IComparer<ClassModel>
        {
            public int Compare(ClassModel x, ClassModel y) => CaseSensitiveImportComparer.CompareImports(x.Name, y.Name);
        }

        public class PackageComparer : IComparer<ClassModel>
        {
            public int Compare(ClassModel x, ClassModel y) => CaseSensitiveImportComparer.CompareImports(x.InFile.Package, y.InFile.Package);
        }

        public class NamePackageComparer : IComparer<ClassModel>
        {
            public int Compare(ClassModel x, ClassModel y) => CaseSensitiveImportComparer.CompareImports(x.Name + "." + x.InFile.Package, y.Name + "." + y.InFile.Package);
        }
    }
}