using NamedPipeWrapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimpLyricsPlugin
{
    public class Message
    {
        public MessageAction Action { get; set; }
        public JToken Data { get; set; }
    }

    public enum MessageAction
    {
        LineChange,
        PlayerClose,
        LyricInfo
    }
}
