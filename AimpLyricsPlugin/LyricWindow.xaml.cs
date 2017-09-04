﻿using AIMP.SDK.Player;
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
            this.player.StateChanged += Player_StateChanged;

            Closing += LyricWindow_Closing;

            setting = Settings.Read();
            DataContext = setting;
            setting.PropertyChanged += Setting_PropertyChanged;

            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Player_StateChanged(object sender, StateChangedEventArgs e)
        {
            switch (e.PlayerState)
            {
                case AIMP.SDK.AimpPlayerState.Playing:
                    timer.Start();
                    break;
                case AIMP.SDK.AimpPlayerState.Stopped:
                    timer.Stop();
                    ChangedText(string.Empty);
                    break;
                case AIMP.SDK.AimpPlayerState.Pause:
                    timer.Stop();
                    break;
            }
        }

        private void Player_TrackChanged(object sender, EventArgs e)
        {
            try
            {
                var fi = player.CurrentFileInfo;
                var file = fi?.FileName;
                if (file == null)
                {
                    lyric = null;
                    return;
                }
                var str = "";
                var innerLrc = string.IsNullOrWhiteSpace(fi.Lyrics) ? null : fi.Lyrics;
                if (setting.Inner)
                {
                    str = innerLrc ?? GetLrcString(file);
                }
                else
                {
                    str = GetLrcString(file) ?? innerLrc;
                }
                if (!string.IsNullOrEmpty(str))
                {
                    lyric = new LyricInfo(str);
                    if (lyric.LrcLines != null)
                        return;
                }
                lyric = null;
                ChangedText("");
            }
            catch
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

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var win = new EditWindow(lyric);
            win.Show();
        }
    }
}
