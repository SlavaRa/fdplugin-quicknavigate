using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ASCompletion.Context;
using ASCompletion.Model;
using PluginCore;

namespace QuickNavigatePlugin
{
    public partial class QuickOutlineForm : Form
    {
        public const int ICON_FILE = 0;
        public const int ICON_FOLDER_CLOSED = 1;
        public const int ICON_FOLDER_OPEN = 2;
        public const int ICON_CHECK_SYNTAX = 3;
        public const int ICON_QUICK_BUILD = 4;
        public const int ICON_PACKAGE = 5;
        public const int ICON_INTERFACE = 6;
        public const int ICON_INTRINSIC_TYPE = 7;
        public const int ICON_TYPE = 8;
        public const int ICON_VAR = 9;
        public const int ICON_PROTECTED_VAR = 10;
        public const int ICON_PRIVATE_VAR = 11;
        public const int ICON_CONST = 12;
        public const int ICON_PROTECTED_CONST = 13;
        public const int ICON_PRIVATE_CONST = 14;
        public const int ICON_FUNCTION = 15;
        public const int ICON_PROTECTED_FUNCTION = 16;
        public const int ICON_PRIVATE_FUNCTION = 17;
        public const int ICON_PROPERTY = 18;
        public const int ICON_PROTECTED_PROPERTY = 19;
        public const int ICON_PRIVATE_PROPERTY = 20;
        public const int ICON_TEMPLATE = 21;
        public const int ICON_DECLARATION = 22;
        
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
            //IContainer components;
            ComponentResourceManager resources = new ComponentResourceManager(typeof(ASCompletion.PluginUI));
            ImageList treeIcons = new ImageList();
            treeIcons.ImageStream = ((ImageListStreamer)(resources.GetObject("treeIcons.ImageStream")));
            treeIcons.TransparentColor = Color.Transparent;
            treeIcons.Images.SetKeyName(0, "FilePlain.png");
            treeIcons.Images.SetKeyName(1, "FolderClosed.png");
            treeIcons.Images.SetKeyName(2, "FolderOpen.png");
            treeIcons.Images.SetKeyName(3, "CheckAS.png");
            treeIcons.Images.SetKeyName(4, "QuickBuild.png");
            treeIcons.Images.SetKeyName(5, "Package.png");
            treeIcons.Images.SetKeyName(6, "Interface.png");
            treeIcons.Images.SetKeyName(7, "Intrinsic.png");
            treeIcons.Images.SetKeyName(8, "Class.png");
            treeIcons.Images.SetKeyName(9, "Variable.png");
            treeIcons.Images.SetKeyName(10, "VariableProtected.png");
            treeIcons.Images.SetKeyName(11, "VariablePrivate.png");
            treeIcons.Images.SetKeyName(12, "Const.png");
            treeIcons.Images.SetKeyName(13, "ConstProtected.png");
            treeIcons.Images.SetKeyName(14, "ConstPrivate.png");
            treeIcons.Images.SetKeyName(15, "Method.png");
            treeIcons.Images.SetKeyName(16, "MethodProtected.png");
            treeIcons.Images.SetKeyName(17, "MethodPrivate.png");
            treeIcons.Images.SetKeyName(18, "Property.png");
            treeIcons.Images.SetKeyName(19, "PropertyProtected.png");
            treeIcons.Images.SetKeyName(20, "PropertyPrivate.png");
            treeIcons.Images.SetKeyName(21, "Template.png");
            treeIcons.Images.SetKeyName(22, "Declaration.png");

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

            // members
            if (model.Members.Count > 0)
            {
                AddMembers(tree.Nodes, model.Members);
            }
            // classes
            foreach (ClassModel classModel in model.Classes)
            {
                int imageNum = ((classModel.Flags & FlagType.Intrinsic) > 0) ? ICON_INTRINSIC_TYPE :
                    ((classModel.Flags & FlagType.Interface) > 0) ? ICON_INTERFACE : ICON_TYPE;
                TreeNode node = new TreeNode(classModel.Name, imageNum, imageNum);
                node.Tag = "class";
                tree.Nodes.Add(node);
                AddMembers(node.Nodes, classModel.Members);
                node.Expand();
            }
        }

        private void AddMembers(TreeNodeCollection nodes, MemberList members)
        {
            String searchedText = textBox.Text.ToLower().Trim();
            foreach (MemberModel member in members)
            {
                String memberText = member.ToString().ToLower();
                if (searchedText.Length > 0 && !memberText.StartsWith(searchedText))
                    continue;
                
                MemberTreeNode node = null;
                int imageIndex;
                if ((member.Flags & FlagType.Constant) > 0)
                {
                    imageIndex = ((member.Access & Visibility.Private) > 0) ? ICON_PRIVATE_CONST :
                        ((member.Access & Visibility.Protected) > 0) ? ICON_PROTECTED_CONST : ICON_CONST;
                    node = new MemberTreeNode(member, imageIndex);
                    nodes.Add(node);
                }
                else if ((member.Flags & FlagType.Variable) > 0)
                {
                    imageIndex = ((member.Access & Visibility.Private) > 0) ? ICON_PRIVATE_VAR :
                        ((member.Access & Visibility.Protected) > 0) ? ICON_PROTECTED_VAR : ICON_VAR;
                    node = new MemberTreeNode(member, imageIndex);
                    nodes.Add(node);
                }
                else if ((member.Flags & (FlagType.Getter | FlagType.Setter)) > 0)
                {
                    if (node != null && node.Text == member.ToString()) // "collapse" properties
                        continue;
                    imageIndex = ((member.Access & Visibility.Private) > 0) ? ICON_PRIVATE_PROPERTY :
                        ((member.Access & Visibility.Protected) > 0) ? ICON_PROTECTED_PROPERTY : ICON_PROPERTY;
                    node = new MemberTreeNode(member, imageIndex);
                    nodes.Add(node);
                }
                else if ((member.Flags & FlagType.Function) > 0)
                {
                    imageIndex = ((member.Access & Visibility.Private) > 0) ? ICON_PRIVATE_FUNCTION :
                        ((member.Access & Visibility.Protected) > 0) ? ICON_PROTECTED_FUNCTION : ICON_FUNCTION;
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
            {
                Close();
            }
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

