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
        private LyricInfo lyric;
        private Settings setting;

        public LyricWindow(IAimpPlayer player)
        {
            Owner = null;
            InitializeComponent();
            this.player = player;
            this.player.TrackChanged += Player_TrackChanged;

            Closing += LyricWindow_Closing;

            setting = Settings.Read();
            DataContext = setting;
            setting.PropertyChanged += Setting_PropertyChanged;

            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Player_TrackChanged(object sender, EventArgs e)
        {
            var fi = player.CurrentFileInfo;
            var file = fi?.FileName;
            if (file == null)
            {
                lyric = null;
                return;
            }
            var str = "";
            if (setting.Inner)
            {
                str = fi.Lyrics ?? GetLrcString(file);
            }
            else
            {
                str = GetLrcString(file) ?? fi.Lyrics;
            }
            if (!string.IsNullOrEmpty(str))
            {
                lyric = new LyricInfo(str);
            }
            else
            {
                lyric = null;
                ChangedText("");
            }
        }

        private string GetLrcString(string file)
        {
            var dir = Path.GetDirectoryName(file);
            var lrc = Path.GetFileNameWithoutExtension(file) + ".lrc";
            var lrcPath = Path.Combine(dir, lrc);
            if (File.Exists(lrcPath))
            {
                return File.ReadAllText(lrcPath);
            }
            return null;
        }

        private void Setting_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(setting.BlurRadius))
            {
                if (t1.Effect is BlurEffect be)
                    be.Radius = setting.BlurRadius;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (lyric == null)
                return;
            ChangedText(lyric.Seek(player.Position));
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

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sw = new SettingWindow(setting);
                sw.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
