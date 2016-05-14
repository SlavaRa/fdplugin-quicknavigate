using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using QuickNavigate.Helpers;

namespace QuickNavigate.Forms
{
    public class ClassModelExplorerForm : QuickForm
    {
        [NotNull] protected readonly Settings Settings;

        public ClassModelExplorerForm([NotNull] Settings settings)
        {
            Settings = settings;
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