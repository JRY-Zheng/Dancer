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
using System.Media;
using System.Windows.Threading;
using System.Windows.Forms;
using System.IO;

namespace Dancer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer processTimer = new DispatcherTimer();
        private List<string> musicPath = new List<string>();
        private int curSong = 37;
        public MainWindow()
        {
            InitializeComponent();
            DirectoryInfo TheFolder = new DirectoryInfo(@"E:\音乐\歌单\");
            DirectoryInfo[] dirInfo = TheFolder.GetDirectories();
            foreach (DirectoryInfo NextFolder in dirInfo)
            {
                FileInfo[] fileInfo = NextFolder.GetFiles();
                foreach (FileInfo NextFile in fileInfo)
                    if(NextFile.FullName.Substring(NextFile.FullName.Length-4)==".mp3")
                        musicPath.Add(NextFile.FullName);
            }
            processTimer.Interval = new TimeSpan(1);
            processTimer.Tick += ProcessTimer_Tick;
            player.MediaEnded += Player_MediaEnded;
            player.Source = new Uri(musicPath[curSong++]);
        }

        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            player.Stop();
            player.Source = new Uri(musicPath[curSong++]);
            player.Play();
        }

        private void ProcessTimer_Tick(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            try
            {
                changeProcess(player.Position.TotalSeconds / player.NaturalDuration.TimeSpan.TotalSeconds);
            }
            catch { }
        }

        private bool playing = false;
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (playing)
            {
                player.Pause();
                btnPlay.Content = " ▷";
                processTimer.Stop();
            }
            else
            {
                player.Play();
                btnPlay.Content = "∥";
                processTimer.Start();
            }
            playing = !playing;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            /*for (int i = 0; i < 20; i++) for (int j = 0; j < 20; j++)
                    MysqlConnector.addNewSong("song" + i.ToString(), "singer" + j.ToString(), "listname");
            */
            string music_name = "", singer = "";
            for(int i = 0; i < 20; i++)
            {
                MysqlConnector.getCurrentSong(ref music_name, ref singer);
                MysqlConnector.addListeningRecord(music_name, singer);
                System.Windows.MessageBox.Show(music_name + " - " + singer);
            }
        }
        private void changeProcess(double k)
        {
            curProcess.Width = k * whlProcess.Width;
        }
    }
}

