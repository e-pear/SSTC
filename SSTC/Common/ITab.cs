using System;
using System.Collections.ObjectModel;
using System.Windows.Input;


namespace SSTC.Common
{
    interface ITab
    {
        // TAB CONTROL -> interactions with tab header
        string TabTitle { get; set; }
        bool IsTabTitleRenaming { get; set; }
        ICommand ToggleRenamingCommand { get; }
        ICommand CloseCommand { get; }
        ICommand MoveTabLeftCommand { get; }
        ICommand MoveTabRightCommand { get; }
        ICommand MoveTabToLeftEndCommand { get; }
        ICommand MoveTabToRightEndCommand { get; }
        event EventHandler ToggleRenamingRequested;
        event EventHandler CloseRequested;
        event EventHandler MoveTabLeftRequested;
        event EventHandler MoveTabRightRequested;
        event EventHandler MoveTabToLeftEndRequested;
        event EventHandler MoveTabToRightEndRequested;
        void TabRefresh();
    }
}
