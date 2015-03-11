using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ASCompletion.Completion;
using ASCompletion.Context;
using ASCompletion.Model;
using FlashDevelop;
using PluginCore;
using PluginCore.Helpers;
using PluginCore.Managers;
using PluginCore.Utilities;
using ProjectManager;
using QuickNavigate.Controls;
using WeifenLuo.WinFormsUI.Docking;

namespace QuickNavigate
{
    /// <summary>
    /// </summary>
    public class PluginMain : IPlugin
	{
        const string PLUGIN_NAME = "QuickNavigate";
        const string PLUGIN_GUID = "5e256956-8f0d-4f2b-9548-08673c0adefd";
        const string PLUGIN_HELP = "http://www.flashdevelop.org/community/";
        const string PLUGIN_AUTH = "Canab, SlavaRa";
	    const string SETTINGS_FILE = "Settings.fdb";
        const string PLUGIN_DESC = "QuickNavigate plugin";
        string settingFilename;
        Settings settings;
	    ControlClickManager controlClickManager;
	    ToolStripMenuItem typeExploreItem;
	    ToolStripMenuItem quickOutlineItem;
        ToolStripMenuItem classHierarchyItem;
        ToolStripMenuItem editorClassHierarchyItem;

	    #region Required Properties

        /// <summary>
        /// </summary>
        public int Api
        {
            get { return 1; }
        }
        
        /// <summary>
        /// Name of the plugin
        /// </summary>
        public string Name
		{
			get { return PLUGIN_NAME; }
		}

        /// <summary>
        /// GUID of the plugin
        /// </summary>
        public string Guid
		{
			get { return PLUGIN_GUID; }
		}

        /// <summary>
        /// Author of the plugin
        /// </summary> 
        public string Author
		{
			get { return PLUGIN_AUTH; }
		}

        /// <summary>
        /// Description of the plugin
        /// </summary>
        public string Description
		{
			get { return PLUGIN_DESC; }
		}

        /// <summary>
        /// Web address for help
        /// </summary>
        public string Help
		{
			get { return PLUGIN_HELP; }
		}

        /// <summary>
        /// Object that contains the settings
        /// </summary>
        [Browsable(false)]
        public object Settings
        {
            get { return settings; }
        }
		
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
            if (settings.CtrlClickEnabled) controlClickManager = new ControlClickManager();
        }
		
		/// <summary>
		/// Disposes the plugin
		/// </summary>
		public void Dispose()
		{
            if (controlClickManager != null)
            {
                controlClickManager.Dispose();
                controlClickManager = null;
            }
            if (classHierarchyItem != null)
            {
                classHierarchyItem.Dispose();
                classHierarchyItem = null;
            }
            if (editorClassHierarchyItem != null)
            {
                editorClassHierarchyItem.Dispose();
                editorClassHierarchyItem = null;
            }
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
                    if (controlClickManager != null) controlClickManager.SciControl = PluginBase.MainForm.CurrentDocument.SciControl;
                    break;
                case EventType.Command:
                    DataEvent da = (DataEvent)e;
                    switch (da.Action)
                    {
                        case ProjectManagerEvents.Project:
                            UpdateMenuItems();
                            break;
                    }
                    break;
            }
		}

        #endregion
        
        #region Custom Methods
       
        /// <summary>
        /// Initializes important variables
        /// </summary>
        public void InitBasics()
        {
            string dataPath = Path.Combine(PathHelper.DataDir, PLUGIN_NAME);
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
            settingFilename = Path.Combine(dataPath, SETTINGS_FILE);
        }

        /// <summary>
        /// Adds the required event handlers
        /// </summary>
        public void AddEventHandlers()
        {
            EventManager.AddEventHandler(this, EventType.UIStarted | EventType.FileSwitch | EventType.Command);
        }

        /// <summary>
        /// Creates the required menu items
        /// </summary>
        void CreateMenuItems()
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)PluginBase.MainForm.FindMenuItem("SearchMenu");
            Image image = PluginBase.MainForm.FindImage("99|16|0|0");
            typeExploreItem = new ToolStripMenuItem("Type Explorer", image, ShowTypeForm, Keys.Control | Keys.Shift | Keys.R);
            PluginBase.MainForm.RegisterShortcutItem("QuickNavigate.TypeExplorer", typeExploreItem);
            menu.DropDownItems.Add(typeExploreItem);
            image = PluginBase.MainForm.FindImage("315|16|0|0");
            quickOutlineItem = new ToolStripMenuItem("Quick Outline", image, ShowQuickOutline, Keys.Control | Keys.Shift | Keys.O);
            PluginBase.MainForm.RegisterShortcutItem("QuickNavigate.Outline", quickOutlineItem);
            menu.DropDownItems.Add(quickOutlineItem);
            classHierarchyItem = new ToolStripMenuItem("Class Hierarchy", null, ShowClassHierarchy);
            menu.DropDownItems.Add(classHierarchyItem);
            editorClassHierarchyItem = new ToolStripMenuItem("Class Hierarchy", null, ShowClassHierarchy);
            PluginBase.MainForm.EditorMenu.Items.Insert(8, editorClassHierarchyItem);
        }

        /// <summary>
        /// Updates the state of the menu items
        /// </summary>
        void UpdateMenuItems()
        {
            typeExploreItem.Enabled = PluginBase.CurrentProject != null;
            quickOutlineItem.Enabled = ASContext.Context.CurrentModel != null;
            bool canShowClassHierarchy = GetCanShowClassHierarchy();
            classHierarchyItem.Enabled = canShowClassHierarchy;
            editorClassHierarchyItem.Enabled = canShowClassHierarchy;
        }

        /// <summary>
        /// Loads the plugin settings
        /// </summary>
        public void LoadSettings()
        {
            settings = new Settings();
            if (!File.Exists(settingFilename)) SaveSettings();
            else settings = (Settings)ObjectSerializer.Deserialize(settingFilename, settings);
        }

        /// <summary>
        /// Saves the plugin settings
        /// </summary>
        public void SaveSettings()
        {
            ObjectSerializer.Serialize(settingFilename, settings);
        }
        
        void ShowTypeForm(object sender, EventArgs e)
        {
            if (PluginBase.CurrentProject == null) return;
            using (TypeExplorer form = new TypeExplorer(settings))
            {
                form.ShowInQuickOutline += ShowQuickOutline;
                form.ShowInClassHierarchy += ShowClassHierarchy;
                form.ShowInProjectManager += ShowInProjectManager;
                form.ShowInFileExplorer += ShowInFileExplorer;
                form.ShowDialog();
            }
        }

        void ShowQuickOutline(object sender, EventArgs e)
        {
            if (ASContext.Context.CurrentModel == null) return;
            using (Form form = new QuickOutlineForm(ASContext.Context.CurrentModel, settings))
            {
                form.ShowDialog();
            }
        }

        void ShowQuickOutline(Form sender, ClassModel model)
        {
            sender.Close();
            ((Control) PluginBase.MainForm).BeginInvoke((MethodInvoker) delegate
            {
                using (Form form = new QuickOutlineForm(model, settings))
                {
                    form.ShowDialog();
                }
            });
        }

        void ShowClassHierarchy(object sender, EventArgs e)
        {
            if (!GetCanShowClassHierarchy()) return;
            ClassModel curClass = ASContext.Context.CurrentClass;
            ShowClassHierarchy(!curClass.IsVoid() ? curClass : ASContext.Context.CurrentModel.GetPublicClass());
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
            using (ClassHierarchy form = new ClassHierarchy(model, settings))
            {
                form.ShowInQuickOutline += ShowQuickOutline;
                form.ShowInClassHierarchy += ShowClassHierarchy;
                form.ShowInProjectManager += ShowInProjectManager;
                form.ShowInFileExplorer += ShowInFileExplorer;
                form.ShowDialog();
            }
        }

        static bool GetCanShowClassHierarchy()
        {
            if (PluginBase.CurrentProject == null) return false;
            ITabbedDocument document = PluginBase.MainForm.CurrentDocument;
            if (document == null || !document.IsEditable) return false;
            IASContext context = ASContext.Context;
            return context != null && context.Features.hasExtends
                && (!context.CurrentClass.IsVoid() || !context.CurrentModel.GetPublicClass().IsVoid());
        }

        static void ShowInProjectManager(Form sender, ClassModel model)
        {
            sender.Close();
            ((Control) PluginBase.MainForm).BeginInvoke((MethodInvoker) delegate
            {
                foreach (DockPane pane in PluginBase.MainForm.DockPanel.Panes)
                {
                    foreach (DockContent content in pane.Contents)
                    {
                        if (content.GetPersistString() != "30018864-fadd-1122-b2a5-779832cbbf23") continue;
                        foreach (Control control in content.Controls)
                        {
                            ProjectManager.PluginUI ui = control as ProjectManager.PluginUI;
                            if (ui == null) continue;
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
                string path = model.InFile.FileName;
                foreach (DockPane pane in PluginBase.MainForm.DockPanel.Panes)
                {
                    foreach (DockContent content in pane.Contents)
                    {
                        if (content.GetPersistString() != "f534a520-bcc7-4fe4-a4b9-6931948b2686") continue;
                        foreach (Control control in content.Controls)
                        {
                            FileExplorer.PluginUI ui = control as FileExplorer.PluginUI;
                            if (ui == null) continue;
                            content.Show();
                            ui.BrowseTo(Path.GetDirectoryName(path));
                            foreach (Control feControl in ui.Controls)
                            {
                                ListView list = feControl as ListView;
                                if (list == null) continue;
                                ListViewItem item = list.FindItemWithText(Path.GetFileName(path));
                                if (item != null) item.Selected = true;
                                break;
                            }
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
        void OnResolvedContextChanged(ResolvedContext resolved)
        {
            UpdateMenuItems();
        }

        #endregion
    }
}