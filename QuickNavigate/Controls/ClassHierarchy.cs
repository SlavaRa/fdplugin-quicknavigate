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
        private readonly Brush selectedNodeBrush = new SolidBrush(SystemColors.ControlDarkDark);
        private readonly Brush defaultNodeBrush;
        private readonly Dictionary<string, List<ClassModel>> extendsToClasses;
        private readonly Dictionary<string, TreeNode> typeToNode = new Dictionary<string, TreeNode>();

        public ClassHierarchy(Settings settings)
        {
            this.settings = settings;
            Font = PluginBase.Settings.ConsoleFont;
            InitializeComponent();
            if (settings.HierarchyExplorerSize.Width > MinimumSize.Width) Size = settings.HierarchyExplorerSize;
            (PluginBase.MainForm as FlashDevelop.MainForm).ThemeControls(this);
            defaultNodeBrush = new SolidBrush(tree.BackColor);
            extendsToClasses = GetAllProjectExtendsClasses();
            InitTree();
            RefreshTree();
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
                if (defaultNodeBrush != null) defaultNodeBrush.Dispose();
                if (components != null) components.Dispose();
            }
            base.Dispose(disposing);
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

        private void InitTree()
        {
            ImageList icons = new ImageList() {TransparentColor = Color.Transparent};
            icons.Images.AddRange(new Bitmap[] {
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
            tree.ImageList = icons;
        }

        private void RefreshTree()
        {
            tree.BeginUpdate();
            tree.Nodes.Clear();
            FillTree();
            tree.ExpandAll();
            tree.EndUpdate();
        }

        private void FillTree()
        {
            typeToNode.Clear();
            ClassModel theClass = GetCurrentClass();
            if (theClass.IsVoid()) return;
            TreeNode parent = null;
            int icon;
            foreach (ClassModel aClass in GetExtends(theClass))
            {
                icon = PluginUI.GetIcon(aClass.Flags, aClass.Access);
                TreeNode child = new ClassNode(aClass, icon, icon);
                if (parent == null) tree.Nodes.Add(child);
                else parent.Nodes.Add(child);
                typeToNode[aClass.Type] = child;
                parent = child;
            }
            icon = PluginUI.GetIcon(theClass.Flags, theClass.Access);
            TreeNode node = new ClassNode(theClass, icon, icon);
            node.NodeFont = new Font(tree.Font, FontStyle.Underline);
            if (parent == null) tree.Nodes.Add(node);
            else parent.Nodes.Add(node);
            tree.SelectedNode = node;
            typeToNode[theClass.Type] = node;
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
                    int icon = PluginUI.GetIcon(aClass.Flags, aClass.Access);
                    TreeNode child = new ClassNode(aClass, icon, icon);
                    node.Nodes.Add(child);
                    typeToNode[aClass.Type] = child;
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

        private ClassModel GetCurrentClass()
        {
            ClassModel curClass = ASContext.Context.CurrentClass;
            return !curClass.IsVoid() ? curClass : ASContext.Context.CurrentModel.GetPublicClass();
        }

        private TreeNode GetNextEnabledNode()
        {
            TreeNode node = tree.SelectedNode;
            while (node.NextVisibleNode != null)
            {
                node = node.NextVisibleNode;
                if ((node.Tag as string) == "enabled") return node;
            }
            return null;
        }

        private TreeNode GetPrevEnabledNode()
        {
            TreeNode node = tree.SelectedNode;
            while (node.PrevVisibleNode != null)
            {
                node = node.PrevVisibleNode;
                if ((node.Tag as string) == "enabled") return node;
            }
            return null;
        }

        private TreeNode GetUpEnabledNode()
        {
            TreeNode result = null;
            TreeNode node = tree.SelectedNode;
            while (node.PrevVisibleNode != null)
            {
                node = node.PrevVisibleNode;
                if ((node.Tag as string) == "enabled") result = node;
            }
            return result;
        }

        private TreeNode GetLastEnabledNode()
        {
            TreeNode result = null;
            TreeNode node = tree.SelectedNode;
            while (node.NextVisibleNode != null)
            {
                node = node.NextVisibleNode;
                if ((node.Tag as string) == "enabled") result = node;
            }
            return result;
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
                    e.Handled = true;
                    Navigate();
                    break;
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            settings.HierarchyExplorerSize = Size;
        }

        private void OnInputTextChanged(object sender, EventArgs e)
        {
            if (tree.Nodes.Count == 0) return;
            List<string> matches = SearchUtil.Matches(new List<string>(typeToNode.Keys), input.Text, ".", settings.MaxItems, settings.HierarchyExplorerWholeWord, settings.HierarchyExplorerMatchCase);
            bool mathesIsEmpty = matches.Count == 0;
            foreach (KeyValuePair<string, TreeNode> k in typeToNode)
            {
                if (mathesIsEmpty) k.Value.Tag = "enabled";
                else k.Value.Tag = matches.Contains(k.Key) ? "enabled" : "disabled";
            }
            tree.Refresh();
            if (mathesIsEmpty)
            {
                ClassModel theClass = GetCurrentClass();
                tree.SelectedNode = typeToNode[theClass.Type];
                return;
            }
            matches.Sort();
            tree.SelectedNode = typeToNode[matches[0]];
        }

        private void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control || e.Shift || tree.SelectedNode == null) return;
            TreeNode node;
            TreeNode enabledNode = null;
            int visibleCount = tree.VisibleCount - 1;
            switch (e.KeyCode)
            {
                case Keys.Down:
                    node = GetNextEnabledNode();
                    if (node != null) tree.SelectedNode = node;
                    else if (settings.WrapList)
                    {
                        node = GetUpEnabledNode();
                        if (node != null) tree.SelectedNode = node;
                    }
                    break;
                case Keys.Up:
                    node = GetPrevEnabledNode();
                    if (node != null) tree.SelectedNode = node;
                    else if (settings.WrapList)
                    {
                        node = GetLastEnabledNode();
                        if (node != null) tree.SelectedNode = node;
                    }
                    break;
                case Keys.Home:
                    node = GetUpEnabledNode();
                    if (node != null) tree.SelectedNode = node;
                    break;
                case Keys.End:
                    node = GetLastEnabledNode();
                    if (node != null) tree.SelectedNode = node;
                    break;
                case Keys.PageUp:
                    node = tree.SelectedNode;
                    for (int i = 0; i < visibleCount; i++)
                    {
                        if (node.PrevVisibleNode == null) break;
                        node = node.PrevVisibleNode;
                        if ((node.Tag as string) == "enabled") enabledNode = node;
                    }
                    if (enabledNode != null) tree.SelectedNode = enabledNode;
                    break;
                case Keys.PageDown:
                    node = tree.SelectedNode;
                    for (int i = 0; i < visibleCount; i++)
                    {
                        if (node.NextVisibleNode == null) break;
                        node = node.NextVisibleNode;
                        if ((node.Tag as string) == "enabled") enabledNode = node;
                    }
                    if (enabledNode != null) tree.SelectedNode = enabledNode;
                    break;
                default: return;
            }
            e.Handled = true;
        }

        private void OnInputKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == (int)Keys.Space) e.Handled = true;
        }

        private void OnTreeNodeMouseDoubleClick(object sender, System.Windows.Forms.TreeNodeMouseClickEventArgs e)
        {
            Navigate();
        }

        private void OnTreeDrawNode(object sender, System.Windows.Forms.DrawTreeNodeEventArgs e)
        {
            string tag = e.Node.Tag as string;
            Brush fillBrush = defaultNodeBrush;
            Brush drawBrush = Brushes.Black;
            if (string.IsNullOrEmpty(tag) || tag == "enabled")
            {
                if ((e.State & TreeNodeStates.Selected) > 0)
                {
                    fillBrush = selectedNodeBrush;
                    drawBrush = Brushes.White;
                }
            }
            else if (tag == "disabled") drawBrush = Brushes.DimGray;
            Rectangle bounds = e.Bounds;
            e.Graphics.FillRectangle(fillBrush, bounds.X, bounds.Y, tree.Width - bounds.X, tree.ItemHeight);
            e.Graphics.DrawString(e.Node.Text, e.Node.NodeFont ?? tree.Font, drawBrush, e.Bounds.Left, e.Bounds.Top, StringFormat.GenericDefault);
        }

        #endregion
    }

    class ClassNode : TreeNode
    {
        public readonly ClassModel Class;

        public ClassNode(ClassModel theClass, int imageIndex, int selectedImageIndex) : base(theClass.Type, imageIndex, selectedImageIndex)
        {
            Class = theClass;
            Name = theClass.Name;
            Tag = "enabled";
        }
    }
}