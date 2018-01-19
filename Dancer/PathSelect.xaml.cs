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
                if (Directory.Exists(input_path.Text))
                {
                    father.load_music(input_path.Text);
                    change_color(1);
                }
                else change_color(-1);
            }
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

        private void input_path_TextChanged(object sender, TextChangedEventArgs e)
        {
            try { change_color(0); }
            catch { };
        }
    }
}
