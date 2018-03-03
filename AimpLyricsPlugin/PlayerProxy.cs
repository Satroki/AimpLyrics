using AIMP.SDK.Player;
using AimpLyricsPlugin;
using NamedPipeWrapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Windows.Threading;

namespace AimpLyricsWindow
{
    class PlayerProxy
    {
        private IAimpPlayer player;

        private DispatcherTimer timer = new DispatcherTimer();
        private LyricInfo lyric;
        private Settings setting;
        private NamedPipeClient<Message> client;

        public PlayerProxy(IAimpPlayer player)
        {
            client = new NamedPipeClient<Message>("AimpLyricsPluginPipe");

            client.ServerMessage += Client_ServerMessage;
            client.Start();

            this.player = player;
            this.player.TrackChanged += Player_TrackChanged;
            this.player.StateChanged += Player_StateChanged;

            setting = Settings.Read();

            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Client_ServerMessage(NamedPipeConnection<Message, Message> connection, Message message)
        {
        }

        private void Player_StateChanged(object sender, StateChangedEventArgs e)
        {
            switch (e.PlayerState)
            {
                case AIMP.SDK.AimpPlayerState.Playing:
                    timer.Start();
                    break;
                case AIMP.SDK.AimpPlayerState.Stopped:
                    timer.Stop();
                    ChangedText(string.Empty);
                    break;
                case AIMP.SDK.AimpPlayerState.Pause:
                    timer.Stop();
                    break;
            }
        }

        private void Player_TrackChanged(object sender, EventArgs e)
        {
            try
            {
                var fi = player.CurrentFileInfo;
                var file = fi?.FileName;
                if (file == null)
                {
                    lyric = null;
                    return;
                }

                var str = "";
                var innerLrc = string.IsNullOrWhiteSpace(fi.Lyrics) ? null : fi.Lyrics;
                if (setting.Inner)
                {
                    str = innerLrc ?? GetLrcString(file);
                }
                else
                {
                    str = GetLrcString(file) ?? innerLrc;
                }
                if (!string.IsNullOrEmpty(str))
                {
                    lyric = new LyricInfo(str);
                    if (lyric.LrcLines != null)
                    {
                        client.PushMessage(new Message { Action = MessageAction.LyricInfo, Data = lyric });
                        return;
                    }
                }
                lyric = null;
                ChangedText("");
            }
            catch
            {
                lyric = null;
                ChangedText("");
            }
        }

        private string GetLrcString(string file)
        {
            var dir = Path.GetDirectoryName(file);
            var lrc = Path.GetFileNameWithoutExtension(file) + ".lrc";
            var lrcPath = Path.Combine(dir, lrc);
            if (File.Exists(lrcPath))
            {
                return File.ReadAllText(lrcPath);
            }
            return null;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (lyric == null)
                return;
            ChangedText(lyric.Seek(player.Position));
        }

        private void ChangedText(string text)
        {
            client.PushMessage(new Message { Action = MessageAction.LineChange, Data = text });
        }

        private void LyricWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
            Save();
        }

        public void Save()
        {
            setting.Save();
        }

        public void Close()
        {
            client.PushMessage(new Message { Action = MessageAction.PlayerClose });
        }
    }
}
