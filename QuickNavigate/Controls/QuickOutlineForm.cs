using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ASCompletion;
using ASCompletion.Context;
using ASCompletion.Model;
using PluginCore;

namespace QuickNavigate.Controls
{
    /// <summary>
    /// </summary>
    public partial class QuickOutlineForm : Form
    {
        readonly ClassModel inClass;
        readonly FileModel inFile;
        readonly Settings settings;
        readonly Brush selectedNodeBrush = new SolidBrush(SystemColors.ControlDarkDark);
        readonly Brush defaultNodeBrush;
        readonly IComparer<MemberModel> comparer = new SmartMemberComparer();
        readonly MemberList tmpMembers = new MemberList();

        /// <summary>
        /// Initializes a new instance of the QuickNavigate.Controls.QuickOutlineForm
        /// </summary>
        /// <param name="inClass"></param>
        /// <param name="settings"></param>
        public QuickOutlineForm(ClassModel inClass, Settings settings) : this(null, inClass, settings)
        {   
        }

        /// <summary>
        /// Initializes a new instance of the QuickNavigate.Controls.QuickOutlineForm
        /// </summary>
        /// <param name="inFile"></param>
        /// <param name="settings"></param>
        public QuickOutlineForm(FileModel inFile, Settings settings) : this(inFile, null, settings)
        {
        }

        /// <summary>
        /// Initializes a new instance of the QuickNavigate.Controls.QuickOutlineForm
        /// </summary>
        /// <param name="settings"></param>
        QuickOutlineForm(FileModel inFile, ClassModel inClass, Settings settings)
        {
            this.inFile = inFile;
            this.inClass = inClass;
            this.settings = settings;
            InitializeComponent();
            if (settings.OutlineFormSize.Width > MinimumSize.Width) Size = settings.OutlineFormSize;
            ((FlashDevelop.MainForm)PluginBase.MainForm).ThemeControls(this);
            defaultNodeBrush = new SolidBrush(tree.BackColor);
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

        /// <summary>
        /// </summary>
        void InitTree()
        {
            ImageList icons = new ImageList() {TransparentColor = Color.Transparent};
            icons.Images.AddRange(new Image[] {
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
            if (inFile != null)
            {
                if (inFile == FileModel.Ignore) return;
                isHaxe = inFile.haXe;
                if (inFile.Members.Count > 0) AddMembers(tree.Nodes, inFile.Members, isHaxe);
                classes = inFile.Classes;
            } 
            else if (inClass != null)
            {
                isHaxe = inClass.InFile.haXe;
                classes = new List<ClassModel> {inClass};
            }
            else return;
            foreach (ClassModel aClass in classes)
            {
                int icon = PluginUI.GetIcon(aClass.Flags, aClass.Access);
                TreeNode node = new TreeNode(aClass.Name, icon, icon) {Tag = "class"};
                tree.Nodes.Add(node);
                AddMembers(node.Nodes, aClass.Members, isHaxe);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="members"></param>
        /// <param name="isHaxe"></param>
        void AddMembers(TreeNodeCollection nodes, MemberList members, bool isHaxe)
        {
            bool noCase = !settings.OutlineFormMatchCase;
            string search = input.Text.Trim();
            bool searchIsNotEmpty = !string.IsNullOrEmpty(search);
            if (searchIsNotEmpty)
            {
                if (noCase) search = search.ToLower();
                tmpMembers.Clear();
                tmpMembers.Add(members);
                ((SmartMemberComparer)comparer).Setup(search, noCase);
                tmpMembers.Sort(comparer);
                members = tmpMembers;
            }
            bool wholeWord = settings.OutlineFormWholeWord;
            foreach (MemberModel member in members)
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
                nodes.Add(new TreeNode(member.ToString(), icon, icon) {
                    Tag = ((isHaxe && (flags & FlagType.Constructor) > 0) ? "new" : fullName) + "@" + member.LineFrom
                });
            }
            if (tree.SelectedNode == null && nodes.Count > 0) tree.SelectedNode = nodes[0];
        }

        /// <summary>
        /// </summary>
        void Navigate()
        {
            if (tree.SelectedNode == null) return;
            if (inFile == null) ModelsExplorer.Instance.OpenFile(inClass.InFile.FileName);
            ASContext.Context.OnSelectOutlineNode(tree.SelectedNode);
            Close();
        }

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"/> that contains the event data. </param>
        protected override void OnKeyDown(KeyEventArgs e)
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
                case Keys.L:
                    if (e.Control)
                    {
                        input.Focus();
                        input.SelectAll();
                    }
                    break;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyPress"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyPressEventArgs"/> that contains the event data. </param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            int keyCode = e.KeyChar;
            e.Handled = keyCode == (int) Keys.Space
                        || keyCode == 12; //Ctrl+L
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
        void OnInputTextChanged(object sender, EventArgs e)
        {
            RefreshTree();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control || e.Shift || tree.SelectedNode == null) return;
            TreeNode node;
            int visibleCount = tree.VisibleCount - 1;
            switch (e.KeyCode)
            {
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
        void OnTreeNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Navigate();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnTreeDrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            Brush fillBrush = defaultNodeBrush;
            Brush drawBrush = Brushes.Black;
            if ((e.State & TreeNodeStates.Selected) > 0)
            {
                fillBrush = selectedNodeBrush;
                drawBrush = Brushes.White;
            }
            Rectangle bounds = e.Bounds;
            e.Graphics.FillRectangle(fillBrush, bounds.X, bounds.Y, tree.Width - bounds.X, tree.ItemHeight);
            e.Graphics.DrawString(e.Node.Text, tree.Font, drawBrush, e.Bounds.Left, e.Bounds.Top, StringFormat.GenericDefault);
        }

        #endregion
    }

    /// <summary>
    /// </summary>
    class SmartMemberComparer : IComparer<MemberModel>
    {
        string search;
        bool noCase;

        public void Setup(string search, bool noCase)
        {
            if (noCase && !string.IsNullOrEmpty(search)) search = search.ToLower();
            this.search = search;
            this.noCase = noCase;
        }

        public int Compare(MemberModel a, MemberModel b)
        {
            int cmp = GetPriority(a.Name).CompareTo(GetPriority(b.Name));
            return cmp != 0 ? cmp : StringComparer.Ordinal.Compare(a.Name, b.Name);
        }

        int GetPriority(string name)
        {
            if (noCase) name = name.ToLower();
            if (name == search) return -100;
            if (name.StartsWith(search)) return -90;
            return 0;
        }
    }
}