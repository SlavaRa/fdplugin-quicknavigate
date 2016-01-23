using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ASCompletion;
using ASCompletion.Completion;
using ASCompletion.Context;
using ASCompletion.Model;
using PluginCore;
using PluginCore.Helpers;
using PluginCore.Managers;
using PluginCore.Utilities;
using ProjectManager;
using QuickNavigate.Forms;
using QuickNavigate.Helpers;
using WeifenLuo.WinFormsUI.Docking;
using PluginUI = ASCompletion.PluginUI;

namespace QuickNavigate
{
    public class PluginMain : IPlugin
	{
        string settingFilename;
	    ControlClickManager controlClickManager;
	    ToolStripMenuItem typeExplorerItem;
	    ToolStripMenuItem quickOutlineItem;
        ToolStripMenuItem classHierarchyItem;
        ToolStripMenuItem editorClassHierarchyItem;

        #region Required Properties

        /// <summary>
        /// Api level of the plugin
        /// </summary>
        public int Api => 1;

        /// <summary>
        /// Name of the plugin
        /// </summary>
        public string Name => nameof(QuickNavigate);

        /// <summary>
        /// GUID of the plugin
        /// </summary>
        public string Guid => "5e256956-8f0d-4f2b-9548-08673c0adefd";

        /// <summary>
        /// Author of the plugin
        /// </summary> 
        public string Author => "Canab, SlavaRa";

        /// <summary>
        /// Description of the plugin
        /// </summary>
        public string Description => "QuickNavigate plugin";

        /// <summary>
        /// Web address for help
        /// </summary>
        public string Help => "http://www.flashdevelop.org/community/";

        /// <summary>
        /// Object that contains the settings
        /// </summary>
        [Browsable(false)]
        public object Settings { get; private set; }
		
		#endregion
		
		#region Required Methods
		
		/// <summary>
		/// Initializes the plugin
		/// </summary>
		public void Initialize()
		{
            InitBasics();
            LoadSettings();
            AddEventHandlers();
            CreateMenuItems();
            UpdateMenuItems();
            if (((Settings) Settings).CtrlClickEnabled) controlClickManager = new ControlClickManager();
        }
		
		/// <summary>
		/// Disposes the plugin
		/// </summary>
		public void Dispose()
		{
		    controlClickManager?.Dispose();
		    classHierarchyItem?.Dispose();
		    editorClassHierarchyItem?.Dispose();
            SaveSettings();
		}
		
		/// <summary>
		/// Handles the incoming events
		/// </summary>
		public void HandleEvent(object sender, NotifyEvent e, HandlingPriority priority)
		{
            switch (e.Type)
            {
                case EventType.UIStarted:
                    ASComplete.OnResolvedContextChanged += OnResolvedContextChanged;
                    UpdateMenuItems();
                    break;
                case EventType.FileSwitch:
                    if (controlClickManager != null) controlClickManager.Sci = PluginBase.MainForm.CurrentDocument.SciControl;
                    UpdateMenuItems();
                    break;
                case EventType.Command:
                    if (((DataEvent)e).Action == ProjectManagerEvents.Project)
                    {
                        #region TODO slavara: ModelExplorer.current not updated after the change of the current project
                        ModelsExplorer.Instance.UpdateTree();
                        UpdateMenuItems();
                        #endregion
                    }
                    break;
            }
		}

        #endregion
        
        #region Custom Methods
       
        /// <summary>
        /// Initializes important variables
        /// </summary>
        void InitBasics()
        {
            string dataPath = Path.Combine(PathHelper.DataDir, Name);
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
            settingFilename = Path.Combine(dataPath, "Settings.fdb");
        }

        /// <summary>
        /// Loads the plugin settings
        /// </summary>
        void LoadSettings()
        {
            Settings = new Settings();
            if (!File.Exists(settingFilename)) SaveSettings();
            else Settings = (Settings)ObjectSerializer.Deserialize(settingFilename, Settings);
        }

        /// <summary>
        /// Adds the required event handlers
        /// </summary>
        void AddEventHandlers() => EventManager.AddEventHandler(this, EventType.UIStarted | EventType.FileSwitch | EventType.Command);

        /// <summary>
        /// Creates the required menu items
        /// </summary>
        void CreateMenuItems()
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)PluginBase.MainForm.FindMenuItem("SearchMenu");
            Image image = PluginBase.MainForm.FindImage("99|16|0|0");
            typeExplorerItem = new ToolStripMenuItem("Type Explorer", image, ShowTypeExplorer, Keys.Control | Keys.Shift | Keys.R);
            PluginBase.MainForm.RegisterShortcutItem($"{Name}.TypeExplorer", typeExplorerItem);
            menu.DropDownItems.Add(typeExplorerItem);
            image = PluginBase.MainForm.FindImage("315|16|0|0");
            quickOutlineItem = new ToolStripMenuItem("Quick Outline", image, ShowQuickOutline, Keys.Control | Keys.Shift | Keys.O);
            PluginBase.MainForm.RegisterShortcutItem($"{Name}.Outline", quickOutlineItem);
            menu.DropDownItems.Add(quickOutlineItem);
            image = PluginBase.MainForm.FindImage("99|16|0|0");
            classHierarchyItem = new ToolStripMenuItem("Class Hierarchy", image, ShowClassHierarchy);
            menu.DropDownItems.Add(classHierarchyItem);
            editorClassHierarchyItem = new ToolStripMenuItem("Class Hierarchy", image, ShowClassHierarchy);
            PluginBase.MainForm.EditorMenu.Items.Insert(8, editorClassHierarchyItem);
            ToolStripMenuItem item = new ToolStripMenuItem("Recent Files", null, ShowRecentFiles, Keys.Control | Keys.E);
            PluginBase.MainForm.RegisterShortcutItem($"{Name}.RecentFiles", item);
            menu.DropDownItems.Add(item);
            item = new ToolStripMenuItem("Recent Projects", null, ShowRecentProjets);
            PluginBase.MainForm.RegisterShortcutItem($"{Name}.RecentProjects", item);
            menu.DropDownItems.Add(item);
        }

        /// <summary>
        /// Updates the state of the menu items
        /// </summary>
        void UpdateMenuItems()
        {
            typeExplorerItem.Enabled = PluginBase.CurrentProject != null;
            quickOutlineItem.Enabled = ASContext.Context.CurrentModel != null;
            var enabled = GetCanShowClassHierarchy();
            classHierarchyItem.Enabled = enabled;
            editorClassHierarchyItem.Enabled = enabled;
        }

        /// <summary>
        /// Saves the plugin settings
        /// </summary>
        void SaveSettings() => ObjectSerializer.Serialize(settingFilename, Settings);

        void ShowRecentFiles(object sender, EventArgs e)
        {
            var form = new OpenRecentFilesForm((Settings) Settings);
            if (form.ShowDialog() != DialogResult.OK) return;
            var plugin = (ProjectManager.PluginMain) PluginBase.MainForm.FindPlugin("30018864-fadd-1122-b2a5-779832cbbf23");
            form.SelectedItems.ForEach(plugin.OpenFile);
        }

        void ShowRecentProjets(object sender, EventArgs e)
        {
            var form = new OpenRecentProjectsForm((Settings) Settings);
            if (form.ShowDialog() != DialogResult.OK) return;
            var plugin = (ProjectManager.PluginMain) PluginBase.MainForm.FindPlugin("30018864-fadd-1122-b2a5-779832cbbf23");
            plugin.OpenFile(form.SelectedItem);
        }

        void ShowTypeExplorer(object sender, EventArgs e)
        {
            if (PluginBase.CurrentProject == null) return;
            var form = new TypeExplorerForm((Settings) Settings);
            string enabledTip;
            string disabledTip;
            var features = ASContext.GetLanguageContext(PluginBase.CurrentProject.Language).Features;
            if (features.hasClasses)
            {
                enabledTip = "Show only classes(Alt+C or left click)";
                disabledTip = "Show all(Alt+C or left click)";
                form.AddFilter(PluginUI.ICON_TYPE, FlagType.Class, Keys.C, enabledTip, disabledTip);
            }
            if (features.hasInterfaces)
            {
                enabledTip = "Show only interfaces(Alt+I or left click)";
                disabledTip = "Show all(Alt+I or left click)";
                form.AddFilter(PluginUI.ICON_INTERFACE, FlagType.Interface, Keys.I, enabledTip, disabledTip);
            }
            // Abstracts
            if (features.hasTypeDefs)
            {
                enabledTip = "Show only typedefs(Alt+T or left click)";
                disabledTip = "Show all(Alt+T or left click)";
                form.AddFilter(PluginUI.ICON_TEMPLATE, FlagType.TypeDef, Keys.T, enabledTip, disabledTip);
            }
            if (features.hasEnums)
            {
                enabledTip = "Show only enums(Alt+E or left click)";
                disabledTip = "Show all(Alt+E or left click)";
                form.AddFilter(PluginUI.ICON_TYPE, FlagType.Enum, Keys.E, enabledTip, disabledTip);
            }
            form.GotoPositionOrLine += OnGotoPositionOrLine;
            form.ShowInQuickOutline += ShowQuickOutline;
            form.ShowInClassHierarchy += ShowClassHierarchy;
            form.ShowInProjectManager += ShowInProjectManager;
            form.ShowInFileExplorer += ShowInFileExplorer;
            if (form.ShowDialog() != DialogResult.OK) return;
            var node = form.SelectedNode;
            if (node == null) return;
            FormHelper.Navigate(node.Model.InFile.FileName, node);
        }

        void ShowQuickOutline(object sender, EventArgs e)
        {
            var context = ASContext.Context;
            ShowOutlineForm(context.CurrentModel, context.CurrentClass);
        }

        void ShowQuickOutline(Form sender, ClassModel inClass)
        {
            sender.Close();
            ((Control) PluginBase.MainForm).BeginInvoke((MethodInvoker) delegate
            {
                ShowOutlineForm(inClass.InFile, inClass);
            });
        }

        void ShowOutlineForm(FileModel inFile, ClassModel inClass)
        {
            var form = new QuickOutlineForm(inFile, inClass, (Settings) Settings);
            form.ShowInClassHierarchy += ShowClassHierarchy;
            var enabledTip = "Show only classes(Alt+C or left click)";
            var disabledTip = "Show all(Alt+C or left click)";
            form.AddFilter(PluginUI.ICON_TYPE, FlagType.Class, Keys.C, enabledTip, disabledTip);
            enabledTip = "Show only fields(Alt+F or left click)";
            disabledTip = "Show all(Alt+F or left click)";
            form.AddFilter(PluginUI.ICON_VAR, FlagType.Variable, Keys.F, enabledTip, disabledTip);
            enabledTip = "Show only properties(Alt+P or left click)";
            disabledTip = "Show all(Alt+P or left click)";
            form.AddFilter(PluginUI.ICON_PROPERTY, FlagType.Getter | FlagType.Setter, Keys.P, enabledTip, disabledTip);
            enabledTip = "Show only methods(Alt+M or left click)";
            disabledTip = "Show all(Alt+M or left click)";
            form.AddFilter(PluginUI.ICON_FUNCTION, FlagType.Function, Keys.M, enabledTip, disabledTip);
            if (form.ShowDialog() != DialogResult.OK) return;
            FormHelper.Navigate(inFile.FileName, form.SelectedNode);
        }

        void ShowClassHierarchy(object sender, EventArgs e)
        {
            if (!GetCanShowClassHierarchy()) return;
            var context = ASContext.Context;
            var curClass = context.CurrentClass;
            ShowClassHierarchy(!curClass.IsVoid() ? curClass : context.CurrentModel.GetPublicClass());
        }

        void ShowClassHierarchy(Form sender, ClassModel model)
        {
            sender.Close();
            ((Control) PluginBase.MainForm).BeginInvoke((MethodInvoker) delegate
            {
                ShowClassHierarchy(model);
            });
        }

        void ShowClassHierarchy(ClassModel model)
        {
            var form = new ClassHierarchyForm(model, (Settings) Settings);
            form.GotoPositionOrLine += OnGotoPositionOrLine;
            form.ShowInQuickOutline += ShowQuickOutline;
            form.ShowInClassHierarchy += ShowClassHierarchy;
            form.ShowInProjectManager += ShowInProjectManager;
            form.ShowInFileExplorer += ShowInFileExplorer;
            if (form.ShowDialog() != DialogResult.OK) return;
            var node = form.SelectedNode;
            if (node == null) return;
            FormHelper.Navigate(node.Model.InFile.FileName, new TreeNode(node.Name) { Tag = node.Tag });
        }

        static bool GetCanShowClassHierarchy()
        {
            if (PluginBase.CurrentProject == null) return false;
            var document = PluginBase.MainForm.CurrentDocument;
            if (document == null || !document.IsEditable) return false;
            var context = ASContext.Context;
            return context != null && context.Features.hasExtends
                && (!context.CurrentClass.IsVoid() || !context.CurrentModel.GetPublicClass().IsVoid());
        }

        static void OnGotoPositionOrLine(Form sender, ClassModel model)
        {
            sender.Close();
            ((Control)PluginBase.MainForm).BeginInvoke((MethodInvoker)delegate
            {
                ModelsExplorer.Instance.OpenFile(model.InFile.FileName);
                PluginBase.MainForm.CallCommand("GoTo", null);
            });
        }

        static void ShowInProjectManager(Form sender, ClassModel model)
        {
            sender.Close();
            ((Control) PluginBase.MainForm).BeginInvoke((MethodInvoker) delegate
            {
                foreach (var pane in PluginBase.MainForm.DockPanel.Panes)
                {
                    foreach (var dockContent in pane.Contents)
                    {
                        var content = (DockContent) dockContent;
                        if (content.GetPersistString() != "30018864-fadd-1122-b2a5-779832cbbf23") continue;
                        foreach (var ui in content.Controls.OfType<ProjectManager.PluginUI>())
                        {
                            content.Show();
                            ui.Tree.Select(model.InFile.FileName);
                            return;
                        }
                    }
                }
            });
        }

        static void ShowInFileExplorer(Form sender, ClassModel model)
        {
            sender.Close();
            ((Control) PluginBase.MainForm).BeginInvoke((MethodInvoker) delegate
            {
                foreach (var pane in PluginBase.MainForm.DockPanel.Panes)
                {
                    foreach (var dockContent in pane.Contents)
                    {
                        var content = (DockContent) dockContent;
                        if (content.GetPersistString() != "f534a520-bcc7-4fe4-a4b9-6931948b2686") continue;
                        foreach (var ui in content.Controls.OfType<FileExplorer.PluginUI>())
                        {
                            ui.BrowseTo(Path.GetDirectoryName(model.InFile.FileName));
                            content.Show();
                            return;
                        }
                    }
                }
            });
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Cursor position changed and word at this position was resolved
        /// </summary>
        void OnResolvedContextChanged(ResolvedContext resolved) => UpdateMenuItems();

        #endregion
	}
}