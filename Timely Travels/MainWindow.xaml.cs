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

namespace Timely_Travels
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        private void goToMultipleSpotWindow(object sender, RoutedEventArgs e)
        {
            MultipleSpotWindow multWindow = new MultipleSpotWindow();
            multWindow.Show();
            multWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.Close();
        }

        private void goToOneSpotWindow(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("TEST");
        }
    }
}
