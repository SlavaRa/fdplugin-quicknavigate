using ASCompletion.Completion;
using PluginCore;
using PluginCore.Helpers;
using PluginCore.Managers;
using PluginCore.Utilities;
using QuickNavigate.Controls;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using ASCompletion.Context;
using ProjectManager;

namespace QuickNavigate
{
	public class PluginMain : IPlugin
	{
        private const string PLUGIN_NAME = "QuickNavigate";
        private const string PLUGIN_GUID = "5e256956-8f0d-4f2b-9548-08673c0adefd";
        private const string PLUGIN_HELP = "http://www.flashdevelop.org/community/";
        private const string PLUGIN_AUTH = "Canab, SlavaRa";
	    private const string SETTINGS_FILE = "Settings.fdb";
        private const string PLUGIN_DESC = "QuickNavigate plugin";
        private string settingFilename;
        private Settings settings;
	    private ControlClickManager controlClickManager;
	    private ToolStripMenuItem typeExploreItem;
	    private ToolStripMenuItem quickOutlineItem;
        private ToolStripMenuItem classHierarchyItem;
        private ToolStripMenuItem editorClassHierarchyItem;

	    #region Required Properties

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
        private void CreateMenuItems()
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)PluginBase.MainForm.FindMenuItem("SearchMenu");
            System.Drawing.Image image = PluginBase.MainForm.FindImage("99|16|0|0");
            typeExploreItem = new ToolStripMenuItem("Type Explorer", image, ShowTypeForm, Keys.Control | Keys.Shift | Keys.R);
            PluginBase.MainForm.RegisterShortcutItem("QuickNavigate.TypeExplorer", typeExploreItem);
            menu.DropDownItems.Add(typeExploreItem);
            image = PluginBase.MainForm.FindImage("315|16|0|0");
            quickOutlineItem = new ToolStripMenuItem("Quick Outline", image, ShowOutlineForm, Keys.Control | Keys.Shift | Keys.O);
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
        private void UpdateMenuItems()
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
        
        private void ShowTypeForm(object sender, EventArgs e)
        {
            if (PluginBase.CurrentProject != null) new TypeExplorer(settings).ShowDialog();
        }

        private void ShowOutlineForm(object sender, EventArgs e)
        {
            if (ASContext.Context.CurrentModel != null) new QuickOutlineForm(settings).ShowDialog();
        }

        private void ShowClassHierarchy(object sender, EventArgs e)
        {
            if (GetCanShowClassHierarchy()) new ClassHierarchy(settings).ShowDialog();
        }

        private static bool GetCanShowClassHierarchy()
        {
            if (PluginBase.CurrentProject == null) return false;
            ITabbedDocument document = PluginBase.MainForm.CurrentDocument;
            if (document == null || !document.IsEditable) return false;
            ASCompletion.Context.IASContext context = ASCompletion.Context.ASContext.Context;
            return context != null && context.Features.hasExtends
                && (!context.CurrentClass.IsVoid() || !context.CurrentModel.GetPublicClass().IsVoid());
        }

		#endregion

        #region Event Handlers

        /// <summary>
        /// Cursor position changed and word at this position was resolved
        /// </summary>
        private void OnResolvedContextChanged(ResolvedContext resolved)
        {
            UpdateMenuItems();
        }

        #endregion
    }
}