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
using log4net;
using System.Data;
using System.ComponentModel;

namespace Dancer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public string cur_music_name, cur_singer;
        private DispatcherTimer processTimer = new DispatcherTimer();
        private PathSelect pathSelect;
        private LyricDisplay lyricDisplay;
        private struct Music
        {
            public string music_path, music_name, file_name, singer, album, belong_to_list, other_singer;
            public int publish_year;
        };
        private struct SimMusic
        {
            public string music_name, singer;
            public SimMusic(string _music_name, string _singer)
            {
                music_name = _music_name;
                singer = _singer;
            }
        };
        public struct Lyric
        {
            public double position;
            public string lyric_content;
        };
        private List<SimMusic> to_upload_music;
        private List<SimMusic> to_download_music;
        private List<Music> musicPath = new List<Music>();
        private List<Music> to_upload_music_list = new List<Music>();
        private Dictionary<string, string> preference = new Dictionary<string, string>();
        private ILog log = log4net.LogManager.GetLogger("Dancer.Logging");
        private BackgroundWorker fill_music_to_mysql = new BackgroundWorker();
        private MysqlConnector mysqlConnector = new MysqlConnector();
        private FtpConnector ftpConnector = new FtpConnector();

        public MainWindow()
        {
            InitializeComponent();
            load_settings();
            log.Info("Settings loaded.");
            //init_ftp(ref ftpConnector);
            //log.Info("Ftp initialized.");
            init_mysql(ref mysqlConnector);
            log.Info("Mysql initialized.");
            fill_music_to_mysql.DoWork += Fill_music_to_mysql_DoWork;
            fill_music_to_mysql.RunWorkerCompleted += Fill_music_to_mysql_RunWorkerCompleted;
            load_music(preference["music_directory"]);
            log.Info("Music loaded.");
            set_theme();
            processTimer.Interval = new TimeSpan(1);
            processTimer.Tick += ProcessTimer_Tick;
            player.MediaEnded += Player_MediaEnded;
            if (preference["display_lyric"] == "true")
            {
                lyricDisplay = new LyricDisplay(this);
                lyricPanel.Children.Add(lyricDisplay);
            }
            if (preference["top_most"] == "true") this.Topmost = true;
            playNewSong();
            pathSelect = new PathSelect(this);
            basePanel.Children.Add(pathSelect);
        }

        private void Fill_music_to_mysql_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //throw new NotImplementedException();
            log.Info("Song list updated.");
        }

        private void Fill_music_to_mysql_DoWork(object sender, DoWorkEventArgs e)
        {
            //throw new NotImplementedException();
            foreach (SimMusic simMusic in to_upload_music)
            {
                to_upload_music_list.Add(musicPath.Find(m => { return m.music_name == simMusic.music_name && m.singer == simMusic.singer; }));
            }
            List<BackgroundWorker> upload_music = new List<BackgroundWorker>();
            for (int i = 0; i < 4; i++) upload_music.Add(new BackgroundWorker());
            upload_music.ForEach(um => { um.DoWork += Um_DoWork; });
            upload_music.ForEach(um => { um.RunWorkerCompleted += Um_RunWorkerCompleted; });
            upload_music.ForEach(um => { um.RunWorkerAsync(); });
            upload_music.ForEach(um => { while (um.IsBusy) ; });
        }

        private void Um_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //throw new NotImplementedException();
            if (to_upload_music_list.Count != 0) (sender as BackgroundWorker).RunWorkerAsync();
        }

        private void Um_DoWork(object sender, DoWorkEventArgs e)
        {
            //throw new NotImplementedException();
            MysqlConnector music_updator = new MysqlConnector();
            init_mysql(ref music_updator);
            FtpConnector music_uploader = new FtpConnector();
            init_ftp(ref music_uploader);
            Music music;
            lock (to_upload_music_list)
            {
                music = to_upload_music_list[0];
                to_upload_music_list.Remove(music);
            }
            int res = music.other_singer == null ? music_updator.addNewSong(music.music_name, music.singer, music.belong_to_list) : music_updator.addNewSong(music.music_name, music.singer, music.belong_to_list, music.other_singer);
            //music_uploader.uploadFile(music.music_path, music.belong_to_list);
        }
        private void init_mysql(ref MysqlConnector m)
        {
            m.init(preference["mysql_server_ip"], preference["mysql_catalog"], preference["mysql_user_id"], preference["mysql_password"], preference["mysql_port"]);
        }
        private void init_ftp(ref FtpConnector f)
        {
            f.init(preference["ftp_server_ip"], preference["ftp_user_id"], preference["ftp_password"]);
        }
        //加载设置
        string[] setting_options = { "mysql_server_ip", "mysql_catalog", "mysql_user_id", "mysql_password", "mysql_port", "ftp_server_ip", "ftp_user_id", "ftp_password", "window_top_color", "window_bottom_color", "play_button_color", "music_title_color", "music_directory", "display_lyric", "display_figure", "top_most"};
        private void load_settings()
        {
            if (!File.Exists("settings.ini"))
            {
                System.Windows.MessageBox.Show("配置文件缺失！");
                this.Close();
            }
            string[] settings = File.ReadAllLines("settings.ini");
            foreach(string setting in settings)
            {
                Match match = Regex.Match(setting, @"^(.+?)\s*=\s*(.+?)(//.*)?$");
                if (!match.Success) continue;
                preference.Add(match.Groups[1].ToString(), match.Groups[2].ToString());
            }
            foreach(string option in setting_options)
            {
                if (!preference.ContainsKey(option))
                {
                    System.Windows.MessageBox.Show("请检查配置文件！\n" + option + "项缺失！");
                    this.Close();
                    break;
                }
            }
        }
        //设置主题
        private void set_theme()
        {
            try
            {
                set_color(preference["window_top_color"], ref windowTopColor);
                set_color(preference["window_bottom_color"], ref windowBottomColor);
                set_color(preference["play_button_color"], ref btnPlay);
                set_color(preference["music_title_color"], ref music_title);
            }
            catch { }
        }
        private void set_color(string color_string, ref GradientStop gradientStop)
        {
            if (color_string != "default")
            {
                try
                {
                    System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml((color_string));
                    gradientStop.Color = Color.FromArgb(color.A, color.R, color.G, color.B);
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("配置文件中颜色项不符合要求，仍采用默认颜色！详细信息：\n" + e);
                }
            }
        }
        private void set_color(string color_string, ref System.Windows.Controls.Button button)
        {
            if (color_string != "default")
            {
                try
                {
                    System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml((color_string));
                    button.Foreground = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("配置文件中颜色项不符合要求，仍采用默认颜色！详细信息：\n" + e);
                }
            }
        }
        private void set_color(string color_string, ref TextBlock textBlock)
        {
            if (color_string != "default")
            {
                try
                {
                    System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml((color_string));
                    textBlock.Foreground = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("配置文件中颜色项不符合要求，仍采用默认颜色！详细信息：\n" + e);
                }
            }
        }
        //加载音乐
        private void load_music(string music_path)
        {
            if (!Directory.Exists(music_path))
            {
                System.Windows.MessageBox.Show("歌曲路径不存在！");
                this.Close();
            }
            musicPath.Clear();
            DirectoryInfo TheFolder = new DirectoryInfo(music_path);
            DirectoryInfo[] dirInfo = TheFolder.GetDirectories();
            if (dirInfo.Length == 0)
            {
                System.Windows.MessageBox.Show("歌单不存在！");
                this.Close();
            }
            foreach (DirectoryInfo NextFolder in dirInfo)
            {
                mysqlConnector.addNewList(NextFolder.Name);
                FileInfo[] fileInfo = NextFolder.GetFiles();
                foreach (FileInfo NextFile in fileInfo)
                {
                    Match match_nc = Regex.Match(NextFile.FullName, @".*\\(.*?)\s-\s(.*)\.mp3");
                    Match match_xm = Regex.Match(NextFile.FullName, @".*\\(.*?)_(.*)\.mp3");
                    if (match_nc.Success || match_xm.Success)
                    {
                        if (match_xm.Success)
                            ;
                        Match match = match_nc.Success ? match_nc : match_xm;
                        int nc_or_xm = match_nc.Success ? 2 : 1;
                        Music music = new Music();
                        music.music_path = NextFile.FullName;
                        music.music_name = match.Groups[nc_or_xm].ToString();
                        music.file_name = System.IO.Path.GetFileNameWithoutExtension(NextFile.Name);
                        music.belong_to_list = NextFolder.Name;
                        Match singer_match = Regex.Match(match.Groups[3-nc_or_xm].ToString(), @"(.*?)(、|&|\s|,)(.*)");
                        if (singer_match.Success)
                        {
                            music.singer = singer_match.Groups[1].ToString();
                            music.other_singer = singer_match.Groups[3].ToString();
                        }
                        else music.singer = match.Groups[3-nc_or_xm].ToString();
                        musicPath.Add(music);
                        //处理爬虫歌词命名不规范问题。
                        string lrc_music_name = System.IO.Path.Combine(NextFolder.FullName, music.music_name + ".lrc");
                        string lrc_full_name = System.IO.Path.Combine(NextFolder.FullName, music.file_name + ".lrc");
                        if (File.Exists(lrc_music_name) && !File.Exists(lrc_full_name))
                        {
                            File.Move(lrc_music_name, lrc_full_name);
                        }
                    }
                }
            }
            mysqlConnector.getSongList();
            List<SimMusic> exist_music_list = new List<SimMusic>();
            foreach (DataRow dr in mysqlConnector.dataset.Tables["all_songs"].Rows) exist_music_list.Add(new SimMusic(dr[0].ToString(), dr[1].ToString()));
            List<SimMusic> cur_music_list = new List<SimMusic>();
            musicPath.ForEach(m => { cur_music_list.Add(new SimMusic(m.music_name, m.singer)); });
            List<SimMusic> except_music = cur_music_list.Except(exist_music_list).ToList();
            to_upload_music = except_music.Except(exist_music_list).ToList();
            to_download_music = except_music.Except(cur_music_list).ToList();
            if (to_upload_music.Count > 0) fill_music_to_mysql.RunWorkerAsync();
        }
        //单曲循环
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
        //自动播放下一曲
        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            log.Info("----Current song ended.");
            if (direct_close)
            {
                this.Close();
                return;
            }
            player.Stop();
            if (cycle_music_name == "" && cycle_singer == "") playNewSong();
            else if (cycle_singer == "") playNewSong(cycle_music_name);
            else playNewSong(cycle_music_name, cycle_singer);
        }
        private void playNewSong()
        {
            string music_name = "", singer = "";
            mysqlConnector.getCurrentSong(ref music_name, ref singer);
            mysqlConnector.addListeningRecord(music_name, singer);
            Music finded_music = musicPath.Find(name => { return name.music_name == music_name && name.singer == singer; });
            player.Source = new Uri(finded_music.music_path);
            music_title.Text = singer + " - " + music_name;
            log.Info(String.Format("Playing song {0} by {1}...", music_name, singer));
            player.Play();
            processTimer.Start();
            cur_music_name = music_name;
            cur_singer = singer;
            if (preference["display_lyric"] == "true")
            {
                LyricReader.init(finded_music.file_name, finded_music.belong_to_list, preference["music_directory"]);
                lyricDisplay.init(music_title.Text, LyricReader.load_lyric());
            }
        }
        private void playNewSong(string music_name)
        {
            Music finded_music = musicPath.Find(name => { return name.music_name == music_name; });
            string singer = finded_music.singer;
            mysqlConnector.addListeningRecord(music_name, singer);
            player.Source = new Uri(finded_music.music_path);
            music_title.Text = singer + " - " + music_name;
            log.Info(String.Format("Playing song {0} by {1}...", music_name, singer));
            player.Play();
            processTimer.Start();
            cur_music_name = music_name;
            cur_singer = singer;
            if (preference["display_lyric"] == "true")
            {
                LyricReader.init(finded_music.file_name, finded_music.belong_to_list, preference["music_directory"]);
                lyricDisplay.init(music_title.Text, LyricReader.load_lyric());
            }
        }
        private void playNewSong(string music_name, string singer)
        {
            mysqlConnector.addListeningRecord(music_name, singer);
            Music finded_music = musicPath.Find(name => { return name.music_name == music_name && name.singer == singer; });
            player.Source = new Uri(finded_music.music_path);
            music_title.Text = singer + " - " + music_name;
            log.Info(String.Format("Playing song {0} by {1}...", music_name, singer));
            player.Play();
            processTimer.Start();
            cur_music_name = music_name;
            cur_singer = singer;
            if (preference["display_lyric"] == "true")
            {
                LyricReader.init(finded_music.file_name, finded_music.belong_to_list, preference["music_directory"]);
                lyricDisplay.init(music_title.Text, LyricReader.load_lyric());
            }
        }
        //更新进度条
        public double playing_process() => player.Position.TotalSeconds;
        private void ProcessTimer_Tick(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            try
            {
                changeProcess(player.Position.TotalSeconds / player.NaturalDuration.TimeSpan.TotalSeconds);
            }
            catch { }
        }
        private void changeProcess(double k)
        {
            curProcess.Width = k * whlProcess.Width;
        }
        //暂停、播放
        private bool playing = true;
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (playing)
            {
                player.Pause();
                btnPlay.Content = " ▷";
                processTimer.Stop();
                log.Info("----Music pauses...");
            }
            else
            {
                player.Play();
                btnPlay.Content = "∥";
                processTimer.Start();
                log.Info("----Music restarted");
            }
            playing = !playing;
        }
        //关闭窗体。判断是否是第一次请求关闭窗体。
        private bool direct_close = false;
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            log.Info(direct_close?"User tries to close the window.":"User double clicks to direct close the window.");
            if (direct_close) this.Close();
            else
            {
                direct_close = true;
                music_title.Text += "  播放结束时退出";
            }
        }
        //最小化窗体
        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        //移动窗体
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        //展开菜单
        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            if (basePanel.Visibility == Visibility.Hidden)
            {
                this.Height += basePanel.ActualHeight;
                basePanel.Visibility = Visibility.Visible;
                log.Info("User opens the menu...");
            }
            else
            {
                this.Height -= basePanel.ActualHeight;
                basePanel.Visibility = Visibility.Hidden;
                log.Info("User closes the menu.");
            }
        }
    }
}

