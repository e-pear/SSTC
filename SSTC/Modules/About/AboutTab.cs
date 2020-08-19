using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SSTC.Common;

namespace SSTC.Modules.About
{
    class AboutTab : Tab
    {
        public string About { get; }
        public AboutTab()
        {
            TabTitle = "About: ";
            About = Properties.Resources.About;
        }
        protected override bool CanExecute_ToggleRenamingCommand()
        {
            return false;
        }
    }
}
