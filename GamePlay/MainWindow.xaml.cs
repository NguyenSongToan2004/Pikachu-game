using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;


namespace GamePlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public PlayerScore player = new PlayerScore();


        private GameModel gameModel;
        private Border[,] pokemonCells;
        private Border firstSelected = null;
        private int score = 0;
        private int timeLeft = DEFAULT_TIME_LEFT; //MERGE CODE 2
        private DispatcherTimer timer;

        private int shuffleCount = 0;  // Biến đếm số lần shuffle //MERGE CODE 2
        private int level = 1; //MERGE CODE 2


        private const int POKEMON_TYPES = 36;  // Số loại pokemon
        private int boardWidth = BOARD_WIDTH;    // Số cột
        private int boardHeight = BOARD_HEIGHT;   // Số hàng
        private const int CELL_SIZE = 40;      // Kích thước mỗi ô
        private const int MAX_SHUFFLE_COUNT = 5;  // Giới hạn số lần shuffle
        private const int DEFAULT_TIME_LEFT = 300;
        private const int BOARD_WIDTH = 4;
        private const int BOARD_HEIGHT = 4;

        private const string playerScoresFile = "playerScores.json"; // Tên file lưu điểm số


        private GameSounds gameSounds = new GameSounds(); // Thêm biến để lưu trữ GameSounds
        private bool isMuted = false; // Biến để theo dõi trạng thái âm thanh


        public MainWindow()
        {
            InitializeComponent();
            StartNewGame();
        }

        private void StartNewGame()
        {
            // Reset game
            score = 0;
            ScoreText.Text = "0";
            TimeText.Text = $"{DEFAULT_TIME_LEFT}";
            timeLeft = DEFAULT_TIME_LEFT;
            shuffleCount = 0;
            ShuffleButton.IsEnabled = true;
            ShuffleButton.Content = $"Shuffle: {MAX_SHUFFLE_COUNT - shuffleCount}";
            GameCanvas.Children.Clear();
            firstSelected = null;
            boardWidth = BOARD_WIDTH + 2;
            boardHeight = BOARD_HEIGHT + 2;

            SetUpWindowSize();

            // Khởi tạo game mới
            gameModel = new GameModel(boardWidth, boardHeight, POKEMON_TYPES);
            pokemonCells = new Border[boardHeight, boardWidth];
            gameSounds.PlayBackgroundMusic(); // Phát nhạc nền
            
            CreateGameBoard();
            if (!gameModel.HasValidPairs())
            {
                System.Windows.MessageBox.Show("There are no moves left, the board will be shuffled.");
                gameModel.ShuffleBoard();
                // Cập nhật giao diện sau khi shuffle
                UpdateGameBoard();
            }
            StartTimer();




        }

        private void SetUpWindowSize()
        {
            GameCanvas.Width = boardWidth * CELL_SIZE;
            GameCanvas.Height = boardHeight * CELL_SIZE;

            ((Viewbox)GameCanvas.Parent).MinWidth = boardWidth * CELL_SIZE;
            ((Viewbox)GameCanvas.Parent).MinHeight = boardHeight * CELL_SIZE;
            ((Viewbox)GameCanvas.Parent).MaxHeight = boardHeight * CELL_SIZE + 100;
            ((Viewbox)GameCanvas.Parent).MaxWidth = boardWidth * CELL_SIZE + 100;

            this.MinWidth = boardWidth * CELL_SIZE + 106;
            this.MinHeight = boardHeight * CELL_SIZE + 106;
        }
        private void NextLevel()
        {
            timeLeft = DEFAULT_TIME_LEFT;
            TimeText.Text = $"{timeLeft}";
            GameCanvas.Children.Clear();
            firstSelected = null;
            level++;

            // Khởi tạo game mới
            IncreaseBoardSize();
            gameModel = new GameModel(boardWidth, boardHeight, POKEMON_TYPES);
            pokemonCells = new Border[boardHeight, boardWidth];

            SetUpWindowSize();

            CreateGameBoard();

            if (!gameModel.HasValidPairs())
            {
                System.Windows.MessageBox.Show("Hết đường đi, màn chơi sẽ được xáo trộn");
                gameModel.ShuffleBoard();
                // Cập nhật giao diện sau khi shuffle
                UpdateGameBoard();
            }
            StartTimer();
        }



        private void CreateGameBoard()
        {
            for (int i = 1; i < boardHeight - 1; i++)
            {
                for (int j = 1; j < boardWidth - 1; j++)
                {
                    // Tạo hình ảnh pokemon
                    Image pokemonImage = new Image
                    {
                        Width = CELL_SIZE - 2,
                        Height = CELL_SIZE - 2,
                        Stretch = Stretch.Uniform,
                        Source = new BitmapImage(new Uri($"pack://application:,,,/Images/pieces{gameModel.GetCell(i, j)}.png"))
                    };

                    // Tạo border chứa pokemon
                    Border cell = new Border
                    {
                        Width = CELL_SIZE,
                        Height = CELL_SIZE,
                        Background = Brushes.White,
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        Child = pokemonImage
                    };

                    // Gán sự kiện và vị trí
                    cell.MouseDown += Cell_MouseDown;
                    cell.Tag = new Point(i, j);
                    Canvas.SetLeft(cell, j * CELL_SIZE);
                    Canvas.SetTop(cell, i * CELL_SIZE);

                    GameCanvas.Children.Add(cell);
                    pokemonCells[i, j] = cell;
                }
            }
        }

        public void IncreaseBoardSize()
        {
            // Tăng kích thước xen kẽ giữa chiều rộng và chiều cao
            if (level % 2 == 0)
            {
                boardWidth += 2; // Tăng chiều rộng
            }
            else
            {
                boardHeight += 2; // Tăng chiều cao
            }

            // Đảm bảo số ô là số chẵn
            if ((boardWidth * boardHeight) % 2 != 0)
            {
                boardWidth++; // Điều chỉnh để tổng số ô là số chẵn
            }

            // Giới hạn kích thước tối đa (tuỳ chọn)
            int maxWidth = 20;
            int maxHeight = 10;

            if (boardWidth > maxWidth)
            {
                boardWidth = maxWidth + 2;
                timeLeft = DEFAULT_TIME_LEFT - level;
            }

            if (boardHeight > maxHeight)
                boardHeight = maxHeight + 2;

        }


        private async void Cell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border clickedCell = sender as Border;
            Point position = (Point)clickedCell.Tag;
            int row = (int)position.X;
            int col = (int)position.Y;

            if (gameModel.GetCell(row, col) == 0)
                return;

            if (firstSelected == null)
            {
                firstSelected = clickedCell;
                clickedCell.Background = Brushes.Yellow;
            }
            else
            {
                Point firstPos = (Point)firstSelected.Tag;
                int firstRow = (int)firstPos.X;
                int firstCol = (int)firstPos.Y;

                if (firstRow == row && firstCol == col)
                {
                    firstSelected.Background = Brushes.White;
                    firstSelected = null;
                    return;
                }

                //=====================MERGE CODE=======================
                if (gameModel.CanConnect(firstRow, firstCol, row, col))
                {
                    gameModel.RemovePair(firstRow, firstCol, row, col);
                    firstSelected.Visibility = Visibility.Hidden;
                    clickedCell.Visibility = Visibility.Hidden;

                    if (!gameModel.ShowPath(GameCanvas, CELL_SIZE))
                    {
                        firstSelected.Visibility = Visibility.Visible;
                        clickedCell.Visibility = Visibility.Visible;
                        return;
                    }

                    //gameModel.ShowPath(GameCanvas, CELL_SIZE);
                    // gameModel.DrawMarkedPathsOnCanvas(GameCanvas, BOARD_WIDTH, BOARD_HEIGHT, CELL_SIZE);

                    await Task.Delay(300);

                    gameModel.ClearPaths(GameCanvas);

                    score += 10 * level;
                    ScoreText.Text = score.ToString();
                    if (gameModel.PossibleMatches.Count > 0)
                    {
                        // Kiểm tra nếu không còn cặp hợp lệ và shuffle
                        if (!gameModel.HasValidPairs())
                        {
                            System.Windows.MessageBox.Show("Hết đường đi, màn chơi sẽ được xáo trộn");
                            gameModel.ShuffleBoard();
                            // Cập nhật giao diện sau khi shuffle
                            UpdateGameBoard();
                        }
                    }
                    else
                    {
                        NextLevel();
                    }
                    gameSounds.PlayCorrectSelectionSound(); // Phát âm thanh khi chọn đúng          
                }

                else
                {
                    gameSounds.PlayWrongSelectionSound();
                }
                if (firstSelected != null)
                {
                    firstSelected.Background = Brushes.White;
                }

                firstSelected = null;
            }
        }

        private void StartTimer()
        {
            if (timer != null)
                timer.Stop();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timeLeft--;
            TimeText.Text = timeLeft.ToString();

            if (timeLeft <= 0)
            {
                timer.Stop();
                player.Score = score;
                SaveScore(player.PlayerName, player.Score);
                MessageBox.Show($"Hết giờ! Điểm của {player.PlayerName}: {score}", "Game Over");
                gameSounds.StopBackgroundMusic();
                GameOver gameOver = new();
                gameOver.list = LoadPlayerScores(); // Lấy danh sách người chơi
                gameOver.playerScore.PlayerName = player.PlayerName;
                gameOver.playerScore.Score = player.Score;
                gameOver.Show();
                this.Close();
            }
        }

        private void btnNewGame_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }

        //private void MenuButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (MenuPanel.Visibility == Visibility.Collapsed)
        //    {
        //        MenuPanel.Visibility = Visibility.Visible; // Hiện menu
        //    }
        //    else
        //    {
        //        MenuPanel.Visibility = Visibility.Collapsed; // Ẩn menu
        //    }
        //}

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you really want to out the game?", "Close?", MessageBoxButton.YesNo, MessageBoxImage.Stop);
            if (result == MessageBoxResult.Yes)
            {
                player.Score = score;
                SaveScore(player.PlayerName, player.Score); // Lưu điểm vào file JSON
                gameSounds.StopBackgroundMusic();
                GameOver gameOver = new();
                gameOver.list = LoadPlayerScores(); // Lấy danh sách người chơi
                gameOver.playerScore.PlayerName = player.PlayerName;
                gameOver.playerScore.Score = player.Score;
                gameOver.Show();
                this.Close();
            }
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            if (shuffleCount >= MAX_SHUFFLE_COUNT)
                return;
            // Gọi phương thức Shuffle từ GameModel
            gameModel.ShuffleBoard();

            // Cập nhật giao diện sau khi shuffle
            UpdateGameBoard();

            shuffleCount++;
            if (shuffleCount > (MAX_SHUFFLE_COUNT - 1))
            {
                ShuffleButton.IsEnabled = false;
            }
            ShuffleButton.Content = $"Shuffle: {MAX_SHUFFLE_COUNT - shuffleCount}";

        }

        private void UpdateGameBoard()
        {
            for (int i = 0; i < boardHeight; i++)
            {
                for (int j = 0; j < boardWidth; j++)
                {
                    if (pokemonCells[i, j] != null)
                    {
                        int pokemonType = gameModel.GetCell(i, j);

                        if (pokemonType == 0)
                        {
                            pokemonCells[i, j].Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            Image pokemonImage = (Image)pokemonCells[i, j].Child;
                            pokemonImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/pieces{pokemonType}.png"));
                            pokemonCells[i, j].Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        public void SaveScore(string playerName, int score) // Cập nhật phương thức để lưu tên và điểm số
        {

            var player = new PlayerScore { PlayerName = playerName, Score = score };
            List<PlayerScore> playerScores = LoadPlayerScores(); // Lấy danh sách người chơi hiện tại
            playerScores.Add(player); // Thêm điểm số mới vào danh sách
            string json = JsonConvert.SerializeObject(playerScores);
            File.WriteAllText(playerScoresFile, json); // Lưu vào file 
        }



        //LẤY DỮ LIỆU NGƯỜI CHƠI RA TỪ FILE JSON
        public List<PlayerScore> LoadPlayerScores() // Phương thức để lấy danh sách người chơi
        {


            string json = File.ReadAllText(playerScoresFile);
            var list = JsonConvert.DeserializeObject<List<PlayerScore>>(json);
            if (string.IsNullOrWhiteSpace(json)) // Kiểm tra xem chuỗi JSON có rỗng không
            {
                return new List<PlayerScore>(); // Trả về danh sách rỗng nếu chuỗi JSON rỗng
            }

            SortPlayerScoresDesc(list);

            return list ?? new List<PlayerScore>(); // Giải mã JSON thành danh sách
        }

        private List<PlayerScore> SortPlayerScoresDesc(List<PlayerScore> list)
        {
            for(int i = 0; i < list.Count - 1; i++)
            {
                for(int j = i + 1; j < list.Count; j++)
                {
                    if (list[i].Score < list[j].Score)
                    {
                        var tmp = list[i];
                        list[i] = list[j];
                        list[j] = tmp;
                    }
                }
            }
            return list;
        }

        private void btnMute_Click(object sender, RoutedEventArgs e)
        {
            isMuted = !isMuted; // Đảo ngược trạng thái âm thanh

            if (isMuted)
            {
                gameSounds.StopBackgroundMusic(); // Dừng nhạc
                btnMute.Content = "On Sound"; // Thay đổi nội dung nút
            }
            else
            {
                gameSounds.PlayBackgroundMusic(); // Phát nhạc
                btnMute.Content = "Mute"; // Thay đổi nội dung nút
            }
        }
    }


}