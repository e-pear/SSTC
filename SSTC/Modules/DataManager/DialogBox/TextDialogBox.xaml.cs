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
using System.Windows.Shapes;

namespace SSTC.Modules.DataManager.DialogBox
{
    /// <summary>
    /// Interaction logic for TextDialogBox.xaml
    /// </summary>
    public partial class TextDialogBox : Window
    {
        public TextDialogBox(string question, string defaultAnswer = "", string customTitle = "Text Dialog Box", IEnumerable<string> exclusions = null)
        {
            InitializeComponent();
            
            this.Title = customTitle;
            this.exclusions = exclusions;
            
            labRequest.Content = question;
            tBxAnswer.Text = defaultAnswer;
            
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            tBxAnswer.Focus();
            tBxAnswer.SelectAll();
        }

        private void textBox_Answer_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (exclusions != null)
            {
                if(exclusions.Contains(tBxAnswer.Text))
                {
                    btnDialogOk.IsEnabled = false;
                    labNotice.Visibility = Visibility.Visible;
                }
                else
                {
                    btnDialogOk.IsEnabled = true;
                    labNotice.Visibility = Visibility.Hidden;
                }
            }
        }

        private IEnumerable<string> exclusions;
        public string Answer
        {
            get { return tBxAnswer.Text; }
        }


    }
}
