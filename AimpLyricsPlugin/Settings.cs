using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace AimpLyricsPlugin
{
    public class Settings
    {
        public double Top { get; set; } = 100;
        public double Left { get; set; } = 100;
        public string ColorString { get; set; } = "#FFE2FFB7";
        public SolidColorBrush Color => ConvertColor(ColorString);
        public double Size { get; set; } = 30;
        public double BlurRadius { get; set; } = 8;

        private const string FileName = "AimpLyricsPlugin.cfg";
        private const string Commemt = "#top left color size blur_radius ";


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
                var line = File.ReadAllLines(FileName).Where(l => !l.StartsWith("#")).FirstOrDefault();
                var cfgs = line?.Split(' ');
                if (cfgs?.Length >= 5)
                {
                    try
                    {
                        return new Settings
                        {
                            Top = double.Parse(cfgs[0]),
                            Left = double.Parse(cfgs[1]),
                            ColorString = cfgs[2],
                            Size = double.Parse(cfgs[3]),
                            BlurRadius = double.Parse(cfgs[4])
                        };
                    }
                    catch { }
                }
            }
            return new Settings();
        }

        public void Save()
        {
            var path = GetPath();
            var content = $"{Commemt}{Environment.NewLine}{Top} {Left} {ColorString} {Size} {BlurRadius}";
            File.WriteAllText(path, content);
        }

        public static SolidColorBrush ConvertColor(string str) => new SolidColorBrush((Color)ColorConverter.ConvertFromString(str));
    }
}
