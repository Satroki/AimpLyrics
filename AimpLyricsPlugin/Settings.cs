using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media;
namespace AimpLyricsPlugin
{
    public class Settings : INotifyPropertyChanged
    {
        [Setting]
        public double Top { get; set; } = 100;
        [Setting]
        public double Left { get; set; } = 100;
        [Setting]
        public double Width { get; set; } = 1000;
        [Setting]
        public string ColorString { get; set; } = "#FFE2FFB7";
        [Setting]
        public double Size { get; set; } = 30;
        [Setting]
        public double BlurRadius { get; set; } = 8;
        [Setting]
        public bool Inner { get; set; } = true;
        [Setting]
        public bool Topmost { get; set; } = true;

        public SolidColorBrush Color => ConvertColor(ColorString);
        private const string FileName = "config.cfg";

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private static string GetPath()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(dir, FileName);
            return path;
        }

        public static Settings Read()
        {
            var path = GetPath();
            if (File.Exists(path))
            {
                try
                {
                    var cfg = File.ReadAllLines(path).Where(l => l != null && l.Contains("="));
                    var setting = new Settings();
                    var t = typeof(Settings);
                    foreach (var line in cfg)
                    {
                        var kv = line.Split('=');
                        var k = kv[0].Trim();
                        var v = kv[1].Trim();
                        var p = t.GetProperty(k);
                        if (p != null)
                            p.SetValue(setting, Convert.ChangeType(v, p.PropertyType));
                    }
                    return setting;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return new Settings();
        }

        public void Save()
        {
            var path = GetPath();
            var sb = new StringBuilder();
            var props = typeof(Settings).GetProperties().Where(p => p.GetCustomAttribute<SettingAttribute>() != null);
            foreach (var p in props)
            {
                sb.AppendLine($"{p.Name}={p.GetValue(this)}");
            }
            File.WriteAllText(path, sb.ToString());
        }

        public static SolidColorBrush ConvertColor(string str) => new SolidColorBrush((Color)ColorConverter.ConvertFromString(str));
    }

    public class SettingAttribute : Attribute
    {

    }
}
