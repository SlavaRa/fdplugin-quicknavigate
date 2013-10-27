using System;
using System.Drawing;
using System.Windows.Forms;
using ASCompletion.Context;
using ASCompletion.Model;

namespace QuickNavigatePlugin
{
    public partial class QuickOutlineForm : Form
    {
        private PluginMain plugin;
        
        public QuickOutlineForm(PluginMain plugin)
        {
            this.plugin = plugin;
            InitializeComponent();

            if ((plugin.Settings as Settings).OutlineFormSize.Width > MinimumSize.Width)
                Size = (plugin.Settings as Settings).OutlineFormSize;

            InitTree();
            RefreshTree();
        }

        private void InitTree()
        {
            ImageList treeIcons = new ImageList();
            treeIcons.TransparentColor = Color.Transparent;
            treeIcons.Images.AddRange(new Bitmap[] {
                new Bitmap(ASCompletion.PluginUI.GetStream("FilePlain.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("FolderClosed.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("FolderOpen.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("CheckAS.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("QuickBuild.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("Package.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("Interface.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("Intrinsic.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("Class.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("Variable.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("VariableProtected.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("VariablePrivate.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("VariableStatic.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("VariableStaticProtected.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("VariableStaticPrivate.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("Const.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("ConstProtected.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("ConstPrivate.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("Const.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("ConstProtected.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("ConstPrivate.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("Method.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("MethodProtected.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("MethodPrivate.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("MethodStatic.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("MethodStaticProtected.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("MethodStaticPrivate.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("Property.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("PropertyProtected.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("PropertyPrivate.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("PropertyStatic.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("PropertyStaticProtected.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("PropertyStaticPrivate.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("Template.png")),
                new Bitmap(ASCompletion.PluginUI.GetStream("Declaration.png"))
            });
        
            tree.ImageList = treeIcons;
        }

        private void tree_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            Rectangle fillBounds = new Rectangle(e.Node.Bounds.Location, e.Node.Bounds.Size);
            fillBounds.X -= 1;
            fillBounds.Width += 10;

            Rectangle textBounds = new Rectangle(e.Node.Bounds.Location, e.Node.Bounds.Size);
            textBounds.X += 2;
            textBounds.Width += 10;
            
            if ((e.State & TreeNodeStates.Selected) != 0)
            {
                e.Graphics.FillRectangle(Brushes.SaddleBrown, fillBounds);
                e.Graphics.DrawString(e.Node.Text, tree.Font, Brushes.White, textBounds);
                using (Pen focusPen = new Pen(Color.Gray))
                {
                    focusPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    Rectangle focusBounds = fillBounds;
                    focusBounds.Size = new Size(focusBounds.Width - 1,
                    focusBounds.Height - 1);
                    e.Graphics.DrawRectangle(focusPen, focusBounds);
                }
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(tree.BackColor), fillBounds);
                e.Graphics.DrawString(e.Node.Text, tree.Font, Brushes.Black, textBounds);
            }
            
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
            if (model == FileModel.Ignore)
                return;

            if (model.Members.Count > 0)
                AddMembers(tree.Nodes, model.Members);
            
            foreach (ClassModel classModel in model.Classes)
            {
                int imageNum = ASCompletion.PluginUI.GetIcon(classModel.Flags, classModel.Access);
                TreeNode node = new TreeNode(classModel.Name, imageNum, imageNum);
                node.Tag = "class";
                tree.Nodes.Add(node);
                AddMembers(node.Nodes, classModel.Members);
                node.Expand();
            }
        }

        private void AddMembers(TreeNodeCollection nodes, MemberList members)
        {
            string searchedText = textBox.Text.ToLower().Trim();
            foreach (MemberModel member in members)
            {
                string memberText = member.ToString().ToLower();
                if (searchedText.Length > 0 && !memberText.StartsWith(searchedText))
                    continue;
                
                MemberTreeNode node = null;
                if ((member.Flags & (FlagType.Constant | FlagType.Variable | FlagType.Function | FlagType.Getter | FlagType.Setter)) > 0)
                {
                    int imageIndex = ASCompletion.PluginUI.GetIcon(member.Flags, member.Access);
                    node = new MemberTreeNode(member, imageIndex);
                    nodes.Add(node);
                }

                if (tree.SelectedNode == null)
                    tree.SelectedNode = node;
            }
        }

        private void QuickOutlineForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
            else if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                Navigate();
            }
        }

        private void QuickOutlineForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            (plugin.Settings as Settings).OutlineFormSize = Size;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            RefreshTree();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && tree.SelectedNode != null && tree.SelectedNode.NextVisibleNode != null)
            {
                tree.SelectedNode = tree.SelectedNode.NextVisibleNode;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up && tree.SelectedNode != null && tree.SelectedNode.PrevVisibleNode != null)
            {
                tree.SelectedNode = tree.SelectedNode.PrevVisibleNode;
                e.Handled = true;
            }
        }

        private void tree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Navigate();
        }

    }
}

class MemberTreeNode : TreeNode
{
    public MemberTreeNode(MemberModel member, int imageIndex)
        : base(member.ToString(), imageIndex, imageIndex)
    {
        Tag = member.Name + "@" + member.LineFrom;
    }
}