using AimpLyricsPlugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using static AimpLyricsWindow.StaticCache;

namespace AimpLyricsWindow
{
    /// <summary>
    /// EditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditWindow : Window
    {
        private LyricInfo lyricInfo;
        private FileSystemWatcher fsw = new FileSystemWatcher();
        private string cacheDir;

        public EditWindow(LyricInfo lyric)
        {
            InitializeComponent();
            Refresh(lyric);
            AppSettings = Settings.Read();

            if (Directory.Exists(AppSettings.LrcTempPath))
            {
                fsw.Path = AppSettings.LrcTempPath;
                fsw.Created += Fsw_Created;
            }

            checkBox.Checked += CheckedChanged;
            checkBox.Unchecked += CheckedChanged;
            CheckedChanged(null, null);

            CreateCache();
        }

        private void CreateCache()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            cacheDir = Path.Combine(dir, "Cache");
            if (!Directory.Exists(cacheDir))
                Directory.CreateDirectory(cacheDir);
        }

        private void Fsw_Created(object sender, FileSystemEventArgs e)
        {
            Thread.Sleep(1000);
            var fi = new FileInfo(e.FullPath);
            if (fi.Length < 20 * 1024)
            {
                var lrc = TxtToLrc(fi.FullName);
                SaveToCache(lrc);
                Dispatcher.Invoke(() => SetText(lrc));
                SaveLrc();
            }
        }

        private void SaveToCache(string lrc)
        {
            var file = $"{lyricInfo.Album} - {lyricInfo.Title}.lrc";
            string.Join("_", file.Split(Path.GetInvalidFileNameChars()));
            File.WriteAllText(Path.Combine(cacheDir, file), lrc);
        }

        private void SetText(string lrc = null)
        {
            if (!string.IsNullOrEmpty(lrc))
                lyricInfo.LrcFormat(lrc);
            TbLrc.Text = lyricInfo?.Lyric;
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            SetText(TbLrc.Text);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveLrc();
            Close();
        }

        private void SaveLrc()
        {
            AppPipeServer.PushMessage(new Message
            {
                Action = MessageAction.LyricInfo,
                Data = lyricInfo
            });
            File.WriteAllText(lyricInfo.LrcPath, lyricInfo.Lyric);
        }

        private void Offset_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbOffset.Text))
                return;
            var ts = double.Parse(tbOffset.Text) / 1000;
            foreach (var line in lyricInfo.LrcLines)
            {
                line.TimePoint += ts;
            }
            SetText();
            SaveLrc();
        }

        internal void Refresh(LyricInfo lyric)
        {
            Dispatcher.Invoke(() =>
            {
                lyricInfo = lyric;
                Title = lyric?.Title + " - 歌词";
                SetText();
                Clipboard.SetText(lyric?.Title ?? string.Empty);
            });
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            SaveLrc();
        }

        private void TbLrc_Drop(object sender, DragEventArgs e)
        {
            var file = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            var lrc = TxtToLrc(file);
            SetText(lrc);
            SaveLrc();
        }

        private void TbLrc_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        private void OpenTemp_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(AppSettings.LrcTempPath);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            fsw.EnableRaisingEvents = false;
            fsw.Dispose();
            LrcEditWindow = null;
        }

        private void CheckedChanged(object sender, RoutedEventArgs e)
        {
            fsw.EnableRaisingEvents = checkBox.IsChecked ?? false;
        }

        private void RegexDel_Click(object sender, RoutedEventArgs e)
        {
            TbLrc.Text = Regex.Replace(TbLrc.Text, TbPatt.Text, string.Empty);
        }

        private void OpenLrc_Clcik(object sender, RoutedEventArgs e)
        {
            if (File.Exists(lyricInfo.LrcPath))
                SetText(File.ReadAllText(lyricInfo.LrcPath, Encoding.UTF8));
        }

        private string TxtToLrc(string fileName)
        {
            try
            {
                var str = File.ReadAllText(fileName, Encoding.UTF8);
                if (fileName.ToUpper().EndsWith(".LRC"))
                    return str;
                else
                {
                    dynamic json = JsonConvert.DeserializeObject(str);
                    return json.lrc.lyric.Value;
                }
            }
            catch
            { return string.Empty; }
        }

        private void ConvertTempLrcFile()
        {
            try
            {
                fsw.EnableRaisingEvents = false;
                var files = new DirectoryInfo(AppSettings.LrcTempPath).GetFiles();
                foreach (var f in files)
                {
                    try
                    {
                        if (f.Name.Contains(".") || f.Length > 15 * 1024)
                            continue;
                        var txt = File.ReadAllText(f.FullName, Encoding.UTF8);
                        if (txt.StartsWith("{"))
                        {
                            var j = JObject.Parse(txt);
                            var lrc = j["lrc"]?["lyric"]?.Value<string>();
                            if (lrc == null)
                                continue;
                            File.WriteAllText(Path.Combine(f.DirectoryName, f.LastWriteTime.ToString("0 yyyyMMddHHmmss") + ".lrc"), lrc);
                        }
                    }
                    catch { }
                }
                MessageBox.Show("完成");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                fsw.EnableRaisingEvents = true;
            }
        }

        private void TempConvert_Click(object sender, RoutedEventArgs e)
        {
            ConvertTempLrcFile();
        }
    }
}
