using System;
using System.Windows;
using System.Windows.Media;

namespace AimpLyricsPlugin
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : Window
    {
        private Settings setting;

        public SettingWindow(Settings setting)
        {
            try
            {
                this.setting = setting;

                InitializeComponent();

                cc.SelectedColor = setting.Color.Color;
                sSize.Value = setting.Size;
                sRadius.Value = setting.BlurRadius;
                tWidth.Text = setting.Width.ToString();

                Closed += SettingWindow_Closed;
                sSize.ValueChanged += FontSize_ValueChanged;
                sRadius.ValueChanged += Radius_ValueChanged;
                tWidth.TextChanged += Width_TextChanged;
                cc.SelectedColorChanged += SelectedColorChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SettingWindow_Closed(object sender, EventArgs e)
        {
            setting.Save();
        }

        private void SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            setting.ColorString = e.NewValue.ToString();
            setting.OnPropertyChanged(nameof(setting.Color));
        }

        private void FontSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            setting.Size = e.NewValue;
            setting.OnPropertyChanged(nameof(setting.Size));
        }

        private void Radius_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            setting.BlurRadius = e.NewValue;
            setting.OnPropertyChanged(nameof(setting.BlurRadius));
        }

        private void Width_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (double.TryParse(tWidth.Text, out double w) && w > 0)
            {
                setting.Width = w;
                setting.OnPropertyChanged(nameof(setting.Width));
            }
        }
    }
}
