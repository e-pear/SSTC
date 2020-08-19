using SSTC_BaseModel;
using SSTC_ViewResources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.Win32;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace SSTC
{
    using System.Windows;
    using SSTC.Common;
    using SSTC.Modules.About;
    using SSTC.Modules.Calculator;
    using SSTC.Modules.DataManager;

    class MainWindowViewModel : INotifyPropertyChanged
    {
        // FIELDS
        // -> Data Managers:
        private DataManager<Conductor> _conductorManager;
        private DataManager<InsulatorSet> _insulatorSetManager;
        // -> Tabs:
        private readonly ObservableCollection<ITab> _tabs;
        private ITab _selectedTab;
        // PROPERTIES
        // -> Globals:
        public MainLanguageTable NameTable { get { return MainLanguageTable.Instance; } }
        // -> Tabs: 
        public ObservableCollection<ITab> Tabs { get { return _tabs; } }
        public ITab SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                _selectedTab = value;
                if (value != null) _selectedTab.TabRefresh();
                RaisePropertyChanged("SelectedTab");
            }
        }
        // COMMAND PROPERTIES

        // -> Main Menu:
        public ICommand NewSectionCommand { get; }
        public ICommand OpenProjectCommand { get; }
        //public ICommand SaveProjectCommand { get; }
        public ICommand SaveProjectAsCommand { get; }
        public ICommand OpenConductorDatabaseManagerCommand { get; }
        public ICommand OpenAttachmentSetDatabaseManagerCommand { get; }
        public ICommand OpenAboutTabCommand { get; }
        public ICommand ExitCommand { get; }
        // EVENTS

        public event PropertyChangedEventHandler PropertyChanged;


        // CONSTRUCTOR
        public MainWindowViewModel()
        {
            // DATA MANAGERS
            _conductorManager = ConductorDataManager.Instance;
            _insulatorSetManager = InsulatorSetDataManager.Instance;
            // TABS CONTROL
            _tabs = new ObservableCollection<ITab>();
            _tabs.CollectionChanged += Tabs_CollectionChanged;
            // COMMANDS
            // -> Main Menu:
            NewSectionCommand = new CommandRelay(NewSection, () => true);
            OpenProjectCommand = new CommandRelay(OpenProject, () => true);
            //SaveProjectCommand = new CommandRelay(SaveProject, () => false);
            SaveProjectAsCommand = new CommandRelay(SaveProjectAs, CanExecute_SaveAsCommand);
            OpenConductorDatabaseManagerCommand = new CommandRelay(OpenConductorDatabaseManager, () => true);
            OpenAttachmentSetDatabaseManagerCommand = new CommandRelay(OpenAttachmentSetDatabaseManager, () => true);
            OpenAboutTabCommand = new CommandRelay(OpenAboutTab, () => true);
            ExitCommand = new CommandRelay(Exit, () => true);
        }


        // EVENT RAISERS
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        // EVENT ACTIONS
        // -> mass subscriber:
        private void Tabs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ITab tab;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    tab = (ITab)e.NewItems[0];
                    tab.CloseRequested += OnTabCloseRequested;
                    tab.ToggleRenamingRequested += OnTabToggleRenamingRequested;
                    tab.MoveTabLeftRequested += OnTabMoveLeftRequested;
                    tab.MoveTabRightRequested += OnTabMoveRightRequested;
                    tab.MoveTabToLeftEndRequested += OnTabMoveToLeftEndRequested;
                    tab.MoveTabToRightEndRequested += OnTabMoveToRightEndRequested;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    tab = (ITab)e.OldItems[0];
                    tab.CloseRequested -= OnTabCloseRequested;
                    tab.ToggleRenamingRequested -= OnTabToggleRenamingRequested;
                    tab.MoveTabLeftRequested -= OnTabMoveLeftRequested;
                    tab.MoveTabRightRequested -= OnTabMoveRightRequested;
                    tab.MoveTabToLeftEndRequested -= OnTabMoveToLeftEndRequested;
                    tab.MoveTabToRightEndRequested -= OnTabMoveToRightEndRequested;
                    break;
            }
        }
        // -> tab control actions:
        private void OnTabCloseRequested(object sender, EventArgs e)
        {
            Tabs.Remove((ITab)sender);
        }
        private void OnTabToggleRenamingRequested(object sender, EventArgs e)
        {
            ITab tab = (ITab)sender;
            if (tab.IsTabTitleRenaming) tab.IsTabTitleRenaming = false;
            else tab.IsTabTitleRenaming = true;
        }
        private void OnTabMoveLeftRequested(object sender, EventArgs e)
        {
            ITab tab = (ITab)sender;
            int index = Tabs.IndexOf(tab);
            if (index == 0) return;
            Tabs.Move(index, index - 1);
        }
        private void OnTabMoveRightRequested(object sender, EventArgs e)
        {
            ITab tab = (ITab)sender;
            int index = Tabs.IndexOf(tab);
            if (index == Tabs.Count - 1) return;
            Tabs.Move(index, index + 1);
        }
        private void OnTabMoveToLeftEndRequested(object sender, EventArgs e)
        {
            ITab tab = (ITab)sender;
            int index = Tabs.IndexOf(tab);
            if (index == 0) return;
            Tabs.Move(index, 0);
        }
        private void OnTabMoveToRightEndRequested(object sender, EventArgs e)
        {
            ITab tab = (ITab)sender;
            int index = Tabs.IndexOf(tab);
            if (index == Tabs.Count - 1) return;
            Tabs.Move(index, Tabs.Count - 1);
        }

        // COMMAND METHODS
        // -> Main menu:
        private void NewSection()
        {
            int i = 1;
            bool furtherProcessing = true;

            IEnumerable<string> titleCollection = Tabs.Select(item => item.TabTitle);

            do
            {
                if (titleCollection.Contains($"Section project ({i})"))
                {
                    i++;
                }
                else
                {
                    Tabs.Add(new CalculatorTab($"Section project ({i})"));
                    SelectedTab = Tabs.Last();
                    furtherProcessing = false;
                }

            } while (furtherProcessing);
        }
        private void OpenProject() // Loads previously serialized tab from HDD
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            string path;

            openFileDialog.FileName = "project.spr";
            openFileDialog.Filter = ".spr File | *.spr";
            openFileDialog.Title = "Open project:";

            if (openFileDialog.ShowDialog() == true) path = openFileDialog.FileName;
            else return;

            ResultantSection openedResultantSection;

            FileStream fS = new FileStream(path, FileMode.Open);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                openedResultantSection = (ResultantSection)formatter.Deserialize(fS);
            }
            catch (SerializationException e)
            {
                Annalist.Instance.MSGBox(this.ToString(), "E202", e.Message);
                return;
            }
            finally
            {
                fS.Close();
            }
            Tabs.Add(new CalculatorTab(openedResultantSection));
            SelectedTab = Tabs.Last();
            Annalist.Instance.DisplayStatus(this.ToString(), "I202");
        }
        //private void SaveProjectBuffer()
        //{
        //    string path = MainSettings.Instance.WorkBufferFileLocation;
        //} 
        private void SaveProjectAs() // Serializes selected tab to hdd as new file
        {
            CalculatorTab tab = (CalculatorTab)SelectedTab;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            string path;

            saveFileDialog.FileName = "new project.spr";
            saveFileDialog.Filter = ".spr File | *.spr";
            saveFileDialog.Title = "Save project as:";

            if (saveFileDialog.ShowDialog() == true) path = saveFileDialog.FileName;
            else return;

            FileStream fS = new FileStream(path, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fS, tab.SectionModel);
            }
            catch (SerializationException e)
            {
                Annalist.Instance.MSGBox(this.ToString(), "E201", e.Message);
                return;
            }
            finally
            {
                fS.Close();
            }
            Annalist.Instance.DisplayStatus(this.ToString(), "I201");
        }
        private void OpenAboutTab()
        {
            AddTabWithRedundanceControl(new AboutTab());
        }
        private void OpenConductorDatabaseManager()
        {
            ITab tab = _conductorManager.GetNewManagerTab();
            AddTabWithRedundanceControl(tab);
        }
        private void OpenAttachmentSetDatabaseManager()
        {
            ITab tab = _insulatorSetManager.GetNewManagerTab();
            AddTabWithRedundanceControl(tab);
        }
        private void Exit() // fancy exit way ... with tab remembering and so on... some day...
        {
            Application.Current.Shutdown();
        }
        private void DummyAction() { } // temp
        // -> Execution Premissions :)
        private bool CanExecute_SaveAsCommand()
        {
            CalculatorTab calculatorTab;
            if (SelectedTab is CalculatorTab) calculatorTab = (CalculatorTab)SelectedTab;
            else return false;

            if (calculatorTab.SectionModel != null) return true;
            else return false;
        }
        // -> Aux
        private void AddTabWithRedundanceControl(ITab tabToAdd) // adds specified tab only if there is no of it's kind :)
        {
            var similarTabs = Tabs.Where(tab => tab.GetType() == tabToAdd.GetType());
            if (similarTabs.Count() == 0)
            {
                Tabs.Add(tabToAdd);
                SelectedTab = Tabs.Last();
            }
            else
            {
                SelectedTab = Tabs[Tabs.IndexOf(similarTabs.First())];
            }
        }
    }
}
