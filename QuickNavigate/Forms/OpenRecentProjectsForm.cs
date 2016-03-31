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
        [NotNull] readonly Brush defaultNodeBrush;
        [NotNull] readonly Brush selectedNodeBrush = new SolidBrush(SystemColors.ControlDarkDark);

        public OpenRecentProjectsForm([NotNull] Settings settings)
        {
            this.settings = settings;
            Font = PluginBase.Settings.DefaultFont;
            InitializeComponent();
            InitializeTree();
            defaultNodeBrush = new SolidBrush(tree.BackColor);
            if (settings.RecentProjectsSize.Width > MinimumSize.Width) Size = settings.RecentProjectsSize;
            RefrestTree();
        }

        [CanBeNull]
        public string SelectedItem => tree?.SelectedNode.Text;

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

        void RefrestTree()
        {
            if (recentProjects.Count == 0) return;
            tree.BeginUpdate();
            tree.Nodes.Clear();
            FillTree();
            if (tree.Nodes.Count > 0) tree.SelectedNode = tree.Nodes[0];
            else open.Enabled = false;
            tree.EndUpdate();
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
                    else if (settings.WrapList) tree.SelectedNode = tree.Nodes[0];
                    break;
                case Keys.Up:
                    if (index > 0) tree.SelectedNode = tree.SelectedNode.PrevNode;
                    else if (settings.WrapList) tree.SelectedNode = tree.Nodes[lastIndex];
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
            var fillBrush = defaultNodeBrush;
            var textBrush = Brushes.Black;
            var moduleBrush = Brushes.DimGray;
            if ((e.State & TreeNodeStates.Selected) > 0)
            {
                fillBrush = selectedNodeBrush;
                textBrush = Brushes.White;
                moduleBrush = Brushes.LightGray;
            }
            var bounds = e.Bounds;
            var text = Path.GetFileNameWithoutExtension(e.Node.Text);
            float x = bounds.X;
            var itemWidth = tree.Width - x;
            var graphics = e.Graphics;
            graphics.FillRectangle(fillBrush, x, bounds.Y, itemWidth, tree.ItemHeight);
            var font = tree.Font;
            graphics.DrawString(text, font, textBrush, x, bounds.Top, StringFormat.GenericDefault);
            var path = Path.GetDirectoryName(e.Node.Text);
            if (string.IsNullOrEmpty(path)) return;
            x += graphics.MeasureString(text, font).Width;
            graphics.DrawString($"({path})", font, moduleBrush, x, bounds.Top, StringFormat.GenericDefault);
        }
    }
}