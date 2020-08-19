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

namespace SSTC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            MainWindowViewModel viewModel = new MainWindowViewModel();
            this.DataContext = viewModel;
            this.Title = "Section Sag & Tension Calculator";
            InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FrameworkElement element = (FrameworkElement)sender;
                StackPanel parent = (StackPanel)element.Parent;
                var button = parent.Children.OfType<Button>().Single(Child => Child.Name == "ToggleTabRename_Button");

                element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up)); // little hack ;) without this snippet the recently altered tab name doesn't display properly (some event queue handling issues I guess)
                button.Command.Execute(button.CommandParameter);
            }
        }

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox tB = (TextBox)sender;
            tB.SelectAll();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                int selectedIndex = TabController.SelectedIndex;
                int maxIndex = TabController.Items.Count - 1;
                int targetIndex;

                if (selectedIndex == maxIndex) targetIndex = 0;
                else targetIndex = selectedIndex + 1;

                TabController.SelectedIndex = targetIndex;
            }
        }
    }
}
