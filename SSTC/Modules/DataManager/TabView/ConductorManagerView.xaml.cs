using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SSTC.Modules.DataManager.TabView
{
    /// <summary>
    /// Interaction logic for ConductorManagerView.xaml
    /// </summary>
    public partial class ConductorManagerView : UserControl
    {
        public ConductorManagerView()
        {
            InitializeComponent();
        }

        // UI responsiveness events
        private void side_TextBox_Code_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                side_TextBox_A.Focus();
                side_TextBox_A.SelectAll();
            }
            if (e.Key == Key.Down) side_TextBox_A.Focus();
            if (e.Key == Key.Up) side_TextBox_R20.Focus();
        }

        private void side_TextBox_A_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                side_TextBox_d.Focus();
                side_TextBox_d.SelectAll();
            }
            if (e.Key == Key.Down) side_TextBox_d.Focus();
            if (e.Key == Key.Up) side_TextBox_Code.Focus();
        }

        private void side_TextBox_d_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                side_TextBox_mC.Focus();
                side_TextBox_mC.SelectAll();
            }
            if (e.Key == Key.Down) side_TextBox_mC.Focus();
            if (e.Key == Key.Up) side_TextBox_A.Focus();
        }

        private void side_TextBox_mC_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                side_TextBox_aT.Focus();
                side_TextBox_aT.SelectAll();
            }
            if (e.Key == Key.Down) side_TextBox_aT.Focus();
            if (e.Key == Key.Up) side_TextBox_d.Focus();
        }

        private void side_TextBox_aT_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                side_TextBox_E.Focus();
                side_TextBox_E.SelectAll();
            }
            if (e.Key == Key.Down) side_TextBox_E.Focus();
            if (e.Key == Key.Up) side_TextBox_mC.Focus();
        }

        private void side_TextBox_E_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                side_TextBox_RTS.Focus();
                side_TextBox_RTS.SelectAll();
            }
            if (e.Key == Key.Down) side_TextBox_RTS.Focus();
            if (e.Key == Key.Up) side_TextBox_aT.Focus();
        }

        private void side_TextBox_RTS_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                side_TextBox_R20.Focus();
                side_TextBox_R20.SelectAll();
            }
            if (e.Key == Key.Down) side_TextBox_R20.Focus();
            if (e.Key == Key.Up) side_TextBox_E.Focus();
        }

        private void side_TextBox_R20_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                side_TextBox_Desc.Focus();
                side_TextBox_Desc.SelectAll();
            }
            if (e.Key == Key.Down) side_TextBox_Code.Focus();
            if (e.Key == Key.Up) side_TextBox_RTS.Focus();
        }

        private void side_TextBox_Desc_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) side_ComboBox_Cat.Focus();
        }

        private void side_ComboBox_Cat_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) side_TextBox_Code.Focus();
        }
    }
}
 
