using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSTC.Common
{
    public sealed class MainLanguageTable
    {
        private static MainLanguageTable _instance;
        private Dictionary<string, string> _languageDictionary;

        private MainLanguageTable()
        {
            _languageDictionary = CreateLanguageDictionary(); 
        }

        public static MainLanguageTable Instance
        {
            get
            {
                if (_instance == null) _instance = new MainLanguageTable();
                return _instance;
            }
        }
        
        // Main Window:
        public string Name_MainWindow_AppTitle { get { return GetLabel("Name_MainWindow_AppTitle"); } }
        // Main Menu:
        public string Name_MainMenu_New { get { return GetLabel("Name_MainMenu_New"); } }
        public string Name_MainMenu_Open { get { return GetLabel("Name_MainMenu_Open"); } }
        public string Name_MainMenu_Save { get { return GetLabel("Name_MainMenu_Save"); } }
        public string Name_Mainmenu_Options { get { return GetLabel("Name_Mainmenu_Options"); } }
        public string Name_Mainmenu_Exit { get { return GetLabel("Name_Mainmenu_Exit"); } }
        // Tab Header:
        public string Name_TabHeader_MoveMenu_Left { get { return GetLabel("Name_TabHeader_MoveMenu_Left"); } }
        public string Name_TabHeader_MoveMenu_Right { get { return GetLabel("Name_TabHeader_MoveMenu_Right"); } }
        public string Name_TabHeader_MoveMenu_ToLeftEnd { get { return GetLabel("Name_TabHeader_MoveMenu_ToLeftEnd"); } }
        public string Name_TabHeader_MoveMenu_ToRightEnd { get { return GetLabel("Name_TabHeader_MoveMenu_ToRightEnd"); } }
        // Tab Content Names:
        public string Name_GlobalCategory { get { return GetLabel("|Select All|").Insert(0, "<") + ">"; } }


        private Dictionary<string, string> CreateLanguageDictionary()
        {
            string path = MainSettings.Instance.LanguagePath;
            IEnumerable<string> importedRawData;
            Dictionary<string, string> languageDictionary = new Dictionary<string, string>();

            try
            {
                importedRawData = File.ReadAllLines(path);
            }
            catch (IOException e)
            {
                return null;
            }

            foreach(string record in importedRawData)
            {
                int splittingIndex = record.IndexOf(";");
                string key = record.Substring(0, splittingIndex);
                string value = record.Substring(splittingIndex + 1);
                languageDictionary.Add(key, value);
            }

            return languageDictionary;
        }
        private string GetLabel(string key)
        {
            string label;
            if (_languageDictionary.TryGetValue(key, out label)) return label;
            else return "LABEL_ERROR";
        }
    }
}
