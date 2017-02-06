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
using AIMP.SDK.Player;
using System.Windows.Threading;
using AIMP.SDK.FileManager;
using System.IO;

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

        public LyricWindow(IAimpPlayer player)
        {
            Owner = null;
            InitializeComponent();
            this.player = player;
            Closing += LyricWindow_Closing;

            timer.Interval = TimeSpan.FromSeconds(1);
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
                    Activate();
                    var str = File.ReadAllText(lrcPath);
                    lyric = new LyricInfo(str);
                }
                else
                {
                    lyric = null;
                    if (!string.IsNullOrEmpty(fi.Artist))
                        ChangedText($"{fi.Artist} - {fi.Title}");
                    else
                        ChangedText(fi.Title);
                }
            }
        }

        private void ChangedText(string text) => label.Content = text;

        private void LyricWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
            IsClosed = true;
        }

        private void label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
