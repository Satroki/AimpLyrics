using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AimpLyricsWindow
{
    public class LyricInfo
    {
        public string Title { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;

        [JsonIgnore]
        public string LrcPath => FilePath.Substring(0, FilePath.LastIndexOf('.')) + ".lrc";

        public LyricInfo()
        {

        }

        public LyricInfo(string lrcString)
        {
            LrcFormat(lrcString);
        }

        public LyricInfo(string title, string filePath, string album = null)
        {
            Title = title;
            FilePath = filePath;
            Album = album;
        }

        public void LrcFormat(string oriLrc)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(oriLrc))
                    return;
                var lines = oriLrc.Split('\n').Where(line => line.StartsWith("["));
                var offset = new TimeSpan();
                var os = lines.FirstOrDefault(o => Regex.IsMatch(o, @"\[offset:[-\+]?\d+\]"));
                if (os != null)
                    offset = TimeSpan.FromMilliseconds(int.Parse(Regex.Match(os, @"-?\d+").Value));
                var list = new List<LrcLine>();
                foreach (var item in lines)
                {
                    var matches = Regex.Matches(item, @"\[(\d{1,2}):(\d{1,2})([\.:](\d{1,3}))?\]").Cast<Match>();
                    if (matches.Count() == 0)
                        continue;
                    var content = item.Substring(matches.Sum(m => m.Value.Length)).Trim();
                    foreach (Match m in matches)
                    {
                        var s = $"0:{m.Groups[1].Value}:{m.Groups[2].Value}";
                        if (m.Groups[4].Success)
                            s = $"{s}.{m.Groups[4].Value}";
                        list.Add(new LrcLine()
                        {
                            Content = content ?? string.Empty,
                            TimePoint = TimeSpan.Parse(s).Add(offset).TotalSeconds,
                        });
                    }
                }
                LrcLines = list.OrderBy(lrc => lrc.TimePoint).ToArray();
            }
            catch
            {
                LrcLines = null;
            }
        }

        public LrcLine[] LrcLines { get; set; }

        [JsonIgnore]
        public string Lyric => LrcLines == null ? null : string.Join("\n", LrcLines.AsEnumerable());

        public string Seek(double sec) => LrcLines?.LastOrDefault(ll => ll.TimePoint < sec)?.Content;

        public static implicit operator JToken(LyricInfo li) => JToken.FromObject(li);
        public static implicit operator LyricInfo(JToken jt) => jt.ToObject<LyricInfo>();
    }

    public class LrcLine
    {
        private double _TimePoint;

        public double TimePoint
        {
            get { return _TimePoint; }
            set { _TimePoint = value < 0 ? 0 : value; }
        }

        public string Content { get; set; }
        public override string ToString() => TimeSpan.FromSeconds(TimePoint).ToString(@"\[mm\:ss\.ff\]") + Content;
    }
}
