using AimpLyricsPlugin;
using NamedPipeWrapper;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using static AimpLyricsWindow.StaticCache;

namespace AimpLyricsWindow
{
    /// <summary>
    /// LyricWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LyricWindow : Window
    {
        public bool IsClosed { get; set; }


        private DispatcherTimer timer = new DispatcherTimer();
        private LyricInfo lyric;

        public LyricWindow()
        {
            ShowInTaskbar = false;
            Loaded += MainWindow_Loaded;
            try
            {
                InitializeComponent();

                Closing += LyricWindow_Closing;

                AppSettings = Settings.Read();
                DataContext = AppSettings;
                AppSettings.PropertyChanged += Setting_PropertyChanged;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var server = AppPipeServer = new NamedPipeServer<Message>("AimpLyricsPluginPipe");
            server.ClientConnected += Server_ClientConnected;
            server.ClientMessage += Server_ClientMessage;
            server.Start();
        }

        private void Server_ClientMessage(NamedPipeConnection<Message, Message> connection, Message m)
        {
            Debug.WriteLine(m);
            switch (m.Action)
            {
                case MessageAction.LineChange:
                    Dispatcher.Invoke(() => ChangedText((string)m.Data));
                    break;
                case MessageAction.PlayerClose:
                    AppPipeServer.Stop();
                    Close();
                    break;
                case MessageAction.LyricInfo:
                    lyric = m.Data;
                    LrcEditWindow?.Refresh(lyric);
                    break;
                case MessageAction.ShowEdit:
                    Dispatcher.Invoke(ShowEdit);
                    break;
            }
        }

        private void Server_ClientConnected(NamedPipeConnection<Message, Message> connection)
        {
            Console.WriteLine("已连接");
        }

        private void Setting_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppSettings.BlurRadius))
            {
                if (t1.Effect is BlurEffect be)
                    be.Radius = AppSettings.BlurRadius;
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
            AppSettings.Save();
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
                var sw = new SettingWindow();
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
            ShowEdit();
        }

        private void ShowEdit()
        {
            LrcEditWindow = new EditWindow(lyric);
            LrcEditWindow.Show();
        }
    }
}
