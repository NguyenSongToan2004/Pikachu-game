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
    /// <summary>
    /// Interaction logic for GameOver.xaml
    /// </summary>
    public partial class GameOver : Window
    {
        public List<PlayerScore> list { get; set; } = new();
        public PlayerScore playerScore { get; set; } = new();
        public GameOver()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ScoreDataGrid.ItemsSource = list;
            No1.Content = list[0].PlayerName;
            No2.Content = list[1].PlayerName;
            No3.Content = list[2].PlayerName;
            
            No1Score.Content = list[0].Score;
            No2Score.Content = list[1].Score;
            No3Score.Content = list[2].Score;
            YourScoreTextBlock.Text = "Your Score: " + playerScore.Score.ToString(); 
        }

        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            main.player.PlayerName = playerScore.PlayerName;
            main.Show();
            this.Close();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            this.Close();
        }
    }
}
