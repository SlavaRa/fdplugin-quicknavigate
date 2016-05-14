using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ASCompletion.Model;
using JetBrains.Annotations;
using QuickNavigate.Helpers;

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
        [NotNull] protected readonly Settings Settings;
        [NotNull] protected readonly ContextMenu InputEmptyContextMenu = new ContextMenu();

        public ClassModelExplorerForm([NotNull] Settings settings)
        {
            Settings = settings;
            InitializeContextMenu();
        }

        [CanBeNull] public virtual TypeNode SelectedNode => null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                InputEmptyContextMenu.Dispose();
            }
            base.Dispose(disposing);
        }

        protected virtual void InitializeContextMenu()
        {
            ContextMenuStrip = new ContextMenuStrip {Renderer = new DockPanelStripRenderer(false)};
            QuickContextMenu.GotoPositionOrLineMenuItem.Click += OnGotoLineOrPosition;
            QuickContextMenu.ShowInQuickOutlineMenuItem.Click += OnShowInQuickOutline;
            QuickContextMenu.ShowInClassHierarchyMenuItem.Click += OnShowInClassHierarchy;
            QuickContextMenu.ShowInProjectManagerMenuItem.Click += OnShowInProjectManager;
            QuickContextMenu.ShowInFileExplorerMenuItem.Click += OnShowInFileExplorer;
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
            GotoPositionOrLine(this, SelectedNode?.Model);
        }

        protected void OnShowInQuickOutline(object sender, EventArgs e)
        {
            Debug.Assert(ShowInQuickOutline != null, "ShowInQuickOutline != null");
            ShowInQuickOutline(this, SelectedNode?.Model);
        }

        protected void OnShowInClassHierarchy(object sender, EventArgs e)
        {
            Debug.Assert(ShowInClassHierarchy != null, "ShowInClassHierarchy != null");
            ShowInClassHierarchy(this, SelectedNode?.Model);
        }

        protected void OnShowInProjectManager(object sender, EventArgs e)
        {
            Debug.Assert(ShowInProjectManager != null, "ShowInProjectManager != null");
            ShowInProjectManager(this, SelectedNode?.Model);
        }

        protected void OnShowInFileExplorer(object sender, EventArgs e)
        {
            Debug.Assert(ShowInFileExplorer != null, "ShowInFileExplorer != null");
            ShowInFileExplorer(this, SelectedNode?.Model);
        }

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