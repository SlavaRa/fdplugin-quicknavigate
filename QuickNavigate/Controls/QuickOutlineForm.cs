using ASCompletion;
using ASCompletion.Context;
using ASCompletion.Model;
using PluginCore;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuickNavigate
{
    public partial class QuickOutlineForm : Form
    {
        private readonly Settings settings;
        private readonly Brush selectedNodeBrush;
        private readonly Brush defaultNodeBrush;
        
        public QuickOutlineForm(Settings settings)
        {
            this.settings = settings;
            InitializeComponent();
            if (settings.OutlineFormSize.Width > MinimumSize.Width) Size = settings.OutlineFormSize;
            (PluginBase.MainForm as FlashDevelop.MainForm).ThemeControls(this);
            InitTree();
            RefreshTree();
            selectedNodeBrush = new SolidBrush(SystemColors.ControlDarkDark);
            defaultNodeBrush = new SolidBrush(tree.BackColor);
        }

        private void InitTree()
        {
            ImageList treeIcons = new ImageList();
            treeIcons.TransparentColor = Color.Transparent;
            treeIcons.Images.AddRange(new Bitmap[] {
                new Bitmap(PluginUI.GetStream("FilePlain.png")),
                new Bitmap(PluginUI.GetStream("FolderClosed.png")),
                new Bitmap(PluginUI.GetStream("FolderOpen.png")),
                new Bitmap(PluginUI.GetStream("CheckAS.png")),
                new Bitmap(PluginUI.GetStream("QuickBuild.png")),
                new Bitmap(PluginUI.GetStream("Package.png")),
                new Bitmap(PluginUI.GetStream("Interface.png")),
                new Bitmap(PluginUI.GetStream("Intrinsic.png")),
                new Bitmap(PluginUI.GetStream("Class.png")),
                new Bitmap(PluginUI.GetStream("Variable.png")),
                new Bitmap(PluginUI.GetStream("VariableProtected.png")),
                new Bitmap(PluginUI.GetStream("VariablePrivate.png")),
                new Bitmap(PluginUI.GetStream("VariableStatic.png")),
                new Bitmap(PluginUI.GetStream("VariableStaticProtected.png")),
                new Bitmap(PluginUI.GetStream("VariableStaticPrivate.png")),
                new Bitmap(PluginUI.GetStream("Const.png")),
                new Bitmap(PluginUI.GetStream("ConstProtected.png")),
                new Bitmap(PluginUI.GetStream("ConstPrivate.png")),
                new Bitmap(PluginUI.GetStream("Const.png")),
                new Bitmap(PluginUI.GetStream("ConstProtected.png")),
                new Bitmap(PluginUI.GetStream("ConstPrivate.png")),
                new Bitmap(PluginUI.GetStream("Method.png")),
                new Bitmap(PluginUI.GetStream("MethodProtected.png")),
                new Bitmap(PluginUI.GetStream("MethodPrivate.png")),
                new Bitmap(PluginUI.GetStream("MethodStatic.png")),
                new Bitmap(PluginUI.GetStream("MethodStaticProtected.png")),
                new Bitmap(PluginUI.GetStream("MethodStaticPrivate.png")),
                new Bitmap(PluginUI.GetStream("Property.png")),
                new Bitmap(PluginUI.GetStream("PropertyProtected.png")),
                new Bitmap(PluginUI.GetStream("PropertyPrivate.png")),
                new Bitmap(PluginUI.GetStream("PropertyStatic.png")),
                new Bitmap(PluginUI.GetStream("PropertyStaticProtected.png")),
                new Bitmap(PluginUI.GetStream("PropertyStaticPrivate.png")),
                new Bitmap(PluginUI.GetStream("Template.png")),
                new Bitmap(PluginUI.GetStream("Declaration.png"))
            });
            tree.ImageList = treeIcons;
        }

        private void Navigate()
        {
            if (tree.SelectedNode != null)
            {
                ASContext.Context.OnSelectOutlineNode(tree.SelectedNode);
                Close();
            }
        }

        private void RefreshTree()
        {
            tree.BeginUpdate();
            tree.Nodes.Clear();
            FillTree();
            tree.EndUpdate();
        }

        private void FillTree()
        {
            FileModel model = ASContext.Context.CurrentModel;
            if (model == FileModel.Ignore) return;
            if (model.Members.Count > 0) AddMembers(tree.Nodes, model.Members);
            foreach (ClassModel aClass in model.Classes)
            {
                int icon = PluginUI.GetIcon(aClass.Flags, aClass.Access);
                TreeNode node = new TreeNode(aClass.Name, icon, icon);
                node.Tag = "class";
                tree.Nodes.Add(node);
                AddMembers(node.Nodes, aClass.Members);
                node.Expand();
            }
        }

        private void AddMembers(TreeNodeCollection nodes, MemberList members)
        {
            bool wholeWord = settings.OutlineFormWholeWord;
            bool matchCase = settings.OutlineFormMatchCase;
            string search = matchCase ? input.Text.Trim() : input.Text.ToLower().Trim();
            bool searchIsNotEmpty = !string.IsNullOrEmpty(search);
            foreach (MemberModel member in members)
            {
                string memberToString = member.ToString().Trim();
                string memberText = matchCase ? memberToString : memberToString.ToLower();
                if (searchIsNotEmpty && (!wholeWord && memberText.IndexOf(search) == -1 || wholeWord && !memberText.StartsWith(search)))
                    continue;
                int imageIndex = PluginUI.GetIcon(member.Flags, member.Access);
                TreeNode node = new TreeNode(memberToString, imageIndex, imageIndex);
                node.Tag = member.Name + "@" + member.LineFrom;
                node.BackColor = Color.Black;
                nodes.Add(node);
                if (tree.SelectedNode == null) tree.SelectedNode = node;
            }
        }

        #region Event Handlers

        private void QuickOutlineForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    Close();
                    break;
                case Keys.Enter:
                    e.Handled = true;
                    Navigate();
                    break;
            }
        }

        private void QuickOutlineForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            settings.OutlineFormSize = Size;
        }

        private void Input_TextChanged(object sender, EventArgs e)
        {
            RefreshTree();
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control || e.Shift || tree.SelectedNode == null) return;
            TreeNode node;
            int visibleCount = tree.VisibleCount - 1;
            switch (e.KeyCode)
            {
                case Keys.Down:
                    if (tree.SelectedNode.NextVisibleNode != null) tree.SelectedNode = tree.SelectedNode.NextVisibleNode;
                    else tree.SelectedNode = tree.Nodes[0];
                    break;
                case Keys.Up:
                    if (tree.SelectedNode.PrevVisibleNode != null) tree.SelectedNode = tree.SelectedNode.PrevVisibleNode;
                    else
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

        private void Tree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Navigate();
        }

        private void Tree_DrawNode(object sender, System.Windows.Forms.DrawTreeNodeEventArgs e)
        {
            if ((e.State & TreeNodeStates.Selected) > 0)
            {
                e.Graphics.FillRectangle(selectedNodeBrush, e.Bounds);
                e.Graphics.DrawString(e.Node.Text, tree.Font, Brushes.White, e.Bounds.Left, e.Bounds.Top, StringFormat.GenericDefault);
            }
            else
            {
                e.Graphics.FillRectangle(defaultNodeBrush, e.Bounds);
                e.Graphics.DrawString(e.Node.Text, tree.Font, Brushes.Black, e.Bounds.Left, e.Bounds.Top, StringFormat.GenericDefault);
            }
        }

        #endregion
    }
}