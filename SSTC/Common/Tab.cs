using SSTC_ViewResources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SSTC.Common
{
    public abstract class Tab : ITab, INotifyPropertyChanged
    {
        // FIELDS
        // -> Globals:
        protected MainSettings _MainSettings;
        protected Annalist _Annalist;
        private MainLanguageTable _globalLanguageDictionary;
        // -> Tab Control:
        private string _tabTitle;
        private bool _isTabTitleRenaming;
        // PROPERTIES
        // -> Globals:
        public MainLanguageTable NameTable { get { return MainLanguageTable.Instance; } }
        // -> Tab Control:
        public string TabTitle
        {
            get { return _tabTitle; }
            set { _tabTitle = value; RaisePropertyChanged("TabTitle"); }
        }
        public bool IsTabTitleRenaming
        {
            get { return _isTabTitleRenaming; }
            set { _isTabTitleRenaming = value; RaisePropertyChanged("IsTabTitleRenaming"); }
        }
        // COMMANDS
        // -> Tab Commands:
        public ICommand ToggleRenamingCommand { get; }
        public ICommand CloseCommand { get; }

        public ICommand MoveTabLeftCommand { get; }

        public ICommand MoveTabRightCommand { get; }

        public ICommand MoveTabToLeftEndCommand { get; }

        public ICommand MoveTabToRightEndCommand { get; }

        // EVENTS
        // -> control tab events:
        public event EventHandler ToggleRenamingRequested;
        public event EventHandler CloseRequested;
        public event EventHandler MoveTabLeftRequested;
        public event EventHandler MoveTabRightRequested;
        public event EventHandler MoveTabToLeftEndRequested;
        public event EventHandler MoveTabToRightEndRequested;
        // -> property events:
        public event PropertyChangedEventHandler PropertyChanged;

        // TAB EVENT RAISERS
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // CONSTRUCTOR
        public Tab()
        {
            // -> Globals:
            _MainSettings = MainSettings.Instance;
            _Annalist = Annalist.Instance;

            // -> Tab Commands:
            ToggleRenamingCommand = new CommandRelay(m => ToggleRenamingRequested?.Invoke(this, EventArgs.Empty), CanExecute_ToggleRenamingCommand);
            CloseCommand = new CommandRelay(CloseTab, () => true);

            MoveTabLeftCommand = new CommandRelay(m => MoveTabLeftRequested?.Invoke(this, EventArgs.Empty), () => true);
            MoveTabRightCommand = new CommandRelay(m => MoveTabRightRequested?.Invoke(this, EventArgs.Empty), () => true);
            MoveTabToLeftEndCommand = new CommandRelay(m => MoveTabToLeftEndRequested?.Invoke(this, EventArgs.Empty), () => true);
            MoveTabToRightEndCommand = new CommandRelay(m => MoveTabToRightEndRequested?.Invoke(this, EventArgs.Empty), () => true);
        }
        // TAB CLOSING COMMAND RELATED METHODS
        private void CloseTab()
        {
            if (ClosingAction()) CloseRequested?.Invoke(this, EventArgs.Empty);
            else return;
        }
        protected virtual bool ClosingAction() { return true; }
        // TAB RENAMING COMMAND RELATED METHODS
        protected virtual bool CanExecute_ToggleRenamingCommand() { return true; }
        // TAB REFRESH FUNCTIONALITY
        public virtual void TabRefresh() { }
    }
}
