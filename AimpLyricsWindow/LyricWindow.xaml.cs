using AimpLyricsPlugin;
using NamedPipeWrapper;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

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
        private Settings setting;
        private NamedPipeServer<Message> server;

        public LyricWindow()
        {
            ShowInTaskbar = false;
            Loaded += MainWindow_Loaded;
            try
            {
                InitializeComponent();

                Closing += LyricWindow_Closing;

                setting = Settings.Read();
                DataContext = setting;
                setting.PropertyChanged += Setting_PropertyChanged;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            server = new NamedPipeServer<Message>("AimpLyricsPluginPipe");
            server.ClientConnected += Server_ClientConnected;
            server.ClientMessage += Server_ClientMessage;
            server.Start();
        }

        private void Server_ClientMessage(NamedPipeConnection<Message, Message> connection, Message m)
        {
            switch (m.Action)
            {
                case MessageAction.LineChange:
                    Dispatcher.Invoke(() => ChangedText((string)m.Data));
                    break;
                case MessageAction.PlayerClose:
                    server.Stop();
                    Close();
                    break;
                case MessageAction.LyricInfo:
                    lyric = m.Data;
                    break;
            }
        }

        private void Server_ClientConnected(NamedPipeConnection<Message, Message> connection)
        {
            Console.WriteLine("已连接");
        }

        private void Setting_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(setting.BlurRadius))
            {
                if (t1.Effect is BlurEffect be)
                    be.Radius = setting.BlurRadius;
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
