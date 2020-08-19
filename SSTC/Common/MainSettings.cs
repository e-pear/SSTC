using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSTC.Common
{
    //Temporary solution, one day it will be done decently.
    public sealed class MainSettings
    {
        // INSTANCE
        private static MainSettings _instance;
        private static readonly object _padlock = new object();
        // CONSTRUCTOR
        private MainSettings()
        {
            if (!InitializeSettingsFromFile()) InitializeDefaultSettings();
        }
        // INSTANCE GETTER
        public static MainSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_padlock)
                    {
                        if (_instance == null)
                        {
                            _instance = new MainSettings();
                        }
                    }
                }
                return _instance;
            }
        }
        // AVAILABLE SETTINGS
        // -> Application Settings:
        public string BuildLabel { get; set; }
        public string SettingsPath { get; set; }
        public bool LoggingActions { get; set; }
        public int LogBufferSize { get; set; }
        public int StatusDisplayDuration { get; set; }
        public string SystemOverseerName { get; set; }
        public string ForbiddenCategoryName { get; set; }
        public string FixedCategoryName0 { get; set; }
        public string FixedCategoryName1 { get; set; }
        public string[] AttachmentSetsTypes { get; set; } 
        public double MainEPS { get; set; }
        public double GravitionalAcceleration { get; set; }
        //public string WorkBufferFileLocation { get; set; }
        // -> Language Settings:
        public string LanguagePath { get; set; }

        // BASE METHODS
        private bool InitializeSettingsFromFile()
        {
            // initializes almost every prop from ini file and language pack
            return false;
        }
        private void InitializeDefaultSettings()
        {
            // soft options -> 

            BuildLabel = "Build: 082020 v0.85B";
            ForbiddenCategoryName = "<Select All>"; //temp
            FixedCategoryName0 = "Dead-End Sets"; //temp
            FixedCategoryName1 = "Suspension Sets"; //temp
            //AttachmentSetsTypes = new string[3] { "Dead-End Set", "Suspension Set", "Inverted-V Set" }; //temp - order is super important
            AttachmentSetsTypes = new string[2] { "Dead-End Set", "Suspension Set" };
            SystemOverseerName = "SSTC Overseer";
            LoggingActions = true;
            LogBufferSize = 15;
            StatusDisplayDuration = 10000;
            MainEPS = 1e-09;
            GravitionalAcceleration = 9.80665;

            //WorkBufferFileLocation = Environment.CurrentDirectory + "\\workbuffer.tmp";
            // hard options -> change will require restart of an app (except it doesn't exist by now)
            LanguagePath = Directory.GetCurrentDirectory() + "\\Language\\en\\";
            SettingsPath = Directory.GetCurrentDirectory() + "\\settings.ini";
        }


    }
}
