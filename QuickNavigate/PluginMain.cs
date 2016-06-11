using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ASCompletion;
using ASCompletion.Completion;
using ASCompletion.Context;
using ASCompletion.Model;
using JetBrains.Annotations;
using PluginCore;
using PluginCore.Helpers;
using PluginCore.Managers;
using PluginCore.Utilities;
using ProjectManager;
using ProjectManager.Projects;
using QuickNavigate.Forms;
using QuickNavigate.Helpers;
using ScintillaNet;

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
        [CanBeNull] QuickForm openedForm;

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
                case EventType.Keys:
                    var value = ((KeyEvent) e).Value;
                    if (value == PluginBase.MainForm.GetShortcutItemKeys(ShortcutId.GotoPreviousMember))
                        GotoPreviousMember();
                    else if (value == PluginBase.MainForm.GetShortcutItemKeys(ShortcutId.GotoNextMember))
                        GotoNextMember();
                    else if (value == PluginBase.MainForm.GetShortcutItemKeys(ShortcutId.GotoPreviousTab))
                        GotoPreviousTab();
                    else if (value == PluginBase.MainForm.GetShortcutItemKeys(ShortcutId.GotoNextTab))
                        GotoNextTab();
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
            var dataPath = Path.Combine(PathHelper.DataDir, Name);
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
            settingFilename = Path.Combine(dataPath, $"{nameof(Settings)}.fdb");
        }

        /// <summary>
        /// Loads the plugin settings
        /// </summary>
        void LoadSettings()
        {
            Settings = new Settings();
            if (!File.Exists(settingFilename)) SaveSettings();
            else Settings = (Settings) ObjectSerializer.Deserialize(settingFilename, Settings);
        }

        /// <summary>
        /// Adds the required event handlers
        /// </summary>
        void AddEventHandlers()
        {
            EventManager.AddEventHandler(this, EventType.UIStarted | EventType.FileSwitch | EventType.Command | EventType.Keys);
            QuickContextMenuItem.SetDocumentClassMenuItem.Click += OnSetDocumentClassMenuClick;
            QuickContextMenuItem.GotoPositionOrLineMenuItem.Click += OnGotoPositionOrLineMenuClick;
            QuickContextMenuItem.ShowInQuickOutlineMenuItem.Click += OnShowInQuickOutlineMenuClick;
            QuickContextMenuItem.ShowInClassHierarchyMenuItem.Click += OnShowInClassHierarchyMenuClick;
            QuickContextMenuItem.ShowInProjectManagerMenuItem.Click += OnShowInProjectManagerMenuClick;
            QuickContextMenuItem.ShowInFileExplorerMenuItem.Click += OnShowInFileExplorerMenuClick;
        }

        /// <summary>
        /// Creates the required menu items
        /// </summary>
        void CreateMenuItems()
        {
            var menu = (ToolStripMenuItem)PluginBase.MainForm.FindMenuItem("SearchMenu");
            var image = PluginBase.MainForm.FindImage("99|16|0|0");
            typeExplorerItem = new ToolStripMenuItem("Type Explorer", image, ShowTypeExplorer);
            PluginBase.MainForm.RegisterShortcutItem(ShortcutId.TypeExplorer, typeExplorerItem);
            menu.DropDownItems.Add(typeExplorerItem);
            image = PluginBase.MainForm.FindImage("315|16|0|0");
            quickOutlineItem = new ToolStripMenuItem("Quick Outline", image, ShowQuickOutline);
            PluginBase.MainForm.RegisterShortcutItem(ShortcutId.QuickOutline, quickOutlineItem);
            menu.DropDownItems.Add(quickOutlineItem);
            image = PluginBase.MainForm.FindImage("99|16|0|0");
            classHierarchyItem = new ToolStripMenuItem("Class Hierarchy", image, ShowClassHierarchy);
            menu.DropDownItems.Add(classHierarchyItem);
            PluginBase.MainForm.RegisterShortcutItem(ShortcutId.ClassHierarchy, classHierarchyItem);
            editorClassHierarchyItem = new ToolStripMenuItem("Class Hierarchy", image, ShowClassHierarchy);
            PluginBase.MainForm.EditorMenu.Items.Insert(8, editorClassHierarchyItem);
            var item = new ToolStripMenuItem("Recent Files", PluginBase.MainForm.FindImage("209"), ShowRecentFiles);
            PluginBase.MainForm.RegisterShortcutItem(ShortcutId.RecentFiles, item);
            menu.DropDownItems.Add(item);
            item = new ToolStripMenuItem("Recent Projects", PluginBase.MainForm.FindImage("274"), ShowRecentProjets);
            PluginBase.MainForm.RegisterShortcutItem(ShortcutId.RecentProjects, item);
            menu.DropDownItems.Add(item);
            PluginBase.MainForm.RegisterShortcutItem(ShortcutId.GotoNextMember, Keys.None);
            PluginBase.MainForm.RegisterShortcutItem(ShortcutId.GotoPreviousMember, Keys.None);
            PluginBase.MainForm.RegisterShortcutItem(ShortcutId.GotoPreviousTab, Keys.None);
            PluginBase.MainForm.RegisterShortcutItem(ShortcutId.GotoNextTab, Keys.None);
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

        void ShowRecentFiles(object sender, EventArgs e) => ShowRecentFiles();

        void ShowRecentFiles()
        {
            var form = new OpenRecentFilesForm((Settings) Settings);
            form.KeyUp += OnFormKeyUp;
            if (form.ShowDialog() != DialogResult.OK) return;
            var plugin = (ProjectManager.PluginMain) PluginBase.MainForm.FindPlugin(FormHelper.ProjectManagerGUID);
            form.SelectedItems.ForEach(plugin.OpenFile);
        }

        void ShowRecentProjets(object sender, EventArgs e) => ShowRecentProjets();

        void ShowRecentProjets()
        {
            var form = new OpenRecentProjectsForm((Settings) Settings);
            form.KeyUp += OnFormKeyUp;
            if (form.ShowDialog() != DialogResult.OK) return;
            var plugin = (ProjectManager.PluginMain) PluginBase.MainForm.FindPlugin(FormHelper.ProjectManagerGUID);
            if (form.InNewWindow) ProcessHelper.StartAsync(Application.ExecutablePath, form.SelectedItem);
            else plugin.OpenFile(form.SelectedItem);
        }

        void ShowTypeExplorer(object sender, EventArgs e) => ShowTypeExplorer();

        void ShowTypeExplorer()
        {
            if (PluginBase.CurrentProject == null) return;
            var form = new TypeExplorerForm((Settings) Settings);
            var features = ASContext.GetLanguageContext(PluginBase.CurrentProject.Language).Features;
            if (features.hasClasses) form.AddFilter(QuickFilterMenuItem.ShowOnlyClasses);
            if (features.hasInterfaces) form.AddFilter(QuickFilterMenuItem.ShowOnlyInterfaces);
            if (features.hasTypeDefs) form.AddFilter(QuickFilterMenuItem.ShowOnlyTypeDefs);
            if (features.hasEnums) form.AddFilter(QuickFilterMenuItem.ShowOnlyEnums);
            form.KeyUp += OnFormKeyUp;
            form.Shown += OnFormShown;
            form.Closing += OnFormClosing;
            if (form.ShowDialog() != DialogResult.OK) return;
            var node = form.SelectedNode as TypeNode;
            if (node == null) return;
            FormHelper.Navigate(node.Model.InFile.FileName, node);
        }

        void ShowQuickOutline(object sender, EventArgs e) => ShowQuickOutline();

        void ShowQuickOutline() => ShowQuickOutline(ASContext.Context.CurrentModel, ASContext.Context.CurrentClass);

        void ShowQuickOutline([NotNull] Form sender, [NotNull] ClassModel inClass)
        {
            sender.Close();
            ((Control) PluginBase.MainForm).BeginInvoke((MethodInvoker) (() => ShowQuickOutline(inClass.InFile, inClass)));
        }

        void ShowQuickOutline([NotNull] FileModel inFile, [NotNull] ClassModel inClass)
        {
            var form = new QuickOutlineForm(inFile, inClass, (Settings) Settings);
            form.AddFilter(QuickFilterMenuItem.ShowOnlyClasses);
            form.AddFilter(QuickFilterMenuItem.ShowOnlyFields);
            form.AddFilter(QuickFilterMenuItem.ShowOnlyProperties);
            form.AddFilter(QuickFilterMenuItem.ShowOnlyMethods);
            form.KeyUp += OnFormKeyUp;
            form.Shown += OnFormShown;
            form.Closing += OnFormClosing;
            if (form.ShowDialog() != DialogResult.OK) return;
            FormHelper.Navigate(inFile.FileName, form.SelectedNode);
        }

        void ShowClassHierarchy(object sender, EventArgs e) => ShowClassHierarchy();

        void ShowClassHierarchy()
        {
            if (!GetCanShowClassHierarchy()) return;
            var context = ASContext.Context;
            var curClass = context.CurrentClass;
            ShowClassHierarchy(!curClass.IsVoid() ? curClass : context.CurrentModel.GetPublicClass());
        }

        void ShowClassHierarchy([NotNull] Form sender, [NotNull] ClassModel model)
        {
            sender.Close();
            ((Control) PluginBase.MainForm).BeginInvoke((MethodInvoker) (() => ShowClassHierarchy(model)));
        }

        void ShowClassHierarchy([NotNull] ClassModel model)
        {
            var form = new ClassHierarchyForm(model, (Settings) Settings);
            form.KeyUp += OnFormKeyUp;
            form.Shown += OnFormShown;
            form.Closing += OnFormClosing;
            if (form.ShowDialog() != DialogResult.OK) return;
            var node = form.SelectedNode as TypeNode;
            if (node == null) return;
            FormHelper.Navigate(node.Model.InFile.FileName, new TreeNode(node.Name) {Tag = node.Tag});
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

        static void GotoPositionOrLine([NotNull] Form sender, [NotNull] MemberModel model)
        {
            sender.Close();
            ((Control)PluginBase.MainForm).BeginInvoke((MethodInvoker)(() =>
            {
                ModelsExplorer.Instance.OpenFile(model.InFile.FileName);
                PluginBase.MainForm.CallCommand("GoTo", null);
            }));
        }

        static void ShowInProjectManager([NotNull] Form sender, [NotNull] MemberModel model)
        {
            sender.Close();
            ((Control) PluginBase.MainForm).BeginInvoke((MethodInvoker) (() =>
            {
                var ui = FormHelper.GetProjectManagerPluginUI();
                Debug.Assert(ui != null, "ProjectManager.PluginMain.pluginUI != null");
                ui.Parent.Show();
                ui.Tree.Select(model.InFile.FileName);
            }));
        }

        static void ShowInFileExplorer([NotNull] Form sender, [NotNull] MemberModel model)
        {
            sender.Close();
            ((Control) PluginBase.MainForm).BeginInvoke((MethodInvoker) (() =>
            {
                var ui = FormHelper.GetFileExplorerPluginUI();
                Debug.Assert(ui != null, "FileExplorer.PluginMain.pluginUI != null");
                ui.BrowseTo(Path.GetDirectoryName(model.InFile.FileName));
                ui.Parent.Show();
            }));
        }

        static void GotoPreviousMember()
        {
            var members = GetCurrentFileMembers();
            if (members.Count == 0) return;
            members.Reverse();
            var line = ASContext.Context.CurrentLine;
            var target = members.FirstOrDefault(member => member.LineFrom < line);
            if (target != null) FormHelper.Navigate(target);
        }

        static void GotoNextMember()
        {
            var members = GetCurrentFileMembers();
            if (members.Count == 0) return;
            var line = ASContext.Context.CurrentLine;
            var target = members.FirstOrDefault(member => member.LineFrom > line);
            if (target != null) FormHelper.Navigate(target);
        }

        [NotNull, ItemNotNull]
        static List<MemberModel> GetCurrentFileMembers()
        {
            var file = ASContext.Context.CurrentModel;
            var result = new List<MemberModel>(file.Members.Items);
            foreach (var it in file.Classes)
            {
                result.Add(it);
                result.AddRange(it.Members.Items);
            }
            return result;
        }

        static void GotoPreviousTab()
        {
            var documents = PluginBase.MainForm.Documents;
            if (documents.Length < 2) return;
            var index = Array.IndexOf(documents, PluginBase.MainForm.CurrentDocument) - 1;
            if (index == -1) index = documents.Length - 1;
            documents[index].Activate();
        }

        static void GotoNextTab()
        {
            var documents = PluginBase.MainForm.Documents;
            if (documents.Length < 2) return;
            var index = Array.IndexOf(documents, PluginBase.MainForm.CurrentDocument) + 1;
            if (index == documents.Length) index = 0;
            documents[index].Activate();
        }

        #endregion

        static void SetDocumentClass([NotNull] MemberModel model)
        {
            var project = (Project)PluginBase.CurrentProject;
            project.SetDocumentClass(model.InFile.FileName, true);
            project.Save();
            var ui = FormHelper.GetProjectManagerPluginUI();
            Debug.Assert(ui != null, "ProjectManager.PluginMain.pluginUI != null");
            ui.Tree.RefreshTree();
        }

        #region Event Handlers

        void OnSetDocumentClassMenuClick(object sender, EventArgs e)
        {
            var node = (TypeNode) openedForm.SelectedNode;
            Debug.Assert(node != null, "node != null");
            SetDocumentClass(node.Model);
        }

        void OnGotoPositionOrLineMenuClick(object sender, EventArgs e)
        {
            var node = (TypeNode) openedForm.SelectedNode;
            Debug.Assert(node != null, "node != null");
            GotoPositionOrLine(openedForm, node.Model);
        }

        void OnShowInQuickOutlineMenuClick(object sender, EventArgs e)
        {
            var node = (TypeNode) openedForm.SelectedNode;
            Debug.Assert(node != null, "node != null");
            ShowQuickOutline(openedForm, node.Model);
        }

        void OnShowInClassHierarchyMenuClick(object sender, EventArgs e)
        {
            var node = (TypeNode) openedForm.SelectedNode;
            Debug.Assert(node != null, "node != null");
            ShowClassHierarchy(openedForm, node.Model);
        }

        void OnShowInProjectManagerMenuClick(object sender, EventArgs e)
        {
            var node = (TypeNode) openedForm.SelectedNode;
            Debug.Assert(node != null, "node != null");
            ShowInProjectManager(openedForm, node.Model);
        }

        void OnShowInFileExplorerMenuClick(object sender, EventArgs e)
        {
            var node = (TypeNode) openedForm.SelectedNode;
            Debug.Assert(node != null, "node != null");
            ShowInFileExplorer(openedForm, node.Model);
        }

        /// <summary>
        /// Cursor position changed and word at this position was resolved
        /// </summary>
        void OnResolvedContextChanged(ResolvedContext resolved) => UpdateMenuItems();

        void OnFormKeyUp(object sender, KeyEventArgs e)
        {
            var shortcutId = PluginBase.MainForm.GetShortcutItemId(e.KeyData);
            if (string.IsNullOrEmpty(shortcutId)) return;
            MethodInvoker invoker = null;
            switch (shortcutId)
            {
                case ShortcutId.ClassHierarchy:
                    if (!(sender is ClassHierarchyForm)) invoker = ShowClassHierarchy;
                    break;
                case ShortcutId.QuickOutline:
                    if (!(sender is QuickOutlineForm)) invoker = ShowQuickOutline;
                    break;
                case ShortcutId.RecentProjects:
                    if (!(sender is OpenRecentProjectsForm)) invoker = ShowRecentProjets;
                    break;
                case ShortcutId.RecentFiles:
                    if (!(sender is OpenRecentFilesForm)) invoker = ShowRecentFiles;
                    break;
                case ShortcutId.TypeExplorer:
                    if (!(sender is TypeExplorerForm)) invoker = ShowTypeExplorer;
                    break;
                default: return;
            }
            if (invoker == null) return;
            ((Form) sender).Close();
            ((Control) PluginBase.MainForm).BeginInvoke(invoker);
        }

        void OnFormShown(object sender, EventArgs eventArgs)
        {
            openedForm = (QuickForm) sender;
            openedForm.Font = PluginBase.Settings.DefaultFont;
            openedForm.ContextMenuStrip = new ContextMenuStrip {Renderer = new DockPanelStripRenderer(false)};
        }

        void OnFormClosing(object sender, CancelEventArgs cancelEventArgs) => openedForm = null;

        #endregion
    }
}