using AIMP.SDK.Player;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace AimpLyricsPlugin
{
    /// <summary>
    /// LyricWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LyricWindow : Window
    {
        public bool IsClosed { get; set; }

        private IAimpPlayer player;

        private DispatcherTimer timer = new DispatcherTimer();
        private string file;
        private LyricInfo lyric;
        private Settings setting;

        public LyricWindow(IAimpPlayer player)
        {
            Owner = null;
            InitializeComponent();
            this.player = player;

            Closing += LyricWindow_Closing;

            setting = Settings.Read();
            Top = setting.Top;
            Left = setting.Left;
            t2.Foreground = setting.Color;
            t1.FontSize = t2.FontSize = setting.Size;
            if (t1.Effect is BlurEffect be)
                be.Radius = setting.BlurRadius;

            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var fi = player.CurrentFileInfo;
            var current = fi?.FileName;
            if (string.IsNullOrEmpty(current))
                return;
            if (file == current)
            {
                if (lyric == null)
                    return;
                ChangedText(lyric.Seek(player.Position));
            }
            else
            {
                file = current;
                var dir = Path.GetDirectoryName(file);
                var lrc = Path.GetFileNameWithoutExtension(file) + ".lrc";
                var lrcPath = Path.Combine(dir, lrc);
                if (File.Exists(lrcPath))
                {
                    var str = File.ReadAllText(lrcPath);
                    lyric = new LyricInfo(str);
                }
                else
                {
                    lyric = null;
                    ChangedText("");
                }
            }
        }

        private void ChangedText(string text)
        {
            t1.Text = text;
            t2.Text = text;
        }

        private void LyricWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
            Save();
            IsClosed = true;
        }

        public void Save()
        {
            setting.Top = Top;
            setting.Left = Left;
            setting.Save();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            grid.Background = new SolidColorBrush(Color.FromArgb(0xb0, 0, 0, 0));
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            grid.Background = Brushes.Transparent;
        }
    }
}
