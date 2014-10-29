using ASCompletion;
using ASCompletion.Context;
using ASCompletion.Model;
using PluginCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace QuickNavigate.Controls
{
    public partial class ClassHierarchy : Form
    {
        private readonly Settings settings;
        private readonly Brush selectedNodeBrush;
        private readonly Brush defaultNodeBrush;
        private readonly Dictionary<string, List<ClassModel>> extendsToClasses;

        public ClassHierarchy(Settings settings)
        {
            this.settings = settings;
            Font = PluginBase.Settings.ConsoleFont;
            InitializeComponent();
            if (settings.HierarchyExplorer.Width > MinimumSize.Width) Size = settings.HierarchyExplorer;
            (PluginBase.MainForm as FlashDevelop.MainForm).ThemeControls(this);
            selectedNodeBrush = new SolidBrush(SystemColors.ControlDarkDark);
            defaultNodeBrush = new SolidBrush(tree.BackColor);
            extendsToClasses = GetAllProjectExtendsClasses();
            RefreshTree();
        }

        private Dictionary<string, List<ClassModel>> GetAllProjectExtendsClasses()
        {
            Dictionary<string, List<ClassModel>> result = new Dictionary<string, List<ClassModel>>();
            foreach (PathModel path in ASContext.Context.Classpath)
            {
                path.ForeachFile((aFile) =>
                {
                    foreach (ClassModel aClass in aFile.Classes)
                    {
                        string extendsType = aClass.ExtendsType;
                        if (!string.IsNullOrEmpty(extendsType))
                        {
                            if (!result.ContainsKey(extendsType)) result[extendsType] = new List<ClassModel>();
                            result[extendsType].Add(aClass);
                        }
                    }
                    return true;
                });
            }
            return result;
        }

        private void RefreshTree()
        {
            tree.BeginUpdate();
            tree.Nodes.Clear();
            FillTree();
            tree.EndUpdate();
            tree.ExpandAll();
        }

        private void FillTree()
        {
            ClassModel theClass = ASContext.Context.CurrentClass;
            if (theClass.IsVoid()) theClass = ASContext.Context.CurrentModel.GetPublicClass();
            if (theClass.IsVoid()) return;
            TreeNode parent = null;
            foreach (ClassModel aClass in GetExtends(theClass))
            {
                TreeNode child = new ClassNode(aClass);
                if (parent == null) tree.Nodes.Add(child);
                else parent.Nodes.Add(child);
                parent = child;
            }
            TreeNode node = new ClassNode(theClass);
            if (parent == null) tree.Nodes.Add(node);
            else parent.Nodes.Add(node);
            tree.SelectedNode = node;
            FillNode(node);
        }

        private List<ClassModel> GetExtends(ClassModel theClass)
        {
            List<ClassModel> result = new List<ClassModel>();
            ClassModel aClass = theClass.Extends;
            while (!aClass.IsVoid())
            {
                result.Add(aClass);
                aClass = aClass.Extends;
            }
            result.Reverse();
            return result;
        }

        private void FillNode(TreeNode node)
        {
            if (!extendsToClasses.ContainsKey(node.Name)) return;
            foreach (ClassModel aClass in extendsToClasses[node.Name])
            {
                ClassModel extends = aClass.InFile.Context.ResolveType(aClass.ExtendsType, aClass.InFile);
                if (extends.Type == node.Text)
                {
                    TreeNode child = new ClassNode(aClass);
                    node.Nodes.Add(child);
                    FillNode(child);
                }
            }
        }

        private void Navigate()
        {
            if (tree.SelectedNode == null) return;
            ClassModel theClass = ((ClassNode)tree.SelectedNode).Class;
            FileModel file = ModelsExplorer.Instance.OpenFile(theClass.InFile.FileName);
            if (file != null)
            {
                theClass = file.GetClassByName(theClass.Name);
                if (!theClass.IsVoid())
                {
                    int line = theClass.LineFrom;
                    ScintillaNet.ScintillaControl sci = PluginBase.MainForm.CurrentDocument.SciControl;
                    if (sci != null && line > 0 && line < sci.LineCount)
                        sci.GotoLine(line);
                }
            }
            Close();
        }

        #region Event Handlers

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    Close();
                    break;
                case Keys.Enter:
                    Navigate();
                    break;
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            settings.OutlineFormSize = Size;
        }

        private void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control || e.Shift || tree.SelectedNode == null) return;
            tree.BeginUpdate();
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
            tree.EndUpdate();
        }

        private void OnInputTextChanged(object sender, EventArgs e)
        {
            if (tree.Nodes.Count == 0) return;

        }

        private void OnTreeNodeMouseDoubleClick(object sender, System.Windows.Forms.TreeNodeMouseClickEventArgs e)
        {
            Navigate();
        }

        private void OnTreeDrawNode(object sender, System.Windows.Forms.DrawTreeNodeEventArgs e)
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

    class ClassNode : TreeNode
    {
        public readonly ClassModel Class;

        public ClassNode(ClassModel theClass) : base(theClass.Type)
        {
            Class = theClass;
            Name = theClass.Name;
        }
    }
}