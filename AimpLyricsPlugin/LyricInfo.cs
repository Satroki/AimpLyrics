using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AimpLyricsPlugin
{
    public class LyricInfo
    {
        public LyricInfo(string lrcString)
        {
            LrcLines = InitLines(lrcString);
        }

        public static string LrcFormat(string oriLrc)
        {
            if (string.IsNullOrWhiteSpace(oriLrc))
                return string.Empty;
            var lines = oriLrc.Split('\n').Where(line => line.StartsWith("["));
            var offset = new TimeSpan();
            var os = lines.FirstOrDefault(o => Regex.IsMatch(o, @"\[offset:[-\+]?\d+\]"));
            if (os != null)
                offset = TimeSpan.FromMilliseconds(int.Parse(Regex.Match(os, @"-?\d+").Value));

            var list = new List<LrcLine>();
            foreach (var item in lines)
            {
                var matches = Regex.Matches(item, @"\[\d{2}:\d{2}[\.:]\d{2,3}\]").Cast<Match>();
                if (matches.Count() == 0)
                    continue;
                var content = item.Substring(matches.Sum(m => m.Value.Length)).Trim();
                foreach (Match m in matches)
                {
                    var s = m.Value.Substring(1, 5) + m.Value.Substring(6, 3).Replace(':', '.');
                    list.Add(new LrcLine()
                    {
                        Content = content,
                        TimePoint = TimeSpan.Parse("00:" + s).Add(offset).TotalSeconds,
                    });
                }
            }
            return string.Join("\n", list.OrderBy(lrc => lrc.TimePoint));
        }

        public LrcLine[] LrcLines { get; set; }

        public string Lyric => string.Join("\n", LrcLines.AsEnumerable());

        private LrcLine[] InitLines(string lrcString)
        {
            if (string.IsNullOrWhiteSpace(lrcString))
                return null;
            try
            {
                return ParseLine(lrcString);
            }
            catch
            {
                lrcString = LrcFormat(lrcString);
                return ParseLine(lrcString);
            }
        }

        private LrcLine[] ParseLine(string str)
        {
            var lines = str.Split('\n');
            var list = new List<LrcLine>();
            foreach (var item in lines)
            {
                list.Add(new LrcLine()
                {
                    Content = item.Substring(10).Trim(),
                    TimePoint = TimeSpan.Parse("00:" + item.Substring(1, 8)).TotalSeconds,
                });
            }
            return list.ToArray();
        }

        public string Seek(double sec) => LrcLines.LastOrDefault(ll => ll.TimePoint < sec)?.Content;
    }

    public class LrcLine
    {
        public double TimePoint { get; set; }
        public string Content { get; set; }
        public override string ToString() => TimePoint.ToString(@"\[mm\:ss\.ff\]") + Content;
    }
}
