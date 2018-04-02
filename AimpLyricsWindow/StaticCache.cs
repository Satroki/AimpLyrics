using AimpLyricsPlugin;
using NamedPipeWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimpLyricsWindow
{
    public static class StaticCache
    {
        public static Settings AppSettings { get; set; }
        public static NamedPipeServer<Message> AppPipeServer { get; set; }
        public static EditWindow LrcEditWindow { get; set; }
    }
}
