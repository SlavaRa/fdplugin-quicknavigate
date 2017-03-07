using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ASCompletion;
using ASCompletion.Context;
using ASCompletion.Model;
using JetBrains.Annotations;
using PluginCore;
using PluginCore.Localization;
using ProjectManager.Controls;
using QuickNavigate.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace QuickNavigate.Helpers
{
    public class QuickForm : Form
    {
        protected QuickForm() { }

        protected QuickForm([NotNull] Settings settings)
        {
            Settings = settings;
        }

        [CanBeNull] protected Settings Settings;

        /// <summary>
        /// The currently selected tree node, or null if nothing is selected.
        /// </summary>
        [CanBeNull] public virtual TreeNode SelectedNode { get; }

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

        protected virtual void OnTreeNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            var node = e.Node as ClassNode;
            if (node == null) return;
            ShowContextMenu(new Point(e.Location.X, node.Bounds.Bottom));
        }
    }

    class FormHelper
    {
        [NotNull] internal static readonly ContextMenu EmptyContextMenu = new ContextMenu();

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

        public static void Navigate([NotNull] MemberModel member)
        {
            string tag;
            if (member is ClassModel) tag = "class";
            else if ((member.Flags & FlagType.Import) > 0) tag = "import";
            else tag = $"{member.Name}@{member.LineFrom}";
            Navigate(new TreeNode(member.Name) {Tag = tag});
        }

        public static void Navigate([NotNull] TreeNode node)
        {
            if (node is ClassNode) ModelsExplorer.Instance.OpenFile(((ClassNode) node).InFile.FileName);
            else if (node is MemberNode) ModelsExplorer.Instance.OpenFile(((MemberNode) node).InFile.FileName);
            ASContext.Context.OnSelectOutlineNode(node);
        }

        public const string ProjectManagerGUID = "30018864-fadd-1122-b2a5-779832cbbf23";

        [CanBeNull]
        public static ProjectManager.PluginUI GetProjectManagerPluginUI()
        {
            return GetPluginUI<ProjectManager.PluginUI>(ProjectManagerGUID);
        }

        [CanBeNull]
        public static FileExplorer.PluginUI GetFileExplorerPluginUI()
        {
            return GetPluginUI<FileExplorer.PluginUI>("f534a520-bcc7-4fe4-a4b9-6931948b2686");
        }

        [CanBeNull]
        static T GetPluginUI<T>(string pluginGUID)
        {
            foreach (var pane in PluginBase.MainForm.DockPanel.Panes)
            {
                foreach (var dockContent in pane.Contents)
                {
                    var content = (DockContent) dockContent;
                    if (content?.GetPersistString() != pluginGUID) continue;
                    foreach (var ui in content.Controls.OfType<T>())
                    {
                        return ui;
                    }
                }
            }
            return default(T);
        }

        static readonly Dictionary<char, char> RuToEn = new Dictionary<char, char>
        {
            {'й', 'q'},
            {'ц', 'w'},
            {'у', 'e'},
            {'к', 'r'},
            {'е', 't'},
            {'н', 'y'},
            {'г', 'u'},
            {'ш', 'i'},
            {'щ', 'o'},
            {'з', 'p'},
            {'ф', 'a'},
            {'ы', 's'},
            {'в', 'd'},
            {'а', 'f'},
            {'п', 'g'},
            {'р', 'h'},
            {'о', 'j'},
            {'л', 'k'},
            {'д', 'l'},
            {'я', 'z'},
            {'ч', 'x'},
            {'с', 'c'},
            {'м', 'v'},
            {'и', 'b'},
            {'т', 'n'},
            {'ь', 'm'},
            {'ю', '.'}
        };

        [NotNull]
        public static string Transcriptor([NotNull] string s)
        {
            if (s.Trim().Length == 0) return s;
            var result = new string(s.ToCharArray().Select(c => RuToEn.ContainsKey(c) ? RuToEn[c] : c).ToArray());
            return result;
        }
    }

    class ShortcutId
    {
        public const string TypeExplorer = "QuickNavigate.TypeExplorer";
        public const string QuickOutline = "QuickNavigate.Outline";
        public const string ClassHierarchy = "QuickNavigate.ClassHierarchy";
        public const string RecentFiles = "QuickNavigate.RecentFiles";
        public const string RecentProjects = "QuickNavigate.RecentProjects";
        public const string GotoNextMember = "QuickNavigate.GotoNextMember";
        public const string GotoPreviousMember = "QuickNavigate.GotoPreviousMember";
        public const string GotoNextTab = "QuickNavigate.GotoNextTab";
        public const string GotoPreviousTab = "QuickNavigate.GotoPreviousTab";
    }

    class QuickContextMenuItem
    {
        [NotNull] internal static ToolStripMenuItem GotoPositionOrLineMenuItem =
            new ToolStripMenuItem("&Goto Position Or Line", PluginBase.MainForm.FindImage("67"))
            {
                ShortcutKeyDisplayString = "G"
            };

        [NotNull] internal static ToolStripMenuItem ShowInQuickOutlineMenuItem =
            new ToolStripMenuItem("Show in Quick &Outline", PluginBase.MainForm.FindImage("315|16|0|0"))
            {
                ShortcutKeyDisplayString = "O"
            };

        [NotNull] internal static ToolStripMenuItem ShowInClassHierarchyMenuItem =
            new ToolStripMenuItem("Show in &Class Hierarchy", PluginBase.MainForm.FindImage("99|16|0|0"))
            {
                ShortcutKeyDisplayString = "C"
            };

        [NotNull] internal static ToolStripMenuItem ShowInProjectManagerMenuItem =
            new ToolStripMenuItem("Show in &Project Manager", PluginBase.MainForm.FindImage("274"))
            {
                ShortcutKeyDisplayString = "P"
            };

        [NotNull] internal static ToolStripMenuItem ShowInFileExplorerMenuItem =
            new ToolStripMenuItem("Show in &File Explorer", PluginBase.MainForm.FindImage("209"))
            {
                ShortcutKeyDisplayString = "F"
            };

        [NotNull] internal static ToolStripMenuItem SetDocumentClassMenuItem =
            new ToolStripMenuItem(TextHelper.GetString("ProjectManager.Label.SetDocumentClass"), Icons.DocumentClass.Img);
    }

    class QuickFilter
    {
        public int ImageIndex;
        public FlagType Flag;
        public Keys Shortcut;
        public string EnabledTip;
        public string DisabledTip;
    }

    class QuickFilterMenuItem
    {
        [NotNull] internal static QuickFilter ShowOnlyClasses = new QuickFilter
        {
            ImageIndex = PluginUI.ICON_TYPE,
            Flag = FlagType.Class,
            Shortcut = Keys.C,
            EnabledTip = "Show only classes(Alt+C or left click)",
            DisabledTip = "Show all(Alt+C or left click)"
        };

        [NotNull] internal static QuickFilter ShowOnlyInterfaces = new QuickFilter
        {
            ImageIndex = PluginUI.ICON_INTERFACE,
            Flag = FlagType.Interface,
            Shortcut = Keys.I,
            EnabledTip = "Show only interfaces(Alt+I or left click)",
            DisabledTip = "Show all(Alt+I or left click)"
        };

        [NotNull] internal static QuickFilter ShowOnlyTypeDefs = new QuickFilter
        {
            ImageIndex = PluginUI.ICON_TEMPLATE,
            Flag = FlagType.TypeDef,
            Shortcut = Keys.T,
            EnabledTip = "Show only typedefs(Alt+T or left click)",
            DisabledTip = "Show all(Alt+T or left click)"
        };

        [NotNull] internal static QuickFilter ShowOnlyEnums = new QuickFilter
        {
            ImageIndex = PluginUI.ICON_TYPE,
            Flag = FlagType.Enum,
            Shortcut = Keys.E,
            EnabledTip = "Show only enums(Alt+E or left click)",
            DisabledTip = "Show all(Alt+E or left click)"
        };

        [NotNull] internal static QuickFilter ShowOnlyFields = new QuickFilter
        {
            ImageIndex = PluginUI.ICON_VAR,
            Flag = FlagType.Variable,
            Shortcut = Keys.F,
            EnabledTip = "Show only fields(Alt+F or left click)",
            DisabledTip = "Show all(Alt+F or left click)"
        };

        [NotNull] internal static QuickFilter ShowOnlyProperties = new QuickFilter
        {
            ImageIndex = PluginUI.ICON_PROPERTY,
            Flag = FlagType.Getter | FlagType.Setter,
            Shortcut = Keys.P,
            EnabledTip = "Show only properties(Alt+P or left click)",
            DisabledTip = "Show all(Alt+P or left click)"
        };

        [NotNull] internal static QuickFilter ShowOnlyMethods = new QuickFilter
        {
            ImageIndex = PluginUI.ICON_FUNCTION,
            Flag = FlagType.Function,
            Shortcut = Keys.M,
            EnabledTip = "Show only methods(Alt+M or left click)",
            DisabledTip = "Show all(Alt+M or left click)"
        };
    }
}