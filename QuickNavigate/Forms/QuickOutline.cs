using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ASCompletion;
using ASCompletion.Model;
using JetBrains.Annotations;
using PluginCore;
using QuickNavigate.Helpers;
using SmartMemberComparer = QuickNavigate.Collections.SmartMemberComparer;

namespace QuickNavigate.Forms
{
    /// <summary>
    /// </summary>
    public sealed partial class QuickOutline : Form
    {
        [CanBeNull] public event ShowInHandler ShowInClassHierarchy;
        readonly Settings settings;
        readonly Brush selectedNodeBrush = new SolidBrush(SystemColors.ControlDarkDark);
        readonly Brush defaultNodeBrush;
        readonly ContextMenuStrip contextMenu = new ContextMenuStrip();
        readonly ContextMenu inputEmptyContextMenu = new ContextMenu();
        readonly List<Button> filters = new List<Button>();
        readonly Dictionary<Keys, Button> keysToFilter = new Dictionary<Keys, Button>();
        readonly Dictionary<Button, string> filterToEnabledTip = new Dictionary<Button, string>();
        readonly Dictionary<Button, string> filterToDisabledTip = new Dictionary<Button, string>();

        /// <summary>
        /// Initializes a new instance of the QuickNavigate.Controls.QuickOutlineForm
        /// </summary>
        /// <param name="inClass"></param>
        /// <param name="settings"></param>
        public QuickOutline([NotNull] ClassModel inClass, [NotNull] Settings settings) : this(null, inClass, settings)
        {
        }

        /// <summary>
        /// Initializes a new instance of the QuickNavigate.Controls.QuickOutlineForm
        /// </summary>
        /// <param name="inFile"></param>
        /// <param name="settings"></param>
        public QuickOutline([NotNull] FileModel inFile, [NotNull] Settings settings) : this(inFile, null, settings)
        {
        }

        /// <summary>
        /// Initializes a new instance of the QuickNavigate.Controls.QuickOutlineForm
        /// </summary>
        /// <param name="inFile"></param>
        /// <param name="inClass"></param>
        /// <param name="settings"></param>
        QuickOutline([CanBeNull] FileModel inFile, [CanBeNull] ClassModel inClass, [NotNull] Settings settings)
        {
            InFile = inFile;
            InClass = inClass;
            this.settings = settings;
            InitializeComponent();
            if (settings.OutlineFormSize.Width > MinimumSize.Width) Size = settings.OutlineFormSize;
            defaultNodeBrush = new SolidBrush(tree.BackColor);
            CreateContextMenu();
            InitializeTree();
            InitializeFilters();
            RefreshTree();
        }

        [CanBeNull] ToolTip filterToolTip;
        [CanBeNull] public ClassModel InClass { get; }
        [CanBeNull] public FileModel InFile { get; }
        [CanBeNull] public TreeNode SelectedNode => tree.SelectedNode;

        [CanBeNull] Button currentFilter;
        [CanBeNull] Button CurrentFilter
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

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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

        /// <summary>
        /// </summary>
        void CreateContextMenu()
        {
            contextMenu.Items.Add("Show in &Class Hierarchy", PluginBase.MainForm.FindImage("99|16|0|0"), OnShowInClassHierarchy);
        }

        /// <summary>
        /// </summary>
        void InitializeTree() => tree.ImageList = FormHelper.GetTreeIcons();

        void InitializeFilters()
        {
            ImageList imageList = tree.ImageList;
            classes.ImageList = imageList;
            classes.ImageIndex = PluginUI.ICON_TYPE;
            classes.Tag = FlagType.Class;
            filterToEnabledTip[classes] = "Show only classes(Alt+C or left click)";
            filterToDisabledTip[classes] = "Show all(Alt+C or left click)";
            keysToFilter[Keys.C] = classes;
            filters.Add(classes);
            fields.ImageList = imageList;
            fields.ImageIndex = PluginUI.ICON_VAR;
            fields.Tag = FlagType.Variable;
            filterToEnabledTip[fields] = "Show only fields(Alt+F or left click)";
            filterToDisabledTip[fields] = "Show all(Alt+F or left click)";
            keysToFilter[Keys.F] = fields;
            filters.Add(fields);
            properties.ImageList = imageList;
            properties.ImageIndex = PluginUI.ICON_PROPERTY;
            properties.Tag = FlagType.Getter | FlagType.Setter;
            filterToEnabledTip[properties] = "Show only properties(Alt+P or left click)";
            filterToDisabledTip[properties] = "Show all(Alt+P or left click)";
            keysToFilter[Keys.P] = properties;
            filters.Add(properties);
            methods.ImageList = imageList;
            methods.ImageIndex = PluginUI.ICON_FUNCTION;
            methods.Tag = FlagType.Function;
            filterToEnabledTip[methods] = "Show only methods(Alt+M or left click)";
            filterToDisabledTip[methods] = "Show all(Alt+M or left click)";
            keysToFilter[Keys.M] = methods;
            filters.Add(methods);
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
            bool isHaxe;
            List<ClassModel> classes;
            if (InFile != null)
            {
                if (InFile == FileModel.Ignore) return;
                isHaxe = InFile.haXe;
                if (InFile.Members.Count > 0) AddMembers(tree.Nodes, InFile.Members, isHaxe);
                classes = InFile.Classes;
            } 
            else if (InClass != null)
            {
                isHaxe = InClass.InFile.haXe;
                classes = new List<ClassModel> {InClass};
            }
            else return;
            foreach (ClassModel aClass in classes)
            {
                int icon = PluginUI.GetIcon(aClass.Flags, aClass.Access);
                TreeNode node = new TypeNode(aClass, icon) { Tag = "class" };
                tree.Nodes.Add(node);
                AddMembers(node.Nodes, aClass.Members, isHaxe);
            }
            if (SelectedNode == null && tree.Nodes.Count > 0) tree.SelectedNode = tree.Nodes[0];
        }

        /// <summary>
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="members"></param>
        /// <param name="isHaxe"></param>
        void AddMembers(TreeNodeCollection nodes, MemberList members, bool isHaxe)
        {
            List<MemberModel> items = members.Items.ToList();
            if (CurrentFilter != null)
            {
                FlagType flags = (FlagType) CurrentFilter.Tag;
                items.RemoveAll(it => (it.Flags & flags) == 0);
            }
            bool noCase = !settings.OutlineFormMatchCase;
            string search = input.Text.Trim();
            bool searchIsNotEmpty = search.Length > 0;
            if (searchIsNotEmpty && noCase) search = search.ToLower();
            items.Sort(new SmartMemberComparer(search, noCase));
            bool wholeWord = settings.OutlineFormWholeWord;
            foreach (MemberModel member in items)
            {
                string fullName = member.FullName;
                if (searchIsNotEmpty)
                {
                    string name = noCase ? fullName.ToLower() : fullName;
                    if (wholeWord && !name.StartsWith(search) || !name.Contains(search))
                        continue;
                }
                FlagType flags = member.Flags;
                int icon = PluginUI.GetIcon(flags, member.Access);
                string constrDeclName = isHaxe && (flags & FlagType.Constructor) > 0 ? "new" : fullName;
                string tag = $"{constrDeclName}@{member.LineFrom}";
                nodes.Add(new TreeNode(member.ToString(), icon, icon) {Tag = tag});
            }
            if (SelectedNode == null && nodes.Count > 0) tree.SelectedNode = nodes[0];
        }

        void RefreshFilterTip(Button filter)
        {
            string text = filter == CurrentFilter ? filterToDisabledTip[filter] : filterToEnabledTip[filter];
            if (filterToolTip == null) filterToolTip = new ToolTip();
            filterToolTip.Show(text, filter, filter.Width, filter.Height);
        }

        /// <summary>
        /// </summary>
        void Navigate()
        {
            if (SelectedNode != null) DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Displays the shortcut menu.
        /// </summary>
        void ShowContextMenu()
        {
            TreeNode node = SelectedNode as TypeNode;
            if (node != null && (string) node.Tag == "class") ShowContextMenu(new Point(node.Bounds.X, node.Bounds.Bottom));
        }

        /// <summary>
        /// Displays the shortcut menu.
        /// </summary>
        void ShowContextMenu(Point position)
        {
            TreeNode node = SelectedNode as TypeNode;
            if (node != null && (string) node.Tag == "class") contextMenu.Show(tree, position);
        }

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"/> that contains the event data. </param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            Keys keyCode = e.KeyCode;
            if (keysToFilter.ContainsKey(keyCode))
            {
                CurrentFilter = keysToFilter[keyCode];
                return;
            }
            switch (keyCode)
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
                    ShowContextMenu();
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"/> that contains the event data. </param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            settings.OutlineFormSize = Size;
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
            if (e.KeyCode == Keys.Apps) input.ContextMenu = SelectedNode != null && (string) SelectedNode.Tag == "class" ? inputEmptyContextMenu : null;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            Keys keyCode = e.KeyCode;
            if (keysToFilter.ContainsKey(keyCode))
            {
                e.Handled = e.Alt;
                return;
            }
            TreeNode node;
            int visibleCount = tree.VisibleCount - 1;
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
        void OnTreeNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            TreeNode node = e.Node as TypeNode;
            if (node == null || (string) node.Tag != "class") return;
            tree.SelectedNode = node;
            ShowContextMenu(new Point(e.Location.X, node.Bounds.Bottom));
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnTreeNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) => Navigate();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnTreeDrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            Brush fillBrush = defaultNodeBrush;
            Brush drawBrush = Brushes.Black;
            Brush moduleBrush = Brushes.DimGray;
            if ((e.State & TreeNodeStates.Selected) > 0)
            {
                fillBrush = selectedNodeBrush;
                drawBrush = Brushes.White;
                moduleBrush = Brushes.LightGray;
            }
            Rectangle bounds = e.Bounds;
            Font font = tree.Font;
            float x = bounds.X;
            float itemWidth = tree.Width - x;
            Graphics graphics = e.Graphics;
            graphics.FillRectangle(fillBrush, x, bounds.Y, itemWidth, tree.ItemHeight);
            string text = e.Node.Text;
            graphics.DrawString(text, font, drawBrush, bounds.Left, bounds.Top, StringFormat.GenericDefault);
            TypeNode node = e.Node as TypeNode;
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

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnShowInClassHierarchy(object sender, EventArgs e) => ShowInClassHierarchy(this, ((TypeNode)SelectedNode).Model);

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