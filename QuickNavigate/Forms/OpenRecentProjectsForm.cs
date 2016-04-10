using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using PluginCore;
using PluginCore.Helpers;
using ProjectManager.Controls;

namespace QuickNavigate.Forms
{
    public sealed partial class OpenRecentProjectsForm : Form
    {
        [NotNull] readonly Settings settings;
        [NotNull] [ItemNotNull] readonly List<string> recentProjects = ProjectManager.PluginMain.Settings.RecentProjects.Where(File.Exists).ToList();

        public OpenRecentProjectsForm([NotNull] Settings settings)
        {
            this.settings = settings;
            Font = PluginBase.Settings.DefaultFont;
            InitializeComponent();
            if (settings.RecentProjectsSize.Width > MinimumSize.Width) Size = settings.RecentProjectsSize;
            InitializeTree();
            InitializeContextMenu();
            InitializeTheme();
            openInNewWindow.Visible = PluginBase.MainForm.MultiInstanceMode;
            RefrestTree();
        }

        [CanBeNull] ContextMenuStrip contextMenu;

        public bool InNewWindow { get; private set; }

        [CanBeNull]
        public string SelectedItem => tree.SelectedNode?.Text;
        
        void InitializeTree()
        {
            tree.ImageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = ScaleHelper.Scale(new Size(16, 16))
            };
            tree.ImageList.Images.Add(Icons.Project.Img);
            tree.ItemHeight = tree.ImageList.ImageSize.Height;
        }

        void InitializeContextMenu()
        {
            if (!PluginBase.MainForm.MultiInstanceMode) return;
            input.ContextMenu = new ContextMenu();
            contextMenu = new ContextMenuStrip {Renderer = new DockPanelStripRenderer(false)};
            contextMenu.Items.Add("Open in new Window").Click += (s, args) => NavigateInNewWindow();
            contextMenu.Items[0].Select();
        }

        void InitializeTheme()
        {
            input.BackColor = PluginBase.MainForm.GetThemeColor("TextBox.BackColor", SystemColors.Window);
            input.ForeColor = PluginBase.MainForm.GetThemeColor("TextBox.ForeColor", SystemColors.WindowText);
            tree.BackColor = PluginBase.MainForm.GetThemeColor("TreeView.BackColor", SystemColors.Window);
            tree.ForeColor = PluginBase.MainForm.GetThemeColor("TreeView.ForeColor", SystemColors.WindowText);
            open.BackColor = PluginBase.MainForm.GetThemeColor("TreeView.BackColor", SystemColors.Window);
            open.ForeColor = PluginBase.MainForm.GetThemeColor("TreeView.ForeColor", SystemColors.WindowText);
            openInNewWindow.BackColor = PluginBase.MainForm.GetThemeColor("TreeView.BackColor", SystemColors.Window);
            openInNewWindow.ForeColor = PluginBase.MainForm.GetThemeColor("TreeView.ForeColor", SystemColors.WindowText);
            cancel.BackColor = PluginBase.MainForm.GetThemeColor("TreeView.BackColor", SystemColors.Window);
            cancel.ForeColor = PluginBase.MainForm.GetThemeColor("TreeView.ForeColor", SystemColors.WindowText);
            BackColor = PluginBase.MainForm.GetThemeColor("TreeView.BackColor", SystemColors.Window);
            ForeColor = PluginBase.MainForm.GetThemeColor("TreeView.ForeColor", SystemColors.WindowText);
        }

        void RefrestTree()
        {
            if (recentProjects.Count == 0) return;
            tree.BeginUpdate();
            tree.Nodes.Clear();
            FillTree();
            if (tree.Nodes.Count > 0) tree.SelectedNode = tree.Nodes[0];
            else RefreshButtons();
            tree.EndUpdate();
        }

        void RefreshButtons()
        {
            open.Enabled = false;
            openInNewWindow.Enabled = false;
        }

        void FillTree()
        {
            var search = input.Text;
            var projects = search.Length > 0 ? SearchUtil.FindAll(recentProjects, search) : recentProjects;
            if (projects.Count == 0) return;
            foreach (var it in projects)
            {
                tree.Nodes.Add(it, it, 0);
            }
        }

        void Navigate()
        {
            if (SelectedItem == null) return;
            DialogResult = DialogResult.OK;
        }

        void NavigateInNewWindow()
        {
            InNewWindow = SelectedItem != null;
            Navigate();
        }

        void ShowContextMenu()
        {
            var selectedNode = tree.SelectedNode;
            if (selectedNode == null) return;
            ShowContextMenu(new Point(selectedNode.Bounds.X, selectedNode.Bounds.Bottom));
        }

        void ShowContextMenu([NotNull] Point position) => contextMenu?.Show(tree, position);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            settings.RecentProjectsSize = Size;
        }

        void OnInputTextChanged(object sender, EventArgs e) => RefrestTree();

        void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            if (tree.Nodes.Count < 2) return;
            var lastIndex = tree.Nodes.Count - 1;
            var index = tree.SelectedNode.Index;
            switch (e.KeyCode)
            {
                case Keys.L:
                    e.Handled = e.Control;
                    return;
                case Keys.Down:
                    if (index < lastIndex) tree.SelectedNode = tree.SelectedNode.NextNode;
                    else if (PluginBase.MainForm.Settings.WrapList) tree.SelectedNode = tree.Nodes[0];
                    break;
                case Keys.Up:
                    if (index > 0) tree.SelectedNode = tree.SelectedNode.PrevNode;
                    else if (PluginBase.MainForm.Settings.WrapList) tree.SelectedNode = tree.Nodes[lastIndex];
                    break;
                case Keys.Home:
                    tree.SelectedNode = tree.Nodes[0];
                    break;
                case Keys.End:
                    tree.SelectedNode = tree.Nodes[lastIndex];
                    break;
                default: return;
            }
            e.Handled = true;
        }

        void OnTreeMouseDoubleClick(object sender, MouseEventArgs e) => Navigate();

        void OnTreeAfterSelect(object sender, TreeViewEventArgs e) => open.Enabled = SelectedItem != null;

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
            var text = Path.GetFileNameWithoutExtension(e.Node.Text);
            float x = bounds.X;
            var itemWidth = tree.Width - x;
            var graphics = e.Graphics;
            graphics.FillRectangle(new SolidBrush(fillBrush), x, bounds.Y, itemWidth, tree.ItemHeight);
            var font = tree.Font;
            graphics.DrawString(text, font, new SolidBrush(textBrush), x, bounds.Top, StringFormat.GenericDefault);
            var path = Path.GetDirectoryName(e.Node.Text);
            if (string.IsNullOrEmpty(path)) return;
            x += graphics.MeasureString(text, font).Width;
            graphics.DrawString($"({path})", font, moduleBrush, x, bounds.Top, StringFormat.GenericDefault);
        }

        void OnOpenInNewWindowClick(object sender, EventArgs e) => NavigateInNewWindow();
    }
}