using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
namespace AimpLyricsPlugin
{
    public class Settings : INotifyPropertyChanged
    {
        public double Top { get; set; } = 100;
        public double Left { get; set; } = 100;
        public double Width { get; set; } = 1000;
        public string ColorString { get; set; } = "#FFE2FFB7";
        public double Size { get; set; } = 30;
        public double BlurRadius { get; set; } = 8;

        [JsonIgnore]
        public SolidColorBrush Color => ConvertColor(ColorString);
        [JsonIgnore]
        private const string FileName = "config.json";

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
                    var cfg = File.ReadAllText(path);
                    return JsonConvert.DeserializeObject<Settings>(cfg);
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
            var content = JsonConvert.SerializeObject(this);
            File.WriteAllText(path, content);
        }

        public static SolidColorBrush ConvertColor(string str) => new SolidColorBrush((Color)ColorConverter.ConvertFromString(str));
    }
}
