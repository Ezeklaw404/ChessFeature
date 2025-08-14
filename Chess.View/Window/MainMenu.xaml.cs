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


namespace Chess.View.Window
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : System.Windows.Window
    {
        public MainMenu()
        {
            InitializeComponent();
        }



        private void ChooseClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string mode = button.Tag.ToString();

                System.Windows.Window nextWindow;
                if (mode == "chess")
                    nextWindow = new MainWindow(); // Replace with your real class
                else
                    nextWindow = new Chess960Window(); // Replace with your real class

                nextWindow.Show();
                this.Close();
            }
        }
    }
}
