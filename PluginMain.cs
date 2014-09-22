using PluginCore;
using PluginCore.Helpers;
using PluginCore.Managers;
using PluginCore.Utilities;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace QuickNavigatePlugin
{
	public class PluginMain : IPlugin
	{
        private const int PLUGIN_API = 1;
        private const string PLUGIN_NAME = "QuickNavigatePlugin";
        private const string PLUGIN_GUID = "5e256956-8f0d-4f2b-9548-08673c0adefd";
        private const string PLUGIN_HELP = "www.flashdevelop.org/community/";
        private const string PLUGIN_AUTH = "Canab";
	    private const string SETTINGS_FILE = "Settings.fdb";
        private const string PLUGIN_DESC = "QuickNavigate plugin";
        private string settingFilename;
        private Settings settings;
	    private ControlClickManager controlClickManager;

	    #region Required Properties

        public int Api
        {
            get { return PLUGIN_API; }
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
            if (settings.CtrlClickEnabled) controlClickManager = new ControlClickManager();
        }
		
		/// <summary>
		/// Disposes the plugin
		/// </summary>
		public void Dispose()
		{
            SaveSettings();
		}
		
		/// <summary>
		/// Handles the incoming events
		/// </summary>
		public void HandleEvent(object sender, NotifyEvent e, HandlingPriority prority)
		{
            switch (e.Type)
            {
                case EventType.FileSwitch:
                    if (controlClickManager != null) controlClickManager.SciControl = PluginBase.MainForm.CurrentDocument.SciControl;
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
            EventManager.AddEventHandler(this, EventType.FileSwitch);
        }

        /// <summary>
        /// Creates the required menu items
        /// </summary>
        public void CreateMenuItems()
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)PluginBase.MainForm.FindMenuItem("SearchMenu");
            ToolStripMenuItem menuItem;
            System.Drawing.Image image;

            image = PluginBase.MainForm.FindImage("209");
            menuItem = new ToolStripMenuItem("Open Resource", image, ShowResourceForm, Keys.Control | Keys.R);
            PluginBase.MainForm.RegisterShortcutItem("QuickNavigate.OpenResource", menuItem);
            menu.DropDownItems.Add(menuItem);

            image = PluginBase.MainForm.FindImage("99|16|0|0");
            menuItem = new ToolStripMenuItem("Open Type", image, ShowTypeForm, Keys.Control | Keys.Shift | Keys.R);
            PluginBase.MainForm.RegisterShortcutItem("QuickNavigate.OpenType", menuItem);
            menu.DropDownItems.Add(menuItem);

            image = PluginBase.MainForm.FindImage("315|16|0|0");
            menuItem = new ToolStripMenuItem("Quick Outline", image, ShowOutlineForm, Keys.Control | Keys.Shift | Keys.O);
            PluginBase.MainForm.RegisterShortcutItem("QuickNavigate.Outline", menuItem);
            menu.DropDownItems.Add(menuItem);
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

        /// <summary>
        /// 
        /// </summary>
        private void ShowResourceForm(object sender, EventArgs e)
	    {
            if (PluginBase.CurrentProject != null) new OpenResourceForm(settings).ShowDialog();
	    }

        /// <summary>
        /// 
        /// </summary>
        private void ShowTypeForm(object sender, EventArgs e)
        {
            if (PluginBase.CurrentProject != null) new OpenTypeForm(settings).ShowDialog();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ShowOutlineForm(object sender, EventArgs e)
        {
            if (PluginBase.CurrentProject != null) new QuickOutlineForm(settings).ShowDialog();
        }

		#endregion

	}
}