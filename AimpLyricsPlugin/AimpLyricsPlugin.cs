using AIMP.SDK;
using AIMP.SDK.MenuManager;

namespace AimpLyricsPlugin
{
    [AimpPlugin("AimpLyricsPlugin", "Satroki", "1", AimpPluginType = AimpPluginType.Addons)]
    public class AimpLyricsPlugin : AimpPlugin
    {
        private LyricWindow win;
        private IAimpMenuItem menu;
        public override void Dispose()
        {
            Player.MenuManager.Delete(menu);
            win.Close();
        }

        public override void Initialize()
        {
            if (Player.MenuManager.CreateMenuItem(out menu) == AimpActionResult.Ok)
            {
                menu.Name = "显示歌词";
                menu.Id = "show_lyric";
                menu.Style = AimpMenuItemStyle.Normal;

                menu.OnExecute += MenuItem_OnExecute; ;
                Player.MenuManager.Add(ParentMenuType.AIMP_MENUID_COMMON_UTILITIES, menu);
            }
            win = new LyricWindow(Player);
            win.Show();
        }

        private void MenuItem_OnExecute(object sender, System.EventArgs e)
        {
            if (win.IsClosed)
            {
                win = new LyricWindow(Player);
                win.Show();
            }
            else
            {
                win.Activate();
            }
        }
    }
}
