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
using System.Text.RegularExpressions;

namespace Dancer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer processTimer = new DispatcherTimer();
        private struct Music
        {
            public 
            string music_path, music_name, singer, album, belong_to_list, other_singer;
            int publish_year;
        };
        private List<Music> musicPath = new List<Music>();
        //private int curSong = 37;
        public MainWindow()
        {
            InitializeComponent();
            MysqlConnector.init();
            DirectoryInfo TheFolder = new DirectoryInfo(@"E:\音乐\歌单\");
            DirectoryInfo[] dirInfo = TheFolder.GetDirectories();
            foreach (DirectoryInfo NextFolder in dirInfo)
            {
                MysqlConnector.addNewList(NextFolder.Name);
                FileInfo[] fileInfo = NextFolder.GetFiles();
                foreach (FileInfo NextFile in fileInfo)
                {
                    //NextFile.FullName.Substring(NextFile.FullName.Length - 4) == ".mp3"
                    Match match = Regex.Match(NextFile.FullName, @".*\\(.*?)\s-\s(.*)\.mp3");
                    if (match.Success)
                    {
                        Music music = new Music();
                        music.music_path = NextFile.FullName;
                        music.music_name = match.Groups[2].ToString();
                        music.belong_to_list = NextFolder.Name;
                        Match singer_match = Regex.Match(match.Groups[1].ToString(), @"(.*?)(、|&|\s|,)(.*)");
                        if(singer_match.Success)
                        {
                            music.singer = singer_match.Groups[1].ToString();
                            music.other_singer = singer_match.Groups[3].ToString();
                        }
                        else music.singer = match.Groups[1].ToString();
                        musicPath.Add(music);
                        /*
                        int res = music.other_singer==null? MysqlConnector.addNewSong(music.music_name, music.singer, music.belong_to_list): MysqlConnector.addNewSong(music.music_name, music.singer, music.belong_to_list, music.other_singer);*/
                    }
                }
            }
            processTimer.Interval = new TimeSpan(1);
            processTimer.Tick += ProcessTimer_Tick;
            player.MediaEnded += Player_MediaEnded;
            playNewSong();
        }

        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            if (direct_close) this.Close();
            player.Stop();
            playNewSong();
        }

        private void playNewSong()
        {
            string music_name = "", singer = "";
            MysqlConnector.getCurrentSong(ref music_name, ref singer);
            MysqlConnector.addListeningRecord(music_name, singer);
            player.Source = new Uri(musicPath.Find(name => { return name.music_name == music_name && name.singer == singer; }).music_path);
            music_title.Text = singer + " - " + music_name;
            player.Play();
            processTimer.Start();
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

        private bool playing = true;
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
        private bool direct_close = false;
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (direct_close) this.Close();
            else
            {
                direct_close = true;
                music_title.Text += "  播放结束时退出";
            }
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

