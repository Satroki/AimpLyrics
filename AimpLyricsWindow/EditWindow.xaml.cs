using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AimpLyricsWindow
{
    /// <summary>
    /// EditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditWindow : Window
    {
        public string Lyric { get; set; }
        public EditWindow(LyricInfo lyric)
        {
            InitializeComponent();
            Lyric = lyric?.Lyric ?? string.Empty;
            DataContext = this;
        }
    }
}
