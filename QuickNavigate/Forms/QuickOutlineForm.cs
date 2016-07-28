using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ASCompletion.Context;
using ASCompletion.Model;
using JetBrains.Annotations;
using PluginCore;
using QuickNavigate.Helpers;

namespace QuickNavigate.Forms
{
    public sealed partial class QuickOutlineForm : QuickForm
    {
        readonly ContextMenuStrip contextMenu = new ContextMenuStrip { Renderer = new DockPanelStripRenderer(false) };
        readonly ContextMenu inputEmptyContextMenu = new ContextMenu();
        readonly List<Button> filters = new List<Button>();
        readonly Dictionary<Keys, Button> keysToFilter = new Dictionary<Keys, Button>();
        readonly Dictionary<Button, string> filterToEnabledTip = new Dictionary<Button, string>();
        readonly Dictionary<Button, string> filterToDisabledTip = new Dictionary<Button, string>();
        readonly Dictionary<FlagType, Button> flagToFilter = new Dictionary<FlagType, Button>();

        /// <summary>
        /// Initializes a new instance of the QuickNavigate.Controls.QuickOutlineForm
        /// </summary>
        /// <param name="inFile"></param>
        /// <param name="inClass"></param>
        /// <param name="settings"></param>
        public QuickOutlineForm([NotNull] FileModel inFile, [CanBeNull] ClassModel inClass, [NotNull] Settings settings) : base(settings)
        {
            InFile = inFile;
            InClass = inClass ?? ClassModel.VoidClass;
            InitializeComponent();
            InitializeTree();
            InitializeTheme();
            RefreshTree();
        }

        [CanBeNull] ToolTip filterToolTip;

        [NotNull]
        public FileModel InFile { get; }

        [NotNull]
        public ClassModel InClass { get; }

        public override TreeNode SelectedNode => tree.SelectedNode;

        [CanBeNull]
        Button currentFilter;

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
            FillTree(input.Text.Trim());
            tree.ExpandAll();
            tree.EndUpdate();
        }

        void FillTree(string search)
        {
            var isHaxe = InFile.haXe;
            if (InFile.Members.Count > 0) FillNodes(tree.Nodes, InFile, InFile.Members, isHaxe, search);
            foreach (var classModel in InFile.Classes)
            {
                var node = NodeFactory.CreateTreeNode(classModel);
                tree.Nodes.Add(node);
                FillNodes(node.Nodes, InFile, classModel.Members, isHaxe, classModel.Equals(InClass), search);
            }
            if (SelectedNode != null || tree.Nodes.Count == 0) return;
            if (search.Length == 0)
            {
                if (InClass.Equals(ClassModel.VoidClass)) tree.SelectedNode = tree.TopNode;
                else tree.SelectedNode = tree.Nodes.OfType<ClassNode>().FirstOrDefault(it => it.Model.Equals(InClass));
            }   
            else
            {
                var nodes = tree.Nodes.OfType<ClassNode>().ToList().FindAll(it =>
                {
                    var word = it.Model.QualifiedName;
                    var score = PluginCore.Controls.CompletionList.SmartMatch(word, search, search.Length);
                    return score > 0 && score < 6;
                });
                tree.Nodes.Clear();
                if (nodes.Count == 0) return;
                tree.Nodes.AddRange(nodes.ToArray());
                tree.SelectedNode = tree.TopNode;
            }
        }

        void FillNodes(TreeNodeCollection nodes, FileModel inFile, MemberList members, bool isHaxe, string search)
        {
            FillNodes(nodes, inFile, members, isHaxe, true, search);
        }

        void FillNodes(TreeNodeCollection nodes, FileModel inFile, MemberList members, bool isHaxe, bool currentClass, string search)
        {
            var items = FilterTypes(members.Items.ToList());
            items = SearchUtil.FindAll(items, search);
            foreach (var it in items)
            {
                nodes.Add(NodeFactory.CreateTreeNode(inFile, isHaxe, it));
            }
            if ((search.Length > 0 && SelectedNode == null || currentClass) && nodes.Count > 0)
                tree.SelectedNode = nodes[0];
        }

        [NotNull]
        List<MemberModel> FilterTypes(List<MemberModel> list)
        {
            if (CurrentFilter != null)
            {
                var flags = (FlagType) CurrentFilter.Tag;
                list.RemoveAll(it => (it.Flags & flags) == 0);
            }
            return list;
        }

        protected override void Navigate()
        {
            if (SelectedNode != null) DialogResult = DialogResult.OK;
        }

        protected override void ShowContextMenu()
        {
            if (!(SelectedNode is ClassNode)) return;
            ShowContextMenu(new Point(SelectedNode.Bounds.X, SelectedNode.Bounds.Bottom));
        }

        protected override void ShowContextMenu(Point position)
        {
            if (!(SelectedNode is ClassNode)) return;
            contextMenu.Items.Clear();
            contextMenu.Items.Add(QuickContextMenuItem.ShowInClassHierarchyMenuItem);
            contextMenu.Show(tree, position);
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

        void RefreshFilterTip(Button filter)
        {
            var text = filter == CurrentFilter ? filterToDisabledTip[filter] : filterToEnabledTip[filter];
            if (filterToolTip == null) filterToolTip = new ToolTip();
            filterToolTip.Show(text, filter, filter.Width, filter.Height);
        }

        #region Event Handlers

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (Settings != null && Settings.QuickOutlineSize.Width > MinimumSize.Width) Size = Settings.QuickOutlineSize;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            var keyCode = e.KeyCode;
            if (e.Alt && keysToFilter.ContainsKey(keyCode))
            {
                CurrentFilter = keysToFilter[keyCode];
                return;
            }
            switch (keyCode)
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

        protected override void OnFormClosing(FormClosingEventArgs e) => Settings.QuickOutlineSize = Size;

        void OnInputTextChanged(object sender, EventArgs e) => RefreshTree();

        void OnInputPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Apps) input.ContextMenu = SelectedNode != null && (string) SelectedNode.Tag == "class" ? inputEmptyContextMenu : null;
        }

        void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            if (tree.Nodes.Count == 0) return;
            var keyCode = e.KeyCode;
            if (e.Alt && keysToFilter.ContainsKey(keyCode))
            {
                e.Handled = true;
                return;
            }
            TreeNode node;
            var visibleCount = tree.VisibleCount - 1;
            switch (keyCode)
            {
                case Keys.Space:
                    e.Handled = true;
                    return;
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

        protected override void OnTreeNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            TreeNode node = e.Node as ClassNode;
            if (node == null || (string) node.Tag != "class") return;
            tree.SelectedNode = node;
            ShowContextMenu(new Point(e.Location.X, node.Bounds.Bottom));
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
            var font = tree.Font;
            float x = bounds.X;
            var itemWidth = tree.Width - x;
            var graphics = e.Graphics;
            graphics.FillRectangle(new SolidBrush(fillBrush), x, bounds.Y, itemWidth, tree.ItemHeight);
            var text = e.Node.Text;
            graphics.DrawString(text, font, new SolidBrush(textBrush), bounds.Left, bounds.Top, StringFormat.GenericDefault);
            var node = e.Node as ClassNode;
            if (node == null) return;
            if (!string.IsNullOrEmpty(node.In))
            {
                x += graphics.MeasureString(text, font).Width;
                graphics.DrawString($"({node.In})", font, moduleBrush, x, bounds.Top, StringFormat.GenericDefault);
            }
            if (!node.IsPrivate) return;
            font = new Font(font, FontStyle.Underline);
            x = itemWidth - graphics.MeasureString("(private)", font).Width;
            graphics.DrawString("(private)", font, moduleBrush, x, bounds.Y, StringFormat.GenericTypographic);
        }

        void OnFilterMouseHover(object sender, EventArgs e) => RefreshFilterTip((Button) sender);

        void OnFilterMouseLeave(object sender, EventArgs e)
        {
            if (filterToolTip == null) return;
            filterToolTip.Hide((Button) sender);
            filterToolTip = null;
        }

        void OnFilterMouseClick(object sender, EventArgs e) => CurrentFilter = filters.First(sender.Equals);

        #endregion
    }
}