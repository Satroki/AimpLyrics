using AIMP.SDK;
using AIMP.SDK.MenuManager;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Security.Principal;
using System.Threading;

namespace AimpLyricsWindow
{
    [AimpPlugin("AimpLyricsPlugin", "Satroki", "1", AimpPluginType = AimpPluginType.Addons)]
    public class AimpLyricsPlugin : AimpPlugin
    {
        private string dir;
        private string winFile;
        private string logFile;
        private PlayerProxy proxy;
        private Process process;
        private IAimpMenuItem menu;
        public override void Dispose()
        {
            process.CloseMainWindow();
            process.Kill();
            Player.MenuManager.Delete(menu);
        }

        public override void Initialize()
        {
            dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            winFile = Path.Combine(dir, "AimpLyricsWindow.exe");
            logFile = Path.Combine(dir, "log.log");
            if (Player.MenuManager.CreateMenuItem(out menu) == AimpActionResult.Ok)
            {
                menu.Name = "显示歌词";
                menu.Id = "show_lyric";
                menu.Style = AimpMenuItemStyle.Normal;

                menu.OnExecute += MenuItem_OnExecute; ;
                Player.MenuManager.Add(ParentMenuType.AIMP_MENUID_COMMON_UTILITIES, menu);
            }
            StartWindow();
            proxy = new PlayerProxy(Player);
        }

        private void MenuItem_OnExecute(object sender, EventArgs e)
        {
            StartWindow();
        }

        private void StartWindow()
        {
            var ps = Process.GetProcessesByName("AimpLyricsWindow");
            if (ps.Length == 0)
                process = Process.Start(winFile);
            else
                process = ps[0];
        }
    }
}
