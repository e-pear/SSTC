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
    /// Interaction logic for InsulatorSetManagerView.xaml
    /// </summary>
    public partial class InsulatorSetManagerView : UserControl
    {
        public InsulatorSetManagerView()
        {
            InitializeComponent();
        }

        // UI responsiveness events
        private void side_TextBox_Code_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                side_TextBox_Lins.Focus();
                side_TextBox_Lins.SelectAll();
            }
            if (e.Key == Key.Down) side_TextBox_Lins.Focus();
            if (e.Key == Key.Up) side_TextBox_ains.Focus();
        }
        private void side_TextBox_Lins_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                side_TextBox_Wins.Focus();
                side_TextBox_Wins.SelectAll();
            }
            if (e.Key == Key.Down) side_TextBox_Wins.Focus();
            if (e.Key == Key.Up) side_TextBox_Code.Focus();
        }
        private void side_TextBox_Wins_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                side_TextBox_ains.Focus();
                side_TextBox_ains.SelectAll();
            }
            if (e.Key == Key.Down) side_TextBox_ains.Focus();
            if (e.Key == Key.Up) side_TextBox_Lins.Focus();
        }
        private void side_TextBox_ains_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                side_TextBox_Desc.Focus();
                side_TextBox_Desc.SelectAll();
            }
            if (e.Key == Key.Down) side_TextBox_Code.Focus();
            if (e.Key == Key.Up) side_TextBox_Wins.Focus();
        }
        private void side_TextBox_Desc_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) side_ComboBox_Type.Focus();
        }
        private void side_ComboBox_Type_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) side_TextBox_Code.Focus();
        }
    }
}
