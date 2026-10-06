using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace GamePlay
{
    public class GameSounds
    {
        private MediaPlayer backgroundMusicPlayer = new(); // Thêm biến để lưu trữ MediaPlayer
        private MediaPlayer wrongSelectionPlayer = new();
        private MediaPlayer correctSelectionPlayer = new();


        public void PlayBackgroundMusic()
        {
                backgroundMusicPlayer.Open(new Uri("Sounds/NhacNen.wav", UriKind.Relative));
                backgroundMusicPlayer.MediaEnded += (sender, e) =>
                {
                    backgroundMusicPlayer.Position = TimeSpan.Zero; // Đặt lại vị trí khi nhạc kết thúc
                    backgroundMusicPlayer.Play(); // Phát lại nhạc
                    backgroundMusicPlayer.Volume = 0.1;

                };

            backgroundMusicPlayer.Play(); // Phát nhạc nền
            backgroundMusicPlayer.Volume = 0.1;

        }

        public void StopBackgroundMusic()
        {
            backgroundMusicPlayer.Stop();   
        }

        public void PlayWrongSelectionSound()
        {
                wrongSelectionPlayer.Open(new Uri("Sounds/wrong.wav", UriKind.Relative)); 
                wrongSelectionPlayer.Play();
            wrongSelectionPlayer.Volume = 0.9;

        }
        public void PlayCorrectSelectionSound()
        {
            correctSelectionPlayer.Open(new Uri("Sounds/correct.wav", UriKind.Relative));
            correctSelectionPlayer.Play();
            correctSelectionPlayer.Volume = 0.9;

        }
    }
}
