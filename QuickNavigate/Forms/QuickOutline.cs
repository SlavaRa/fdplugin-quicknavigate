using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ASCompletion;
using ASCompletion.Context;
using ASCompletion.Model;
using JetBrains.Annotations;
using PluginCore;

namespace QuickNavigate.Forms
{
    public sealed partial class QuickOutline : Form
    {
        [NotNull]
        readonly Settings settings;

        [NotNull]
        readonly Brush defaultNodeBrush;

        readonly Brush selectedNodeBrush = new SolidBrush(SystemColors.ControlDarkDark);
        readonly ContextMenuStrip contextMenu = new ContextMenuStrip();
        readonly ContextMenu inputEmptyContextMenu = new ContextMenu();
        readonly List<Button> filters = new List<Button>();
        readonly Dictionary<Keys, Button> keysToFilter = new Dictionary<Keys, Button>();
        readonly Dictionary<Button, string> filterToEnabledTip = new Dictionary<Button, string>();
        readonly Dictionary<Button, string> filterToDisabledTip = new Dictionary<Button, string>();
        readonly Dictionary<FlagType, Button> flagToButton = new Dictionary<FlagType, Button>(); 

        /// <summary>
        /// Initializes a new instance of the QuickNavigate.Controls.QuickOutlineForm
        /// </summary>
        /// <param name="inFile"></param>
        /// <param name="inClass"></param>
        /// <param name="settings"></param>
        public QuickOutline([NotNull] FileModel inFile, [CanBeNull] ClassModel inClass, [NotNull] Settings settings)
        {
            InFile = inFile;
            InClass = inClass ?? ClassModel.VoidClass;
            this.settings = settings;
            Font = PluginBase.Settings.DefaultFont;
            InitializeComponent();
            if (settings.QuickOutlineSize.Width > MinimumSize.Width) Size = settings.QuickOutlineSize;
            defaultNodeBrush = new SolidBrush(tree.BackColor);
            InitializeContextMenu();
            InitializeTree();
            RefreshTree();
        }

        [CanBeNull]
        public event ShowInHandler ShowInClassHierarchy;

        [CanBeNull]
        ToolTip filterToolTip;

        [NotNull]
        public FileModel InFile { get; }

        [NotNull]
        public ClassModel InClass { get; }

        [CanBeNull]
        public TreeNode SelectedNode => tree.SelectedNode;

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                selectedNodeBrush.Dispose();
                defaultNodeBrush?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        void InitializeContextMenu()
        {
            contextMenu.Items.Add("Show in &Class Hierarchy", PluginBase.MainForm.FindImage("99|16|0|0"), OnShowInClassHierarchy);
        }

        void InitializeTree()
        {
            tree.ImageList = ASContext.Panel.TreeIcons;
            tree.ItemHeight = tree.ImageList.ImageSize.Height;
        }

        public void AddFilter(int imageIndex, FlagType flag, Keys shortcut, string enabledTip, string disabledTip)
        {
            if (flagToButton.ContainsKey(flag)) return;
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
            flagToButton[flag] = button;
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
            var isHaxe = InFile.haXe;
            if (InFile.Members.Count > 0) AddMembers(tree.Nodes, InFile.Members, isHaxe);
            foreach (var aClass in InFile.Classes)
            {
                var icon = PluginUI.GetIcon(aClass.Flags, aClass.Access);
                TreeNode node = new TypeNode(aClass, icon);
                tree.Nodes.Add(node);
                AddMembers(node.Nodes, aClass.Members, isHaxe, aClass.Equals(InClass));
            }
            if (SelectedNode != null || tree.Nodes.Count == 0) return;
            var search = input.Text.Trim();
            if (search.Length == 0)
                tree.SelectedNode = tree.Nodes.OfType<TypeNode>().First(it => it.Model.Equals(InClass));
            else
            {
                var nodes = tree.Nodes.OfType<TreeNode>().ToList().FindAll(it =>
                {
                    var word = ((TypeNode) it).Model.QualifiedName;
                    var score = PluginCore.Controls.CompletionList.SmartMatch(word, search, search.Length);
                    return score > 0 && score < 6;
                });
                tree.Nodes.Clear();
                if (nodes.Count == 0) return;
                tree.Nodes.AddRange(nodes.ToArray());
                tree.SelectedNode = tree.Nodes[0];
            }
        }

        void AddMembers(TreeNodeCollection nodes, MemberList members, bool isHaxe)
        {
            AddMembers(nodes, members, isHaxe, true);
        }
        void AddMembers(TreeNodeCollection nodes, MemberList members, bool isHaxe, bool currentClass)
        {
            var items = members.Items.ToList();
            if (CurrentFilter != null)
            {
                var flags = (FlagType) CurrentFilter.Tag;
                items.RemoveAll(it => (it.Flags & flags) == 0);
            }
            var search = input.Text.Trim();
            var searchIsNotEmpty = search.Length > 0;
            if (searchIsNotEmpty) items = SearchUtil.FindAll(items, search);
            foreach (var it in items)
            {
                var flags = it.Flags;
                var icon = PluginUI.GetIcon(flags, it.Access);
                var constrDecl = isHaxe && (flags & FlagType.Constructor) > 0 ? "new" : it.FullName;
                var node = new TreeNode(it.ToString(), icon, icon) {Tag = $"{constrDecl}@{it.LineFrom}"};
                nodes.Add(node);
            }
            if ((searchIsNotEmpty && SelectedNode == null || currentClass) && nodes.Count > 0)
                tree.SelectedNode = nodes[0];
        }

        void RefreshFilterTip(Button filter)
        {
            var text = filter == CurrentFilter ? filterToDisabledTip[filter] : filterToEnabledTip[filter];
            if (filterToolTip == null) filterToolTip = new ToolTip();
            filterToolTip.Show(text, filter, filter.Width, filter.Height);
        }

        void Navigate()
        {
            if (SelectedNode != null) DialogResult = DialogResult.OK;
        }

        void ShowContextMenu()
        {
            if (!(SelectedNode is TypeNode)) return;
            ShowContextMenu(new Point(SelectedNode.Bounds.X, SelectedNode.Bounds.Bottom));
        }

        void ShowContextMenu(Point position)
        {
            if (SelectedNode is TypeNode) contextMenu.Show(tree, position);
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
            settings.QuickOutlineSize = Size;
        }

        void OnInputTextChanged(object sender, EventArgs e) => RefreshTree();

        void OnInputPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Apps) input.ContextMenu = SelectedNode != null && (string) SelectedNode.Tag == "class" ? inputEmptyContextMenu : null;
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

        void OnTreeNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            TreeNode node = e.Node as TypeNode;
            if (node == null || (string) node.Tag != "class") return;
            tree.SelectedNode = node;
            ShowContextMenu(new Point(e.Location.X, node.Bounds.Bottom));
        }

        void OnTreeNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) => Navigate();

        void OnTreeDrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            var fillBrush = defaultNodeBrush;
            var drawBrush = Brushes.Black;
            var moduleBrush = Brushes.DimGray;
            if ((e.State & TreeNodeStates.Selected) > 0)
            {
                fillBrush = selectedNodeBrush;
                drawBrush = Brushes.White;
                moduleBrush = Brushes.LightGray;
            }
            var bounds = e.Bounds;
            var font = tree.Font;
            float x = bounds.X;
            var itemWidth = tree.Width - x;
            var graphics = e.Graphics;
            graphics.FillRectangle(fillBrush, x, bounds.Y, itemWidth, tree.ItemHeight);
            var text = e.Node.Text;
            graphics.DrawString(text, font, drawBrush, bounds.Left, bounds.Top, StringFormat.GenericDefault);
            var node = e.Node as TypeNode;
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

        void OnShowInClassHierarchy(object sender, EventArgs e)
        {
            Debug.Assert(ShowInClassHierarchy != null, "ShowInClassHierarchy != null");
            ShowInClassHierarchy(this, ((TypeNode) SelectedNode).Model);
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