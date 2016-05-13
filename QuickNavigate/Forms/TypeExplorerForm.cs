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
using ProjectManager.Projects;
using QuickNavigate.Collections;
using QuickNavigate.Helpers;

namespace QuickNavigate.Forms
{
    public sealed partial class TypeExplorerForm : ClassModelExplorerForm
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
        private int filesCount;

        /// <summary>
        /// Initializes a new instance of the QuickNavigate.Controls.TypeExplorer
        /// </summary>
        /// <param name="settings"></param>
        public TypeExplorerForm([NotNull] Settings settings) : base(settings)
        {
            Font = PluginBase.Settings.DefaultFont;
            InitializeComponent();
            if (settings.TypeExplorerSize.Width > MinimumSize.Width) Size = settings.TypeExplorerSize;
            searchingInExternalClasspaths.Checked = settings.TypeExplorerSearchExternalClassPath;
            CreateItemsList();
            InitializeTree();
            InitializeTheme();
            RefreshTree();
            timer.Interval = PluginBase.MainForm.Settings.DisplayDelay;
            timer.Tick += OnTimerTick;
            timer.Start();
        }

        [CanBeNull] public ShowInHandler SetDocumentClass;
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

        [CanBeNull] public TypeNode SelectedNode => tree.SelectedNode as TypeNode;

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
            tree.BeginUpdate();
            tree.Nodes.Clear();
            FillTree();
            tree.ExpandAll();
            tree.EndUpdate();
        }

        void FillTree()
        {
            var search = input.Text.Trim();
            var openedTypes = this.openedTypes.ToList();
            var closedTypes = this.closedTypes.ToList();
            if (CurrentFilter != null)
            {
                var flags = (FlagType)CurrentFilter.Tag;
                openedTypes.RemoveAll(it => (TypeToClassModel[it].Flags & flags) == 0);
                closedTypes.RemoveAll(it => (TypeToClassModel[it].Flags & flags) == 0);
            }
            var openedCount = openedTypes.Count;
            if (search.Length == 0)
            {
                if (openedCount > 0) tree.Nodes.AddRange(CreateNodes(openedTypes, string.Empty).ToArray());
            }
            else
            {
                var maxItems = Settings.MaxItems;
                var openedMatches = openedCount > 0 ? SearchUtil.FindAll(openedTypes, search) : new List<string>();
                var closedMatches = new List<string>();
                if (maxItems > 0)
                {
                    if (openedMatches.Count >= maxItems) openedMatches = openedMatches.GetRange(0, maxItems);
                    maxItems -= openedMatches.Count;
                    if (maxItems > 0)
                    {
                        closedMatches = SearchUtil.FindAll(closedTypes, search);
                        if (closedMatches.Count >= maxItems) closedMatches = closedMatches.GetRange(0, maxItems);
                    }
                }
                else closedMatches = SearchUtil.FindAll(closedTypes, search);
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
        static TypeNode CreateNode([NotNull] string type)
        {
            var aClass = TypeToClassModel[type];
            return new TypeNode(aClass, PluginUI.GetIcon(aClass.Flags, aClass.Access));
        }

        [NotNull]
        static IEnumerable<TypeNode> SortNodes([NotNull] IEnumerable<TypeNode> nodes, [NotNull] string search)
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

        protected override void InitializeContextMenu()
        {
            base.InitializeContextMenu();
            QuickContextMenu.SetDocumentClassMenuItem.Click += OnSetDocumentClassMenuItemClick;
        }

        protected override void ShowContextMenu()
        {
            if (SelectedNode == null) return;
            ShowContextMenu(new Point(SelectedNode.Bounds.X, SelectedNode.Bounds.Bottom));
        }

        protected override void ShowContextMenu(Point position)
        {
            if (SelectedNode == null) return;
            ContextMenuStrip.Items.Clear();
            var classModel = SelectedNode.Model;
            var flags = classModel.Flags;
            var fileName = classModel.InFile.FileName;
            if ((flags & FlagType.Class) > 0
                && (flags & FlagType.Interface) == 0
                && (classModel.Access & Visibility.Public) > 0
                && !((Project)PluginBase.CurrentProject).IsDocumentClass(fileName))
            {
                ContextMenuStrip.Items.Add(QuickContextMenu.SetDocumentClassMenuItem);
                ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }
            ContextMenuStrip.Items.Add(QuickContextMenu.GotoPositionOrLineMenuItem);
            ContextMenuStrip.Items.Add(QuickContextMenu.ShowInQuickOutlineMenuItem);
            ContextMenuStrip.Items.Add(QuickContextMenu.ShowInClassHierarchyMenuItem);
            ContextMenuStrip.Items.Add(QuickContextMenu.ShowInProjectManagerMenuItem);
            if (File.Exists(fileName)) ContextMenuStrip.Items.Add(QuickContextMenu.ShowInFileExplorerMenuItem);
            ContextMenuStrip.Show(tree, position);
        }

        protected override void Navigate()
        {
            if (SelectedNode == null) return;
            DialogResult = DialogResult.OK;
        }

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

        void RefreshFilterTip(Button filter)
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
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            timer.Stop();
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
            if (tree.Nodes.Count == 0) return;
            var keyCode = e.KeyCode;
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

        void OnSetDocumentClassMenuItemClick(object sender, EventArgs e) => SetDocumentClass?.Invoke(this, SelectedNode.Model);

        #endregion
    }
}