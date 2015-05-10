using System;
using System.Drawing;
using System.Windows.Forms;
using ASCompletion;
using ASCompletion.Model;
using PluginCore;
using ScintillaNet;

namespace QuickNavigate.Forms
{
    /// <summary>
    /// </summary>
    /// <param name="model"></param>
    public delegate void ShowInHandler(Form sender, ClassModel model);

    public class ClassModelExplorerForm : Form
    {
        public event ShowInHandler GotoPositionOrLine;
        public event ShowInHandler ShowInQuickOutline;
        public event ShowInHandler ShowInClassHierarchy;
        public event ShowInHandler ShowInProjectManager;
        public event ShowInHandler ShowInFileExplorer;
        readonly protected Settings Settings;
        readonly protected ContextMenu InputEmptyContextMenu = new ContextMenu();
        readonly protected Brush SelectedNodeBrush = new SolidBrush(SystemColors.ControlDarkDark);

        public ClassModelExplorerForm(Settings settings)
        {
            Settings = settings;
            Font = PluginBase.Settings.ConsoleFont;
            CreateContextMenu();
        }

        protected override void Dispose(bool disposing)
        {

            if (disposing)
            {
                SelectedNodeBrush.Dispose();
                InputEmptyContextMenu.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// </summary>
        protected void CreateContextMenu()
        {
            ContextMenuStrip = new ContextMenuStrip();
            ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Goto Position Or Line", PluginBase.MainForm.FindImage("67"),
                OnGotoLineOrPosition)
            {
                ShortcutKeyDisplayString = "G"
            });
            ContextMenuStrip.Items.Add(new ToolStripMenuItem("Show in Quick &Outline",
                PluginBase.MainForm.FindImage("315|16|0|0"), OnShowInQuickOutline)
            {
                ShortcutKeyDisplayString = "O"
            });
            ContextMenuStrip.Items.Add(new ToolStripMenuItem("Show in &Class Hierarchy",
                PluginBase.MainForm.FindImage("99|16|0|0"), OnShowInClassHiearachy)
            {
                ShortcutKeyDisplayString = "C"
            });
            ContextMenuStrip.Items.Add(new ToolStripMenuItem("Show in &Project Manager", PluginBase.MainForm.FindImage("274"),
                OnShowInProjectManager)
            {
                ShortcutKeyDisplayString = "P"
            });
            ContextMenuStrip.Items.Add(new ToolStripMenuItem("Show in &File Explorer", PluginBase.MainForm.FindImage("209"),
                OnShowInFileExplorer)
            {
                ShortcutKeyDisplayString = "F"
            });
        }

        protected virtual void InitTree()
        {
        }

        protected virtual void RefreshTree()
        {
        }

        protected virtual void FillTree()
        {
        }

        /// <summary>
        /// </summary>
        protected virtual void Navigate()
        {
        }

        /// <summary>
        /// </summary>
        protected void Navigate(TypeNode node)
        {
            if (node == null) return;
            ClassModel classModel = node.Model;
            FileModel file = ModelsExplorer.Instance.OpenFile(classModel.InFile.FileName);
            if (file != null)
            {
                classModel = file.GetClassByName(classModel.Name);
                if (!classModel.IsVoid())
                {
                    int line = classModel.LineFrom;
                    ScintillaControl sci = PluginBase.MainForm.CurrentDocument.SciControl;
                    if (sci != null && line > 0 && line < sci.LineCount)
                        sci.GotoLine(line);
                }
            }
            Close();
        }

        /// <summary>
        /// Displays the shortcut menu.
        /// </summary>
        protected virtual void ShowContextMenu()
        {
        }

        /// <summary>
        /// Displays the shortcut menu.
        /// </summary>
        protected virtual void ShowContextMenu(Point position)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnGotoLineOrPosition(object sender, EventArgs e)
        {
            TreeView tree = (TreeView) ContextMenuStrip.SourceControl;
            GotoPositionOrLine(this, ((TypeNode) tree.SelectedNode).Model);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnShowInQuickOutline(object sender, EventArgs e)
        {
            TreeView tree = (TreeView) ContextMenuStrip.SourceControl;
            ShowInQuickOutline(this, ((TypeNode) tree.SelectedNode).Model);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnShowInClassHiearachy(object sender, EventArgs e)
        {
            TreeView tree = (TreeView) ContextMenuStrip.SourceControl;
            ShowInClassHierarchy(this, ((TypeNode)tree.SelectedNode).Model);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnShowInProjectManager(object sender, EventArgs e)
        {
            TreeView tree = (TreeView) ContextMenuStrip.SourceControl;
            ShowInProjectManager(this, ((TypeNode) tree.SelectedNode).Model);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnShowInFileExplorer(object sender, EventArgs e)
        {
            TreeView tree = (TreeView) ContextMenuStrip.SourceControl;
            ShowInFileExplorer(this, ((TypeNode)tree.SelectedNode).Model);
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
                case Keys.Apps:
                    e.Handled = true;
                    ShowContextMenu();
                    break;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnTreeNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            TypeNode node = e.Node as TypeNode;
            if (node == null) return;
            ShowContextMenu(new Point(e.Location.X, node.Bounds.Y + node.Bounds.Height));
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnTreeNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Navigate();
        }

        #endregion
    }
}