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

namespace GamePlay
{
    
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            string playerName = PlayerNameTextBox.Text;
            if (!string.IsNullOrWhiteSpace(playerName))
            {
                MainWindow mainWindow = new();
                mainWindow.player.PlayerName = playerName;
                mainWindow.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Please enter your name.");
            }
           
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(); 
        }   
    }
}
