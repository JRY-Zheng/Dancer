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
using System.IO;

namespace Dancer
{
    /// <summary>
    /// PathSelect.xaml 的交互逻辑
    /// </summary>
    public partial class PathSelect : UserControl
    {
        private MainWindow father;
        public PathSelect(MainWindow _father)
        {
            InitializeComponent();
            father = _father;
        }
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key==Key.Enter)
            {
                search_song();
            }
            if (e.Key==Key.Tab)
            {
                input_path.Text = father.cur_music_name + " " + father.cur_singer;
                search_song();
            }
            father.btnPlay.Focus();
        }
        private void search_song()
        {
            if (father.checkSong(input_path.Text) >= 0)
            {
                change_color(1);
            }
            else change_color(-1);
        }
        private void change_color(int state)
        {
            switch (state)
            {
                case -1:
                    lamp.Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 72, 72));
                    lamp_center.Color = Color.FromArgb(255, 255, 0, 0);
                    lamp_middle.Color = Color.FromArgb(255, 255, 88, 88);
                    lamp_edge.Color = Color.FromArgb(255, 255, 124, 124);
                    break;
                case 0:
                    lamp.Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 163, 72));
                    lamp_center.Color = Color.FromArgb(255, 255, 124, 0);
                    lamp_middle.Color = Color.FromArgb(255, 255, 171, 124);
                    lamp_edge.Color = Color.FromArgb(255, 255, 189, 124);
                    break;
                case 1:
                    lamp.Stroke = new SolidColorBrush(Color.FromArgb(255, 72, 210, 72));
                    lamp_center.Color = Color.FromArgb(255, 0, 210, 0);
                    lamp_middle.Color = Color.FromArgb(255, 87, 210, 87);
                    lamp_edge.Color = Color.FromArgb(255, 167, 210, 167);
                    break;
            }
        }
        private bool hasText = false;
        private void input_path_LostFocus(object sender, RoutedEventArgs e)
        {
            if (input_path.Text == "")
            {
                input_path.Text = "选择一首歌单曲循环";
                input_path.Foreground = new SolidColorBrush(Colors.LightGray);
                hasText = false;
            }
            else hasText = true;
        }

        private void input_path_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!hasText) input_path.Text = "";
            input_path.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void input_path_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (input_path.Text == "选择一首歌单曲循环") return;
            try {change_color(0); } catch { }
        }
    }
}
