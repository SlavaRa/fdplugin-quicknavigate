using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ASCompletion.Model;
using PluginCore;

namespace QuickNavigate.Forms
{
    public delegate void ShowInHandler(Form sender, ClassModel model);

    public class ClassModelExplorerForm : Form
    {
        public event ShowInHandler GotoPositionOrLine;
        public event ShowInHandler ShowInQuickOutline;
        public event ShowInHandler ShowInClassHierarchy;
        public event ShowInHandler ShowInProjectManager;
        public event ShowInHandler ShowInFileExplorer;
        protected readonly Settings Settings;
        protected readonly ContextMenu InputEmptyContextMenu = new ContextMenu();

        public ClassModelExplorerForm(Settings settings)
        {
            Settings = settings;
            InitializeContextMenu();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                InputEmptyContextMenu.Dispose();
            }
            base.Dispose(disposing);
        }

        protected void InitializeContextMenu()
        {
            ContextMenuStrip = new ContextMenuStrip();
            ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Goto Position Or Line", PluginBase.MainForm.FindImage("67"), OnGotoLineOrPosition)
            {
                ShortcutKeyDisplayString = "G"
            });
            ContextMenuStrip.Items.Add(new ToolStripMenuItem("Show in Quick &Outline", PluginBase.MainForm.FindImage("315|16|0|0"), OnShowInQuickOutline)
            {
                ShortcutKeyDisplayString = "O"
            });
            ContextMenuStrip.Items.Add(new ToolStripMenuItem("Show in &Class Hierarchy", PluginBase.MainForm.FindImage("99|16|0|0"), OnShowInClassHierarchy)
            {
                ShortcutKeyDisplayString = "C"
            });
            ContextMenuStrip.Items.Add(new ToolStripMenuItem("Show in &Project Manager", PluginBase.MainForm.FindImage("274"), OnShowInProjectManager)
            {
                ShortcutKeyDisplayString = "P"
            });
            ContextMenuStrip.Items.Add(new ToolStripMenuItem("Show in &File Explorer", PluginBase.MainForm.FindImage("209"), OnShowInFileExplorer)
            {
                ShortcutKeyDisplayString = "F"
            });
        }
        
        protected virtual void Navigate()
        {
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

        protected void OnGotoLineOrPosition(object sender, EventArgs e)
        {
            Debug.Assert(GotoPositionOrLine != null, "GotoPositionOrLine != null");
            GotoPositionOrLine(this, GetModelFromSelectedNode());
        }

        protected void OnShowInQuickOutline(object sender, EventArgs e)
        {
            Debug.Assert(ShowInQuickOutline != null, "ShowInQuickOutline != null");
            ShowInQuickOutline(this, GetModelFromSelectedNode());
        }

        protected void OnShowInClassHierarchy(object sender, EventArgs e)
        {
            Debug.Assert(ShowInClassHierarchy != null, "ShowInClassHierarchy != null");
            ShowInClassHierarchy(this, GetModelFromSelectedNode());
        }

        protected void OnShowInProjectManager(object sender, EventArgs e)
        {
            Debug.Assert(ShowInProjectManager != null, "ShowInProjectManager != null");
            ShowInProjectManager(this, GetModelFromSelectedNode());
        }

        protected void OnShowInFileExplorer(object sender, EventArgs e)
        {
            Debug.Assert(ShowInFileExplorer != null, "ShowInFileExplorer != null");
            ShowInFileExplorer(this, GetModelFromSelectedNode());
        }

        TreeView GetTreeView()
        {
            var tree = ContextMenuStrip.SourceControl as TreeView;
            return tree ?? ContextMenuStrip.SourceControl.Controls.OfType<TreeView>().FirstOrDefault();
        }

        ClassModel GetModelFromSelectedNode() => ((TypeNode) GetTreeView().SelectedNode).Model;

        #region Event Handlers
        
        protected virtual void OnTreeNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            var node = e.Node as TypeNode;
            if (node == null) return;
            ShowContextMenu(new Point(e.Location.X, node.Bounds.Bottom));
        }
        
        #endregion
    }
}