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
        private PathSelect pathSelect;
        private struct Music
        {
            public 
            string music_path, music_name, singer, album, belong_to_list, other_singer;
            int publish_year;
        };
        private List<Music> musicPath = new List<Music>();
        public MainWindow()
        {
            InitializeComponent();
            MysqlConnector.init();
            load_music();
            processTimer.Interval = new TimeSpan(1);
            processTimer.Tick += ProcessTimer_Tick;
            player.MediaEnded += Player_MediaEnded;
            playNewSong();
            pathSelect = new PathSelect(this);
            basePanel.Children.Add(pathSelect);
        }
        public void load_music(string music_path = @"E:\音乐\歌单\")
        {
            musicPath.Clear();
            DirectoryInfo TheFolder = new DirectoryInfo(music_path);
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
                        if (singer_match.Success)
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
        }
        private string cycle_music_name = "", cycle_singer= "";
        public int checkSong(string song_info)
        {
            string music_name, singer;
            Match match = Regex.Match(song_info, @"^(.+?)(\s(.+))?$");
            if (match.Success)
            {
                music_name = match.Groups[1].ToString();
                if (match.Groups.Count > 3 && match.Groups[3].ToString()!="")
                {
                    singer = match.Groups[3].ToString();
                    List<Music> find_music_list = musicPath.FindAll(name => { return name.music_name == music_name && name.singer == singer; });
                    if (find_music_list.ToArray().Length == 0) return -1;
                    else
                    {
                        cycle_music_name = music_name;
                        cycle_singer = singer;
                    }
                }
                else
                {
                    List<Music> find_music_list = musicPath.FindAll(name => { return name.music_name == music_name; });
                    if (find_music_list.ToArray().Length == 0) return -1;
                    else
                    {
                        cycle_music_name = music_name;
                        cycle_singer = "";
                    }
                }
            }
            else
            {
                cycle_singer = "";
                cycle_music_name = "";
                if (song_info == "") return 1;
                else return -1;
            }
            return 0;
        }
        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            if (direct_close) this.Close();
            player.Stop();
            if (cycle_music_name == "" && cycle_singer == "") playNewSong();
            else if (cycle_singer == "") playNewSong(cycle_music_name);
            else playNewSong(cycle_music_name, cycle_singer);
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
        private void playNewSong(string music_name)
        {
            Music finded_music = musicPath.Find(name => { return name.music_name == music_name; });
            string singer = finded_music.singer;
            MysqlConnector.addListeningRecord(music_name, singer);
            player.Source = new Uri(finded_music.music_path);
            music_title.Text = singer + " - " + music_name;
            player.Play();
            processTimer.Start();
        }
        private void playNewSong(string music_name, string singer)
        {
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
            if (basePanel.Visibility == Visibility.Hidden)
            {
                this.Height += basePanel.ActualHeight;
                basePanel.Visibility = Visibility.Visible;
            }
            else
            {
                this.Height -= basePanel.ActualHeight;
                basePanel.Visibility = Visibility.Hidden;
            }
        }
        private void changeProcess(double k)
        {
            curProcess.Width = k * whlProcess.Width;
        }
    }
}

