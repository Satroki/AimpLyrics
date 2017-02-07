﻿using System;
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
            InitializeComponent();
            this.setting = setting;

            cc.SelectedColor = setting.Color.Color;
            sSzie.Value = setting.Size;

            Closed += SettingWindow_Closed;
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

        private void FontSzie_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            setting.Size = e.NewValue;
            setting.OnPropertyChanged(nameof(setting.Size));
        }
    }
}
