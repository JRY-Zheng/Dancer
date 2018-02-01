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
using System.Windows.Threading;

namespace Dancer
{
    /// <summary>
    /// LyricDisplay.xaml 的交互逻辑
    /// </summary>
    public partial class LyricDisplay : UserControl
    {
        private MainWindow father;
        private DispatcherTimer refreshTimer = new DispatcherTimer();
        private List<MainWindow.Lyric> lyric;
        public LyricDisplay(MainWindow _father)
        {
            InitializeComponent();
            father = _father;
            refreshTimer.Interval = new TimeSpan(1);
            refreshTimer.Tick += RefreshTimer_Tick;
        }
        public void init(string title, List<MainWindow.Lyric> _lyric)
        {
            cur_lyric.Text = title;
            if (_lyric.Count == 0) return;
            next_lyric.Text = _lyric[0].lyric_content;
            lyric = _lyric;
            lyric.Sort((x,y)=> { return x.position==y.position?0:(x.position>y.position?1:-1); });
            cur_line = 0;
            refreshTimer.Start();
        }
        private int cur_line = 0;
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            if (father.playing_process() > lyric[cur_line].position)
            {
                cur_lyric.Text = next_lyric.Text;
                if (lyric.Count > cur_line + 1)
                    next_lyric.Text = lyric[++cur_line].lyric_content;
                else
                {
                    next_lyric.Text = "";
                    refreshTimer.Stop();
                }
            }
        }
    }
}
