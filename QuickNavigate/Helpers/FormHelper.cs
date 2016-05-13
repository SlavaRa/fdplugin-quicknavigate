using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ASCompletion;
using ASCompletion.Context;
using JetBrains.Annotations;
using PluginCore;
using PluginCore.Localization;
using ProjectManager.Controls;
using WeifenLuo.WinFormsUI.Docking;
using PluginUI = ProjectManager.PluginUI;

namespace QuickNavigate.Helpers
{
    class FormHelper
    {
        public static bool IsFileOpened([NotNull] string fileName)
        {
            return PluginBase.MainForm.Documents.Any(it => it.FileName == fileName);
        }

        [NotNull]
        public static List<string> FilterOpenedFiles([NotNull] ICollection<string> fileNames)
        {
            var result = (from doc in PluginBase.MainForm.Documents
                          let fileName = doc.FileName
                          where fileNames.Contains(fileName)
                          select fileName).ToList();
            return result;
        }

        public static void Navigate([NotNull] string fileName, [NotNull] TreeNode node)
        {
            ModelsExplorer.Instance.OpenFile(fileName);
            Navigate(node);
        }

        public static void Navigate([NotNull] TreeNode node) => ASContext.Context.OnSelectOutlineNode(node);

        public const string ProjectManagerGUID = "30018864-fadd-1122-b2a5-779832cbbf23";

        [CanBeNull]
        public static ProjectManager.PluginUI GetProjectManagerPluginUI()
        {
            foreach (var pane in PluginBase.MainForm.DockPanel.Panes)
            {
                foreach (var dockContent in pane.Contents)
                {
                    var content = (DockContent) dockContent;
                    if (content?.GetPersistString() != ProjectManagerGUID) continue;
                    foreach (var ui in content.Controls.OfType<PluginUI>())
                    {
                        return ui;
                    }
                }
            }
            return null;
        }

        [CanBeNull]
        public static FileExplorer.PluginUI GetFileExplorerPluginUI()
        {
            foreach (var pane in PluginBase.MainForm.DockPanel.Panes)
            {
                foreach (var dockContent in pane.Contents)
                {
                    var content = (DockContent) dockContent;
                    if (content?.GetPersistString() != "f534a520-bcc7-4fe4-a4b9-6931948b2686") continue;
                    foreach (var ui in content.Controls.OfType<FileExplorer.PluginUI>())
                    {
                        return ui;
                    }
                }
            }
            return null;
        }
    }

    public class ShortcutId
    {
        public const string TypeExplorer = "QuickNavigate.TypeExplorer";
        public const string QuickOutline = "QuickNavigate.Outline";
        public const string ClassHierarchy = "QuickNavigate.ClassHierarchy";
        public const string RecentFiles = "QuickNavigate.RecentFiles";
        public const string RecentProjects = "QuickNavigate.RecentProjects";
    }

    class QuickContextMenu
    {
        [NotNull] internal static ToolStripMenuItem GotoPositionOrLineMenuItem = new ToolStripMenuItem("&Goto Position Or Line", PluginBase.MainForm.FindImage("67")) { ShortcutKeyDisplayString = "G" };
        [NotNull] internal static ToolStripMenuItem ShowInQuickOutlineMenuItem = new ToolStripMenuItem("Show in Quick &Outline", PluginBase.MainForm.FindImage("315|16|0|0")) {ShortcutKeyDisplayString = "O"};
        [NotNull] internal static ToolStripMenuItem ShowInClassHierarchyMenuItem = new ToolStripMenuItem("Show in &Class Hierarchy", PluginBase.MainForm.FindImage("99|16|0|0")) { ShortcutKeyDisplayString = "C" };
        [NotNull] internal static ToolStripMenuItem ShowInProjectManagerMenuItem = new ToolStripMenuItem("Show in &Project Manager", PluginBase.MainForm.FindImage("274")) { ShortcutKeyDisplayString = "P" };
        [NotNull] internal static ToolStripMenuItem ShowInFileExplorerMenuItem = new ToolStripMenuItem("Show in &File Explorer", PluginBase.MainForm.FindImage("209")) { ShortcutKeyDisplayString = "F" };
        [NotNull] internal static ToolStripMenuItem SetDocumentClassMenuItem = new ToolStripMenuItem(TextHelper.GetString("ProjectManager.Label.SetDocumentClass"), Icons.DocumentClass.Img);
    }
}